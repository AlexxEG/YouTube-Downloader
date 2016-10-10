using System;

namespace YouTube_Downloader_DLL.Operations
{
    public class StatusChangedEventArgs : EventArgs
    {
        public Operation Operation { get; private set; }
        public OperationStatus NewStatus { get; private set; }
        public OperationStatus OldStatus { get; private set; }

        public StatusChangedEventArgs(Operation operation, OperationStatus newStatus, OperationStatus oldStatus)
        {
            this.Operation = operation;
            this.NewStatus = newStatus;
            this.OldStatus = oldStatus;
        }
    }
}
