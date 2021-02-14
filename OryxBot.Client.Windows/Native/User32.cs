using System;
using System.Runtime.InteropServices;

namespace OryxBot.Client.Windows.Native
{
    public static class User32
    {
        
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);
        
        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);
        
        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);
        
        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, ref Rect lpRect);

        [DllImport("user32.dll")]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "PostMessageA", SetLastError = true)]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, long wParam, long lParam);

        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
        
        [DllImport("user32.dll", CharSet=CharSet.Ansi, SetLastError=true)]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        
        public static int MakeLParam(int LoWord, int HiWord)
        {
            return (int)((HiWord << 16) | (LoWord & 0xFFFF));
        }
        
        public const int WM_MOUSEMOVE = 0x0200;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;
        public const int WM_CHAR = 0x0102;
        
        // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getsystemmetrics
        public const int SM_CMONITORS = 80;
        public const int SM_CXFULLSCREEN = 0;
        public const int SM_CYFULLSCREEN = 1;


        public struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }
        
        public struct POINT
        {
            public int X;
            public int Y;
        }
        
        //assorted constants needed
        public static int GWL_STYLE = -16;
        public static int WS_SIZEBOX = 0x00040000;
        public static int WS_BORDER = 0x00800000;
        public static int WS_DLGFRAME = 0x00400000;
        public static int WS_CAPTION = WS_BORDER | WS_DLGFRAME | WS_SIZEBOX; //window with a title bar
    }
}
