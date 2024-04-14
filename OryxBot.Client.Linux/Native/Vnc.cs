using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using OryxBot.Client.Linux.Services;
using OryxBot.Shared.Design;

namespace OryxBot.Client.Linux.Native
{
    public static class Vnc
    {
        public static bool closing;
        public static bool Connected;
        public static (int x, int y)? Dimensions;
        public static int? ScalingPercent = 100;
        private static Process? process;
        private static bool processHasStarted;
        public static event EventHandler? ConnectionEstablished;
        public static event EventHandler? VncError;


        public static void CloseVnc() {
            closing = true;
            
            if (process != null)
                FileLogger.Common.Info($"Closing the VNC client");
            if (processHasStarted) {
                process?.Kill();
                process?.WaitForExit();
                processHasStarted = false;
            }

            var process2 = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "pkill",
                    Arguments = "-f /etc/java-se-7u75-ri/bin/java",
                    UseShellExecute = false
                }
            };
            process2.Start();
            process2.WaitForExit();
            Connected = default;
            Dimensions = default;
            ScalingPercent = 100;
            
            closing = false;
        }


        public static bool StartVnc() {
            try {
                FileLogger.Common.Info($"Starting the VNC client");
                process = new Process {
                    StartInfo = new ProcessStartInfo {
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        FileName = @"/etc/java-se-7u75-ri/bin/java",
                        Arguments =
                            String.Join(' ',
                                "-Dawt.toolkit=ghostawt.GhostToolkit -Djava.awt.graphicsenv=ghostawt.image.GhostGraphicsEnvironment -Djava.awt.headless=false -Dsun.font.fontmanager=ghostawt.sun.GFontManager -Dsun.boot.library.path=/home/oryxbot/libs/jdk1.7.0_51/bin",
                                "-jar /home/oryxbot/apps/VncClient.jar",
                                "VncViewer HOST 10.0.0.100 PORT 5900 PASSWORD oryxbot \"\\\"Scaling\" \"factor\\\"\" auto \"\\\"Encoding\\\"\" rAW \"\\\"Show\" \"controls\\\"\" No \"\\\"JPEG\" image \"quality\\\"\" 9 \"\\\"Offer\" \"relogin\\\"\" No \"\\\"Restricted\" \"colors\\\"\" No \"\\\"Compression\" \"level\\\"\" 7"
                            ),
                        WorkingDirectory = "/home/oryxbot",
                        UseShellExecute = false
                    }
                };
                process.OutputDataReceived += OnVncOutputDataReceived;
                process.ErrorDataReceived += OnVncErrorDataReceived;
                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExitAsync().ContinueWith(task => {
                    processHasStarted = false;
                    FileLogger.Common.Info("Process exit code of VncClient: " + process.ExitCode);
                    if (process.ExitCode != 0) {
                        VncError?.Invoke(null, EventArgs.Empty);
                        if (!closing) { // If it stopped not gracefully, restart
                            StartVnc();
                        }
                    }
                });
                processHasStarted = true;
                
                var waitedFor = 0;
                var maxWaitFor = 15000;
                FileLogger.Common.Info($"Establishing connection to the customer VNC server");
                do {
                    Thread.Sleep(1000);
                    waitedFor += 1000;
                    FileLogger.Common.Info((waitedFor/1000)+"s since attempting to connect to customer VNC server");
                    
                    if (process.HasExited && process.ExitCode != 0) {
                        FileLogger.Common.Error(
                            $"The VNC client failed to connect to the VNC server. Aborting!");
                        return false;
                    }
                } while ((!Connected || Dimensions == null) && waitedFor < maxWaitFor);

                if (waitedFor >= maxWaitFor) {
                    FileLogger.Common.Error(
                        $"The VNC client was attempting to connect to the VNC server for {waitedFor}ms. Aborting!");
                    return false;
                }

                // Any more
                Thread.Sleep(200);
                
                
                ResponsivePoint.CurrentResolution = (
                    (int)(Vnc.Dimensions!.Value.x * (Vnc.ScalingPercent!.Value / 100d)),
                    (int)(Vnc.Dimensions!.Value.y * (Vnc.ScalingPercent!.Value / 100d))
                );;

                ConnectionEstablished?.Invoke(null, EventArgs.Empty);
                return true;
            } catch (Exception e) {
                processHasStarted = false;
                return false;
            }
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
                FileLogger.Common.Error($"Scaling desktop: {ScalingPercent}%");
            }
        }
        
        private static void OnVncErrorDataReceived(object sender, DataReceivedEventArgs e) {
            FileLogger.Common.Error("VNC client error: "+e.Data);
        }
    }
}
