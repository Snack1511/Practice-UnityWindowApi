using System;
using TMPro;
using UnityEngine;

public class GameProcess : MonoBehaviour
{
    public TextMeshProUGUI debugText;
    protected void Awake()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        string str = WindowNativeManager.SetWindowFrame(1280, 720);
        debugText.SetText(str);
#endif
    }

    public void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space)) 
//        {
//#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
//            bool isBlockClick = WindowNativeManager.IsTransparentClick;
//            WindowNativeManager.SetTransparentClick(!isBlockClick);

//            string str = $"IsTransparentClick : {isBlockClick}";
//            debugText.SetText(str);
//#endif
//        }
    }

}
