using UnityEngine;

public class PlaceToCamera : MonoBehaviour
{
    public Camera targetCamera;
    public float originalObjectHeight = 1f; // 오브젝트의 기본 높이

    void Awake()
    {
        PlaceBelowViewport();
    }

    void PlaceBelowViewport()
    {
        if (!targetCamera.orthographic)
        {
            Debug.LogError("Orthographic 카메라에서만 지원합니다.");
            return;
        }

        float viewportHeight = targetCamera.orthographicSize * 2f;

        // 화면 아래의 World 좌표를 얻습니다.
        Vector3 bottomViewportPoint = targetCamera.ViewportToWorldPoint(
            new Vector3(0.5f, 0f, Mathf.Abs(targetCamera.transform.position.z - transform.position.z))
        );

        Vector3 position = transform.position;

        // 오브젝트의 높이 절반만큼 아래로 내려서, 뷰포트 하단에 딱 맞춰 붙입니다.
        position.y = bottomViewportPoint.y + (originalObjectHeight * transform.localScale.y * 0.5f);

        transform.position = position;
    }
}
