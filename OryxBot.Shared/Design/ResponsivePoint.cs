using System;
using System.Diagnostics;

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

        public int X =>  _anchorStyle switch {
            AnchorStyle.Left => ResolveXForLeftDock(),
            AnchorStyle.Right => ResolveXForRightDock(),
            _ => throw new NotImplementedException()
        };

        public int Y => (int)(_y / heightRatio());
        
        public ResponsivePoint(int x, int y, int screenWidth, int screenHeight, AnchorStyle anchorStyle) =>
            (_x, _y, _screenWidth, _screenHeight, _anchorStyle) = (x, y, screenWidth, screenHeight, anchorStyle);

        public float heightRatio() =>
            1f*_screenHeight / currentHeight;
        public float widthRatio() =>
            1f*_screenWidth / currentWidth;
        
        private int ResolveXForLeftDock() => (int)(_x / (widthRatio()/heightRatio()));

        private int ResolveXForRightDock() {
            
            var sw = _screenWidth / 1.052631578947368f;
            var ratio = 1-currentWidth/sw;
            return (int)(ratio * _x);
            
            var diffFromRight = _screenWidth - _x;
            var w = diffFromRight / widthRatio();
            return (int)(currentWidth-w);
        }
    }
}
