using System;
using Cysharp.Threading.Tasks;
using Manager;
using Script.Define;
using Script.Manager.SingletonManager;
using UnityEngine;

namespace Script.GameFlow.GameScene
{
    public class StartSceneInfoContext :  ISceneInfoContext
    {
        
    }

    public class StartScene : SceneBase
    {
        public override async UniTask OnLoadResourceAsync(IProgress<LoadingProgressResult> progress)
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

        public override void OnLoadComplete()
        {
            Debug.Log("StartScene::LoadComplete");
        }
        
        public StartScene(ESceneType SceneType) : base(SceneType)
        {
            
        }

        public override void EnterScene(ISceneInfoContext context)
        {
            base.EnterScene(context);
            Debug.Log("StartScene::EnterScene Enter");
            SceneManager.Instance.ChangeScene(ESceneType.TestScene, null, null, true);
        }
    }
}