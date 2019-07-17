using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using FeedReader;
using QuestomAssets;

namespace BeatOn
{
    /// <summary>
    /// Overall configuration for configuration of the SyncManager
    /// </summary>
    public class SyncConfig : INotifyPropertyChanged
    {
        public SyncConfig()
        {
            FeedReaders = new ObservableCollection<FeedConfig>();
        }

        /// <summary>
        /// BeastSaber username
        /// </summary>
        public string BeastSaberUsername { get; set; }

        /// <summary>
        /// The last date/time that a sync was run
        /// </summary>
        public DateTime? LastSyncDate { get; set; }

        /// <summary>
        /// True to check all existing songs for any updates to the song itself.  This will likely be extremely slow.
        /// </summary>
        public bool CheckExistingSongsUpdated { get; set; } = false;

        private ObservableCollection<FeedConfig> _feedReaders;

       

        /// <summary>
        /// List of definitions and settings for feed readers that will sync.
        /// </summary>
        public ObservableCollection<FeedConfig> FeedReaders
        {
            get
            {
                return _feedReaders;
            }
            set
            {
                if (value != _feedReaders)
                {
                    if (_feedReaders != null)
                        _feedReaders.CollectionChanged -= _feedReaders_CollectionChanged;

                    _feedReaders = value;

                    if (value != null)
                        _feedReaders.CollectionChanged += _feedReaders_CollectionChanged;

                    PropChanged(nameof(FeedReaders));
                }

            }
        }

        private void _feedReaders_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var oi in e.OldItems)
                {
                    var f = oi as FeedConfig;
                    f.PropertyChanged -= feedReader_PropertyChanged;
                }
            }
            if (e.NewItems != null)
            {
                foreach (var ni in e.NewItems)
                {
                    var f = ni as FeedConfig;
                    f.PropertyChanged += feedReader_PropertyChanged;
                }
            }
        }

        private void feedReader_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropChanged(nameof(FeedReaders));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void PropChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }

    public abstract class FeedConfig : INotifyPropertyChanged
    {
        private Guid _id = Guid.NewGuid();
        public Guid ID
        {
            get
            {
                return _id;
            }
            set
            {
                bool changed = value != _id;
                _id = value;
                if (changed)
                    PropChanged(nameof(ID));
            }
        }

        private bool _isEnabled;

        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                bool changed = value != _isEnabled;
                _isEnabled = value;
                if (changed)
                    PropChanged(nameof(IsEnabled));
            }
        }

        protected abstract IFeedReader FeedReader { get; }
        protected abstract IFeedSettings FeedSettings { get; }
        public abstract string PlaylistID { get; }
        public abstract string DisplayName { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void PropChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public Dictionary<string, ScrapedSong> GetSongs()
        {
            return FeedReader.GetSongsFromFeed(FeedSettings);
        }
    }

    public class BeastSaberFeedConfig : FeedConfig
    {
        public override string PlaylistID => $"SyncService_BeastSaber{FeedType.ToString()}";
        public override string DisplayName
        {
            get
            {
                switch (FeedType)
                {
                    case BeastSaberFeeds.BOOKMARKS:
                        return "Bookmarks";
                    case BeastSaberFeeds.CURATOR_RECOMMENDED:
                        return "Curator Recommended";
                    case BeastSaberFeeds.FOLLOWING:
                        return "Following";
                    default:
                        Log.LogErr($"Unhandled Beast Saber FeedType {FeedType}");
                        return "Sync";
                }
            }
        }
        //todo: eval what the quest can handle
        private const int MAX_CONCURRENCY = 4;
        internal string BeastSaberUsername { get; set; }

        private BeastSaberFeeds _feedType;
        public BeastSaberFeeds FeedType
        {
            get
            {
                return _feedType;
            }
            set
            {
                bool changed = _feedType != value;
                _feedType = value;
                if (changed)
                    PropChanged(nameof(FeedType));
            }
        }

        protected override IFeedSettings FeedSettings
        {
            get
            {
                return new BeastSaberFeedSettings((int)FeedType);
            }
        }

        private BeastSaberReader _bsaberReader;
        private string _readerUsername;
        protected override IFeedReader FeedReader
        {
            get
            {
                if (_bsaberReader == null || _readerUsername != BeastSaberUsername)
                {
                    _bsaberReader = new BeastSaberReader(BeastSaberUsername, MAX_CONCURRENCY);
                    _readerUsername = BeastSaberUsername;
                }
                return _bsaberReader;
            }
        }
    }

    public class ScoreSaberFeedConfig : FeedConfig
    {
        public override string PlaylistID => $"SyncService_ScoreSaber{FeedType.ToString()}";
        public override string DisplayName
        {
            get
            {
                switch (FeedType)
                {
                    case ScoreSaberFeeds.LATEST_RANKED:
                        return "Latest Ranked";
                    case ScoreSaberFeeds.TOP_PLAYED:
                        return "Top Played";
                    case ScoreSaberFeeds.TOP_RANKED:
                        return "Top Ranked";
                    case ScoreSaberFeeds.TRENDING:
                        return "Trending";
                    default:
                        Log.LogErr($"Unhandled Score Saber FeedType {FeedType}");
                        return "Sync";
                }
            }
        }

        private int _maxSongs = 100;
        public int MaxSongs
        {
            get
            {
                return _maxSongs;
            }
            set
            {
                bool changed = _maxSongs != value;
                _maxSongs = value;
                if (changed)
                    PropChanged(nameof(MaxSongs));
            }
        }

        private ScoreSaberFeeds _feedType;

        public ScoreSaberFeeds FeedType
        {
            get
            {
                return _feedType;
            }
            set
            {
                bool changed = _feedType != value;
                _feedType = value;
                if (changed)
                    PropChanged(nameof(FeedType));
            }
        }

        private ScoreSaberReader _ssReader;
        protected override IFeedReader FeedReader
        {
            get
            {
                if (_ssReader == null)
                {
                    _ssReader = new ScoreSaberReader();
                }
                return _ssReader;
            }
        }

        protected override IFeedSettings FeedSettings
        {
            get
            {
                return new ScoreSaberFeedSettings((int)FeedType) { MaxSongs = MaxSongs };
            }
        }
    }
}