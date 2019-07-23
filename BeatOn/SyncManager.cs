using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public List<FeedSyncStatus> ActiveSyncs { get { return _activeSyncs.ToList(); } }
        private ConcurrentBag<FeedSyncStatus> _activeSyncs = new ConcurrentBag<FeedSyncStatus>();
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
            WebUtils.Initialize(new System.Net.Http.HttpClient());
        }

        public class FeedSyncStatus
        {
            public FeedSyncStatus(FeedConfig config)
            {
                FeedConfig = config;
            }

            public FeedConfig FeedConfig {get; private set;}
            
            public FeedSyncStatusType StatusType { get; set; }

            public string ErrorMessage { get; set; }

            public enum FeedSyncStatusType
            {
                Started,
                Finished,
                Failed
            }
        }

        public event EventHandler<FeedSyncStatus> SyncStatusUpdate;

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

        public List<FeedSyncStatus> Sync(Guid? onlyID = null)
        {
            var feedStatus = new List<FeedSyncStatus>();
            try
            {
                var cfg = _getConfig();
                var qae = _getQae();
                List<Task> tasks = new List<Task>();
                foreach (var sync in SyncConfig.FeedReaders)
                {
                    if (!sync.IsEnabled || (onlyID.HasValue && sync.ID != onlyID))
                        continue;
                    FeedSyncStatus fss = new FeedSyncStatus(sync);
                    Task syncTask = new Task(() =>
                    {
                        
                        
                        //seems bad to have this here doing this, but whatever it'll work until it doesn't, then I'll fix it.
                        var bsSync = sync as BeastSaberFeedConfig;
                        if (bsSync != null)
                        {
                            bsSync.BeastSaberUsername = SyncConfig.BeastSaberUsername;
                        }
                        
                        fss.StatusType = FeedSyncStatus.FeedSyncStatusType.Started;
                        try
                        {                            
                            _activeSyncs.Add(fss);
                            SyncStatusUpdate?.Invoke(this, fss);
                            sync.LastSyncAttempt = DateTime.Now;
                            var songsPl = sync.GetSongsByPlaylist();
                            foreach (var spl in songsPl)
                            {
                                var playlist = cfg.Config.Playlists.FirstOrDefault(x => x.PlaylistID == spl.Key);
                                if (playlist == null)
                                {
                                    playlist = new QuestomAssets.Models.BeatSaberPlaylist()
                                    {
                                        PlaylistID = spl.Key,
                                        PlaylistName = spl.Value.Item1
                                    };
                                    var op = new AddOrUpdatePlaylistOp(playlist);
                                    qae.OpManager.QueueOp(op);
                                    op.FinishedEvent.WaitOne();
                                    if (op.Status != OpStatus.Complete)
                                        throw new SyncException($"Failed to create playlist for {sync.DisplayName}!");
                                    _getConfig().Config = qae.GetCurrentConfig();
                                    cfg = _getConfig();
                                }
                                var toAdd = spl.Value.Item2.Where(x => !playlist.SongList.Any(y => y.SongID.ToLower().StartsWith(x.Key.ToLower()))).ToList();
                                var toRemove = playlist.SongList.Where(x => !spl.Value.Item2.Any(y => x.SongID.ToLower().StartsWith(y.Key.ToLower()))).ToList();
                                toAdd.ForEach(x =>
                                {
                                    try
                                    {
                                        if (cfg.Config.Playlists.SelectMany(z => z.SongList).Any(z => z.SongID.ToLower().StartsWith(x.Key.ToLower())))
                                        {
                                            Log.LogErr($"Syncing playlist ID {spl.Key} includes songID {x.Key} which is already elsewhere in the library, skipping it.");
                                        }
                                        else
                                        {
                                            _downloadManager.DownloadFile(x.Value.DownloadUrl.ToLower(), true, spl.Key, true);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.LogErr($"Exception trying to start download of {x.Value.DownloadUrl}", ex);
                                    }
                                });
                                List<AssetOp> delOps = new List<AssetOp>();
                                toRemove.ForEach(x =>
                                {
                                    var d = new DeleteSongOp(x.SongID);
                                    qae.OpManager.QueueOp(d);
                                    delOps.Add(d);
                                });
                                if (delOps.Count > 0) {
                                    delOps.WaitForFinish();
                                    _getConfig().Config = qae.GetCurrentConfig();
                                }
                            }
                            sync.LastSyncSuccess = DateTime.Now;
                            fss.StatusType = FeedSyncStatus.FeedSyncStatusType.Finished;
                        }
                        catch (Exception ex)
                        {
                            Log.LogErr($"Exception syncing from {sync.DisplayName}!", ex);
                            _showToast($"Sync {sync.DisplayName} Failed", "", ClientModels.ToastType.Error, 2);
                            fss.StatusType = FeedSyncStatus.FeedSyncStatusType.Failed;
                            fss.ErrorMessage = ex.Message;
                        }
                        finally
                        {
                            FeedSyncStatus f;
                            _activeSyncs.TryTake(out f);
                            SyncStatusUpdate?.Invoke(this, fss);
                        }
                    });
                    tasks.Add(syncTask);
                    feedStatus.Add(fss);
                }
                tasks.ForEach(x => x.Start());
                Task.WhenAll(tasks).ContinueWith((t) =>
                {
                    t.Wait();
                    Save();
                });
            }
            catch (Exception ex)
            {
                Log.LogErr("Exception syncing!", ex);
                _showToast($"Failed Sync!", "There was an error syncing.", ClientModels.ToastType.Error, 3);
            }
            return feedStatus;     
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

            Action<BeatSaverFeeds> checkMakeBV = (BeatSaverFeeds feed) =>
            {
                if (!SyncConfig.FeedReaders.Any(x => (x is BeatSaverFeedConfig) && (x as BeatSaverFeedConfig).FeedType == feed))
                    SyncConfig.FeedReaders.Add(new BeatSaverFeedConfig() { FeedType = feed, IsEnabled = false });
            };

            foreach (BeastSaberFeeds feed in Enum.GetValues(typeof(BeastSaberFeeds)))
                checkMakeBS(feed);

            foreach (ScoreSaberFeeds feed in Enum.GetValues(typeof(ScoreSaberFeeds)))
                checkMakeSS(feed);

            foreach (BeatSaverFeeds feed in Enum.GetValues(typeof(BeatSaverFeeds)))
            {
                if (feed != BeatSaverFeeds.SEARCH)
                    checkMakeBV(feed);
            }                
        }


    }
}