#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System;
using Cysharp.Threading.Tasks;
using Script.GameContent.UI;
using UnityEngine;

namespace Script.GameFlow.Command
{
    /// <summary>
    /// 치트 패널을 만든다. 개발 빌드와 에디터에서만 컴파일된다.
    /// 생성 결과를 콜백으로 돌려주므로, 씬이 그것을 들고 있다가 ReleaseResource 에서 파기한다.
    /// </summary>
    public class CmdCreateCheatPanel : CmdBase
    {
        private readonly Action<CheatPanel> onCreated;

        public CmdCreateCheatPanel(Action<CheatPanel> onCreated)
        {
            this.onCreated = onCreated;
        }

        public override async UniTask Execute()
        {
            CheatPanel panel = await CheatPanel.CreateAsync();

            if (panel == null)
            {
                Debug.LogWarning("[CmdCreateCheatPanel] 치트 패널 생성에 실패했습니다. 앞선 로그를 확인하세요.");
                return;
            }

            onCreated?.Invoke(panel);
        }
    }
}
#endif
