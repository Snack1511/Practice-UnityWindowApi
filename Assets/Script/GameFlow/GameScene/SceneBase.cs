using System;
using Cysharp.Threading.Tasks;
using Script.Define;
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