using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BeatOn.ClientModels;
using BeatOn.Core.MessageHandlers;
using BeatOn.Core.RequestHandlers;
using Newtonsoft.Json;
using QuestomAssets;
using QuestomAssets.AssetOps;
using QuestomAssets.Models;

namespace BeatOn.Core
{
    public class BeatOnCore : IDisposable
    {
        private Context _context;
        public BeatOnCore(Context context, Action<string> triggerPackageInstall, Action<string> triggerPackageUninstall)
        {
            _context = context;
            _mod = new Mod(_context, triggerPackageInstall, triggerPackageUninstall);
            _mod.StatusUpdated += _mod_StatusUpdated;
            _SongDownloadManager = new DownloadManager(() => Engine, () => CurrentConfig.Config, _qaeConfig.SongFileProvider, _qaeConfig.SongsPath);
            _SongDownloadManager.StatusChanged += _SongDownloadManager_StatusChanged;
        }

        public void Start()
        {
            SetupWebApp();
        }

        private void Cleanup()
        {
            try
            {
                _webServer.Dispose();
            }
            catch
            { }
            
            _webServer = null;
            if (_qae != null)
            {
                try
                {
                    _qae.Dispose();
                }
                catch
                { }
                _qae = null;
            }
            if (_currentConfig != null)
            {
                ClearCurrentConfig();
            }
        }

        private DownloadManager _SongDownloadManager;
        private WebServer _webServer;
        private Mod _mod;
        private QuestomAssetsEngine _qae;
        private BeatOnConfig _currentConfig;
        private List<AssetOp> _trackedOps = new List<AssetOp>();

        private QuestomAssetsEngine Engine
        {
            get
            {
                if (_qae == null)
                {
                    _qae = new QuestomAssetsEngine(_qaeConfig);
                    _qae.OpManager.OpStatusChanged += OpManager_OpStatusChanged;
                }
                return _qae;
            }
        }

        private void ClearCurrentConfig()
        {
            if (_currentConfig != null)
            {
                _currentConfig.PropertyChanged -= CurrentConfig_PropertyChanged;
                _currentConfig = null;
            }
        }

        private void DisposeEngine()
        {
            ClearCurrentConfig();
            if (_qae != null)
            {
                _qae.Dispose();
            }
        }

        private void UpdateConfig(BeatSaberQuestomConfig config)
        {
            var currentCfg = Engine.GetCurrentConfig();
            ClearCurrentConfig();
            bool matches = config.Matches(currentCfg);

            if (!matches)
            {
                Engine.UpdateConfig(config);
                config = Engine.GetCurrentConfig();
            }
            else
            {
                config = currentCfg;
            }
            _currentConfig = new BeatOnConfig()
            {
                Config = config,
                IsCommitted = matches
            };
            _currentConfig.PropertyChanged += CurrentConfig_PropertyChanged;
            SendConfigChangeMessage();
        }

        private BeatOnConfig CurrentConfig
        {
            get
            {
                if (_currentConfig == null)
                {
                    try
                    {
                        var engineNull = _qae == null;
                        var config = Engine.GetCurrentConfig();

                        _currentConfig = new BeatOnConfig()
                        {
                            Config = config
                        };
                        _currentConfig.IsCommitted = true;
                        _currentConfig.PropertyChanged += CurrentConfig_PropertyChanged;
                    }
                    catch (Exception ex)
                    {
                        Log.LogErr("Critical exception loading current config", ex);
                        ShowToast("Critical Error", "Something has gone wrong and Beat On can't function.  Try Reset Assets in the tools menu.", ToastType.Error, 60);
                    }
                }

                return _currentConfig;
            }
        }

        public void DownloadUrl(string url, string mimeType)
        {
            if (mimeType != "application/zip")
            {
                ShowToast("Unable to Download", "File isn't a zip file!  Not downloading it.", ToastType.Error, 8);
                return;
            }
            var uri = new Uri(url);
            //ShowToast("Starting Download...", uri.ToString(), ToastType.Info, 2);

            var fileName = Path.GetFileNameWithoutExtension(uri.LocalPath);
            if (_qaeConfig.FileProvider.FileExists(Path.Combine(_qaeConfig.SongsPath, fileName)))
            {
                ShowToast("Unable to Download", "A custom song folder with the name of this zip already exists.  Not downloading it.", ToastType.Error, 8);
                return;
            }
            _SongDownloadManager.DownloadFile(url);
        }

