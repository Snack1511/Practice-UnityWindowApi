using System;
using UnityEngine;
using Framework.Extension.Component;

public class Ground : MonoBehaviour
{
    private FitToCamera fitToCamera;
    private PlaceToCamera placeToCamera;

    private void Awake()
    {
        fitToCamera = GetComponent<FitToCamera>();
        placeToCamera = GetComponent<PlaceToCamera>();
        
        Debug.Assert(fitToCamera is not null, $"not found {typeof(FitToCamera)}");
        Debug.Assert(placeToCamera is not null, $"not found {typeof(PlaceToCamera)}");
    }

    private void Start()
    {
        SetGroundByCamera();
    }

    private void SetGroundByCamera()
    {
        fitToCamera.FitWidthToViewport();
        placeToCamera.PlaceViewport(new Vector2(0.5f, 0.0f), Vector2.down);
    }
}
