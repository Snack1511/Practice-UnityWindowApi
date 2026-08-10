using System;
using Cysharp.Threading.Tasks;
using Script.Define;
using Script.GameFlow.Command;
using UnityEngine;

namespace Script.GameFlow.GameScene
{
    public enum ESceneType
    {
        None,
        StartScene,
        LoadingScene,
        LobbyScene,
        MenuScene,
        TestScene,
    }


    public interface ISceneInfo
    {
    }

    public abstract class SceneBase
    {
        public ESceneType SceneType { get; protected set; }
        protected SceneBase(ESceneType SceneType) { this.SceneType = SceneType; }
        private bool enterScene = false;

        private ISceneInfo _sceneInfo = null;
        
        public virtual void EnterScene(ISceneInfo context)
        {
            enterScene = true;
            _sceneInfo = context;
        }

        public virtual void ExitScene()
        {
            _sceneInfo = null;
            enterScene = false;
            
            ReleaseResource();
            Resources.UnloadUnusedAssets();
        }
        public virtual void UpdateScene() { }

        public virtual async UniTask OnLoadResourceAsync(IProgress<LoadingProgressResult> progress) { }

        /// <summary>
        /// 이 씬이 시작될 때 필요한 오브젝트의 생성 명령을 큐에 넣는다.
        /// 로딩 씬이 활성 씬을 이 씬으로 바꾼 직후 실행하므로, 여기서 만든 오브젝트는
        /// 로딩 씬·이전 씬이 언로드되어도 살아남는다.
        /// OnLoadResourceAsync 단계에서 만들면 아직 활성 씬이 아니라 함께 파기된다.
        /// </summary>
        public virtual void OnEnqueueStartCommands(CmdExecutor executor) { }

        public virtual void OnLoadComplete() { }
        
        public virtual void ReleaseResource()
        {
            
        }

        public virtual bool IsActiveScene()
        {
            return enterScene;
        }
    }
}