using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;
using OryxBot.Bot;
using OryxBot.Client.Windows.Native;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;

namespace OryxBot.Client.Windows.Services
{
    public class Win32Input : Input
    {
        private const int MaxTimeToMoveCursor = 200;
        private Task moveCursorTask = Task.CompletedTask;
        private Point cursorPosition;
        private Point cursorTargetPosition;
        private InputSimulator input = new ();

        public void MoveCursorRelativeToCenter(Vector2 direction) {
            var center = ResolveScreenCenter();
            var (pixelX, pixelY) = ((int) (direction.X * 300), -(int) (direction.Y * 300));

            var (targetX, targetY) = (center.X + pixelX, center.Y + pixelY);
            var newCursorPosition = new Point(targetX, targetY);

            cursorTargetPosition = newCursorPosition;
            EnsureCursorMoveTaskIsRunning();
        }

        public void MoveCursor(ResponsivePoint point) {
            cursorTargetPosition = new Point(point.X, point.Y);
            EnsureCursorMoveTaskIsRunning();
        }

        public void ShiftClick(ResponsivePoint point) {
            MoveCursor(point);
            moveCursorTask.Wait();
            
            input.Keyboard.KeyDown(VirtualKeyCode.LSHIFT);
            input.Mouse.LeftButtonClick();
            input.Keyboard.KeyUp(VirtualKeyCode.LSHIFT);
        }

        public void Click(ResponsivePoint point) {
            MoveCursor(point);
            moveCursorTask.Wait();
            
            input.Mouse.LeftButtonClick();
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

        public void RightMouseDown() =>
            input.Mouse.RightButtonDown();

        public void RightMouseUp() =>
            input.Mouse.RightButtonUp();

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
