using System.Threading;

namespace System.ComponentModel
{
    public class AbortableBackgroundWorker : BackgroundWorker
    {
        private Thread workerThread;

        public object Tag { get; set; }

        public AbortableBackgroundWorker()
        {
            this.WorkerSupportsCancellation = true;
            this.WorkerReportsProgress = true;
        }

        /// <summary>
        /// Kill the background thread
        /// </summary>
        public void Abort()
        {
            if (!IsBusy)
                return;

            this.CancelAsync();

            try
            {
                if (workerThread != null)
                {
                    workerThread.Abort();
                    workerThread = null;
                }
            }
            catch { }
        }

        /// <summary>
        /// Start work in current thread
        /// </summary>
        /// <param name="objectState"></param>
        public void RunWork(object objectState)
        {
            DoWorkEventArgs args = new DoWorkEventArgs(objectState);
            Exception eee = null;

            try
            {
                OnDoWork(args);
            }
            catch (Exception ex)
            {
                eee = ex;
            }

            OnRunWorkerCompleted(new RunWorkerCompletedEventArgs(args.Result, eee, args.Cancel));
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            workerThread = Thread.CurrentThread;

            try
            {
                base.OnDoWork(e);
                workerThread = null;
            }
            catch (ThreadAbortException)
            {
                e.Cancel = true; //We must set Cancel property to true!
                Thread.ResetAbort(); //Prevents ThreadAbortException propagation
            }
        }
    }
}