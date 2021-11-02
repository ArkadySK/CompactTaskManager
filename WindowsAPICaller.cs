using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CompactTaskManager
{
    public static class WindowsAPICaller
    {
        [DllImport("User32.dll")]
        public static extern int FindWindow(string lpClassName, string lpWindowName);

        [DllImport("User32.dll")]
        public static extern int SetForegroundWindow(int hWnd);

        [DllImport("User32.dll")]
        public static extern bool EnumChildWindows(int hWndParent, Delegate lpEnumFunc, int lParam);

        [DllImport("User32.dll")]
        public static extern int GetWindowText(int hWnd, StringBuilder s, int nMaxCount);

        [DllImport("User32.dll")]
        public static extern int GetWindowTextLength(int hwnd);

        [DllImport("user32.dll")]
        public static extern int GetDesktopWindow();

        [DllImport("User32.dll")]
        public static extern bool MessageBeep(uint beepType);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
