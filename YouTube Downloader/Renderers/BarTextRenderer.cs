using System;
using System.Drawing;
using System.Globalization;
using BrightIdeasSoftware;

namespace YouTube_Downloader.Renderers
{
    public class BarTextRenderer : BarRenderer
    {
        #region Constructors

        /// <summary>
        /// Make a BarTextRenderer
        /// </summary>
        public BarTextRenderer() : base() { }

        /// <summary>
        /// Make a BarTextRenderer for the given range of data values
        /// </summary>
        public BarTextRenderer(int minimum, int maximum) : base(minimum, maximum) { }

        /// <summary>
        /// Make a BarTextRenderer using a custom bar scheme
        /// </summary>
        public BarTextRenderer(Pen pen, Brush brush) : base(pen, brush) { }
        /// <summary>
        /// Make a BarTextRenderer using a custom bar scheme
        /// </summary>
        public BarTextRenderer(int minimum, int maximum, Pen pen, Brush brush) : base(minimum, maximum, pen, brush) { }

        /// <summary>
        /// Make a BarTextRenderer that uses a horizontal gradient
        /// </summary>
        public BarTextRenderer(Pen pen, Color start, Color end) : base(pen, start, end) { }

        /// <summary>
        /// Make a BarTextRenderer that uses a horizontal gradient
        /// </summary>
        public BarTextRenderer(int minimum, int maximum, Pen pen, Color start, Color end) : base(minimum, maximum, pen, start, end) { }

        #endregion

        public override void Render(Graphics g, Rectangle r)
        {
            this.MaximumWidth = r.Width;
            this.Padding = 1;

            base.Render(g, r);

            r = this.GetProgressBarRect(r);

            IConvertible convertable = this.Aspect as IConvertible;
            if (convertable == null)
                return;

            var text = Convert.ToString(convertable);
            
            Size textSize = g.MeasureString(text, this.Font).ToSize();
            // Text position, center of the progress bar rect
            Point textPoint = new Point(
                r.X + ((r.Width / 2) - (textSize.Width / 2)),
                r.Y + ((r.Height / 2) - (textSize.Height / 2)));
            // The area which is filled in progress bar
            Rectangle fill = this.GetFillRect(r);

            // Get the area of the text that is covered over by progress bar fill. This area will be white text
            int leftWidth = fill.Right - textPoint.X;

            var textBrush = new TextureBrush(GetTexureBrush(leftWidth, textSize.Width, textSize.Height));
            textBrush.TranslateTransform(textPoint.X, textPoint.Y);

            g.DrawString(text, this.Font, textBrush, textPoint);
        }

        private Rectangle GetFillRect(Rectangle r)
        {
            IConvertible convertable = this.Aspect as IConvertible;
            if (convertable == null)
                return Rectangle.Empty;
            double aspectValue = convertable.ToDouble(NumberFormatInfo.InvariantInfo);

            Rectangle fillRect = Rectangle.Inflate(r, -1, -1);
            if (aspectValue <= this.MinimumValue)
                fillRect.Width = 0;
            else if (aspectValue < this.MaximumValue)
                fillRect.Width = (int)(fillRect.Width * (aspectValue - this.MinimumValue) / this.MaximumValue);

            return fillRect;
        }

        private Rectangle GetProgressBarRect(Rectangle r)
        {
            r = this.ApplyCellPadding(r);

            Rectangle frameRect = Rectangle.Inflate(r, 0 - this.Padding, 0 - this.Padding);
            frameRect.Width = Math.Min(frameRect.Width, this.MaximumWidth);
            frameRect.Height = Math.Min(frameRect.Height, this.MaximumHeight);
            frameRect = this.AlignRectangle(r, frameRect);

            return frameRect;
        }

        private Image GetTexureBrush(int leftWidth, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bmp);

            var left = new Rectangle(0, 0, leftWidth, height);
            var right = new Rectangle(leftWidth, 0, width - leftWidth, height);

            g.FillRectangle(Brushes.White, left);
            g.FillRectangle(Brushes.Black, right);

            g.Dispose();

            return bmp;

        }
    }
}
