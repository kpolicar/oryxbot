using System.Diagnostics;
using System.Drawing;
using System.Numerics;
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
                Debug.WriteLine("not close enough");
                return;
            }

            User32.SetCursorPos(targetX, targetY);
            cursorPosition = newCursorPosition;
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
