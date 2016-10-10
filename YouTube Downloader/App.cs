using Microsoft.VisualBasic.ApplicationServices;
using YouTube_Downloader.Properties;
using YouTube_Downloader_DLL.Helpers;

namespace YouTube_Downloader
{
    public class App : WindowsFormsApplicationBase
    {
        public App()
        {
            this.IsSingleInstance = true;
            this.EnableVisualStyles = true;
            this.ShutdownStyle = ShutdownMode.AfterMainFormCloses;
        }

        protected override void OnCreateMainForm()
        {
            string[] args = new string[this.CommandLineArgs.Count];

            this.CommandLineArgs.CopyTo(args, 0);
            
            DownloadQueueHandler.LimitDownloads = Settings.Default.ShowMaxSimDownloads;
            DownloadQueueHandler.StartWatching(Settings.Default.MaxSimDownloads);

            if (this.CommandLineArgs.Count > 0)
                this.MainForm = new MainForm(args);
            else
                this.MainForm = new MainForm();
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            eventArgs.BringToForeground = true;
            base.OnStartupNextInstance(eventArgs);
            if (eventArgs.CommandLine.Count > 0)
            {
                MainForm mainForm = (MainForm)this.MainForm;
                mainForm.InsertVideo(eventArgs.CommandLine[0]);
            }
        }

        protected override void OnShutdown()
        {
            DownloadQueueHandler.Stop();
            base.OnShutdown();
        }
    }
}
