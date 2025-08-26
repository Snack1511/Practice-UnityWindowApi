using System;
using Cysharp.Threading.Tasks;
using Manager;
using Script.Define;
using UnityEngine;

namespace Script.GameFlow.GameScene
{
    public class StartScene : SceneBase
    {
        public override async UniTask OnLoadResource(IProgress<LoadingProgressResult> progress)
        {
            Debug.Log("StartScene::LoadResource Start");

            for (int i = 0; i < 10; ++i)
            {
                progress?.Report(new LoadingProgressResult()
                {
                    amount = ((float)(i + 1)/10)
                });
                await UniTask.WaitForSeconds(1.0f);                
            }

            Debug.Log("StartScene::LoadResource End");
        }

        public override async UniTask OnLoadComplete()
        {
            Debug.Log("StartScene::LoadComplete Start");
            await UniTask.WaitForSeconds(10.0f);
            Debug.Log("StartScene::LoadComplete End");
        }
        
        public StartScene(ESceneType SceneType) : base(SceneType)
        {
        }

        protected override void EnterScene()
        {
            base.EnterScene();
            Debug.Log("StartScene::EnterScene Enter");
            SceneManager.Instance.ChangeScene(ESceneType.TestScene);
        }
    }
}