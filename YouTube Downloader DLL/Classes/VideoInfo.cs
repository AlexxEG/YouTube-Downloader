﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Newtonsoft.Json.Linq;
using YouTube_Downloader_DLL.Enums;

namespace YouTube_Downloader_DLL.Classes
{
    public class VideoInfo : INotifyPropertyChanged
    {
        long _duration = 0;
        string _title = string.Empty;
        string _thumbnailUrl = string.Empty;

        /// <summary>
        /// Gets or sets the playlist index. Default value is -1.
        /// </summary>
        public int PlaylistIndex { get; set; } = -1;

        /// <summary>
        /// Gets the video duration in seconds.
        /// </summary>
        public long Duration
        {
            get { return _duration; }
            set
            {
                _duration = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets whether there was a failure retrieving video information.
        /// </summary>
        public bool Failure { get; set; }

        /// <summary>
        /// Gets or sets whether authentication is required to get video information.
        /// </summary>
        public bool RequiresAuthentication { get; set; }

        /// <summary>
        /// Gets or sets the reason for failure retrieving video information.
        /// </summary>
        public string FailureReason { get; set; }

        /// <summary>
        /// Gets or sets the video ID.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Gets the video source (Twitch/YouTube).
        /// </summary>
        public VideoSource VideoSource { get; private set; }

        /// <summary>
        /// Gets the video title.
        /// </summary>
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the video thumbnail url.
        /// </summary>
        public string ThumbnailUrl
        {
            get { return _thumbnailUrl; }
            set
            {
                _thumbnailUrl = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the video url.
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// Gets all the available formats.
        /// </summary>
        public List<VideoFormat> Formats { get; private set; }

        /// <summary>
        /// Occurs when one of the format's file size has been updated.
        /// </summary>
        public event FileSizeUpdateHandler FileSizeUpdated;

        public VideoInfo()
        {
            this.Formats = new List<VideoFormat>();
        }

        public VideoInfo(string json_file)
            : this()
        {
            this.DeserializeJson(json_file);
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
            this.FileSizeUpdated?.Invoke(this, new FileSizeUpdateEventArgs(videoFormat));
        }

        public void DeserializeJson(string json_file)
        {
            string raw_json = ReadJSON(json_file);
            JObject json = JObject.Parse(raw_json);

            this.Duration = json.Value<long>("duration");
            this.Title = json.Value<string>("fulltitle");
            this.ID = json.Value<string>("id");

            string displayId = json.Value<string>("display_id");

            // Get thumbnail
            if (json.Value<string>("extractor") == "twitch:vod")
            {
                this.VideoSource = VideoSource.Twitch;
                this.ThumbnailUrl = json.Value<string>("thumbnail");
            }
            else
            {
                this.VideoSource = VideoSource.YouTube;
                // Don't use thumbnail from .json as this fits better
                this.ThumbnailUrl = string.Format("https://i.ytimg.com/vi/{0}/mqdefault.jpg", displayId);
            }

            this.Url = json.Value<string>("webpage_url");

            foreach (JToken token in (JArray)json["formats"])
            {
                this.Formats.Add(new VideoFormat(this, token));
            }
        }

        private static string ReadJSON(string json_file)
        {
            string json = string.Empty;

            // Should try for about 10 seconds. */
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
                    if (ex is FileNotFoundException || ex.Message.EndsWith("because it is being used by another process."))
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

            return json;
        }

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            OnPropertyChangedExplicit(propertyName);
        }

        private void OnPropertyChangedExplicit(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
