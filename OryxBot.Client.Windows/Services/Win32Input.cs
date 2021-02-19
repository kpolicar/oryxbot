using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OryxBot.Bot;
using OryxBot.Client.Windows.Native;
using OryxBot.Shared.Contracts;

namespace OryxBot.Client.Windows.Services
{
    public class Win32Input : Input
    {
        private Point cursorPosition;

        public void MoveCursorRelativeToCenter(Vector2 direction) {
            var center = ResolveScreenCenter();
            var (pixelX, pixelY) = ((int) (direction.X * 300), -(int) (direction.Y * 300));

            var (targetX, targetY) = (center.X + pixelX, center.Y + pixelY);
            var newCursorPosition = new Point(targetX, targetY);

            var distanceFromPreviousCursor = Vector2.Distance(
                new Vector2(newCursorPosition.X, newCursorPosition.Y),
                new Vector2(cursorPosition.X, cursorPosition.Y));

            if (distanceFromPreviousCursor <= 10) {
                return;
            }

            User32.SetCursorPos(targetX, targetY);
            cursorPosition = newCursorPosition;
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
