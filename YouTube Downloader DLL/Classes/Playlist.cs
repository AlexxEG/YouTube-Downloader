using System.Collections.Generic;

namespace YouTube_Downloader_DLL.Classes
{
    public class Playlist
    {
        /// <summary>
        /// Gets the play list ID.
        /// </summary>
        public string ID { get; private set; }
        /// <summary>
        /// Gets the playlist name.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Gets the expected video count. Expected because some videos might not be included because of errors.
        /// Look at 'Playlist.Videos' property for actual count.
        /// </summary>
        public int OnlineCount { get; set; }
        /// <summary>
        /// Gets the videos in the playlist. Videos with errors not included, for example country restrictions.
        /// </summary>
        public ICollection<VideoInfo> Videos { get; private set; }

        public Playlist(string id, string name, int onlineCount, ICollection<VideoInfo> videos)
        {
            this.ID = id;
            this.Name = name;
            this.OnlineCount = onlineCount;
            this.Videos = videos;
        }
    }
}
