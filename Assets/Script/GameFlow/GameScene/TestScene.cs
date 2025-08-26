using System;
using Cysharp.Threading.Tasks;
using Manager;
using Script.Define;
using UnityEngine;

namespace Script.GameFlow.GameScene
{
    public class TestScene : SceneBase
    {
        public override async UniTask OnLoadResource(IProgress<LoadingProgressResult> progress)
        {
            Debug.Log("TestScene::LoadResource Start");
            for (int i = 0; i < 1000; ++i)
            {
                float duration = 0.001f;
                float amount = ((float)(i+1) / 1000);
                progress?.Report(new LoadingProgressResult()
                {
                    amount = amount
                });
                await UniTask.WaitForSeconds(duration);                
            }
            Debug.Log("TestScene::LoadResource End");
        }

        public override async UniTask OnLoadComplete()
        {
            Debug.Log("TestScene::LoadComplete Start");
            await UniTask.WaitForSeconds(10.0f);
            Debug.Log("TestScene::LoadComplete End");
        }
        public TestScene(ESceneType SceneType) : base(SceneType)
        {
        }
        protected override void EnterScene()
        {
            base.EnterScene();
            Debug.Log("TestScene::EnterScene Enter");
        }
    }
}