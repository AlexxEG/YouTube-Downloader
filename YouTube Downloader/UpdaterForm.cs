using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using YouTube_Downloader.Classes;
using Html = HtmlAgilityPack;

namespace YouTube_Downloader
{
    public partial class UpdaterForm : Form
    {
        private string FFmpegDownloadUrl = "";
        private DateTime FFmpegOnlineVersion = DateTime.MinValue;
        private string YouTubeDlDownloadUrl = "";
        private DateTime YouTubeDlOnlineVersion = DateTime.MinValue;

        public UpdaterForm()
        {
            InitializeComponent();
        }

        private void UpdaterForm_Load(object sender, EventArgs e)
        {
            new Thread(GetFFmpegVersion).Start();
            new Thread(GetYoutubeDlVersion).Start();
        }

        private void btnFFmpegInstall_Click(object sender, EventArgs e)
        {
            btnFFmpegInstall.Enabled = false;
            btnFFmpegInstall.Text = "Downloading...";

            new Thread(DownloadFFmpeg).Start();
        }

        private void btnYoutubeDlInstall_Click(object sender, EventArgs e)
        {
            btnYoutubeDlInstall.Enabled = false;
            btnYoutubeDlInstall.Text = "Downloading...";

            new Thread(DownloadYoutubeDl).Start();
        }

