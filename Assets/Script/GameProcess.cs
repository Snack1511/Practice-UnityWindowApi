using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameProcess : MonoBehaviour
{
    public TextMeshProUGUI debugText;
    //public TextMeshProUGUI frameText;

    private float deltaTime = 0f;
    private int size = 25;
    private Color color = Color.red;

    private bool isOnDebugObject = false;
    public List<GameObject> debugObjects = new List<GameObject>();
    protected void Awake()
    {
        int rWidth = Display.main.systemWidth;
        int rHeight = Display.main.systemHeight - 49;

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        string str = WindowNativeManager.SetWindowFrame(0, 0, rWidth , rHeight);
        debugText.SetText(str);
#endif
    }

    public void Update()
    {
        deltaTime = (deltaTime >= 1.0f) ? 
            deltaTime - 1.0f : deltaTime;
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

#if DEBUG
        if (Input.GetKeyDown(KeyCode.Tab)) 
        {
            isOnDebugObject = !isOnDebugObject;
            debugObjects.ForEach(x => x.gameObject.SetActive(isOnDebugObject));
        }
#endif
    }
#if DEBUG
    public void OnGUI()
    {
        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(30, 30, Screen.width, Screen.height);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = size;
        style.normal.textColor = color;

        float ms = deltaTime * 1000f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.} FPS ({1:0.0} ms)", fps, ms);

        GUI.Label(rect, text, style);
    }
#endif
}
