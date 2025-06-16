using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Script
{
    public class DebuggingComponent : MonoBehaviour
    {
        private const float PerMilli = 1000f;
        
        public List<GameObject> debugObjects = new List<GameObject>();
        public TextMeshProUGUI debugText;
        //public TextMeshProUGUI frameText;

        private float unscaleddeltaTime = 0f;
        private int size = 25;
        private Color color = Color.red;
        private bool isOnDebugObject = false;

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        private void Awake()
        {
            string str = WindowNativeManager.GetWindowName();
            debugText.SetText(str);
        }
#endif



#if DEBUG
        private void DebuggingFrameCounter()
        {
            unscaleddeltaTime = (unscaleddeltaTime >= 1.0f) ? 
                unscaleddeltaTime - 1.0f : unscaleddeltaTime;
            unscaleddeltaTime += (Time.unscaledDeltaTime - unscaleddeltaTime) * 0.1f;
        }
        
        public void Update()
        {
            DebuggingFrameCounter();
            
            if (Input.GetKeyDown(KeyCode.Tab)) 
            {
                isOnDebugObject = !isOnDebugObject;
                debugObjects.ForEach(x => x.gameObject.SetActive(isOnDebugObject));
            }
        }

        public void OnGUI()
        {
            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(30, 30, Screen.width, Screen.height);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = size;
            style.normal.textColor = color;

            float ms = unscaleddeltaTime * PerMilli;
            float fps = 1.0f / unscaleddeltaTime;
            string text = string.Format("{0:0.} FPS ({1:0.0} ms)", fps, ms);

            GUI.Label(rect, text, style);
        }
#endif
    }
}