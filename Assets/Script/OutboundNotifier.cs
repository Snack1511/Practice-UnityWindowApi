using System;
using JetBrains.Annotations;
using UnityEngine;



public class OutboundNotifier : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

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

    }

    public Vector3 ConvertViewPosition() 
    {
        Vector3 worldPos = transform.position;
        Vector3 viewPos = targetCamera.WorldToViewportPoint(worldPos);
        return viewPos;
    }
}
