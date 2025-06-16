using UnityEngine;

public static class MainProcess
{
    //프로그램 진입점
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    public static void OnLoadyBeforeSplashScreen()
    {
        //리소스 프리 로드
        
        //해상도 설정
        Manager.ResolutionManager.Initialize();
        
        //씬 전환 -> StartScene
        
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

