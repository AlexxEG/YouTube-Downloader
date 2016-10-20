using System;
using System.Drawing;
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
            base.Render(g, r);

            r = this.GetProgressBarRect(r);

            string text = $"{this.Aspect}%";

            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;

            g.DrawString(text, this.Font, this.TextBrush, r, stringFormat);
        }

        public Rectangle GetProgressBarRect(Rectangle r)
        {
            r = this.ApplyCellPadding(r);

            Rectangle frameRect = Rectangle.Inflate(r, 0 - this.Padding, 0 - this.Padding);
            frameRect.Width = Math.Min(frameRect.Width, this.MaximumWidth);
            frameRect.Height = Math.Min(frameRect.Height, this.MaximumHeight);
            frameRect = this.AlignRectangle(r, frameRect);

            return frameRect;
        }
    }
}
