using Cysharp.Threading.Tasks;
using Manager;
using UnityEngine;

namespace Script.GameFlow.GameScene
{
    public class TestScene : SceneBase
    {
        public override async UniTask OnLoadResource()
        {
            Debug.Log("TestScene::LoadResource Start");
            await UniTask.WaitForSeconds(10.0f);
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