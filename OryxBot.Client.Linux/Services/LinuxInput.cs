using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using OryxBot.Client.Linux.Bot;
using OryxBot.Client.Linux.Native;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;

namespace OryxBot.Client.Linux.Services
{
    public class LinuxInput : Input
    {
        static LinuxInput() {
            MaxTimeToMoveCursor = DefaultMaxTimeToMoveCursor;
        }
        
        private const int DefaultMaxTimeToMoveCursor = 100;
        private static readonly int MaxTimeToMoveCursor;
        
        private Task moveCursorTask = Task.CompletedTask;
        private Point cursorPosition;
        private Point cursorTargetPosition;

        public void MoveCursorRelativeToCenter(Vector2 direction) {
            var center = ResolveScreenCenter();
            var offsetFromCenter = ResponsivePoint.CurrentScreenHeight / 10;
            
            var (pixelX, pixelY) =(
                (int) (direction.X * offsetFromCenter),
                -(int) (direction.Y * offsetFromCenter));

            var (targetX, targetY) = (center.X + pixelX, center.Y + pixelY);
            var newCursorPosition = new Point(targetX, targetY);

            cursorTargetPosition = newCursorPosition;
            InputCommands.SetCursorPos(cursorTargetPosition.X, cursorTargetPosition.Y);
        }

        public void Click() {
            moveCursorTask.Wait();
            InputCommands.LeftButtonClick();
        }

        public void Key(char character) {
            if (character == 's')
                InputCommands.Key("s");
        }

        public void MoveCursor(ResponsivePoint point) =>
            MoveCursor(point.X, point.Y);

        public void MoveCursor(Point point) =>
            MoveCursor(point.X, point.Y);

        private void MoveCursor(int x, int y) {
            if (cursorTargetPosition == new Point(x, y))
                cursorPosition = new Point(x, y+1);
            cursorTargetPosition = new Point(x, y);
            
            InputCommands.SetCursorPos(cursorTargetPosition.X, cursorTargetPosition.Y);
        }

        public void DragAndDrop(ResponsivePoint point1, ResponsivePoint point2) {
            MoveCursor(point1);
            Thread.Sleep(300);
            
            InputCommands.LeftButtonDown();
            Thread.Sleep(300);
            
            MoveCursor(point2);
            Thread.Sleep(300);
            
            InputCommands.LeftButtonUp();
        }
        
        public void Click(Point point) {
            MoveCursor(point);
            Click();
        }

        public void Click(ResponsivePoint point) {
            MoveCursor(point);
            Click();
        }

        private void EnsureCursorMoveTaskIsRunning() {
            if (!moveCursorTask.IsCompleted)
                return;
            
            moveCursorTask = Task.Run(async () => {
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
                    
                    InputCommands.SetCursorPos(cursorPosition.X, cursorPosition.Y);
                    cursorPosition = stepToTargetPosition;

                    await Task.Delay(5);
                }
                
                sw.Stop();
            });
        }

        public void RightMouseDown() =>
            InputCommands.RightButtonDown();

        public void RightMouseUp() =>
            InputCommands.RightButtonUp();

        public Point ResolveScreenCenter() =>
            new(AlbionInterface.Character.X, AlbionInterface.Character.Y);
    }
}
