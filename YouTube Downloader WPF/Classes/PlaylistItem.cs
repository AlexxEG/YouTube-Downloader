using System.ComponentModel;
using System.Runtime.CompilerServices;
using YouTube_Downloader_WPF.Classes;

namespace YouTube_Downloader_WPF
{
    public class PlaylistItem : INotifyPropertyChanged
    {
        private string _duration;
        private bool _selected;
        private string _title;

        public string Duration
        {
            get { return _duration; }
            set
            {
                _duration = value;
                this.OnPropertyChanged();
            }
        }
        public bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                this.OnPropertyChanged();
            }
        }
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                this.OnPropertyChanged();
            }
        }

        public VideoInfo VideoInfo { get; set; }

        public PlaylistItem(VideoInfo video)
        {
            this.Duration = Helper.FormatVideoLength(video.Duration);
            this.Selected = true;
            this.Title = video.Title;
            this.VideoInfo = video;
        }

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            OnPropertyChangedExplicit(propertyName);
        }

        private void OnPropertyChangedExplicit(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
