using UnityEngine;

namespace Script.GameContent.UI
{
    /// <summary>UI 를 붙일 레이어. 값이 클수록 위에 그려진다.</summary>
    public enum EUILayer
    {
        /// <summary>HUD·로비 패널 등 기본 화면.</summary>
        Window,

        /// <summary>다이얼로그·확인창.</summary>
        Popup,

        /// <summary>UI 연출·파티클. 팝업까지 덮는다.</summary>
        CanvasEffect,

        /// <summary>로딩·토스트·시스템 메시지. 항상 최상단.</summary>
        Overlay,
    }

    /// <summary>
    /// 전역 UI 컨테이너. <see cref="Script.Manager.SingletonManager.UIManager"/> 가 부팅 시 프리팹을 띄우고
    /// DontDestroyOnLoad 로 앱 수명 동안 유지한다.
    ///
    /// 레이어 캔버스와 EventSystem 을 자식으로 들고 있다.
    /// EventSystem 을 씬에 두면 Additive 로드 때 중복되어 UI 입력이 통째로 죽으므로 여기 하나만 존재한다 (advise/002-8).
    /// </summary>
    public class UIRoot : MonoBehaviour
    {
        [SerializeField] private RectTransform window;
        [SerializeField] private RectTransform popup;
        [SerializeField] private RectTransform canvasEffect;
        [SerializeField] private RectTransform overlay;

        /// <summary>해당 레이어의 부모 Transform. 없으면 null.</summary>
        public RectTransform GetLayer(EUILayer layer)
        {
            switch (layer)
            {
                case EUILayer.Window: return window;
                case EUILayer.Popup: return popup;
                case EUILayer.CanvasEffect: return canvasEffect;
                case EUILayer.Overlay: return overlay;
                default: return null;
            }
        }
    }
}
