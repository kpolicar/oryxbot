using System.Diagnostics;
using System.Net.Http;

namespace OryxBot.Client.Linux.Native
{
    public static class XDoTool
    {
        private static int mouseX = 0;
        private static int mouseY = 0;
        
        public static void SetCursorPos(int x, int y) {
            //(mouseX, mouseY) = (x+10,y+65);
            (mouseX, mouseY) = (x,y);
            //StartProcessForInputCommand($"mousemove {x + 10} {y + 65}");
            new HttpClient().GetAsync($"http://localhost:8010/mouse/move?x={mouseX}&y={mouseY}");
        }

        public static void LeftButtonClick() {
            //StartProcessForInputCommand("click 1");
            new HttpClient().GetAsync($"http://localhost:8010/mouse/click?x={mouseX}&y={mouseY}&mouse=left");
        }

        public static void LeftButtonDown() {
            //StartProcessForInputCommand("mousedown 1");
            new HttpClient().GetAsync($"http://localhost:8010/mouse/down?x={mouseX}&y={mouseY}&mouse=left");
        }

        public static void LeftButtonUp() {
            //StartProcessForInputCommand("mouseup 1");
            new HttpClient().GetAsync($"http://localhost:8010/mouse/up?x={mouseX}&y={mouseY}&mouse=left");
        }

        public static void RightButtonDown() {
            //StartProcessForInputCommand("mousedown 3");
            new HttpClient().GetAsync($"http://localhost:8010/mouse/down?x={mouseX}&y={mouseY}&mouse=right");
        }

        public static void RightButtonUp() {
            //StartProcessForInputCommand("mouseup 3");
            new HttpClient().GetAsync($"http://localhost:8010/mouse/up?x={mouseX}&y={mouseY}&mouse=right");
        }

        public static void RightButtonClick() {
            //StartProcessForInputCommand("click 3");
            new HttpClient().GetAsync($"http://localhost:8010/mouse/click?x={mouseX}&y={mouseY}&mouse=right");
        }

        public static void Key(string key) {
            //StartProcessForInputCommand($"key {key}");
            new HttpClient().GetAsync($"http://localhost:8010/key/press?key=key");
        }

        public static void KeyDown(string key) {
            //StartProcessForInputCommand($"keydown {key}");
            new HttpClient().GetAsync($"http://localhost:8010/key/down?key=key");
        }

        public static void KeyUp(string key) {
            //StartProcessForInputCommand($"keyup {key}");
            new HttpClient().GetAsync($"http://localhost:8010/key/up?key=key");
        }

        public static int GetActiveWindow() {
            var process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = @"xdotool",
                    Arguments = "getactivewindow",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
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
                    UseShellExecute = false
                }
            };
            process.Start();
            process.WaitForExit();
        }
    }
}
