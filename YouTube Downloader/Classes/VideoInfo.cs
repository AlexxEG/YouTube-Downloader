using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace YouTube_Downloader.Classes
{
    public class VideoInfo
    {
        public long Duration { get; set; }
        public string ID { get; set; }
        public string Title { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Url { get; set; }
        public IList<VideoFormat> Formats { get; set; }

        public event FileSizeUpdateHandler FileSizeUpdated;

        public VideoInfo()
        {
            this.Formats = new List<VideoFormat>();
        }

        /// <summary>
        /// Aborts all the requests for file size for each video format.
        /// </summary>
        public void AbortUpdateFileSizes()
        {
            foreach (VideoFormat format in this.Formats)
            {
                format.AbortUpdateFileSize();
            }
        }

        internal void OnFileSizeUpdated(VideoFormat videoFormat)
        {
            if (this.FileSizeUpdated != null)
                this.FileSizeUpdated(this, new FileSizeUpdateEventArgs(videoFormat));
        }

        public static VideoInfo DeserializeJson(string json_file)
        {
            /* Parse JSON */
            VideoInfo info = new VideoInfo();

            string json = "";

            /* Should try for about 10 seconds. */
            int attempts = 0; int maxAttempts = 20;

            while ((attempts++) <= maxAttempts)
            {
                try
                {
                    json = File.ReadAllText(json_file);
                    break;
                }
                catch (IOException ex)
                {
                    if (ex.Message.EndsWith("because it is being used by another process."))
                    {
                        Console.WriteLine(ex);
                        Thread.Sleep(500);
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }

            JObject jObject = JObject.Parse(json);

            info.Duration = long.Parse(jObject["duration"].ToString());
            info.Title = jObject["fulltitle"].ToString();

            string displayId = jObject["display_id"].ToString();

            info.ThumbnailUrl = string.Format("https://i.ytimg.com/vi/{0}/mqdefault.jpg", displayId);
            info.Url = jObject["webpage_url"].ToString();

            JArray array = (JArray)jObject["formats"];

            foreach (JToken token in array)
            {
                VideoFormat format = new VideoFormat(info);

                JToken format_note = token.SelectToken("format_note");

                if (format_note != null && format_note.ToString().Contains("DASH"))
                    format.DASH = true;

                format.DownloadUrl = token["url"].ToString();
                format.Extension = token["ext"].ToString();
                format.Format = token["format"].ToString();

                // Check for 60fps videos. If there is no 'fps' token, default to 30fps.
                JToken fps = token.SelectToken("fps", false);

                format.FPS = fps == null ? "30" : fps.ToString();
                format.UpdateFileSizeAsync();

                info.Formats.Add(format);
            }

            return info;
        }
    }
}
