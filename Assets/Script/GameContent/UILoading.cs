using Script.Define;
using UnityEngine;
using UnityEngine.UI;

namespace Script.GameContent
{
    public class UILoading : MonoBehaviour
    {
        [SerializeField] private Slider uiLoadingSlider; 

        public void ActiveLoadingUI()
        {
            gameObject.SetActive(true);
        }

        public void SetLoadingProgress(LoadingProgressResult progressInfo)
        {
            if(null != progressInfo)
                uiLoadingSlider.SetValueWithoutNotify(progressInfo.amount);
        }
    }
}