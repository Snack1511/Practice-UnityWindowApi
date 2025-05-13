using UnityEngine;

public class FitToCamera : MonoBehaviour
{
    public Camera targetCamera;
    public float originalObjectWidth = 1f; // 기본 스케일(1,1,1)일 때 오브젝트의 실제 너비값

    void Awake()
    {
        FitWidthToViewport();
    }

    void FitWidthToViewport()
    {
        if (!targetCamera.orthographic)
        {
            Debug.LogError("이 스크립트는 Orthographic 카메라에서만 정확히 동작합니다.");
            return;
        }

        float viewportWidth = targetCamera.orthographicSize * 2f * targetCamera.aspect;

        Vector3 scale = transform.localScale;
        scale.x = viewportWidth / originalObjectWidth;

        transform.localScale = scale;
    }
}