using System.Drawing;
using System.Numerics;
using OryxBot.Shared.Design;

namespace OryxBot.Shared.Contracts
{
    public interface Input
    {
        void MoveCursorRelativeToCenter(Vector2 direction);
        void Click();
        void Click(Point point);
        void Click(ResponsivePoint point);
        void DragAndDrop(ResponsivePoint point1, ResponsivePoint point2);
        void RightMouseDown();
        void RightMouseUp();
        void MoveCursor(ResponsivePoint point);
        void Key(char character);
    }
}
