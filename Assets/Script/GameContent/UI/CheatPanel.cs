#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Script.Manager.SingletonManager;
using Script.Manager.StaticManager;
using UnityEngine;
using UnityEngine.UIElements;

namespace Script.GameContent.UI
{
    /// <summary>
    /// 런타임 치트 패널 + 콘솔.
    /// 빌드에서 기능을 켜고 끄며 비교하기 위한 개발용 도구다.
    /// DEVELOPMENT_BUILD 또는 에디터에서만 컴파일된다 — 릴리즈 빌드에는 들어가지 않는다.
    ///
    /// 씬에 배치하지 않고 <see cref="CreateAsync"/> 로 런타임에 만든다.
    /// </summary>
    public class CheatPanel : MonoBehaviour
    {
        // ResourcesManager.LoadAsync 는 Path.GetExtension 결과로 Replace 를 하므로
        // 확장자 없는 경로를 넘기면 ArgumentException 이 난다. 확장자를 붙여 호출한다.
        // Resources 는 확장자를 뗀 경로로 색인하므로 uxml 과 uss 의 파일명이 같으면
        // "UI/CheatPanel" 하나로 충돌한다. 그래서 스타일시트 파일명을 따로 뒀다.
        // PanelSettings 는 런타임 생성이 아니라 에셋으로 둔다.
        // 6000.5 의 Advanced Text Generator 는 PanelSettings 의 ICU 데이터를 요구하는데,
        // ScriptableObject.CreateInstance 로 만든 인스턴스에는 에디터가 그 참조를 붙여주지 못한다.
        // 그 상태로 두면 InitTextLib 이 실패하면서 레이아웃 갱신이 매 프레임 예외로 죽는다.
        private const string SettingsPath = "UI/CheatPanelSettings.asset";
        private const string TreePath = "UI/CheatPanel.uxml";
        private const string StylePath = "UI/CheatPanelStyle.uss";
        private const string UpdateKey = "CheatPanel";

        private const string FoldedClass = "panel--folded";

        /// <summary>콘솔이 보관하는 최대 줄 수. 넘으면 오래된 것부터 버린다.</summary>
        private const int MaxConsoleLines = 200;

        /// <summary>
        /// 에디터에서 치트 패널을 켤지 저장하는 키. 에디터 툴바 토글이 같은 키를 쓴다.
        /// 런타임 스크립트라 <c>EditorPrefs</c> 를 쓸 수 없어 <c>PlayerPrefs</c> 에 둔다 —
        /// <c>UnityEditor</c> 네임스페이스를 참조하면 asmdef 가 없어 빌드에서만 터진다.
        /// </summary>
        public const string EnabledPrefKey = "CheatPanel.EnabledInEditor";

        /// <summary>
        /// 치트 패널을 만들지 여부.
        /// **에디터는 기본 꺼짐** — 치트가 전부 빌드 창을 대상으로 하고, 에디터에서는 게임 뷰를 가리기만 한다.
        /// **개발 빌드는 항상 켜짐** — 빌드에서 상태를 볼 수단이 이것뿐이다.
        /// </summary>
        public static bool IsEnabled
        {
#if UNITY_EDITOR
            get { return PlayerPrefs.GetInt(EnabledPrefKey, 0) != 0; }
#else
            get { return true; }
#endif
        }

        public KeyCode ToggleKey { get; set; } = KeyCode.O;

        private VisualElement root;
        private VisualElement cheatRoot;
        private VisualElement consoleRoot;
        private VisualElement displayRoot;
        private ScrollView list;
        private ScrollView consoleList;
        private ScrollView displayList;

        private bool visible;

        //세부 치트 패널은 치트 패널과 따로 열고 닫는다.
        private bool displayPanelVisible;

        // UI 클릭이 막혀도 치트를 쓸 수 있도록 숫자키로도 토글한다.
        // 등록 순서가 곧 Alpha1..Alpha9 순서다.
        private readonly List<Toggle> toggles = new List<Toggle>();

        // 사용자가 토글로 요청한 클릭 통과 여부. 실제 창 상태와는 다를 수 있다 —
        // 커서가 패널 위에 있으면 일시적으로 해제하기 때문이다.
        private bool clickThroughRequested;
        private bool clickThroughApplied;

