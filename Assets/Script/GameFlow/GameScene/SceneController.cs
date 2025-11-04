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

        public void ChangeScene(ESceneType SceneType, ISceneInfo sceneInfo = null, Action SceneLoadComplete = null, bool isVisitLoadingScene = false)
        {
            if (currentScene == SceneType) return;
            
            ESceneType prevSceneType = currentScene;
            nextScene = SceneType;
            
            //로딩씬을 거칠 경우 로딩씬을 Additive모드로 액티브
            if (isVisitLoadingScene)
            {
                //로딩할 씬의 정보 등록
                LoadingSceneInfo loadingSceneInfo = new LoadingSceneInfo()
                {
                    TargetSceneInfo = sceneInfo,
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
                            targetSceneInstance.EnterScene(sceneInfo);
                            SceneLoadComplete?.Invoke();
                        }
                    }
                };
                
                //로딩씬 활성화
                LoadScene(ESceneType.LoadingScene, loadingSceneInfo);
            }
            else
            {
                if (scenes.TryGetValue(nextScene, out SceneBase nextSceneInstance))
                {
                    //nextSceneInstance.EnterScene(sceneInfoContext);
                    LoadScene(nextScene, sceneInfo, () =>
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

        public void ChangeSceneSingle(ESceneType SceneType, ISceneInfo sceneInfo = null,
            Action SceneLoadComplete = null)
        {
            if (currentScene == SceneType) return;
            
            if (scenes.TryGetValue(SceneType, out SceneBase targetSceneInstance))
            {                        
                currentScene = SceneType;
                SceneManager.LoadScene(currentScene.ToString(), LoadSceneMode.Single);
                targetSceneInstance.EnterScene(sceneInfo);
                SceneLoadComplete?.Invoke();
                UpdateCurrentSceneType();
            }
            else
            {
                currentScene = ESceneType.None;
            }

        }

        private void UpdateCurrentSceneType()
        {
            if (ESceneType.None != nextScene)
            {
                SetCurrentScene(nextScene);
                nextScene = ESceneType.None;
            }
        }

        public void LoadScene(ESceneType SceneType, ISceneInfo sceneInfo = null, Action SceneLoadComplete = null)
        {
            if (scenes.TryGetValue(SceneType, out SceneBase targetSceneInstance))
            {
                var asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(SceneType.ToString(), LoadSceneMode.Additive);
                if (null != asyncOperation)
                {
                    asyncOperation.completed += (operation) =>
                    {
                        targetSceneInstance.EnterScene(sceneInfo);
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
            if(scenes.ContainsKey(currentScene))
                scenes[currentScene].UpdateScene();
        }

        public void ChangeSceneWithOutLoadSceneObject(ESceneType sceneType, ISceneInfo sceneInfo = null)
        {
            if (scenes.TryGetValue(sceneType, out SceneBase targetSceneInstance))
            {
                SetCurrentScene(sceneType);
                targetSceneInstance.EnterScene(sceneInfo);
            }
        }

        public void SetCurrentScene(ESceneType SceneType)
        {
            currentScene = SceneType;
        }
    }
}