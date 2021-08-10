using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace OryxBot.Client.Linux.Native
{
    public static class Anydesk
    {
        public static void CloseAnydesk() {
            Console.WriteLine("Closing Anydesk");
            
            var process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "bash",
                    Arguments = "-c \"xdotool search --name anydesk windowkill %@\"",
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
                    WindowStyle = ProcessWindowStyle.Maximized,
                    FileName = @"anydesk",
                    Arguments = "kpolicar@ad --silent --fullscreen",
                    UseShellExecute = false,
                }
            };
            process.Start();
        }
    }
}