        /// <summary>GameObject 를 만들고 초기화까지 마친 뒤 반환한다. 실패하면 null.</summary>
        public static async UniTask<CheatPanel> CreateAsync()
        {
            GameObject go = new GameObject(nameof(CheatPanel));
            CheatPanel panel = go.AddComponent<CheatPanel>();

            if (await panel.InitializeAsync())
                return panel;

            Destroy(go);
            return null;
        }

        private async UniTask<bool> InitializeAsync()
        {
            if (!await BuildDocumentAsync())
                return false;

            RegisterCheats();
            SetVisible(false);

            Application.logMessageReceived += OnLogMessageReceived;
            GameProcessManager.AddUpdate(UpdateKey, OnUpdate);

            Debug.Log($"[CheatPanel] 생성 완료. 토글 {toggles.Count} 개, 씬 '{gameObject.scene.name}', 토글키 {ToggleKey}. 숫자키 1~{toggles.Count} 로도 조작 가능.");

            //스타일이 실제로 먹었는지는 레이아웃이 한 번 돈 뒤에야 알 수 있다.
            cheatRoot.schedule.Execute(() =>
            {
                Debug.Log($"[CheatPanel] 스타일 진단 : styleSheets={root.styleSheets.count}, " +
                          $"position={cheatRoot.resolvedStyle.position}, " +
                          $"bg={cheatRoot.resolvedStyle.backgroundColor}, " +
                          $"size={cheatRoot.resolvedStyle.width}x{cheatRoot.resolvedStyle.height}");
            }).ExecuteLater(100);

            return true;
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
        }

        private async UniTask<bool> BuildDocumentAsync()
        {
            PanelSettings settings = await ResourcesManager.Instance.LoadAsync<PanelSettings>(SettingsPath);
            VisualTreeAsset tree = await ResourcesManager.Instance.LoadAsync<VisualTreeAsset>(TreePath);
            StyleSheet style = await ResourcesManager.Instance.LoadAsync<StyleSheet>(StylePath);

            if (settings == null || tree == null || style == null)
            {
                Debug.LogError("[CheatPanel] asset / uxml / uss 를 불러오지 못했습니다.");
                return false;
            }

            UIDocument document = gameObject.AddComponent<UIDocument>();
            document.panelSettings = settings;
            document.visualTreeAsset = tree;

            root = document.rootVisualElement;
            if (root == null)
            {
                Debug.LogError("[CheatPanel] rootVisualElement 가 생성되지 않았습니다.");
                return false;
            }

            root.styleSheets.Add(style);

            cheatRoot = root.Q<VisualElement>("cheat-root");
            consoleRoot = root.Q<VisualElement>("console-root");
            displayRoot = root.Q<VisualElement>("display-root");
            list = root.Q<ScrollView>("cheat-list");
            consoleList = root.Q<ScrollView>("console-list");
            displayList = root.Q<ScrollView>("display-list");

            if (cheatRoot == null || consoleRoot == null || displayRoot == null
                || list == null || consoleList == null || displayList == null)
            {
                Debug.LogError("[CheatPanel] uxml 에서 필요한 요소를 찾지 못했습니다.");
                return false;
            }

            SetupPanelChrome(cheatRoot, "cheat-header", "cheat-fold", draggable: true);
            SetupPanelChrome(consoleRoot, "console-header", "console-fold", draggable: true);
            SetupPanelChrome(displayRoot, "display-header", "display-fold", draggable: true);

            return true;
        }