        object _debounceLock = new object();
        CancellationTokenSource _debounceTokenSource;
        private void CurrentConfig_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            lock (_debounceLock)
            {
                if (_debounceTokenSource != null)
                {
                    _debounceTokenSource.Cancel(true);
                    _debounceTokenSource = null;
                }
                _debounceTokenSource = new CancellationTokenSource();
                var task = Task.Delay(10, _debounceTokenSource.Token);
                task.ContinueWith((t) =>
                {
                    try
                    {
                        try
                        {
                          //  t.Wait();
                        }
                        catch (AggregateException aex)
                        { }
                        lock (_debounceLock)
                        {
                            SendConfigChangeMessage();
                            _debounceTokenSource = null;
                        }
                    }
                    catch (Exception)
                    {
                    }
                });
            }
        }

        private QaeConfig _qaeConfig
        {
            get
            {
                var q = new QaeConfig()
                {
                    FileProvider = new FolderFileProvider(Constants.ROOT_BEAT_ON_DATA_PATH, false),
                    PlaylistArtPath = "Art",
                    AssetsPath = "BeatSaberAssets",
                    ModsSourcePath = "Mods",
                    SongsPath = "CustomSongs"
                };
                q.SongFileProvider = q.FileProvider;
                return q;
            }
        }

        private object _configFileLock = new object();

        private void LoadConfigFromFile()
        {
            //todo: reset engine, load config from json, call UpdateConfig
        }

        /// <summary>
        /// Saves the currently loaded configuration to the config file, but not to the beat saber assets
        /// </summary>
        private void SaveCurrentConfigToFile()
        {
            //save to json in filesystem
            lock (_configFileLock)
            {
                using (StreamWriter sw = new StreamWriter(Constants.CONFIG_FILE, false))
                {
                    sw.Write(JsonConvert.SerializeObject(CurrentConfig.Config));
                }
            }
        }

        private void SendStatusMessage(string message)
        {
            SendMessageToClient(new HostSetupEvent() { SetupEvent = SetupEventType.StatusMessage, Message = message });
        }

        private bool _suppressConfigChangeMessage = false;
        private void SendConfigChangeMessage()
        {
            if (!_suppressConfigChangeMessage)
                _webServer.SendMessage(new HostConfigChangeEvent() { UpdatedConfig = CurrentConfig });
        }

        private void OpManager_OpStatusChanged(object sender, QuestomAssets.AssetOps.AssetOp e)
        {
            List<AssetOp> opCopy;
            lock (_trackedOps)
            {
                if (!_trackedOps.Any(x=> x.ID == e.ID))
                    _trackedOps.Add(e);

                _trackedOps.RemoveAll(x => x.Status == OpStatus.Complete);

                opCopy = _trackedOps.ToList();
            }

            HostOpStatus opstat = new HostOpStatus();

            foreach (var op in opCopy)
            {
                string exmsg = null;
                //if (op.Exception != null)
                //{
                //    exmsg = $"{op.Exception.Message} {op.Exception.StackTrace}";
                //    var ex = op.Exception.InnerException;
                //    while (ex != null)
                //    {
                //        exmsg += $"\nInnerException: {ex.Message} {ex.StackTrace}";
                //        ex = ex.InnerException;
                //    }
                //}
                opstat.Ops.Add(new HostOp() { ID = op.ID, OpDescription = op.GetType().Name, Status = op.Status, Error = op.Exception?.Message });
            }

            SendMessageToClient(opstat);
        }

        private void _SongDownloadManager_StatusChanged(object sender, DownloadStatusChangeArgs e)
        {
            var dl = sender as Download;
            if (e.UpdateType == DownloadStatusChangeArgs.DownloadStatusUpdateType.StatusChange)
            {
                switch (e.Status)
                {
                    case DownloadStatus.Downloading:
                        ShowToast("Downloading song...", dl.DownloadUrl.ToString(), ToastType.Info, 3);
                        break;
                    case DownloadStatus.Failed:
                        ShowToast("Song failed to download", dl.DownloadUrl.ToString(), ToastType.Error, 5);
                        break;
                    case DownloadStatus.Installed:
                        ShowToast("Song added to Beat Saber", dl.DownloadUrl.ToString(), ToastType.Success, 3);
                        break;
                }
                var hds = new HostDownloadStatus();
                var dls = _SongDownloadManager.Downloads;
                dls.ForEach(x => hds.Downloads.Add(new HostDownload() { ID = x.ID, PercentageComplete = x.PercentageComplete, Status = x.Status, Url = x.DownloadUrl.ToString() }));
                SendMessageToClient(hds);
            }
        }

