using UnityEditor;
using UnityEngine;

public class MainProcess
{
    //프로그램 진입점
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    public static void OnLoadyBeforeSplashScreen()
    {
        Manager.ResolutionManager.Initialize();
        
        //프로그램 흐름 등록
        Application.quitting += OnApplicationQuit;
        Application.focusChanged += OnApplicationChangedFocus;
    }

    //프로그램 종료점
    public static void OnApplicationQuit()
    {

    }

    public static void OnApplicationChangedFocus(bool isFocus)
    {

    }
}

