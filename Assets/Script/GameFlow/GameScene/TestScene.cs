using System;
using Cysharp.Threading.Tasks;
using Manager;
using Script.Define;
using UnityEngine;

namespace Script.GameFlow.GameScene
{
    public class TestScene : SceneBase
    {
        public override async UniTask OnLoadResourceAsync(IProgress<LoadingProgressResult> progress)
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

        public override void OnLoadComplete()
        {
            Debug.Log("TestScene::LoadComplete");
        }
        public TestScene(ESceneType SceneType) : base(SceneType)
        {
        }
        public override void EnterScene(ISceneInfoContext context)
        {
            base.EnterScene(context);
            Debug.Log("TestScene::EnterScene Enter");
        }
    }
}