using UnityEngine;

public class PlaceToCamera : MonoBehaviour
{
    public Camera targetCamera;
    public float originalObjectHeight = 1f; // 오브젝트의 기본 높이
    public float originalObjectWidth = 1f; // 오브젝트의 기본 높이

    public void PlaceViewport(Vector2 viewportPosition, Vector2 moveDir)
    {
        if (!targetCamera.orthographic)
        {
            Debug.LogError("Orthographic 카메라에서만 지원합니다.");
            return;
        }
        
        // 화면 아래의 World 좌표를 얻습니다.
        Vector3 viewportToWorldPoint = targetCamera.ViewportToWorldPoint(
            new Vector3(viewportPosition.x, viewportPosition.y, Mathf.Abs(targetCamera.transform.position.z - transform.position.z))
        );

        Vector3 position = transform.position;

        // 오브젝트의 높이 절반만큼 아래로 내려서, 뷰포트 하단에 딱 맞춰 붙입니다.
        if(!float.Epsilon.Equals(moveDir.x))
            position.x = viewportToWorldPoint.x + ((originalObjectWidth * transform.localScale.x * 0.5f)*moveDir.x);
        if(!float.Epsilon.Equals(moveDir.y))
            position.y = viewportToWorldPoint.y + ((originalObjectHeight * transform.localScale.y * 0.5f)*moveDir.y);

        transform.position = position;
    }
    
}
