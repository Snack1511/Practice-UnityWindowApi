﻿using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR;
using static UnityEngine.Rendering.DebugUI.MessageBox;

public static class WindowNativeManager
{
#if (UNITY_STANDALONE_WIN && !UNITY_EDITOR) || DEBUG
    #region Win API
    [DllImport("User32.dll")] public static extern IntPtr GetActiveWindow();

    [DllImport("User32.dll", EntryPoint = "FindWindowA")] public static extern IntPtr FindWindow(string className, string windowName);

    [DllImport("User32.dll")] public static extern IntPtr GetForegroundWindow();

    [DllImport("User32.dll")] public static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("User32.dll")] public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("User32.dll", EntryPoint = "SetWindowPos", SetLastError = true)] public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("Dwmapi.dll")] public static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    [DllImport("user32.dll", SetLastError = true)] private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);
    #endregion

    #region Const Value Struct
    public struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
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
        IsTransparentClick = flag;
        uint exFlag = GetWindowLong(hWnd, GWL.EXSTYLE);
        exFlag |= (WS_EX.TRANSPARENT);

        SetWindowLong(hWnd, GWL.EXSTYLE, exFlag);
    }
#endif
}
