using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Script.Manager.StaticManager
{
    public static class WindowNativeManager
    {
#if (UNITY_STANDALONE_WIN && !UNITY_EDITOR) || DEBUG
        #region Win API
        [DllImport("User32.dll")] public static extern IntPtr GetActiveWindow();

        [DllImport("User32.dll", EntryPoint = "FindWindowA")] public static extern IntPtr FindWindow(string className, string windowName);

        [DllImport("User32.dll")] public static extern IntPtr GetForegroundWindow();

        [DllImport("User32.dll")] public static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("User32.dll", SetLastError = true)] public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("User32.dll", EntryPoint = "SetWindowPos", SetLastError = true)] public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("Dwmapi.dll")] public static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

        [DllImport("User32.dll", SetLastError = true)] public static extern bool SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)] private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("User32.dll")] private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

        [DllImport("User32.dll", CharSet = CharSet.Auto)] private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

        [DllImport("User32.dll")] private static extern IntPtr MonitorFromWindow(IntPtr hWnd, uint dwFlags);

        [DllImport("User32.dll")] private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

        private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdc, ref RECT rect, IntPtr data);
        #endregion

        #region Const Value Struct
        public struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public int Width => right - left;
            public int Height => bottom - top;
        }

        // szDevice 가 고정 길이 배열이라 CharSet 과 SizeConst 를 맞춰야 GetMonitorInfo 가 실패하지 않는다.
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MONITORINFOEX
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string szDevice;
        }

        private const uint MONITORINFOF_PRIMARY = 0x00000001;

        //창이 어느 모니터에도 완전히 안 걸쳐 있어도 가장 가까운 모니터를 돌려준다.
        private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

        /// <summary>모니터 하나의 전체 영역과 작업 영역(작업 표시줄 제외).</summary>
        public struct MonitorInfo
        {
            public string device;
            public bool isPrimary;

            public int x;
            public int y;
            public int width;
            public int height;

            public int workX;
            public int workY;
            public int workWidth;
            public int workHeight;

            public override string ToString()
            {
                return $"{device} {width}x{height} @{x},{y}{(isPrimary ? " (주)" : "")}";
            }
        }

        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        public static readonly IntPtr HWND_TOP = new IntPtr(0);
        public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        public struct GWL
        {
            public const int STYLE = -16;
            public const int EXSTYLE = -20;
        }

        public struct WS
        {
            public const uint POPUP = 0x80000000;
            public const uint VISIBLE = 0x10000000;
            public const uint OVERLAPPEDWINDOW = 0x00CF0000;
            public const uint BORDER = 0x00800000;
            public const uint DLGFRAME = 0x00400000;
            public const uint CAPTION = 0x00C00000;
            public const uint SYSMENU = 0x00080000;
            public const uint THICKFRAME = 0x00040000;
            public const uint MINIMIZEBOX = 0x00020000;
            public const uint MAXIMIZEBOX = 0x00010000;
        }

        public struct WS_EX
        {
            public const uint LAYERED = 0x00080000;
            public const uint TRANSPARENT = 0x00000020;
            public const uint TOPMOST = 0x00000008;
        }

        public struct SWP
        {
            public const uint NOMOVE = 0x0002;
            public const uint NOSIZE = 0x0001;
            public const uint NOOWNERZORDER = 0x0200;
            public const uint FRAMECHANGED = 0x0020;
            public const uint SHOWWINDOW = 0x0040;
            public const uint NOACTIVATE = 0x0010;
            public const uint TOPMOST = NOMOVE | NOSIZE | NOOWNERZORDER;
        }

        public struct LWA
        {
            public const uint ALPHA = 0x00000002;
        }
        #endregion

        public static IntPtr hWnd;

        public static string SetWindowFrame(int windowPosX = 0, int windowPosY = 0, int windowW = 1280, int windowH = 720) 
        {
            string str = "None";
            hWnd = GetActiveWindow();

            // 타이틀바 설정 변경 플래그
            uint sFlag = GetWindowLong(hWnd, GWL.STYLE);
            sFlag &= ~(WS.CAPTION | WS.THICKFRAME | WS.MINIMIZEBOX | WS.MAXIMIZEBOX | WS.SYSMENU);
            sFlag |= WS.POPUP;//(WS.OVERLAPPEDWINDOW);

            // 윈도우 상태 설정
            SetWindowLong(hWnd, GWL.STYLE, sFlag);
 
            // 얜 뭐냐
            //uint exFlag = GetWindowLong(hWnd, GWL.EXSTYLE);
            //exFlag |= (WS_EX.LAYERED);
            //SetWindowLong(hWnd, GWL.EXSTYLE, exFlag);

            //윈도우 위치 및 크기 지정
            SetWindowPos(hWnd, HWND_TOPMOST, windowPosX, windowPosY, windowW, windowH, SWP.FRAMECHANGED);

            //렌더링 영역 제거
            MARGINS margins = new MARGINS { 
                cxLeftWidth = -1
            };
            DwmExtendFrameIntoClientArea(hWnd, ref margins);

            //윈도우 전체 색상 조절 --> 렌더링 한 결과까지 투명화 처리
            //SetLayeredWindowAttributes(hWnd, 0, 0, LWA.ALPHA);

            var buffer = new System.Text.StringBuilder(256);
            GetWindowText(hWnd, buffer, buffer.Capacity);
            str = buffer.ToString();
            return str;
        }
    
        // ─────────────────────────────────────────────────────────────────────
        // 개선 메모 — 작업 영역 변경을 즉시 감지하는 방법
        //
        // 지금은 부팅 시 1회 + 창이 포커스를 얻을 때 rcWork 를 다시 읽는 방식이다
        // (ResolutionManager.OnApplicationFocusChanged).
        // 실행 중에 작업 표시줄을 옮기거나 자동 숨김을 켜도, 창을 다시 클릭하기 전까지는 반영되지 않는다.
        //
        // 정석은 창 프로시저에서 아래 메시지를 받는 것이다.
        //
        //   WM_SETTINGCHANGE (0x001A)  wParam == SPI_SETWORKAREA (0x002F)  작업 영역 변경
        //   WM_DISPLAYCHANGE (0x007E)                                      해상도·모니터 구성 변경
        //   WM_DPICHANGED    (0x02E0)                                      DPI 배율 변경
        //
        // Unity 는 WndProc 을 노출하지 않으므로 창 서브클래싱이 필요하다.
        //
        //   원래 프로시저 = SetWindowLongPtr(hWnd, GWLP_WNDPROC(-4), 새 프로시저)
        //   새 프로시저에서 위 메시지를 처리하고 CallWindowProc 으로 원래 것에 넘긴다
        //   앱 종료 시 반드시 원래 프로시저로 되돌린다
        //
        // 도입 전에 확인할 것:
        //   - 델리게이트를 static 필드로 붙들어야 한다. GC 되면 네이티브가 죽은 포인터를 호출해 크래시다.
        //   - 콜백은 메인 스레드에서 불리지만 Unity 프레임 경계가 아니다.
        //     Unity API 를 직접 부르지 말고 플래그만 세우고 GameProcessManager 업데이트에서 처리한다.
        //   - 되돌리기를 빠뜨리면 에디터에서 도메인 리로드 때 죽은 프로시저가 남는다.
        //     #if !UNITY_EDITOR 로 막거나 Release 에서 반드시 복원한다.
        //
        // 포커스 방식으로 부족하다는 근거가 생기기 전에는 넣지 않는다.
        // 크래시 비용이 얻는 것보다 크다.
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>연결된 모니터 목록. 실패하면 빈 목록.</summary>
        public static List<MonitorInfo> GetMonitors()
        {
            List<MonitorInfo> monitors = new List<MonitorInfo>();

            // 콜백이 네이티브 호출 도중 수집되지 않도록 지역 변수로 붙들어 둔다.
            MonitorEnumProc callback = (IntPtr hMonitor, IntPtr hdc, ref RECT rect, IntPtr data) =>
            {
                MONITORINFOEX info = new MONITORINFOEX { cbSize = Marshal.SizeOf(typeof(MONITORINFOEX)) };

                if (GetMonitorInfo(hMonitor, ref info))
                    monitors.Add(ToMonitorInfo(info));

                return true;
            };

            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);
            GC.KeepAlive(callback);

            return monitors;
        }

        private static MonitorInfo ToMonitorInfo(MONITORINFOEX info)
        {
            return new MonitorInfo
            {
                device = info.szDevice,
                isPrimary = (info.dwFlags & MONITORINFOF_PRIMARY) != 0,

                x = info.rcMonitor.left,
                y = info.rcMonitor.top,
                width = info.rcMonitor.Width,
                height = info.rcMonitor.Height,

                workX = info.rcWork.left,
                workY = info.rcWork.top,
                workWidth = info.rcWork.Width,
                workHeight = info.rcWork.Height,
            };
        }

        /// <summary>
        /// 창이 현재 올라가 있는 모니터. 창을 다른 모니터로 옮겨둔 상태를 존중해야 할 때 쓴다.
        /// 주 모니터를 기준으로 잡으면 옮겨둔 창이 도로 끌려온다.
        /// </summary>
        public static bool TryGetMonitorForWindow(out MonitorInfo monitor)
        {
            if (hWnd == IntPtr.Zero)
                hWnd = GetActiveWindow();

            IntPtr hMonitor = MonitorFromWindow(hWnd, MONITOR_DEFAULTTONEAREST);
            if (hMonitor == IntPtr.Zero)
            {
                monitor = default;
                return false;
            }

            MONITORINFOEX info = new MONITORINFOEX { cbSize = Marshal.SizeOf(typeof(MONITORINFOEX)) };
            if (!GetMonitorInfo(hMonitor, ref info))
            {
                monitor = default;
                return false;
            }

            monitor = ToMonitorInfo(info);
            return true;
        }

        /// <summary>현재 창 사각형. 이미 맞는 위치인지 확인해 불필요한 SetWindowPos 를 건너뛰는 용도.</summary>
        public static bool TryGetWindowRect(out RECT rect)
        {
            if (hWnd == IntPtr.Zero)
                hWnd = GetActiveWindow();

            return GetWindowRect(hWnd, out rect);
        }

        /// <summary>주 모니터. 열거에 실패하면 false.</summary>
        public static bool TryGetPrimaryMonitor(out MonitorInfo monitor)
        {
            List<MonitorInfo> monitors = GetMonitors();

            foreach (MonitorInfo candidate in monitors)
            {
                if (!candidate.isPrimary)
                    continue;

                monitor = candidate;
                return true;
            }

            //주 모니터 플래그를 못 찾아도 하나라도 있으면 그것을 쓴다.
            if (monitors.Count > 0)
            {
                monitor = monitors[0];
                return true;
            }

            monitor = default;
            return false;
        }

        /// <summary>창을 해당 모니터의 작업 영역에 꽉 채운다. 작업 표시줄을 덮지 않는다.</summary>
        public static void SetWindowToMonitor(MonitorInfo monitor)
        {
            if (hWnd == IntPtr.Zero)
                hWnd = GetActiveWindow();

            SetWindowPos(hWnd, HWND_TOPMOST, monitor.workX, monitor.workY, monitor.workWidth, monitor.workHeight,
                SWP.FRAMECHANGED | SWP.SHOWWINDOW);

            UnityEngine.Debug.Log($"[WindowNative] 창을 {monitor} 의 작업 영역으로 이동 " +
                                  $"({monitor.workX},{monitor.workY} {monitor.workWidth}x{monitor.workHeight})");
        }

        /// <summary>창 사각형이 이미 그 작업 영역과 같은지.</summary>
        public static bool IsWindowFittedTo(MonitorInfo monitor)
        {
            if (!TryGetWindowRect(out RECT rect))
                return false;

            return rect.left == monitor.workX
                   && rect.top == monitor.workY
                   && rect.Width == monitor.workWidth
                   && rect.Height == monitor.workHeight;
        }

        public static string GetWindowName() 
        {
            string str = "None";
            hWnd = GetActiveWindow();

            var buffer = new System.Text.StringBuilder(256);
            GetWindowText(hWnd, buffer, buffer.Capacity);
            str = buffer.ToString();
            return str;
        }

        public static bool IsTransparentClick { get; set; } = false;
        public static void SetTransparentClick(bool flag)
        {
            // SetWindowFrame 호출 전이면 hWnd 가 비어있다
            if (hWnd == IntPtr.Zero)
                hWnd = GetActiveWindow();

            uint before = GetWindowLong(hWnd, GWL.EXSTYLE);

            // WS_EX_TRANSPARENT 만으로는 히트 테스트가 통과되지 않는다.
            // 레이어드 윈도우일 때만 마우스가 아래 창으로 넘어간다.
            uint exFlag = before;
            if (flag)
                exFlag |= (WS_EX.LAYERED | WS_EX.TRANSPARENT);
            else
                exFlag &= ~(WS_EX.LAYERED | WS_EX.TRANSPARENT);

            // SetWindowLong 은 실패 시 0 을 반환한다 (직전 값이 0 이었을 수도 있어 에러 코드를 같이 본다)
            if (SetWindowLong(hWnd, GWL.EXSTYLE, exFlag) == 0 && Marshal.GetLastWin32Error() != 0)
            {
                UnityEngine.Debug.LogWarning($"SetTransparentClick 실패 : Win32Error {Marshal.GetLastWin32Error()}");
                return;
            }

            // 레이어드 윈도우는 표시 방식을 지정해야 그려진다. 알파 255 = 전체 불투명이라
            // DWM 의 픽셀 단위 알파(투명 배경)는 그대로 유지된다.
            if (flag)
                SetLayeredWindowAttributes(hWnd, 0, 255, LWA.ALPHA);

            // 확장 스타일 변경은 SetWindowPos 로 커밋해야 창에 반영된다.
            // SetWindowLong 만으로는 값만 바뀌고 히트 테스트 동작이 갱신되지 않는다.
            SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0,
                SWP.NOMOVE | SWP.NOSIZE | SWP.NOOWNERZORDER | SWP.NOACTIVATE | SWP.FRAMECHANGED);

            uint after = GetWindowLong(hWnd, GWL.EXSTYLE);
            UnityEngine.Debug.Log($"[WindowNative] SetTransparentClick({flag}) hWnd=0x{hWnd.ToInt64():X} " +
                                  $"exStyle 0x{before:X} -> 0x{after:X}, LAYERED={(after & WS_EX.LAYERED) != 0}, TRANSPARENT={(after & WS_EX.TRANSPARENT) != 0}");

            IsTransparentClick = flag;
        }
#endif
    }
}
