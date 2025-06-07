using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ContentPlatform : MonoBehaviour
{
    //[SerializeField] private Camera targetCamera;
    //
    //private UniversalAdditionalCameraData baseAdditionalCameraData;
    private void Awake()
    {
        //RegistAddistionalCamera();
    }
    
    private void OnDestroy()
    {
        //RegistAddistionalCamera();
    }

    // private void RegistAddistionalCamera(Camera camera)
    // {
    //     baseAdditionalCameraData ??= Camera.main.GetComponent<UniversalAdditionalCameraData>();
    //     baseAdditionalCameraData.cameraStack.Add(camera);        
    // }
    //
    // private void UnregistAddistionalCamera(Camera camera)
    // {
    //     baseAdditionalCameraData ??= Camera.main.GetComponent<UniversalAdditionalCameraData>();
    //     baseAdditionalCameraData.cameraStack.Add(camera);        
    // }


}
