using System;
using Cysharp.Threading.Tasks;
using Script.Define;
using Script.GameFlow.Command;
#if DEVELOPMENT_BUILD || UNITY_EDITOR
using Script.GameContent.UI;
#endif

namespace Script.GameFlow.GameScene
{
    public class LobbyScene : SceneBase
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        private CheatPanel cheatPanel;
#endif

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
            await base.OnLoadResourceAsync(progress);
        }

        /// <summary>
        /// 이 씬이 시작될 때 만들어야 하는 오브젝트를 명령으로 등록한다.
        /// 로딩 씬이 활성 씬을 로비로 바꾼 뒤 실행하므로 생성물이 로비 씬에 들어간다.
        /// </summary>
        public override void OnEnqueueStartCommands(CmdExecutor executor)
        {
            base.OnEnqueueStartCommands(executor);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            executor.AddCommand(new CmdCreateCheatPanel(panel => cheatPanel = panel));
#endif
        }

        public override void OnLoadComplete()
        {
            base.OnLoadComplete();
        }

        public override void ReleaseResource()
        {
            ReleaseCheatPanel();

            base.ReleaseResource();
        }

        private void ReleaseCheatPanel()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (cheatPanel == null)
                return;

            UnityEngine.Object.Destroy(cheatPanel.gameObject);
            cheatPanel = null;
#endif
        }
    }
}
