using System;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Exceptions;

namespace OryxBot.Shared.Design
{
    public readonly struct ResponsivePoint
    {
        
        [Flags]
        public enum AnchorStyle
        {
            Left,
            Right,
            Center
        }
        
        private readonly double _ratioX;
        private readonly double _ratioY;
        private readonly int _x;
        private readonly int _y;
        private readonly int _screenWidth;
        private readonly int _screenHeight;
        private readonly double _screenRatio => 1d*_screenWidth/_screenHeight;

        public static event EventHandler? ResolutionChanged;
        public static (int width, int height) CurrentResolution {
            set {
                (CurrentScreenWidth, CurrentScreenHeight) = (value.width, value.height);
                ResolutionChanged?.Invoke(null, EventArgs.Empty);
            }
            get => (CurrentScreenWidth, CurrentScreenHeight);
        }
            
        private static int? _currentScreenWidth;
        public static int CurrentScreenWidth {
            get => _currentScreenWidth ?? throw new UnknownScreenResolutionExtension();
            private set => _currentScreenWidth = value;
        }
        private static int? _currentScreenHeight;
        public static int CurrentScreenHeight {
            get => _currentScreenHeight?? throw new UnknownScreenResolutionExtension();
            private set => _currentScreenHeight = value;
        }
        
        private double _currentScreenRatio => 1d*CurrentScreenWidth/CurrentScreenHeight;
        private readonly int _currentScreenXOffset => (int) ((_screenRatio / _currentScreenRatio) * (_ratioX * CurrentScreenWidth));

        private readonly AnchorStyle _anchorStyle;

        public int X => _anchorStyle switch {
            AnchorStyle.Left => _currentScreenXOffset,
            AnchorStyle.Right => CurrentScreenWidth - _currentScreenXOffset,
            AnchorStyle.Center => CurrentScreenWidth/2 - _currentScreenXOffset,
            _ => throw new ArgumentOutOfRangeException(nameof(_anchorStyle), _anchorStyle, null)
        };
        public int Y => (int) (_ratioY * CurrentScreenHeight);

        public ResponsivePoint(int x, int y, int screenWidth, int screenHeight, AnchorStyle anchorStyle) =>
            (_x, _y, _screenWidth, _screenHeight, _anchorStyle, _ratioX, _ratioY) =
            (x, y, screenWidth, screenHeight, anchorStyle, CalculateRatioX(x, screenWidth, anchorStyle), 1d*y/screenHeight);

        
        private static double CalculateRatioX(double a, int width, AnchorStyle anchor) =>
            anchor switch {
                AnchorStyle.Left => 1d*a/width,
                AnchorStyle.Right => 1d*(width-a)/width,
                AnchorStyle.Center => 1d*((int)(1d*width/2)-a)/width,
                _ => throw new ArgumentOutOfRangeException(nameof(anchor), anchor, null)
            };
    }
}
