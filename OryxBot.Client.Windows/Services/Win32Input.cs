using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;
using OryxBot.Client.Windows.Bot;
using OryxBot.Client.Windows.Native;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;

namespace OryxBot.Client.Windows.Services
{
    public class Win32Input : Input
    {
        static Win32Input() {
            var successfulParse =
                int.TryParse(ConfigurationManager.AppSettings.Get("cursorMoveDuration")!, out MaxTimeToMoveCursor);
            if (!successfulParse)
                MaxTimeToMoveCursor = DefaultMaxTimeToMoveCursor;
        }
        
        private const int DefaultMaxTimeToMoveCursor = 300;
        private static readonly int MaxTimeToMoveCursor;
        
        private Task moveCursorTask = Task.CompletedTask;
        private Point cursorPosition;
        private Point cursorTargetPosition;
        private InputSimulator input = new ();

        public void MoveCursorRelativeToCenter(Vector2 direction) {
            var center = ResolveScreenCenter();
            center.Y -= 50;
            var (pixelX, pixelY) = ((int) (direction.X * 150), -(int) (direction.Y * 150));

            var (targetX, targetY) = (center.X + pixelX, center.Y + pixelY);
            var newCursorPosition = new Point(targetX, targetY);

            cursorTargetPosition = newCursorPosition;
            EnsureCursorMoveTaskIsRunning();
        }

        public void Click() {
            moveCursorTask.Wait();
            input.Mouse.LeftButtonClick();
        }

        public void Key(char character) {
            if (character == 's')
                input.Keyboard.KeyPress(VirtualKeyCode.VK_S);
        }

        public void MoveCursor(ResponsivePoint point) {
            if (cursorTargetPosition == new Point(point.X, point.Y))
                cursorPosition = new Point(point.X, point.Y+1);
            cursorTargetPosition = new Point(point.X, point.Y);
            EnsureCursorMoveTaskIsRunning();
        }

        public void ShiftClick(ResponsivePoint point) {
            MoveCursor(point);
            moveCursorTask.Wait();
            
            input.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
            Thread.Sleep(150);
            input.Mouse.LeftButtonDown();
            Thread.Sleep(150);
            input.Mouse.LeftButtonUp();
            Thread.Sleep(150);
            input.Mouse.LeftButtonDown();
            Thread.Sleep(150);
            input.Mouse.LeftButtonUp();
            Thread.Sleep(150);
            input.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
        }

        public void Click(ResponsivePoint point) {
            MoveCursor(point);
            Click();
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

        public Point ResolveScreenCenter() =>
            new(AlbionInterface.Character.X, AlbionInterface.Character.Y);
    }
}
