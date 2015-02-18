using System.Windows.Forms;

namespace YouTube_Downloader.Delegates
{
    /// <summary>
    /// Used to invoke text change on ListViewItem.ListViewSubItem.
    /// </summary>
    public delegate void SetItemTextDelegate(ListViewItem.ListViewSubItem item, string text);
}
