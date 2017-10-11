using System.Collections.Generic;
using System.Windows.Forms;

namespace YouTube_Downloader.Dialogs
{
    public class BatchDownloadResult
    {
        public DialogResult Result { get; private set; }
        public ICollection<string> Inputs { get; private set; }

        public BatchDownloadResult(DialogResult result, ICollection<string> inputs)
        {
            this.Result = result;
            this.Inputs = inputs;
        }
    }
}
