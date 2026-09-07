using UnityEngine;

namespace Script.Manager.StaticManager
{
    public static class ResolutionManager
    {
        public static void Initialize()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        // 작업 표시줄 높이를 OS 에서 직접 읽는다.
        // 이전에는 systemHeight - 49 로 상수 가정했는데 실제 작업 표시줄이 48px 이라 하단에 1px 이 비었다.
        if (WindowNativeManager.TryGetPrimaryMonitor(out WindowNativeManager.MonitorInfo primary))
        {
            WindowNativeManager.SetWindowFrame(primary.workX, primary.workY, primary.workWidth, primary.workHeight);
            return;
        }

        // 모니터 열거가 실패했을 때만 쓰는 예비 경로. 하단이 정확히 맞지 않을 수 있다.
        Debug.LogWarning("[ResolutionManager] 모니터 열거 실패. systemHeight 기준 예비 경로로 창을 맞춥니다.");
        WindowNativeManager.SetWindowFrame(0, 0, Display.main.systemWidth, Display.main.systemHeight - 49);
#endif
        }

        public static void Release()
        {
        }
    }
}