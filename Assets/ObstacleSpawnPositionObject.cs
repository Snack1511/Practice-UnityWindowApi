using System;
using UnityEngine;

public class ObstacleSpawnPositionObject : MonoBehaviour
{        
    private PlaceToCamera placeToCamera;
    private void Awake()
    {
        placeToCamera = GetComponent<PlaceToCamera>();
        
        Debug.Assert(placeToCamera is not null, $"not found {typeof(PlaceToCamera)}");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        SetGroundByCamera();
    }

    private void SetGroundByCamera()
    {
        placeToCamera.PlaceViewport(new Vector2(1.0f, 0.0f), Vector2.right);
    }

}
