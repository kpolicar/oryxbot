using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OryxBot.Client.Linux.Native
{
    public static class Anydesk
    {
        public static event EventHandler? ConnectionEstablished;

        public static int WindowId;
        public static bool Connected;
        public static (int x, int y)? Dimensions;
        public static int? ScalingPercent;
        
        
        public static void CloseAnydesk() {
            Console.WriteLine("Closing Anydesk");
            
            var process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "bash",
                    Arguments = "-c \"xdotool search --name tightvnc windowkill %@\"",
                    UseShellExecute = false,
                }
            };
            process.Start();
            process.WaitForExit();
        }


        public static void StartAnydesk() {
            Console.WriteLine("Starting Anydesk");
            var process = new Process {
                StartInfo = new ProcessStartInfo {
                    RedirectStandardOutput = true,
                    FileName = @"java",
                    Arguments = "-jar /home/user/Applications/VncViewer/VncViewer.jar HOST 10.0.0.100 \"Scaling factor\" auto \"Show controls\" No PASSWORD ***REMOVED*** \"JPEG image quality\" 0 \"Offer relogin\" No \"Restricted colors\" Yes \"Compression level\" 1",
                    UseShellExecute = false,
                }
            };
            process.OutputDataReceived += OnVncOutputDataReceived;
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExitAsync().ContinueWith(task => {
                Console.WriteLine(">>>>>>> "+process.ExitCode);
            });

            do {
                WindowId = WindowId == 0 ? XDoTool.GetActiveWindow() : WindowId;
                Thread.Sleep(50);
            } while (WindowId == 0 || !Connected || Dimensions == null || ScalingPercent == null);
            
            ConnectionEstablished?.Invoke(null, EventArgs.Empty);
        }

        private static void OnVncOutputDataReceived(object sender, DataReceivedEventArgs e) {
            if (e.Data == "VNC authentication: success") {
                Connected = true;
                Console.WriteLine("Connected: "+Connected);
            }

            if (e.Data?.StartsWith("Desktop size is ") ?? false) {
                var regex = Regex.Match(e.Data, @"(\d+) ?x ?(\d+)", RegexOptions.Singleline|RegexOptions.IgnoreCase);
                Dimensions = (int.Parse(regex.Groups[1].Value), int.Parse(regex.Groups[2].Value));
                Console.WriteLine("Dimensions: "+Dimensions);
            } 

            if (e.Data?.StartsWith("Scaling desktop at ") ?? false) {
                var regex = Regex.Match(e.Data, @"(\d+)", RegexOptions.Singleline);
                ScalingPercent = int.Parse(regex.Groups[1].Value);
                Console.WriteLine("Scaling: "+ScalingPercent);
            } 
        }
    }
}
