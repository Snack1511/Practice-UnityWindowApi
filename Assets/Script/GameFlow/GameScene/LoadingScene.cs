using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Script.Define;
using Script.GameContent;
using UnityEngine;

namespace Script.GameFlow.GameScene
{
    public class LoadingScene : SceneBase
    {
        private static UILoading uiLoading;
        
        private CancellationTokenSource loadTokenSource;
        
        public LoadingScene(ESceneType SceneType) : base(SceneType)
        {
            loadTokenSource = new CancellationTokenSource();
        }

        ~LoadingScene()
        {
            loadTokenSource.Cancel();
            loadTokenSource.Dispose();
            loadTokenSource = null;
        }

        private async UniTask LoadingProcess(SceneBase prevScene, SceneBase sceneBase, Action callback, CancellationToken token)
        {
            //로딩씬 활성화
            await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(SceneType.ToString());

            ResourceRequest request = Resources.LoadAsync<UILoading>("Prefabs/UILoading");
            await UniTask.WaitUntil(()=>request.isDone, cancellationToken:token);
            
            UILoading origin = request.asset as UILoading;
            uiLoading = GameObject.Instantiate(origin);
                
            //리소스 로딩 동안, 로딩UI노출
            uiLoading.ActiveLoadingUI();
            
            if(null != prevScene)
                prevScene.Exit();
            
            await sceneBase.OnLoadResource(new Progress<LoadingProgressResult>((result) =>
            {
                uiLoading.SetLoadingProgress(result);
            }));
            
            await sceneBase.OnLoadComplete();
            //불러올 씬 활성화
            await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneBase.SceneType.ToString());

            callback?.Invoke();
            sceneBase.Enter();
        }

        protected override void EnterScene()
        {
            base.EnterScene();
        }

        public void LoadScene(SceneBase prevScene, SceneBase currentScene, Action SceneLoadComplete)
        {
            //로딩 씬 활성화 및 로더 등록
            LoadingProcess(prevScene, currentScene,SceneLoadComplete, loadTokenSource.Token).Forget();
        }
    }
}