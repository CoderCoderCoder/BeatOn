using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using BeatOn.ClientModels;
using BeatOn.Core.MessageHandlers;
using BeatOn.Core.RequestHandlers;
using Com.Emulamer.Installerhelper;
using Newtonsoft.Json;
using QuestomAssets;
using QuestomAssets.Models;

namespace BeatOn
{
    [Activity(Name = "com.emulamer.beaton.MainActivity", Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        public MainActivity()
        {
            QuestomAssets.Utils.ImageUtils.Instance = new ImageUtilsDroid();
        }

        private WebView _webView;
        private DownloadManager _SongDownloadManager;
        private JSWebViewClient _webViewClient;
        private WebServer _webServer;
        private Mod _mod;
        private QuestomAssetsEngine _qae;
        private BeatOnConfig _currentConfig;

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
            } else
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
                            t.Wait();
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
                    ModsPath = "Mods",
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

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Log.SetLogSink(new AndroidLogger());
            Log.SetLogSink(new FileLogger(Constants.LOGFILE));


            if (CheckSelfPermission(Android.Manifest.Permission.WriteExternalStorage)
                != Android.Content.PM.Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new String[] { Android.Manifest.Permission.WriteExternalStorage }, 1);
            }
            if (CheckSelfPermission(Android.Manifest.Permission.ReadExternalStorage)
                    != Android.Content.PM.Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new String[] { Android.Manifest.Permission.ReadExternalStorage }, 1);
            }
            //TODO: check that we actually got these permissions above
            ContinueStartup();
            
        }

        protected override void OnStop()
        {
            base.OnStop();
            //_webServer.Stop();
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override void OnStart()
        {
            base.OnStart();
            //if (_webServer != null && !_webServer.IsRunning)
            //    _webServer.Start();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void ContinueStartup()
        {
            //has to be the activity context to do the package manager stuff
            _mod = new Mod(this);
            _mod.StatusUpdated += _mod_StatusUpdated;
            _webView = FindViewById<WebView>(Resource.Id.webView1);
            _SongDownloadManager = new DownloadManager(() => { return Engine; }, () => { return CurrentConfig.Config; }, _qaeConfig.SongFileProvider, _qaeConfig.SongsPath);
            _SongDownloadManager.StatusChanged += _SongDownloadManager_StatusChanged;
            _webView.Download += _webView_Download;
            SetupWebApp();
            //don't force the config to load
            //var x = CurrentConfig;
        }

        private void OpManager_OpStatusChanged(object sender, QuestomAssets.AssetOps.AssetOp e)
        {
            if (e.Status == QuestomAssets.AssetOps.OpStatus.Complete)
            {
              //  ClearCurrentConfig();
              //  SendConfigChangeMessage();
            }
                
        }

        private void _SongDownloadManager_StatusChanged(object sender, DownloadStatusChangeArgs e)
        {
            RunOnUiThread(() =>
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
            });
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

        private void _webView_Download(object sender, DownloadEventArgs e)
        {
            if (e.Mimetype != "application/zip")
            {
                ShowToast("Unable to Download", "File isn't a zip file!  Not downloading it.", ToastType.Error, 8);
                return;
            }
            var uri = new Uri(e.Url);
            //ShowToast("Starting Download...", uri.ToString(), ToastType.Info, 2);

            var fileName = Path.GetFileNameWithoutExtension(uri.LocalPath);
            if (_qaeConfig.FileProvider.FileExists(Path.Combine(_qaeConfig.SongsPath, fileName)))
            {
                ShowToast("Unable to Download", "A custom song folder with the name of this zip already exists.  Not downloading it.", ToastType.Error, 8);
                return;
            }
            _SongDownloadManager.DownloadFile(e.Url);
        }


        private void SetupWebApp()
        {
            _webServer = new WebServer(Assets, "www");
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

            _webServer.AddMessageHandler(MessageType.DeletePlaylist, new ClientDeletePlaylistHandler(() => Engine, () => CurrentConfig));
            _webServer.AddMessageHandler(MessageType.AddOrUpdatePlaylist, new ClientAddOrUpdatePlaylistHandler(() => Engine, () => CurrentConfig));
            _webServer.AddMessageHandler(MessageType.MoveSongToPlaylist, new ClientMoveSongToPlaylistHandler(() => Engine, () => CurrentConfig));
            _webServer.AddMessageHandler(MessageType.DeleteSong, new ClientDeleteSongHandler(() => Engine, () => CurrentConfig));

            _webServer.Start();
            _webViewClient = new JSWebViewClient(this, _webView);
            _webView.LoadUrl($"http://localhost:{_webServer.Port}");
        }

        
    }


    public static class ResponseExtensions
    {
        public static void NotFound(this HttpListenerResponse resp)
        {
            resp.StatusCode = 400;
        }

        public static void BadRequest(this HttpListenerResponse resp, string message = null)
        {
            resp.StatusCode = 400;
            if (message != null)
            {
                WriteBody(resp, message);
            }
        }

        public static void Error(this HttpListenerResponse resp, string message = null)
        {
            resp.StatusCode = 500;
            if (message != null)
            {
                WriteBody(resp, message);
            }
        }

        private static void WriteBody(HttpListenerResponse resp, string body)
        {
            using (var sw = new StreamWriter(resp.OutputStream, System.Text.Encoding.UTF8, 1024, true))
                sw.Write(body);
        }

        public static void Ok(this HttpListenerResponse resp, string message = null)
        {
            if (message == null)
            {
                resp.StatusCode = 204;
            }
            else
            {
                resp.StatusCode = 200;
                WriteBody(resp, message);
            }
        }

        public static void Serialize<T>(this HttpListenerResponse resp, T obj)
        {
            resp.StatusCode = 200;
            resp.ContentType = "application/json";
            WriteBody(resp, JsonConvert.SerializeObject(obj));
        }

    }

}

