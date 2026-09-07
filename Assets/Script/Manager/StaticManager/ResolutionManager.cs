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

        /// <summary>
        /// 포커스를 얻을 때 창을 현재 모니터의 작업 영역에 다시 맞춘다.
        /// 실행 중에 작업 표시줄이 옮겨지거나 자동 숨김이 켜지면 부팅 때 읽은 값이 낡기 때문이다.
        ///
        /// 주 모니터가 아니라 <b>창이 올라가 있는 모니터</b>를 기준으로 잡는다.
        /// 주 모니터로 맞추면 치트 패널로 다른 모니터에 옮겨둔 창이 도로 끌려온다.
        ///
        /// 즉시 감지하는 방법은 WindowNativeManager 의 개선 메모 참고.
        /// </summary>
        public static void OnApplicationFocusChanged(bool isFocus)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        if (!isFocus)
            return;

        if (!WindowNativeManager.TryGetMonitorForWindow(out WindowNativeManager.MonitorInfo monitor))
            return;

        // 이미 맞으면 아무것도 하지 않는다. 포커스마다 SetWindowPos 를 때리지 않기 위한 것이다.
        if (WindowNativeManager.IsWindowFittedTo(monitor))
            return;

        WindowNativeManager.SetWindowToMonitor(monitor);
#endif
        }

        public static void Release()
        {
        }
    }
}