        /// <summary>헤더에 드래그 이동과 접기를 붙인다.</summary>
        private void SetupPanelChrome(VisualElement panel, string headerName, string foldName, bool draggable)
        {
            VisualElement header = root.Q<VisualElement>(headerName);
            Label fold = root.Q<Label>(foldName);

            if (fold != null)
            {
                fold.RegisterCallback<PointerDownEvent>(evt =>
                {
                    bool folded = panel.ClassListContains(FoldedClass);
                    panel.EnableInClassList(FoldedClass, !folded);
                    fold.text = folded ? "—" : "+";

                    //헤더 드래그로 번지지 않게 막는다.
                    evt.StopPropagation();
                });
            }

            if (!draggable || header == null)
                return;

            Vector2 grabOffset = Vector2.zero;
            bool dragging = false;

            header.RegisterCallback<PointerDownEvent>(evt =>
            {
                dragging = true;
                grabOffset = (Vector2)evt.position - new Vector2(panel.worldBound.x, panel.worldBound.y);
                header.CapturePointer(evt.pointerId);
                evt.StopPropagation();
            });

            header.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (!dragging)
                    return;

                Vector2 target = (Vector2)evt.position - grabOffset;

                //좌상단 기준으로 옮긴다. right/bottom 을 풀지 않으면 폭이 늘어난다.
                panel.style.left = target.x;
                panel.style.top = target.y;
                panel.style.right = StyleKeyword.Auto;
                panel.style.bottom = StyleKeyword.Auto;
                panel.style.width = panel.resolvedStyle.width;

                evt.StopPropagation();
            });

            header.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (!dragging)
                    return;

