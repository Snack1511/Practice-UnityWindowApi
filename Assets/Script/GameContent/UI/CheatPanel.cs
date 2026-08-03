#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System;
using System.Collections.Generic;
using Script.Manager.StaticManager;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

namespace Script.GameContent.UI
{
    /// <summary>
    /// 런타임 치트 패널.
    /// advise/001 의 1-5(BlitPass 스택이 필요한지)와 1-4(클릭 통과 해제가 되는지)를
    /// 빌드 하나에서 켜고 끄며 비교하기 위한 개발용 도구다.
    /// DEVELOPMENT_BUILD 또는 에디터에서만 컴파일된다 — 릴리즈 빌드에는 들어가지 않는다.
    /// </summary>
    public class CheatPanel : MonoBehaviour
    {
        private const string ResourceRoot = "UI/";
        private const string UpdateKey = "CheatPanel";

        [SerializeField, Tooltip("Assets/Settings/PC_Renderer.asset 을 연결한다")]
        private UniversalRendererData rendererData;

        [SerializeField]
        private KeyCode toggleKey = KeyCode.F1;

        private VisualElement root;
        private ScrollView list;

        // 에디터에서 토글하면 PC_Renderer.asset 이 실제로 변경되어 플레이 모드를 나가도 유지된다.
        // 원래 값을 들고 있다가 파기 시점에 되돌린다.
        private readonly Dictionary<ScriptableRendererFeature, bool> featureOriginalState =
            new Dictionary<ScriptableRendererFeature, bool>();

        private void Awake()
        {
            if (!BuildDocument())
                return;

            RegisterCheats();
            SetVisible(false);

            GameProcessManager.AddUpdate(UpdateKey, OnUpdate);
        }

        private void OnDestroy()
        {
            RestoreFeatures();
        }

        /// <summary>
        /// PanelSettings 와 테마를 런타임에 만든다. 프로젝트에 UI Toolkit 런타임 자산이 없어서
        /// .asset 을 손으로 만들지 않고 Resources 에서 읽어 조립한다.
        /// </summary>
        private bool BuildDocument()
        {
            ThemeStyleSheet theme = Resources.Load<ThemeStyleSheet>(ResourceRoot + "UnityDefaultRuntimeTheme");
            VisualTreeAsset tree = Resources.Load<VisualTreeAsset>(ResourceRoot + "CheatPanel");
            StyleSheet style = Resources.Load<StyleSheet>(ResourceRoot + "CheatPanel");

            if (theme == null || tree == null || style == null)
            {
                Debug.LogError($"[CheatPanel] Resources/{ResourceRoot} 아래 tss / uxml / uss 를 찾지 못했습니다.");
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

            list = root.Q<ScrollView>("cheat-list");
            if (list == null)
            {
                Debug.LogError("[CheatPanel] uxml 에서 'cheat-list' ScrollView 를 찾지 못했습니다.");
                return false;
            }

            return true;
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
                    rendererData.SetDirty();
                });
            }

#if !UNITY_EDITOR
            // 에디터에서 켜면 에디터 창 자체가 클릭 통과 상태가 되어 조작이 불가능해진다. 빌드에서만 노출한다.
            AddToggle("Set TransparentClick", WindowNativeManager.IsTransparentClick,
                value => WindowNativeManager.SetTransparentClick(value));
#endif
        }

        /// <summary>치트 항목을 추가한다. 새 치트는 여기에 한 줄이면 된다.</summary>
        public void AddToggle(string label, bool initial, Action<bool> onChanged)
        {
            Toggle toggle = new Toggle(label) { value = initial };
            toggle.AddToClassList("cheat-toggle");
            toggle.RegisterValueChangedCallback(evt => onChanged(evt.newValue));

            list.Add(toggle);
        }

        private T FindFeature<T>() where T : ScriptableRendererFeature
        {
            if (rendererData == null)
            {
                Debug.LogError("[CheatPanel] rendererData 가 비어 있습니다. 인스펙터에 Assets/Settings/PC_Renderer.asset 을 연결하세요.");
                return null;
            }

            foreach (ScriptableRendererFeature feature in rendererData.rendererFeatures)
            {
                if (feature is T typed)
                    return typed;
            }

            Debug.LogWarning($"[CheatPanel] {typeof(T).Name} 를 rendererFeatures 에서 찾지 못했습니다.");
            return null;
        }

        // GameProcessManager 에 RemoveUpdate 가 없어서, 파기된 뒤에도 호출될 수 있다.
        private void OnUpdate()
        {
            if (this == null || root == null)
                return;

            if (Input.GetKeyDown(toggleKey))
                SetVisible(root.style.display == DisplayStyle.None);
        }

        private void SetVisible(bool visible)
        {
            root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
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

            if (rendererData != null)
                rendererData.SetDirty();
        }
    }
}
#endif