        private void DownloadFFmpeg()
        {
            WebClient client = new WebClient()
            {
                Proxy = null
            };

            string file = Path.Combine(Application.StartupPath, "ffmpeg.exe");
            string dest = FFmpegDownloadUrl.Substring(FFmpegDownloadUrl.LastIndexOf('/') + 1);

            dest = Path.Combine(Application.StartupPath, dest);

            client.DownloadProgressChanged += delegate(object sender, DownloadProgressChangedEventArgs e)
            {
                this.SetControlText(lFFmpegUpdateAvailable, string.Format("{0}/{1}", Helper.FormatFileSize(e.BytesReceived), Helper.FormatFileSize(e.TotalBytesToReceive)));
            };
            client.DownloadFileCompleted += delegate(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
            {
                /* Extract ffmpeg.exe from downloaded archive. */
                SevenZip.SevenZipExtractor.SetLibraryPath(Path.Combine(Application.StartupPath, "7za.dll"));

                using (var extractor = new SevenZip.SevenZipExtractor(dest))
                {
                    using (var stream = File.Open(Path.Combine(Application.StartupPath, "ffmpeg.exe"), FileMode.Create))
                    {
                        string ffmpeg = Path.Combine(Path.GetFileNameWithoutExtension(dest), "bin", "ffmpeg.exe");

                        extractor.ExtractFile(ffmpeg, stream);
                    }
                }

                File.Delete(dest);

                /* Set write time to online version to track current version. */
                File.SetLastWriteTime(file, FFmpegOnlineVersion);

                this.SetControlText(btnFFmpegInstall, "Done");
                this.SetControlText(lFFmpegInstalled, lFFmpegOnline.Text);
                this.SetControlVisible(lFFmpegUpdateAvailable, false);
            };

            client.DownloadFileAsync(new Uri(FFmpegDownloadUrl), dest);
        }

        private void DownloadYoutubeDl()
        {
            WebClient client = new WebClient()
            {
                Proxy = null
            };

            string file = Path.Combine(Application.StartupPath, "youtube-dl.exe");

            client.DownloadProgressChanged += delegate(object sender, DownloadProgressChangedEventArgs e)
            {
                this.SetControlText(lYoutubeDlUpdateAvailable, string.Format("{0}/{1}", Helper.FormatFileSize(e.BytesReceived), Helper.FormatFileSize(e.TotalBytesToReceive)));
            };
            client.DownloadFileCompleted += delegate(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
            {
                /* Set write time to online version to track current version. */
                File.SetLastWriteTime(file, YouTubeDlOnlineVersion);

                this.SetControlText(btnYoutubeDlInstall, "Done");
                this.SetControlText(lYoutubeDlInstalled, lYoutubeDlOnline.Text);
                this.SetControlVisible(lYoutubeDlUpdateAvailable, false);
            };

            client.DownloadFileAsync(new Uri(YouTubeDlDownloadUrl), file);
        }

        private string DownloadWebSource(string url)
        {
            return new WebClient()
            {
                Proxy = null
            }.DownloadString(url);
        }

        private void GetFFmpegVersion()
        {
            string file = Path.Combine(Application.StartupPath, "ffmpeg.exe");
            DateTime installed = DateTime.MinValue;

            if (File.Exists(file))
            {
                installed = File.GetLastWriteTime(Path.Combine(Application.StartupPath, "ffmpeg.exe"));

                this.SetControlText(lFFmpegInstalled, installed.ToShortDateString());
            }
            else
            {
                this.SetControlText(lFFmpegInstalled, "Not installed");
            }

            this.SetControlText(lFFmpegOnline, "Checking...");

            string url = "http://ffmpeg.zeranoe.com/builds/";
            Html.HtmlDocument document = new Html.HtmlDocument();

            document.LoadHtml(DownloadWebSource(url));

            /* Get date node. */
            string xpath = "/html/body/div[@id='container']/div[@class='latest_ver_title']";
            Html.HtmlNode node = document.DocumentNode.SelectSingleNode(xpath);

            /* Get date in string. */
            var regex = new Regex(@"\b\d{4}\-\d{2}-\d{2}\b");
            Match m = regex.Match(node.InnerText);

            DateTime online = DateTime.ParseExact(m.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            FFmpegOnlineVersion = online;

            this.SetControlText(lFFmpegOnline, online.ToShortDateString());

            /* Get download url. */
            xpath = "/html/body/div[@id='container']/div[@class='build_container']/a[@class='latest']";

            foreach (Html.HtmlNode foundNode in document.DocumentNode.SelectNodes(xpath))
            {
                /* Probably first one, but check anyway. */
                if (foundNode.InnerText.Contains("32-bit Static"))
                {
                    FFmpegDownloadUrl = url + foundNode.Attributes["href"].Value.Substring(2);
                    break;
                }
            }

            if (online > installed)
            {
                this.SetControlEnabled(btnFFmpegInstall, true);
                this.SetControlVisible(lFFmpegUpdateAvailable, true);
            }
        }

        private void GetYoutubeDlVersion()
        {
            string file = Path.Combine(Application.StartupPath, "youtube-dl.exe");
            DateTime installed = DateTime.MinValue;

            if (File.Exists(file))
            {
                installed = File.GetLastWriteTime(file);

                this.SetControlText(lYoutubeDlInstalled, installed.ToShortDateString());
            }
            else
            {
                this.SetControlText(lYoutubeDlInstalled, "Not installed");
            }

            this.SetControlText(lYoutubeDlOnline, "Checking...");

            string url = "http://rg3.github.io/youtube-dl/download.html";
            Html.HtmlDocument document = new Html.HtmlDocument();

            document.LoadHtml(DownloadWebSource(url));

            string xpath = "/html/body/h2/a[@href]";
            Html.HtmlNode node = document.DocumentNode.SelectSingleNode(xpath);

            /* Get date in string. */
            var regex = new Regex(@"\b\d{4}\.\d{2}.\d{2}\b");
            Match m = regex.Match(node.InnerText);

            DateTime online = DateTime.ParseExact(m.Value, "yyyy.MM.dd", CultureInfo.InvariantCulture);

            YouTubeDlOnlineVersion = online;

            this.SetControlText(lYoutubeDlOnline, online.ToShortDateString());

            /* Get download url. */
            xpath = "/html/body/p/a[contains(@href, '.exe')]";
            node = document.DocumentNode.SelectSingleNode(xpath);

            YouTubeDlDownloadUrl = node.Attributes["href"].Value;

            if (online > installed)
            {
                this.SetControlEnabled(btnYoutubeDlInstall, true);
                this.SetControlVisible(lYoutubeDlUpdateAvailable, true);
            }
        }

        delegate void SetControlEnabledDelegate(Control control, bool enabled);
        delegate void SetControlTextDelegate(Control control, string text);
        delegate void SetControlVisibleDelegate(Control control, bool visible);

        private void SetControlEnabled(Control control, bool enabled)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new SetControlEnabledDelegate(SetControlEnabled), new object[] { control, enabled });
            }
            else
            {
                control.Enabled = enabled;
            }
        }

        private void SetControlText(Control control, string text)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new SetControlTextDelegate(SetControlText), new object[] { control, text });
            }
            else
            {
                control.Text = text;
            }
        }

        private void SetControlVisible(Control control, bool visible)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new SetControlVisibleDelegate(SetControlVisible), new object[] { control, visible });
            }
            else
            {
                control.Visible = visible;
            }
        }
    }
}
