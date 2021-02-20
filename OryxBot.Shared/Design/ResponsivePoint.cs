namespace OryxBot.Shared.Design
{
    public readonly struct ResponsivePoint
    {
        private readonly int _x;
        private readonly int _y;
        private readonly int _screenWidth;
        private readonly int _screenHeight;
        
        public int X => _x;
        public int Y => _x;
        
        public ResponsivePoint(int x, int y, int screenWidth, int screenHeight) =>
            (_x, _y, _screenWidth, _screenHeight) = (x, y, screenWidth, screenHeight);
    }
}
