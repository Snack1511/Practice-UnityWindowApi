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
    
        /// <summary>연결된 모니터 목록. 실패하면 빈 목록.</summary>
        public static List<MonitorInfo> GetMonitors()
        {
            List<MonitorInfo> monitors = new List<MonitorInfo>();

            // 콜백이 네이티브 호출 도중 수집되지 않도록 지역 변수로 붙들어 둔다.
            MonitorEnumProc callback = (IntPtr hMonitor, IntPtr hdc, ref RECT rect, IntPtr data) =>
            {
                MONITORINFOEX info = new MONITORINFOEX { cbSize = Marshal.SizeOf(typeof(MONITORINFOEX)) };

                if (!GetMonitorInfo(hMonitor, ref info))
                    return true;

                monitors.Add(new MonitorInfo
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
                });

                return true;
            };

            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);
            GC.KeepAlive(callback);

            return monitors;
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
