using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YouTube_Downloader.Controls
{
    public partial class DurationPicker : UserControl
    {
        public TimeSpan Duration
        {
            get
            {
                int hours = int.Parse(Hours.Text);
                int minutes = int.Parse(Minutes.Text);
                int seconds = int.Parse(Seconds.Text);

                return new TimeSpan(hours, minutes, seconds);
            }
        }

        public DurationPicker()
        {
            InitializeComponent();
        }

        public void SelectNext(DurationPickerTextBox dptxt, bool loop)
        {
            if (dptxt == Hours)
            {
                Minutes.Focus();
            }
            else if (dptxt == Minutes)
            {
                Seconds.Focus();
            }
            else if (dptxt == Seconds)
            {
                if (loop)
                    Hours.Focus();
            }
        }

        public void SelectPrevious(DurationPickerTextBox dptxt, bool loop)
        {
            if (dptxt == Hours)
            {
                if (loop)
                    Seconds.Focus();
            }
            else if (dptxt == Minutes)
            {
                Hours.Focus();
            }
            else if (dptxt == Seconds)
            {
                Minutes.Focus();
            }
        }
    }
}
