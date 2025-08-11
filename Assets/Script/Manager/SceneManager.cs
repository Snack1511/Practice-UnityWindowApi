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

        public void ChangeScene(ESceneType SceneType)
        {
            if (scenes.ContainsKey(SceneType))
            {
                if (prevScene == currentScene) return;
                
                prevScene = currentScene;
                currentScene = SceneType;
                
                //로딩 씬 활성화 및 로더 등록
                
                //리소스 로딩 동안, 로딩UI노출
                
                //다음 씬의 리소스 로딩 후 씬 전환
                
                //이전 씬의 리소스 정리
                if(prevScene != ESceneType.None)
                    scenes[prevScene].Exit();
                
                //현재 씬의 리소스 캐싱
                if(currentScene != ESceneType.None)
                    scenes[currentScene].Enter();
            }
            else Debug.Log($"Not found scene: {SceneType}");
        }
    }
}