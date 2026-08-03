using System;
using Cysharp.Threading.Tasks;
using Script.Define;
using Script.Manager.SingletonManager;

namespace Script.GameFlow.GameScene
{
    public class MenuSceneInfo :  ISceneInfo
    {
        
    }
    
    //여기부터 디자인해보자
    /*
     * 근대 생각해보니, 여기서 들어갈 필요가 없는디..
     * 바로 Lobby로 던져주고, 메뉴는 로비 내에서 던져주자.
     */
    public class MenuScene : SceneBase
    {
        public MenuScene(ESceneType SceneType) : base(SceneType)
        {
            
        }
        public override void EnterScene(ISceneInfo context)
        {
            base.EnterScene(context);
            // UI 전체 활성화
            //active MenuPrefab
        }
        public override void ExitScene()
        {
            // UI전체 제거
            base.ExitScene();
        }
        
        public override void UpdateScene()
        {
            base.UpdateScene();
        }

        public override async UniTask OnLoadResourceAsync(IProgress<LoadingProgressResult> progress)
        {
            await base.OnLoadResourceAsync(progress);
            //필요한 테이블 로드
            await LoadTable(progress);
            //필요한 UI로드
            await LoadPrefab(progress);
            //여기서 네트워크 통신도 하면 좋을듯?
        }
        public override void OnLoadComplete()
        {
            base.OnLoadComplete();
        }
        
        public override void ReleaseResource()
        {
            base.ReleaseResource();

            UnloadTable();
            UnloadPrefab();
        }

        private async UniTask LoadTable(IProgress<LoadingProgressResult> progress)
        {
            //필요 테이블 로드
        }

        private void UnloadTable()
        {
        }

        private async UniTask LoadPrefab(IProgress<LoadingProgressResult> progress)
        {
            //필요 프리팹 로드
        }
        private void UnloadPrefab()
        {
        }
    }
}