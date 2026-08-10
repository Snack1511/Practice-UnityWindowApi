using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Framework.Extension.Collection;
using Script.Define;
using Script.GameContent;
using Script.GameFlow.Command;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Script.GameFlow.GameScene
{
    public class LoadingSceneInfo : ISceneInfo
    {
        public ISceneInfo TargetSceneInfo;
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

            //활성 씬이 바뀐 뒤에 실행해야 생성된 오브젝트가 대상 씬에 들어간다.
            //이 위에서 만들면 로딩 씬·이전 씬이 언로드될 때 함께 파기된다.
            await ExecuteStartCommands(sceneInstance);

            sceneInstance.OnLoadComplete();
            loadComplete?.Invoke();
        }

        /// <summary>
        /// 다음 씬으로부터 생성 명령 큐를 받아 순차 실행한다.
        /// 명령이 없으면 큐도 비어 있으므로 바로 반환한다.
        /// </summary>
        private async UniTask ExecuteStartCommands(SceneBase sceneInstance)
        {
            CmdExecutor executor = new CmdExecutor();
            sceneInstance.OnEnqueueStartCommands(executor);

            if (executor.Count == 0)
                return;

            //ExecuteCommand 는 동기 반환이므로 완료원을 await 해서 로딩 진행을 붙잡는다.
            await executor.ExecuteCommand().Task;
        }

        public override void EnterScene(ISceneInfo sceneInfo)
        {
            base.EnterScene(sceneInfo);

            LoadingSceneInfo info = sceneInfo as LoadingSceneInfo;
            //로딩 씬 활성화 및 로더 등록
            LoadingProcess(info.loadingTargetScene, info.LoadComplete, loadTokenSource.Token).Forget();
        }
    }
}