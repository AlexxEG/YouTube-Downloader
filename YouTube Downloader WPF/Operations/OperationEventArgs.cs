using System;

namespace YouTube_Downloader_WPF.Operations
{
    public class OperationEventArgs : EventArgs
    {
        public Operation Operation { get; set; }
        public OperationStatus Status { get; set; }

        public OperationEventArgs(Operation operation, OperationStatus status)
        {
            this.Operation = operation;
            this.Status = status;
        }
    }
}
