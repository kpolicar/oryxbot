using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OryxBot.Client.Linux.Native
{
    public static class XDoTool
    {
        public static void SetCursorPos(int x, int y) =>
            StartProcess($"mousemove {x} {y}");

        public static void LeftButtonClick() =>
            StartProcess("click 1");

        public static void LeftButtonDown() =>
            StartProcess("mousedown 1");

        public static void LeftButtonUp() =>
            StartProcess("mouseup 1");

        public static void RightButtonDown() =>
            StartProcess("mousedown 3");

        public static void RightButtonUp() =>
            StartProcess("mouseup 3");
        
        public static void RightButtonClick() =>
            StartProcess("click 3");
        
        public static void Key(string key) =>
            StartProcess($"key {key}");
        
        public static void KeyDown(string key) =>
            StartProcess($"keydown {key}");

        public static void KeyUp(string key) =>
            StartProcess($"keyup {key}");

        private static void StartProcess(string arguments) {
            var process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = @"xdotool",
                    Arguments = arguments,
                    UseShellExecute = false,
                }
            };
            process.Start();
            process.WaitForExit();
        }
    }
}
