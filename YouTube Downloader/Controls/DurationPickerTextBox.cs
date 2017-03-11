using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YouTube_Downloader.Controls
{
    public class DurationPickerTextBox : TextBox
    {
        [DefaultValue(0)]
        public int MaxDuration { get; set; }

        int prevLength;
        string prevText;

        public DurationPickerTextBox() : base()
        {
            prevLength = this.Text.Length;
            prevText = this.Text;
        }

        protected override void OnGotFocus(EventArgs e)
        {
            this.SelectAll();

            base.OnGotFocus(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    {
                        int newValue = int.Parse(this.Text) + 1;
                        int max = MaxDuration > 0 ? MaxDuration : 0;
                        this.Text = Math.Min(newValue, max).ToString();
                        break;
                    }
                case Keys.Down:
                    {
                        int newValue = (int.Parse(this.Text) - 1);
                        this.Text = Math.Max(newValue, 0).ToString();
                        break;
                    }
                case Keys.Back:
                    if (string.IsNullOrEmpty(this.Text))
                    {
                        var parent = this.Parent as DurationPicker;
                        parent.SelectPrevious(this, false);
                    }
                    break;
                case Keys.Left:
                    if (this.SelectionStart == 0)
                        (this.Parent as DurationPicker).SelectPrevious(this, false);
                    break;
                case Keys.Right:
                    if (this.SelectionStart == this.TextLength)
                        (this.Parent as DurationPicker).SelectNext(this, false);
                    break;
            }

            base.OnKeyUp(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }

            base.OnKeyPress(e);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            if (this.MaxDuration <= 0)
            {
                base.OnTextChanged(e);
                return;
            }

            if (string.IsNullOrEmpty(this.Text))
            {
                base.OnTextChanged(e);
                return;
            }

            if (int.Parse(this.Text) > this.MaxDuration)
            {
                int ss = this.SelectionStart;
                this.Text = prevText;
                this.SelectionStart = ss - 1;
            }
            else
            {
                if (this.SelectionStart == 2)
                {
                    var parent = this.Parent as DurationPicker;
                    parent.SelectNext(this, false);
                }

                base.OnTextChanged(e);
            }

            prevText = this.Text;
        }
    }
}
