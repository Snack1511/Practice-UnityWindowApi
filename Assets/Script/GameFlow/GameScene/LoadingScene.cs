using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Framework.Extension.Collection;
using Script.Define;
using Script.GameContent;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Script.GameFlow.GameScene
{
    public class LoadingSceneInfoContext : ISceneInfoContext
    {
        public ISceneInfoContext targetSceneInfoContext;
        public SceneBase loadingTargetScene;
        public Action LoadComplete;
    }

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

        private async UniTask LoadingProcess(SceneBase sceneInstance, Action loadComplete, CancellationToken token)
        {
            //Unload되는타이밍에 다시 치고 들어오나..?
            Scene LoadingSceneObject = SceneManager.GetSceneByName(SceneType.ToString());
            
            //리소스 로딩 동안, 로딩UI노출
            ResourceRequest request = Resources.LoadAsync<UILoading>("Prefabs/UILoading");
            await UniTask.WaitUntil(()=>request.isDone, cancellationToken:token);

            GameObject[] roots = LoadingSceneObject.GetRootGameObjects();
            UILoading origin = request.asset as UILoading;
            uiLoading = GameObject.Instantiate(origin);
            if(!roots.IsNullOrEmpty())
                uiLoading.transform.SetParent(roots[0].transform);
            uiLoading.ActiveLoadingUI();
            
            //불러올 씬 활성화
            await SceneManager.LoadSceneAsync(sceneInstance.SceneType.ToString(), LoadSceneMode.Additive);
            await sceneInstance.OnLoadResourceAsync(new Progress<LoadingProgressResult>((result) =>
            {
                uiLoading.SetLoadingProgress(result);
            }));
            
            Scene scene = SceneManager.GetSceneByName(sceneInstance.SceneType.ToString());
            SceneManager.SetActiveScene(scene);
            sceneInstance.OnLoadComplete();
            loadComplete?.Invoke();
        }

        public override void EnterScene(ISceneInfoContext sceneInfoContext)
        {
            base.EnterScene(sceneInfoContext);
            
            LoadingSceneInfoContext info = sceneInfoContext as LoadingSceneInfoContext;
            //로딩 씬 활성화 및 로더 등록
            LoadingProcess(info.loadingTargetScene, info.LoadComplete, loadTokenSource.Token).Forget();
        }
    }
}