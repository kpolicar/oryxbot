using System.Numerics;
using OryxBot.Shared.Design;

namespace OryxBot.Shared.Contracts
{
    public interface Input
    {
        void MoveCursorRelativeToCenter(Vector2 direction);
        void Click(ResponsivePoint point);
        void ShiftClick(ResponsivePoint point);
        void RightMouseDown();
        void RightMouseUp();
        void MoveCursor(ResponsivePoint point);
    }
}
