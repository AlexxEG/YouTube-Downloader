using System;
using System.Windows.Forms;

namespace YouTube_Downloader.Operations
{
    public class OperationEventArgs : EventArgs
    {
        public ListViewItem Item { get; set; }
        public OperationStatus Status { get; set; }

        public OperationEventArgs(ListViewItem item, OperationStatus status)
        {
            this.Item = item;
            this.Status = status;
        }
    }
}