                dragging = false;
                header.ReleasePointer(evt.pointerId);
                evt.StopPropagation();
            });
        }

        private void RegisterCheats()
        {
#if !UNITY_EDITOR
            // 에디터에서 켜면 에디터 창 자체가 클릭 통과 상태가 되어 조작이 불가능해진다. 빌드에서만 노출한다.
            AddToggle("Set TransparentClick", WindowNativeManager.IsTransparentClick, value =>
            {
                clickThroughRequested = value;
                ApplyClickThrough(value && !IsPointerOverPanel());

                Debug.Log($"[CheatPanel] TransparentClick 요청 = {value}");
            });

            // 세부 치트는 별도 패널로 연다. 치트가 늘어도 한 패널에 다 밀어넣지 않기 위한 형태다.
            // 에디터에서 창을 옮기면 에디터 창 자체가 움직이므로 빌드에서만 노출한다.
            BuildDisplayCheats();
            AddButton("화면 설정", ToggleDisplayPanel);
#endif

            // 오버레이 창은 테두리가 없어 닫을 방법이 마땅치 않다. 종료 경로를 남긴다.
            // 에디터에서 Application.Quit 은 아무 일도 하지 않는다 — 로그로 눌린 것만 확인된다.
            AddButton("Quit", () =>
            {
                Debug.Log("[CheatPanel] 종료 요청");
                Application.Quit();
            });
        }

        /// <summary>치트 항목을 추가한다. 새 치트는 여기에 한 줄이면 된다.</summary>
        public void AddToggle(string label, bool initial, Action<bool> onChanged)
        {
            Toggle toggle = new Toggle(label) { value = initial };
            toggle.AddToClassList("cheat-toggle");
            toggle.RegisterValueChangedCallback(evt => onChanged(evt.newValue));

            list.Add(toggle);
            toggles.Add(toggle);
        }

        /// <summary>동작 버튼을 추가한다. 토글이 아니므로 숫자키 매핑에는 들어가지 않는다.</summary>
        public void AddButton(string label, Action onClick)
        {
            Button button = new Button(onClick) { text = label };
            button.AddToClassList("cheat-button");

            list.Add(button);
        }

        /// <summary>세부 치트 패널을 열고 닫는다.</summary>
        private void ToggleDisplayPanel()
        {
            displayPanelVisible = !displayPanelVisible;
            displayRoot.style.display = displayPanelVisible ? DisplayStyle.Flex : DisplayStyle.None;

            Debug.Log($"[CheatPanel] 화면 설정 패널 = {displayPanelVisible}");
        }

        /// <summary>세부 치트 : 연결된 모니터의 작업 영역으로 창을 옮긴다.</summary>
        private void BuildDisplayCheats()
        {
#if !UNITY_EDITOR
            displayList.Clear();

            System.Collections.Generic.List<WindowNativeManager.MonitorInfo> monitors = WindowNativeManager.GetMonitors();
            if (monitors.Count == 0)
            {
                Label empty = new Label("모니터를 찾지 못했습니다.");
                empty.AddToClassList("cheat-empty");
                displayList.Add(empty);

                Debug.LogWarning("[CheatPanel] 모니터 열거 결과가 0개입니다.");
                return;
            }

            for (int i = 0; i < monitors.Count; i++)
            {
                //람다가 붙들 값이라 반복마다 새 지역 변수에 담는다.
                WindowNativeManager.MonitorInfo monitor = monitors[i];
                string label = $"{i + 1}. {monitor.width}x{monitor.height}{(monitor.isPrimary ? " (주)" : "")}";

                Button button = new Button(() => WindowNativeManager.SetWindowToMonitor(monitor)) { text = label };
                button.AddToClassList("cheat-button");
                displayList.Add(button);
            }

            Debug.Log($"[CheatPanel] 화면 설정 : 모니터 {monitors.Count} 개 등록");
#endif
        }

        private void OnUpdate()
        {
            if (this == null || root == null)
                return;

            if (Input.GetKeyDown(ToggleKey))
                SetVisible(!visible);

            UpdateKeyboardShortcuts();
            UpdateClickThrough();
        }

        /// <summary>
        /// UI 클릭이 막혀도 치트를 쓸 수 있게 숫자키로 토글한다.
        /// 입력 경로 문제와 치트 기능 자체를 분리해서 확인하기 위한 경로다.
        /// </summary>
        private void UpdateKeyboardShortcuts()
        {
            if (!visible)
                return;

            for (int i = 0; i < toggles.Count && i < 9; i++)
            {
                if (!Input.GetKeyDown(KeyCode.Alpha1 + i))
                    continue;

                Toggle toggle = toggles[i];
                toggle.value = !toggle.value;

                Debug.Log($"[CheatPanel] 숫자키 {i + 1} → '{toggle.label}' = {toggle.value}");
            }
        }

        /// <summary>
        /// WS_EX_TRANSPARENT 는 창 전체에 걸리는 플래그라 영역별 예외를 둘 수 없다.
        /// 커서가 패널 위에 있는 동안만 플래그를 빼서, 클릭 통과 중에도 패널은 조작 가능하게 만든다.
        /// </summary>
        private void UpdateClickThrough()
        {
            if (!clickThroughRequested)
                return;

            ApplyClickThrough(!IsPointerOverPanel());
        }

        private void ApplyClickThrough(bool enable)
        {
            if (clickThroughApplied == enable)
                return;

            clickThroughApplied = enable;
            WindowNativeManager.SetTransparentClick(enable);
        }

        private bool IsPointerOverPanel()
        {
            if (!visible || root?.panel == null)
                return false;

            //Input.mousePosition 은 좌하단 기준, UI Toolkit 패널은 좌상단 기준이라 Y 를 뒤집는다.
            Vector3 mouse = Input.mousePosition;
            Vector2 screenPoint = new Vector2(mouse.x, Screen.height - mouse.y);
            Vector2 panelPoint = RuntimePanelUtils.ScreenToPanel(root.panel, screenPoint);

            return Contains(cheatRoot, panelPoint)
                   || Contains(consoleRoot, panelPoint)
                   || Contains(displayRoot, panelPoint);
        }

        private static bool Contains(VisualElement element, Vector2 point)
        {
            return element != null
                   && element.resolvedStyle.display != DisplayStyle.None
                   && element.worldBound.Contains(point);
        }

        private void SetVisible(bool value)
        {
            visible = value;
            root.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;

            if (clickThroughRequested)
                ApplyClickThrough(!value || !IsPointerOverPanel());
        }

        /// <summary>Unity 로그를 화면 하단 콘솔에 그대로 보여준다.</summary>
        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (consoleList == null)
                return;

            Label line = new Label(condition);
            line.AddToClassList("console-line");
            line.AddToClassList(ClassOf(type));

            consoleList.Add(line);

            while (consoleList.childCount > MaxConsoleLines)
                consoleList.RemoveAt(0);

            consoleList.ScrollTo(line);
        }

        private static string ClassOf(LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                case LogType.Assert:
                    return "console-error";

                case LogType.Warning:
                    return "console-warning";

                default:
                    return "console-log";
            }
        }
    }
}
#endif
