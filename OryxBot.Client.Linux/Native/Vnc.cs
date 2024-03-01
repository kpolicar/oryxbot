using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;

namespace OryxBot.Client.Linux.Native
{
    public static class Vnc
    {
        public static bool Connected;
        public static (int x, int y)? Dimensions;
        public static int? ScalingPercent;
        public static event EventHandler? ConnectionEstablished;


        public static void CloseVnc() {
            Console.WriteLine("Closing VNC client");

            var process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "bash",
                    Arguments = "-c \"xdotool search --name tightvnc windowkill %@\"",
                    UseShellExecute = false
                }
            };
            process.Start();
            process.WaitForExit();
            Connected = default;
            Dimensions = default;
            ScalingPercent = default;
        }


        public static bool StartVnc() {
            Console.WriteLine("Starting VNC client");
            var process = new Process {
                StartInfo = new ProcessStartInfo {
                    RedirectStandardOutput = true,
                    FileName = @"java",
                    Arguments =
                        "-jar /home/user/Applications/VncClient.jar",
                    UseShellExecute = false
                }
            };
            process.OutputDataReceived += OnVncOutputDataReceived;
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExitAsync().ContinueWith(task => { Console.WriteLine(">>>>>>> " + process.ExitCode); });

            var waitedFor = 0;
            var maxWaitFor = 15000;
            do {
                Thread.Sleep(50);
            } while ((!Connected || Dimensions == null || ScalingPercent == null) &&
                     waitedFor < maxWaitFor);

            if (waitedFor >= maxWaitFor) {
                Console.WriteLine("Tried to connect to VNC server for " + waitedFor + "ms. Aborting!");
                return false;
            }

            ConnectionEstablished?.Invoke(null, EventArgs.Empty);
            return true;
        }

        private static void OnVncOutputDataReceived(object sender, DataReceivedEventArgs e) {
            if (e.Data == "VNC authentication: success") {
                Connected = true;
                Console.WriteLine("Connected: " + Connected);
            }

            if (e.Data?.StartsWith("Desktop size is ") ?? false) {
                var regex = Regex.Match(e.Data, @"(\d+) ?x ?(\d+)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                Dimensions = (int.Parse(regex.Groups[1].Value), int.Parse(regex.Groups[2].Value));
                Console.WriteLine("Dimensions: " + Dimensions);
            }

            if (e.Data?.StartsWith("Scaling desktop at ") ?? false) {
                var regex = Regex.Match(e.Data, @"(\d+)", RegexOptions.Singleline);
                ScalingPercent = int.Parse(regex.Groups[1].Value);
                Console.WriteLine("Scaling: " + ScalingPercent);
            }
        }
    }
}
