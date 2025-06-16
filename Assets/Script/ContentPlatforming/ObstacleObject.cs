using System;
using pattern;
using UnityEngine;
using Random = System.Random;

public enum PlatformingObstacleType
{
    Triangle,
    Circle,
    Square,
}

public class ObstacleObject : MonoBehaviour
{
    [SerializeField] private MoveController moveController = null;
    [SerializeField] private SpriteController spriteController = null;

    private PlatformingObstacleType obstacleType;
    
    public float debugmovedist = 3;
    private void Awake()
    {
        moveController.SetupComponent();
        spriteController.SetupComponent();
    }

    private void OnEnable()
    {
        SetObstacle();
        NotifyLocator.Notify(new OutboundRegister()
        {
            targetTransform = transform,
            BoundCheckAction = OnBoundary 
        });
    }
    
    private void SetObstacle()
    {
        string[] spritePath =
        {
            "Sprite/Square",
            "Sprite/Triangle",
        };
        
        int targetIndex = UnityEngine.Random.Range(0, spritePath.Length);
        string targetString = spritePath[targetIndex]; 
        spriteController.LoadSprite(targetString);
    }

    private void OnDisable()
    {
        NotifyLocator.Notify(new OutboundUnregister()
        {
            targetTransform = transform,
        });
    }

    private void Update()
    {
        moveController.Move(Vector3.left, debugmovedist, Time.deltaTime);
    }

    private void OnBoundary(Camera targetCamera)
    {
        Vector3 viewpos = targetCamera.WorldToViewportPoint(transform.position);
        if(viewpos.x < 0)
            gameObject.SetActive(false);
    }
}
