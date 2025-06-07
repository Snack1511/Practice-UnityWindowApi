using System;
using pattern;
using UnityEngine;

public class ObstacleObject : MonoBehaviour
{
    [SerializeField] private MoveController moveController = null;
    [SerializeField] private SpriteController spriteController = null;

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
        string spritePath = "Sprite/Square";
        spriteController.LoadSprite(spritePath);
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
