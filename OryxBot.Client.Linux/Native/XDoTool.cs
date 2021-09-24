using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace OryxBot.Client.Linux.Native
{
    public static class XDoTool
    {
        public static void SetCursorPos(int x, int y) =>
            StartProcessForInputCommand($"mousemove {x+10} {y+65}");

        public static void LeftButtonClick() =>
            StartProcessForInputCommand($"click 1");

        public static void LeftButtonDown() =>
            StartProcessForInputCommand($"mousedown 1");

        public static void LeftButtonUp() =>
            StartProcessForInputCommand($"mouseup 1");

        public static void RightButtonDown() =>
            StartProcessForInputCommand($"mousedown 3");

        public static void RightButtonUp() =>
            StartProcessForInputCommand($"mouseup 3");
        
        public static void RightButtonClick() =>
            StartProcessForInputCommand($"click 3");
        
        public static void Key(string key) =>
            StartProcessForInputCommand($"key {key}");
        
        public static void KeyDown(string key) =>
            StartProcessForInputCommand($"keydown {key}");

        public static void KeyUp(string key) =>
            StartProcessForInputCommand($"keyup {key}");

        public static int GetActiveWindow() {
            var process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = @"xdotool",
                    Arguments = "getactivewindow",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                }
            };
            process.Start();
            process.WaitForExit();

            int.TryParse(process.StandardOutput.ReadToEnd(), out var windowId);
            return windowId;
        }

        private static void StartProcessForInputCommand(string arguments) {
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