        private void _mod_StatusUpdated(object sender, string e)
        {
            SendStatusMessage(e);
        }

        private void SendMessageToClient(ClientModels.MessageBase message)
        {
            _webServer.SendMessage(message);
        }
        private void ShowToast(string title, string message, ToastType type = ToastType.Info, float durationSec = 3.0F)
        {
            SendMessageToClient(new HostShowToast() { Title = title, Message = message, ToastType = type, Timeout = (int)(durationSec * 1000) });
        }

        private void SetupWebApp()
        {
            _webServer = new WebServer(_context.Assets, "www");
            _webServer.Router.AddRoute("GET", "beatsaber/config", new GetConfig(() => CurrentConfig));
            _webServer.Router.AddRoute("PUT", "beatsaber/config", new PutConfig(cfg => UpdateConfig(cfg)));
            _webServer.Router.AddRoute("GET", "beatsaber/song/cover", new GetSongCover(() => CurrentConfig));
            _webServer.Router.AddRoute("GET", "beatsaber/playlist/cover", new GetPlaylistCover(() => CurrentConfig));
            _webServer.Router.AddRoute("POST", "beatsaber/upload", new PostFileUpload(_mod, ShowToast, () => _SongDownloadManager));
            _webServer.Router.AddRoute("POST", "beatsaber/commitconfig", new PostCommitConfig(_mod, ShowToast, SendMessageToClient, () => Engine, () => CurrentConfig, SendConfigChangeMessage));
            _webServer.Router.AddRoute("POST", "beatsaber/reloadsongfolders", new PostReloadSongFolders(_mod, _qaeConfig, ShowToast, SendMessageToClient, () => Engine, () => CurrentConfig, SendConfigChangeMessage, (suppress) => { _suppressConfigChangeMessage = suppress; }));
            _webServer.Router.AddRoute("GET", "mod/status", new GetModStatus(_mod));
            _webServer.Router.AddRoute("GET", "mod/netinfo", new GetNetInfo(_webServer));
            _webServer.Router.AddRoute("POST", "mod/install/step1", new PostModInstallStep1(_mod, SendMessageToClient));
            _webServer.Router.AddRoute("POST", "mod/install/step2", new PostModInstallStep2(_mod, SendMessageToClient));
            _webServer.Router.AddRoute("POST", "mod/install/step3", new PostModInstallStep3(_mod, SendMessageToClient));
            _webServer.Router.AddRoute("POST", "mod/resetassets", new PostResetAssets(_mod, ShowToast, SendConfigChangeMessage, () =>
            {
                _currentConfig = null;
                _qae.Dispose();
                _qae = null;
            }));
            _webServer.Router.AddRoute("POST", "mod/uninstallbeatsaber", new PostUninstallBeatSaber(_mod, ShowToast));

            //if you add a new MessageType and a handler here, make sure the type is added in MessageTypeConverter.cs
            _webServer.AddMessageHandler(MessageType.DeletePlaylist, new ClientDeletePlaylistHandler(() => Engine, () => CurrentConfig));
            _webServer.AddMessageHandler(MessageType.AddOrUpdatePlaylist, new ClientAddOrUpdatePlaylistHandler(() => Engine, () => CurrentConfig));
            _webServer.AddMessageHandler(MessageType.MoveSongToPlaylist, new ClientMoveSongToPlaylistHandler(() => Engine, () => CurrentConfig));
            _webServer.AddMessageHandler(MessageType.DeleteSong, new ClientDeleteSongHandler(() => Engine, () => CurrentConfig));
            _webServer.AddMessageHandler(MessageType.GetOps, new ClientGetOpsHandler(_trackedOps, SendMessageToClient));

            _webServer.Start();

        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Cleanup();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        public string Url
        {
            get
            {
                if (_webServer == null)
                    return null;
                if (!_webServer.IsRunning)
                    return null;
                return _webServer.ListeningOnUrl;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BeatOnCore()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}