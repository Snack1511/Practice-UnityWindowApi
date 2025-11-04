using System;
using Cysharp.Threading.Tasks;
using Script.Define;

namespace Script.GameFlow.GameScene
{
    public class MenuSceneInfo :  ISceneInfo
    {
        
    }
    
    //여기부터 디자인해보자
    public class MenuScene : SceneBase
    {
        public MenuScene(ESceneType SceneType) : base(SceneType)
        {
            
        }
        public override void EnterScene(ISceneInfo context)
        {
            base.EnterScene(context);
            // UI 전체 활성화
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
            
        }
        public override void OnLoadComplete()
        {
            base.OnLoadComplete();
        }
        
        public override void ReleaseResource()
        {
            base.ReleaseResource();
        }

    }
}