using UnityEngine;

namespace Script.GameFlow
{
    public static class MainProcess
    {
        //프로그램 진입점
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        public static void OnCallBeforeSplashScreen()
        {
            Debug.Log("OnLoadyBeforeSplashScreen");
            //프로그램 흐름 등록
            Application.quitting += OnApplicationQuit;
            Application.focusChanged += OnApplicationChangedFocus;

            //스태틱 클래스 매니저 초기화
            Manager.StaticManager.GameProcessManager.Initialize();
            WindowNative.ResolutionManager.Initialize();
        
            //싱글톤 클래스 매니저 초기화
            Manager.SingletonManager.ResourcesManager.Instance.Initialize();
            Manager.SingletonManager.TableManager.Instance.Initialize();
            Manager.SingletonManager.SceneManager.Instance.Initialize();
            Manager.SingletonManager.IOManager.Instance.Initialize();
            //UIRoot 를 띄운다. EventSystem 이 여기 들어 있으므로 첫 씬보다 먼저 만들어져야 한다.
            Manager.SingletonManager.UIManager.Instance.Initialize();
      
            //모노 싱글톤 클래스 매니저 초기화
            Manager.MonoSingleManager.ContentManager.Instance.Initialize();
      
            //리소스 프리 로드 
            
            //해상도 설정
            
            //업데이트 매니저 등록
            Manager.StaticManager.GameProcessManager.AddUpdate("SceneManager", Manager.SingletonManager.SceneManager.Instance.Update);

        }
    
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void OnCallAfterSplashScreen()
        {
            Manager.SingletonManager.SceneManager.Instance.OnCallFirstLoadedSceneAfter();
        }

        //프로그램 종료점
        public static void OnApplicationQuit()
        {
            //모노 싱글톤 클래스 매니저 초기화
            Manager.MonoSingleManager.ContentManager.Instance.Release();
            
            //싱글톤 클래스 매니저 초기화
            Manager.SingletonManager.UIManager.Instance.Release();
            Manager.SingletonManager.IOManager.Instance.Release();
            Manager.SingletonManager.SceneManager.Instance.Release();
            Manager.SingletonManager.TableManager.Instance.Release();
            Manager.SingletonManager.ResourcesManager.Instance.Release();
            
            //스태틱 클래스 매니저 초기화
            WindowNative.ResolutionManager.Release();
            Manager.StaticManager.GameProcessManager.Release();
            
            Application.focusChanged -= OnApplicationChangedFocus;
            Application.quitting -= OnApplicationQuit;
        }

        public static void OnApplicationChangedFocus(bool isFocus)
        {
            //작업 표시줄이 옮겨졌거나 자동 숨김이 켜졌으면 여기서 창 크기를 다시 맞춘다.
            WindowNative.ResolutionManager.OnApplicationFocusChanged(isFocus);
        }
    }
}

