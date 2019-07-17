using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using FeedReader;
using Newtonsoft.Json;
using QuestomAssets;
using QuestomAssets.AssetOps;

namespace BeatOn
{
    public class SyncManager
    {
        private SyncConfig _syncConfig;
        public SyncConfig SyncConfig
        {
            get
            {
                lock (_configLock)
                {
                    if (_syncConfig == null)
                        LoadConfig();

                    return _syncConfig;
                }
            }
        }
        private DownloadManager _downloadManager;
        private GetBeatOnConfigDelegate _getConfig;
        private GetQaeDelegate _getQae;
        private ShowToastDelegate _showToast;
        private QaeConfig _config;
        public SyncManager(QaeConfig config, DownloadManager downloadManager, GetBeatOnConfigDelegate getConfig, GetQaeDelegate getQae, ShowToastDelegate showToast)
        {
            _downloadManager = downloadManager;
            _getConfig = getConfig;
            _getQae = getQae;
            _showToast = showToast;
            _config = config;
        }

        private object _configLock = new object();
        public void Save()
        {
            lock (_configLock)
            {
                try
                {
                    var settings = new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.All,
                        Formatting = Formatting.Indented            //because I like it purdy and bits are cheap
                    };
                    _config.RootFileProvider.Write(Constants.SYNC_SABER_CONFIG, System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(SyncConfig, settings)), true);
                }
                catch (Exception ex)
                {
                    Log.LogErr("Exception trying to save SyncManager config", ex);
                }
            }
        }

        public void Sync(Guid? onlyID = null)
        {
            try
            {
                var cfg = _getConfig();
                var qae = _getQae();
                foreach (var sync in SyncConfig.FeedReaders)
                {
                    if (!sync.IsEnabled || (onlyID.HasValue && sync.ID != onlyID))
                        continue;

                    //seems bad to have this here doing this, but whatever it'll work until it doesn't, then I'll fix it.
                    var bsSync = sync as BeastSaberFeedConfig;
                    if (bsSync != null)
                    {
                        bsSync.BeastSaberUsername = SyncConfig.BeastSaberUsername;
                    }
                    try
                    {
                        var songs = sync.GetSongs();
                        var playlist = cfg.Config.Playlists.FirstOrDefault(x => x.PlaylistID == sync.PlaylistID);
                        if (playlist == null)
                        {
                            playlist = new QuestomAssets.Models.BeatSaberPlaylist()
                            {
                                PlaylistID = sync.PlaylistID,
                                PlaylistName = sync.DisplayName
                            };
                            var op = new AddOrUpdatePlaylistOp(playlist);
                            qae.OpManager.QueueOp(op);
                            op.FinishedEvent.WaitOne();
                            if (op.Status != OpStatus.Complete)
                                throw new SyncException($"Failed to create playlist for {sync.DisplayName}!");
                            _getConfig().Config = qae.GetCurrentConfig();
                            cfg = _getConfig();
                        }
                        int i = 0;
                        var toAdd = songs.Where(x => !playlist.SongList.Any(y => y.SongID.ToLower().StartsWith(x.Key.ToLower()))).ToList();
                        var toRemove = playlist.SongList.Where(x => !songs.Any(y => x.SongID.ToLower().StartsWith(y.Key.ToLower()))).ToList();
                        toAdd.ForEach(x => {
                            try
                            {
                                _downloadManager.DownloadFile(x.Value.DownloadUrl.ToLower(), true, sync.PlaylistID, true);
                            }
                            catch (Exception ex)
                            {
                                Log.LogErr($"Exception trying to start download of {x.Value.DownloadUrl}", ex);
                            }
                        });
                        toRemove.ForEach(x =>
                        {
                            var d = new DeleteSongOp(x.SongID);
                            qae.OpManager.QueueOp(d);
                        });
                    }
                    catch (Exception ex)
                    {
                        Log.LogErr($"Exception syncing from {sync.DisplayName}!", ex);
                        _showToast($"Sync {sync.DisplayName} Failed", "", ClientModels.ToastType.Error, 2);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogErr("Exception syncing!", ex);
                _showToast($"Failed Sync!", "There was an error syncing.", ClientModels.ToastType.Error, 3);
            }
        }

        private void LoadConfig()
        {
            lock (_configLock)
            {
                bool isNew = false;
                try
                {
                    if (!_config.RootFileProvider.FileExists(Constants.SYNC_SABER_CONFIG))
                    {
                        isNew = true;
                        _syncConfig = new SyncConfig();
                    }
                    else
                    {
                        var settings = new JsonSerializerSettings()
                        {
                            TypeNameHandling = TypeNameHandling.All,
                            Formatting = Formatting.Indented            //because I like it purdy and bits are cheap
                        };
                        _syncConfig = JsonConvert.DeserializeObject<SyncConfig>(_config.RootFileProvider.ReadToString(Constants.SYNC_SABER_CONFIG), settings);
                    }
                }
                catch (Exception ex)
                {
                    Log.LogErr("Exception trying to load SyncManager config", ex);
                    isNew = true;
                    _syncConfig = new SyncConfig();
                }
                SetConfigDefaults();
                if (isNew)
                    Save();
            }            
        }

        private void SetConfigDefaults()
        {
            Action<BeastSaberFeeds> checkMakeBS = (BeastSaberFeeds feed) =>
            {
                if (!SyncConfig.FeedReaders.Any(x => (x is BeastSaberFeedConfig) && (x as BeastSaberFeedConfig).FeedType == feed))
                    SyncConfig.FeedReaders.Add(new BeastSaberFeedConfig() { FeedType = feed, IsEnabled = false });
            };

            Action<ScoreSaberFeeds> checkMakeSS = (ScoreSaberFeeds feed) =>
            {
                if (!SyncConfig.FeedReaders.Any(x => (x is ScoreSaberFeedConfig) && (x as ScoreSaberFeedConfig).FeedType == feed))
                    SyncConfig.FeedReaders.Add(new ScoreSaberFeedConfig() { FeedType = feed, IsEnabled = false });
            };
            foreach (BeastSaberFeeds feed in Enum.GetValues(typeof(BeastSaberFeeds)))
                checkMakeBS(feed);

            foreach (ScoreSaberFeeds feed in Enum.GetValues(typeof(ScoreSaberFeeds)))
                checkMakeSS(feed);
        }


    }
}