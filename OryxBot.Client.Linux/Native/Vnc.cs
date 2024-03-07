using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using OryxBot.Client.Linux.Services;

namespace OryxBot.Client.Linux.Native
{
    public static class Vnc
    {
        public static bool Connected;
        public static (int x, int y)? Dimensions;
        public static int? ScalingPercent = 100;
        private static Process? process;
        public static event EventHandler? ConnectionEstablished;


        public static void CloseVnc() {
            if (process != null)
                FileLogger.Common.Info($"Closing the VNC client");
            process?.Kill();
            process?.WaitForExit();

            var process2 = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "bash",
                    Arguments = "-c \"pkill -f /home/oryxbot/libs/jdk1.7.0_51/bin/java\"",
                    UseShellExecute = false
                }
            };
            process2.Start();
            process2.WaitForExit();
            Connected = default;
            Dimensions = default;
            ScalingPercent = 100;
        }


        public static bool StartVnc() {
            FileLogger.Common.Info($"Starting the VNC client");
            process = new Process {
                StartInfo = new ProcessStartInfo {
                    RedirectStandardOutput = true,
                    FileName = @"/home/oryxbot/libs/jdk1.7.0_51/bin/java",
                    Arguments =
                        String.Join(' ', 
                            "-Dawt.toolkit=ghostawt.GhostToolkit -Djava.awt.graphicsenv=ghostawt.image.GhostGraphicsEnvironment -Djava.awt.headless=false -Dsun.font.fontmanager=ghostawt.sun.GFontManager -Dsun.boot.library.path=/home/oryxbot/libs/jdk1.7.0_51/bin",
                            "-jar /home/oryxbot/apps/VncClient.jar",
                            "VncViewer HOST 10.0.0.100 PORT 5901 PASSWORD oryxbot \"\\\"Scaling\" \"factor\\\"\" auto \"\\\"Encoding\\\"\" rAW \"\\\"Show\" \"controls\\\"\" No \"\\\"JPEG\" image \"quality\\\"\" 9 \"\\\"Offer\" \"relogin\\\"\" No \"\\\"Restricted\" \"colors\\\"\" No \"\\\"Compression\" \"level\\\"\" 7"
                            ),
                    WorkingDirectory = "/home/oryxbot",
                    UseShellExecute = false
                }
            };
            process.OutputDataReceived += OnVncOutputDataReceived;
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExitAsync().ContinueWith(task => { Console.WriteLine(">>>>>>> " + process.ExitCode); });
    
            var waitedFor = 0;
            var maxWaitFor = 15000;
            FileLogger.Common.Info($"Establishing connection to the customer VNC server");
            do {
                Thread.Sleep(50);
            } while ((!Connected || Dimensions == null) &&
                     waitedFor < maxWaitFor);

            if (waitedFor >= maxWaitFor) {
                FileLogger.Common.Info($"The VNC client was attempting to connect to the VNC server for {waitedFor}ms. Aborting!");
                return false;
            }
            
            // Any more
            Thread.Sleep(200);

            ConnectionEstablished?.Invoke(null, EventArgs.Empty);
            return true;
        }

        private static void OnVncOutputDataReceived(object sender, DataReceivedEventArgs e) {
            if (e.Data == "VNC authentication: success") {
                Connected = true;
                FileLogger.Common.Info($"Connection to the customer VNC server has been established");
            }

            if (e.Data?.StartsWith("Desktop size is ") ?? false) {
                var regex = Regex.Match(e.Data, @"(\d+) ?x ?(\d+)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                Dimensions = (int.Parse(regex.Groups[1].Value), int.Parse(regex.Groups[2].Value));
                FileLogger.Common.Info($"VNC server is using the dimensions"+Dimensions);
            }

            if (e.Data?.StartsWith("Scaling desktop at ") ?? false) {
                var regex = Regex.Match(e.Data, @"(\d+)", RegexOptions.Singleline);
                ScalingPercent = int.Parse(regex.Groups[1].Value);
                Console.WriteLine("Scaling: " + ScalingPercent);
            }
        }
    }
}
