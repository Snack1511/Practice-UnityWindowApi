using System;
using System.Collections.Generic;
using pattern;
using Script.GameFlow.GameScene;
using UnityEngine;

namespace Manager
{
    public class SceneManager : Singleton<SceneManager>
    {
        private Dictionary<ESceneType, SceneBase> scenes = new();
        private ESceneType currentScene;
        private ESceneType prevScene;
        public void Initialize()
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

        public void ChangeScene(ESceneType SceneType, Action SceneLoadComplete = null)
        {
            if (scenes.ContainsKey(SceneType))
            {
                if (SceneType == currentScene) return;
                
                prevScene = currentScene;
                currentScene = SceneType;
                if (scenes.TryGetValue(ESceneType.LoadingScene, out SceneBase loadSceneBase))
                {
                    LoadingScene loadScene = loadSceneBase as LoadingScene;
                    SceneBase curSceneBase = scenes[SceneType];
                    if (scenes.TryGetValue(prevScene, out SceneBase prevSceneBase))
                    {
                        loadScene.LoadScene(prevSceneBase, curSceneBase, SceneLoadComplete);
                    }
                    else
                    {
                        loadScene.LoadScene(null, curSceneBase, SceneLoadComplete);
                    }
                }
                else
                {
                    Debug.LogError($"not found loadScene");
                }
            }
            else 
                Debug.Log($"Not found scene: {SceneType}");
        }
    }
}