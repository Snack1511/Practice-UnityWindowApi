using System;
using System.Collections.Generic;
using pattern;
using Script.GameFlow.GameScene;
using UnityEngine;

namespace Script.Manager.SingletonManager
{
    public class SceneManager : Singleton<SceneManager>
    {
        private Dictionary<ESceneType, SceneBase> scenes = new();
        private SceneController  sceneController;
        public void Initialize()
        {
            RegistScenes();
            
            sceneController??= new SceneController();
            sceneController.Initilalize(new SceneControlModelContext()
            {
                scenes = scenes,
            });
        }

        public void Release()
        {
        }

        //첫 씬 로드 이후 호출
        public void OnCallFirstLoadedSceneAfter()
        {
            sceneController.ChangeSceneSingle(ESceneType.StartScene, new StartSceneInfo());
        }

        private void RegistScenes()
        {
            AddScene(ESceneType.StartScene, new StartScene(ESceneType.StartScene));
            AddScene(ESceneType.LoadingScene, new LoadingScene(ESceneType.LoadingScene));
            AddScene(ESceneType.LobbyScene, new LobbyScene(ESceneType.LobbyScene));
            AddScene(ESceneType.MenuScene, new MenuScene(ESceneType.MenuScene));
            AddScene(ESceneType.TestScene, new TestScene(ESceneType.TestScene));
        }

        private void AddScene(ESceneType sceneType, SceneBase scene)
        {
            if (!scenes.TryAdd(sceneType, scene))
                Debug.LogError($"Failed to add scene {sceneType}");
        }
        
        public void ChangeScene(ESceneType SceneType, ISceneInfo sceneInfo, Action SceneLoadComplete = null, bool isVisitLoadingScene = false)
        {
            if(null != sceneController)
                sceneController.ChangeScene(SceneType, sceneInfo, SceneLoadComplete, isVisitLoadingScene);
        }

        public void Update()
        {
            if(null != sceneController)
                sceneController.UpdateScene();
        }
    }
}