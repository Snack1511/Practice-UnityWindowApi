using System;
using pattern;
using UnityEngine;

public class ObstacleObject : MonoBehaviour
{
    [SerializeField] private MoveController moveController = null;

    public float debugmovedist = 3;
    private void Awake()
    {
        moveController.SetupComponent();
    }

    private void OnEnable()
    {
        NotifyLocator.Notify(new OutboundRegister()
        {
            targetTransform = transform,
            BoundCheckAction = OnBoundary 
        });
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
        gameObject.SetActive(false);
    }
}
