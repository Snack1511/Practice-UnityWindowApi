using JetBrains.Annotations;
using UnityEngine;

public class LoopEdgeController : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    private int screenWidth;
    private int screenHeight;
    private void Awake()
    {
        //screenWidth = Screen.width;
        //screenHeight = Screen.height;
        //Display.main.
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 viewPos = ConvertViewPosition();

        viewPos.x = Mathf.Repeat(viewPos.x, 1.0f);
        viewPos.y = Mathf.Repeat(viewPos.y, 1.0f);
        SetViewPosToWorld(viewPos);

    }

    public Vector3 ConvertViewPosition() 
    {
        Vector3 worldPos = transform.position;
        Vector3 viewPos = targetCamera.WorldToViewportPoint(worldPos);
        return viewPos;
        //return ViewPos.x > 0 && ViewPos.x <= 1 && ViewPos.y > 0 && ViewPos.y <= 1;
    }

    public void SetViewPosToWorld(Vector3 viewPos) 
    {
        Vector3 newWorldPos = targetCamera.ViewportToWorldPoint(viewPos);
        transform.position = newWorldPos;
    }
}
