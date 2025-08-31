using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using pattern;
using Script.GameFlow.GameScene;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Manager
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
            Scene curScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (null != curScene)
            {
                ESceneType sceneType = Enum.Parse<ESceneType>(curScene.name);
                if (sceneType == ESceneType.StartScene)
                {
                    sceneController.ChangeSceneWithOutLoadSceneObject(sceneType,  new StartSceneInfoContext());
                }
                else
                {
                    //씬 전환 -> StartScene
                    Manager.SceneManager.Instance.ChangeScene(ESceneType.StartScene,
                        new StartSceneInfoContext(), null, true);
                }

            }
        }

        private void RegistScenes()
        {
            AddScene(ESceneType.StartScene, new StartScene(ESceneType.StartScene));
            AddScene(ESceneType.LoadingScene, new LoadingScene(ESceneType.LoadingScene));
            AddScene(ESceneType.TestScene, new TestScene(ESceneType.TestScene));
        }

        private void AddScene(ESceneType sceneType, SceneBase scene)
        {
            if (!scenes.TryAdd(sceneType, scene))
                Debug.LogError($"Failed to add scene {sceneType}");
        }
        
        public void ChangeScene(ESceneType SceneType, ISceneInfoContext sceneInfoContext, Action SceneLoadComplete = null, bool isVisitLoadingScene = false)
        {
            if(null != sceneController)
                sceneController.ChangeScene(SceneType, sceneInfoContext, SceneLoadComplete, isVisitLoadingScene);
        }
    }
}