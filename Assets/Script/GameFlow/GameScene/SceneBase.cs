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
        GameScene,
        TestScene,
    }


    public interface ISceneInfoContext
    {
    }

    public abstract class SceneBase
    {
        public ESceneType SceneType { get; protected set; }
        protected SceneBase(ESceneType SceneType) { this.SceneType = SceneType; }
        private bool enterScene = false;

        private ISceneInfoContext sceneInfoContext = null;
        
        public virtual void EnterScene(ISceneInfoContext context)
        {
            enterScene = true;
            sceneInfoContext = context;
        }

        public virtual void ExitScene()
        {
            sceneInfoContext = null;
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