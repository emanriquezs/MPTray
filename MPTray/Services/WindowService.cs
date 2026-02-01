using H.NotifyIcon;
using MPTray.ViewModels;
using MPTray.Views;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Graphics;

namespace MPTray.Services
{
    public static class WindowService
    {
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out POINT lpPoint);

        public struct POINT { public int X; public int Y; }

        public static void OpenPlayerWindow(PlayerVM playerVM)
        {
            GetCursorPos(out POINT point);
            var playerWindow = new PlayerWindow(playerVM, new PointInt32(point.X, point.Y));
            playerWindow.AppWindow.IsShownInSwitchers = false;
            playerWindow.Activate();
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(playerWindow);
            SetForegroundWindow(hWnd);
        }

        public static void OpenSettingsWindow(SettingsVM settingsVM)
        {
            var settingsWindow = new SettingsWindow(settingsVM);
            settingsWindow.Activate();
        }

    }
}
