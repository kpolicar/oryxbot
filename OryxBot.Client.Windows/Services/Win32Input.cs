using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OryxBot.Bot;
using OryxBot.Client.Windows.Native;
using OryxBot.Shared.Contracts;

namespace OryxBot.Client.Windows.Services
{
    public class Win32Input : Input
    {
        private const int MaxTimeToMoveCursor = 200;
        private Task moveCursorTask = Task.CompletedTask;
        private Point cursorPosition;
        private Point cursorTargetPosition;

        public void MoveCursorRelativeToCenter(Vector2 direction) {
            var center = ResolveScreenCenter();
            var (pixelX, pixelY) = ((int) (direction.X * 300), -(int) (direction.Y * 300));

            var (targetX, targetY) = (center.X + pixelX, center.Y + pixelY);
            var newCursorPosition = new Point(targetX, targetY);

            cursorTargetPosition = newCursorPosition;
            EnsureCursorMoveTaskIsRunning();
        }

        private void EnsureCursorMoveTaskIsRunning() {
            if (!moveCursorTask.IsCompleted)
                return;
            
            moveCursorTask = Task.Run(() => {
                var sw = new Stopwatch();
                sw.Start();
                var previousCursorTargetPosition = cursorTargetPosition;
                
                while (cursorPosition != cursorTargetPosition) {
                    if (cursorTargetPosition != previousCursorTargetPosition) {
                        previousCursorTargetPosition = cursorTargetPosition;
                        sw.Restart();
                    }

                    var step = System.Math.Min(1F, sw.ElapsedMilliseconds / (float)MaxTimeToMoveCursor);
                    var stepToTargetPosition = Math.Lerp(cursorPosition, cursorTargetPosition, step);
                    
                    User32.SetCursorPos(cursorPosition.X, cursorPosition.Y);
                    cursorPosition = stepToTargetPosition;

                    Thread.Sleep(5);
                }
                
                sw.Stop();
            });
        }
        
        public void RightMouseDown() {
            var input = new User32.Input {
                Type = User32.InputMouse,
                MouseInput = new User32.MouseInput {
                    Flags = User32.MouseEventRightDown
                }
            };
            var inputs = new[] {input};
            var result = User32.SendInput(1, inputs, Marshal.SizeOf(input));
            if(result == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public void RightMouseUp() {
            var input = new User32.Input {
                Type = User32.InputMouse,
                MouseInput = new User32.MouseInput {
                    Flags = User32.MouseEventRightUp
                }
            };
            var inputs = new[] {input};
            var result = User32.SendInput(1, inputs, Marshal.SizeOf(input));
            if(result == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public Point ResolveScreenCenter() {
            var dimensions = ResolveScreenDimensions();
            return new Point(dimensions.X/2, dimensions.Y/2);
        }

        public Point ResolveScreenDimensions() =>
            new(
                User32.GetSystemMetrics(User32.SM_CXFULLSCREEN), 
                User32.GetSystemMetrics(User32.SM_CYFULLSCREEN));
    }
}
