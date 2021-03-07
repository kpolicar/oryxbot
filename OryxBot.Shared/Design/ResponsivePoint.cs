using System;

namespace OryxBot.Shared.Design
{
    public readonly struct ResponsivePoint
    {
        [Flags]
        public enum AnchorStyle
        {
            Top = 0x01,
            Bottom = 0x02,
            Left = 0x04,
            Right = 0x08,
            None = 0,
        }
        
        private readonly int _x;
        private readonly int _y;
        private readonly int _screenWidth;
        private readonly int _screenHeight;

        private readonly int currentWidth => 1920;
        private readonly int currentHeight => 1080;

        private readonly AnchorStyle _anchorStyle;

        public int X => _x;
        public int Y => _y;

        public ResponsivePoint(int x, int y, int screenWidth, int screenHeight, AnchorStyle anchorStyle) =>
            (_x, _y, _screenWidth, _screenHeight, _anchorStyle) = (x, y, screenWidth, screenHeight, anchorStyle);
    }
}
