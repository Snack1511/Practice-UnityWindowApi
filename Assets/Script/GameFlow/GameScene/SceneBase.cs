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

    public interface IState
    {
        void Enter();
        void Exit();
        void Update();
    }

    public abstract class SceneBase : IState
    {
        public ESceneType SceneType { get; protected set; }
        protected SceneBase(ESceneType SceneType) { this.SceneType = SceneType; }

        public void Enter() { EnterScene(); }
        public void Exit() { ExitScene(); }
        public void Update() { UpdateScene(); }

        protected virtual void EnterScene() { }

        protected virtual void ExitScene()
        {
            ReleaseResource();
            Resources.UnloadUnusedAssets();
        }
        protected virtual void UpdateScene() { }

        public virtual async UniTask OnLoadResource(IProgress<LoadingProgressResult> progress)
        {
        }
        public virtual async UniTask OnLoadComplete() { }

        public virtual void ReleaseResource()
        {
            
        }
    }
}