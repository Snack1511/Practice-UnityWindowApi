using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Script.GameFlow.GameScene
{
    public class LoadingScene : SceneBase
    {
        public LoadingScene(ESceneType SceneType) : base(SceneType)
        {
        }

        private async UniTask LoadingProcess(SceneBase prevScene, SceneBase sceneBase, Action callback)
        {
            if(null != prevScene)
                prevScene.Exit();
            
            await sceneBase.OnLoadResource();
            await sceneBase.OnLoadComplete();
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneBase.SceneType.ToString());
            Resources.UnloadUnusedAssets();
            callback?.Invoke();
            sceneBase.Enter();
        }

        public void LoadScene(SceneBase prevScene, SceneBase currentScene, Action SceneLoadComplete)
        {
            //로딩 씬 활성화 및 로더 등록
            LoadingProcess(prevScene, currentScene,SceneLoadComplete).Forget();
            
            //리소스 로딩 동안, 로딩UI노출
        }
    }
}