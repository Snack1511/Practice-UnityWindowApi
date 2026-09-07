using pattern;
using Script.GameContent.UI;
using UnityEngine;

namespace Script.Manager.SingletonManager
{
    /// <summary>
    /// 전역 UIRoot 를 소유하는 매니저.
    /// UIRoot 는 씬에 두지 않고 부팅 시 1회 만들어 DontDestroyOnLoad 로 유지한다 — 씬마다 두면 EventSystem 이 중복되어 UI 입력이 죽는다 (advise/002-8).
    ///
    /// 로드를 동기로 하는 이유: EventSystem 이 첫 씬보다 늦게 생기면 그 사이에 뜬 UI 가 입력을 못 받는다.
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        private const string UIRootPath = "Prefabs/UIRoot.prefab";

        private UIRoot uiRoot;

        public void Initialize()
        {
            if (null != uiRoot)
                return;

            GameObject origin = ResourcesManager.Instance.Load<GameObject>(UIRootPath);
            if (null == origin)
            {
                Debug.LogError($"[UIManager] UIRoot 프리팹을 찾지 못했습니다 : {UIRootPath}. UI 입력이 동작하지 않습니다.");
                return;
            }

            GameObject instance = Object.Instantiate(origin);
            //"(Clone)" 이 붙으면 하이어라키에서 찾기 번거로워진다.
            instance.name = origin.name;
            Object.DontDestroyOnLoad(instance);

            uiRoot = instance.GetComponent<UIRoot>();
            if (null == uiRoot)
                Debug.LogError($"[UIManager] {UIRootPath} 에 UIRoot 컴포넌트가 없습니다.");
            else
                Debug.Log("[UIManager] 전역 UIRoot 생성 (EventSystem 포함)");
        }

        public void Release()
        {
            if (null != uiRoot)
                Object.Destroy(uiRoot.gameObject);

            uiRoot = null;
        }

        /// <summary>해당 레이어의 부모 Transform. UIRoot 가 없으면 null.</summary>
        public RectTransform GetLayer(EUILayer layer)
        {
            if (null == uiRoot)
            {
                Debug.LogError("[UIManager] UIRoot 가 없습니다. Initialize 가 실패했는지 확인하세요.");
                return null;
            }

            return uiRoot.GetLayer(layer);
        }

        /// <summary>프리팹을 해당 레이어 밑에 만든다. 실패하면 null.</summary>
        public GameObject CreateUI(string prefabPath, EUILayer layer)
        {
            RectTransform parent = GetLayer(layer);
            if (null == parent)
                return null;

            GameObject origin = ResourcesManager.Instance.Load<GameObject>(prefabPath);
            if (null == origin)
                return null;

            //worldPositionStays: false — 부모 RectTransform 기준으로 로컬 좌표를 유지한다.
            GameObject instance = Object.Instantiate(origin, parent, false);
            instance.name = origin.name;
            return instance;
        }
    }
}
