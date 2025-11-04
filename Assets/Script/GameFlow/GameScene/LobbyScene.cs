using System;
using Cysharp.Threading.Tasks;
using Script.Define;

namespace Script.GameFlow.GameScene
{
    public class LobbyScene : SceneBase
    {
        public LobbyScene(ESceneType SceneType) : base(SceneType)
        {
            
        }
        public override void EnterScene(ISceneInfo context)
        {
            base.EnterScene(context);
        }
        public override void ExitScene()
        {
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