using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Rect = System.Drawing.Rectangle;

namespace OryxBot.Client.Windows.Controls
{
    public partial class CursorIndicator : UserControl
    {
        
        public CursorIndicator()
        {
            InitializeComponent();
            Paint += Rectangle_Paint;
            ResizeRedraw = true;
            Rectangle_Paint(this, EventArgs.Empty);
        }

        private void Rectangle_Paint(object sender, EventArgs e)
        {
            var path = new GraphicsPath();
            path.AddRectangle(new Rectangle(
                new Point(Width/2-2, 0), new Size(4, Height)));
            path.AddRectangle(new Rectangle(
                new Point(0, Height/2-2), new Size(Width, 4)));
            Region = new Region(path);
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
            x -= width/2;
            y -= height/2;
            base.SetBoundsCore(x, y, width, height, specified);
        }
    }
}

