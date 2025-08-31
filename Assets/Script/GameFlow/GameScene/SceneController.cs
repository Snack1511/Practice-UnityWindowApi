using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Script.GameFlow.GameScene
{
    public class SceneControlModelContext
    {
        public Dictionary<ESceneType, SceneBase> scenes;
    }

    public class SceneController
    {
        public ESceneType CurSceneType => currentScene;
        private ESceneType currentScene;
        private ESceneType nextScene;
        private Dictionary<ESceneType, SceneBase> scenes;
        public void Initilalize(SceneControlModelContext context)
        {
            if(null == context)
                Debug.LogError("SceneController.Initialize::need context");
            scenes = context.scenes;
        }

        public void Release()
        {
            scenes = null;
        }

        public void ChangeScene(ESceneType SceneType, ISceneInfoContext sceneInfoContext = null, Action SceneLoadComplete = null, bool isVisitLoadingScene = false)
        {
            if (currentScene == SceneType) return;
            
            ESceneType prevSceneType = currentScene;
            nextScene = SceneType;
            
            //로딩씬을 거칠 경우 로딩씬을 Additive모드로 액티브
            if (isVisitLoadingScene)
            {
                //로딩할 씬의 정보 등록
                LoadingSceneInfoContext loadingSceneInfoContext = new LoadingSceneInfoContext()
                {
                    targetSceneInfoContext = sceneInfoContext,
                    loadingTargetScene = scenes.GetValueOrDefault(nextScene),
                    LoadComplete = () =>
                    {
                        UnloadScene(ESceneType.LoadingScene);
                        if (scenes.TryGetValue(SceneType, out SceneBase targetSceneInstance))
                        {                        
                            //이전씬 비활성화
                            if (scenes.TryGetValue(prevSceneType, out SceneBase prevSceneInstance))
                            {
                                UnloadScene(prevSceneType);
                            }
                            targetSceneInstance.EnterScene(sceneInfoContext);
                            SceneLoadComplete?.Invoke();
                        }
                    }
                };
                
                //로딩씬 활성화
                LoadScene(ESceneType.LoadingScene, loadingSceneInfoContext);
            }
            else
            {
                if (scenes.TryGetValue(nextScene, out SceneBase nextSceneInstance))
                {
                    //nextSceneInstance.EnterScene(sceneInfoContext);
                    LoadScene(nextScene, sceneInfoContext, () =>
                    {
                        //이전씬 비활성화
                        if (scenes.TryGetValue(prevSceneType, out SceneBase prevSceneInstance))
                        {
                            UnloadScene(prevSceneType);
                        }
                        SceneLoadComplete?.Invoke();
                    });
                }
            }
            
            UpdateCurrentSceneType();
        }

        private void UpdateCurrentSceneType()
        {
            if (ESceneType.None != nextScene)
            {
                currentScene = nextScene;
                nextScene = ESceneType.None;
            }
        }

        public void LoadScene(ESceneType SceneType, ISceneInfoContext sceneInfoContext = null, Action SceneLoadComplete = null)
        {
            if (scenes.TryGetValue(SceneType, out SceneBase targetSceneInstance))
            {
                var asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(SceneType.ToString(), LoadSceneMode.Additive);
                if (null != asyncOperation)
                {
                    asyncOperation.completed += (operation) =>
                    {
                        targetSceneInstance.EnterScene(sceneInfoContext);
                    };
                }
            }
        }

        public void UnloadScene(ESceneType SceneType)
        {
            if (scenes.TryGetValue(SceneType, out SceneBase targetSceneInstance))
            {
                var asyncOperation = SceneManager.UnloadSceneAsync(SceneType.ToString());
                if (null != asyncOperation)
                {
                    asyncOperation.completed += (operation) =>
                    {
                        targetSceneInstance.ExitScene();
                    };
                }
            }
        }

        public void UpdateScene()
        {
            foreach (var sceneBase in scenes)
            {
                if(scenes[currentScene].IsActiveScene())
                    scenes[currentScene].UpdateScene();
            }
        }

        public void ChangeSceneWithOutLoadSceneObject(ESceneType sceneType, ISceneInfoContext sceneInfoContext = null)
        {
            if (scenes.TryGetValue(sceneType, out SceneBase targetSceneInstance))
            {
                currentScene = sceneType;
                targetSceneInstance.EnterScene(sceneInfoContext);
            }
        }
    }
}