using Script.GameFlow.GameScene;
using UnityEngine;

public static class MainProcess
{
    //프로그램 진입점
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    public static void OnLoadyBeforeSplashScreen()
    {
        
        //프로그램 흐름 등록
        Application.quitting += OnApplicationQuit;
        Application.focusChanged += OnApplicationChangedFocus;
        
        //리소스 프리 로드
        
        
        //해상도 설정
        
        //스태틱 클래스 매니저 초기화
        Manager.GameProcessManager.Initialize();
        Manager.ResolutionManager.Initialize();
        
        //싱글톤 클래스 매니저 초기화
        Manager.SceneManager.Instance.Initialize();
        Manager.ResourcesManager.Instance.Initialize();
        Manager.TableManager.Instance.Initialize();
        
        //모노 싱글톤 클래스 매니저 초기화
        Manager.ContentManager.Instance.Initialize();
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void OnLoadAfterSceneLoad()
    {
        Manager.SceneManager.Instance.OnCallFirstLoadedSceneAfter();
    }

    //프로그램 종료점
    public static void OnApplicationQuit()
    {

    }

    public static void OnApplicationChangedFocus(bool isFocus)
    {

    }
}

