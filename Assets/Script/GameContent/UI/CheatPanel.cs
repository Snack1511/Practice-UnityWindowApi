#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Script.Manager.SingletonManager;
using Script.Manager.StaticManager;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

namespace Script.GameContent.UI
{
    /// <summary>
    /// 런타임 치트 패널 + 콘솔.
    /// advise/001 의 1-5(BlitPass 스택이 필요한지)와 1-4(클릭 통과 해제가 되는지)를
    /// 빌드 하나에서 켜고 끄며 비교하기 위한 개발용 도구다.
    /// DEVELOPMENT_BUILD 또는 에디터에서만 컴파일된다 — 릴리즈 빌드에는 들어가지 않는다.
    ///
    /// 씬에 배치하지 않고 <see cref="CreateAsync"/> 로 런타임에 만든다.
    /// 인스펙터가 없으므로 렌더러 데이터는 파이프라인 에셋에서 직접 찾는다.
    /// </summary>
    public class CheatPanel : MonoBehaviour
    {
        // ResourcesManager.LoadAsync 는 Path.GetExtension 결과로 Replace 를 하므로
        // 확장자 없는 경로를 넘기면 ArgumentException 이 난다. 확장자를 붙여 호출한다.
        // Resources 는 확장자를 뗀 경로로 색인하므로 uxml 과 uss 의 파일명이 같으면
        // "UI/CheatPanel" 하나로 충돌한다. 그래서 스타일시트 파일명을 따로 뒀다.
        private const string ThemePath = "UI/UnityDefaultRuntimeTheme.tss";
        private const string TreePath = "UI/CheatPanel.uxml";
        private const string StylePath = "UI/CheatPanelStyle.uss";
        private const string UpdateKey = "CheatPanel";

        private const string FoldedClass = "panel--folded";

        /// <summary>콘솔이 보관하는 최대 줄 수. 넘으면 오래된 것부터 버린다.</summary>
        private const int MaxConsoleLines = 200;

        public KeyCode ToggleKey { get; set; } = KeyCode.O;

        private VisualElement root;
        private VisualElement cheatRoot;
        private VisualElement consoleRoot;
        private ScrollView list;
        private ScrollView consoleList;
        private ScriptableRendererData ownerRendererData;

        private bool visible;

        // UI 클릭이 막혀도 치트를 쓸 수 있도록 숫자키로도 토글한다.
        // 등록 순서가 곧 Alpha1..Alpha9 순서다.
        private readonly List<Toggle> toggles = new List<Toggle>();

        // 사용자가 토글로 요청한 클릭 통과 여부. 실제 창 상태와는 다를 수 있다 —
        // 커서가 패널 위에 있으면 일시적으로 해제하기 때문이다.
        private bool clickThroughRequested;
        private bool clickThroughApplied;

        private readonly Dictionary<ScriptableRendererFeature, bool> featureOriginalState =
            new Dictionary<ScriptableRendererFeature, bool>();

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
            RestoreFeatures();
        }

        private async UniTask<bool> BuildDocumentAsync()
        {
            ThemeStyleSheet theme = await ResourcesManager.Instance.LoadAsync<ThemeStyleSheet>(ThemePath);
            VisualTreeAsset tree = await ResourcesManager.Instance.LoadAsync<VisualTreeAsset>(TreePath);
            StyleSheet style = await ResourcesManager.Instance.LoadAsync<StyleSheet>(StylePath);

            if (theme == null || tree == null || style == null)
            {
                Debug.LogError("[CheatPanel] tss / uxml / uss 를 불러오지 못했습니다.");
                return false;
            }

            PanelSettings settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.themeStyleSheet = theme;
            settings.scaleMode = PanelScaleMode.ConstantPixelSize;

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
            list = root.Q<ScrollView>("cheat-list");
            consoleList = root.Q<ScrollView>("console-list");

            if (cheatRoot == null || consoleRoot == null || list == null || consoleList == null)
            {
                Debug.LogError("[CheatPanel] uxml 에서 필요한 요소를 찾지 못했습니다.");
                return false;
            }

            SetupPanelChrome(cheatRoot, "cheat-header", "cheat-fold", draggable: true);
            SetupPanelChrome(consoleRoot, "console-header", "console-fold", draggable: true);

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
            BlitFeature blit = FindFeature<BlitFeature>();
            if (blit != null)
            {
                featureOriginalState[blit] = blit.isActive;

                AddToggle("Set BitBlitPass", blit.isActive, value =>
                {
                    blit.SetActive(value);
                    if (ownerRendererData != null)
                        ownerRendererData.SetDirty();

                    Debug.Log($"[CheatPanel] BlitFeature.isActive = {blit.isActive}");
                });
            }

#if !UNITY_EDITOR
            // 에디터에서 켜면 에디터 창 자체가 클릭 통과 상태가 되어 조작이 불가능해진다. 빌드에서만 노출한다.
            AddToggle("Set TransparentClick", WindowNativeManager.IsTransparentClick, value =>
            {
                clickThroughRequested = value;
                ApplyClickThrough(value && !IsPointerOverPanel());

                Debug.Log($"[CheatPanel] TransparentClick 요청 = {value}");
            });
#endif
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

        private T FindFeature<T>() where T : ScriptableRendererFeature
        {
            if (GraphicsSettings.currentRenderPipeline is not UniversalRenderPipelineAsset pipeline)
            {
                Debug.LogError("[CheatPanel] 현재 렌더 파이프라인이 URP 가 아닙니다.");
                return null;
            }

            FieldInfo field = typeof(UniversalRenderPipelineAsset)
                .GetField("m_RendererDataList", BindingFlags.NonPublic | BindingFlags.Instance);

            if (field?.GetValue(pipeline) is not ScriptableRendererData[] dataList)
            {
                Debug.LogError("[CheatPanel] 렌더러 데이터 목록을 찾지 못했습니다. URP 내부 필드명이 바뀌었을 수 있습니다.");
                return null;
            }

            foreach (ScriptableRendererData data in dataList)
            {
                if (data == null)
                    continue;

                foreach (ScriptableRendererFeature feature in data.rendererFeatures)
                {
                    if (feature is not T typed)
                        continue;

                    ownerRendererData = data;
                    return typed;
                }
            }

            Debug.LogWarning($"[CheatPanel] {typeof(T).Name} 를 찾지 못했습니다.");
            return null;
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

            return Contains(cheatRoot, panelPoint) || Contains(consoleRoot, panelPoint);
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

        private void RestoreFeatures()
        {
            if (featureOriginalState.Count == 0)
                return;

            foreach (KeyValuePair<ScriptableRendererFeature, bool> pair in featureOriginalState)
            {
                if (pair.Key != null)
                    pair.Key.SetActive(pair.Value);
            }

            if (ownerRendererData != null)
                ownerRendererData.SetDirty();
        }
    }
}
#endif
