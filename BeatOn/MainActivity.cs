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
                    _qae = new QuestomAssetsEngine(_qaeConfig);
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
            _webServer.Stop();
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (_webServer != null && !_webServer.IsRunning)
                _webServer.Start();
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
            //force the config to load
            var x = CurrentConfig;
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

        private void SendMessageToClient(HostMessage message)
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
            _webServer.Router.AddRoute("GET", "beatsaber/config", HandleGetConfig);
            _webServer.Router.AddRoute("PUT", "beatsaber/config", HandlePutConfig);
            _webServer.Router.AddRoute("GET", "beatsaber/song/cover", HandleGetSongCover);
            _webServer.Router.AddRoute("GET", "beatsaber/playlist/cover", HandleGetPlaylistCover);
            _webServer.Router.AddRoute("POST", "beatsaber/upload", HandleFileUpload);
            _webServer.Router.AddRoute("POST", "beatsaber/commitconfig", HandleCommitConfig);
            _webServer.Router.AddRoute("POST", "beatsaber/reloadsongfolders", HandleReloadSongFolders);
            _webServer.Router.AddRoute("GET", "mod/status", HandleModStatus);
            _webServer.Router.AddRoute("GET", "mod/netinfo", HandleGetNetInfo);
            _webServer.Router.AddRoute("POST", "mod/install/step1", HandleModInstallStep1);
            _webServer.Router.AddRoute("POST", "mod/install/step2", HandleModInstallStep2);
            _webServer.Router.AddRoute("POST", "mod/install/step3", HandleModInstallStep3);
            _webServer.Router.AddRoute("POST", "mod/resetassets", HandleResetAssets);
            _webServer.Router.AddRoute("POST", "mod/uninstallbeatsaber", HandleUninstallBeatSaber);
            
            _webServer.Start();
            _webViewClient = new JSWebViewClient(this, _webView);
            _webView.LoadUrl($"http://localhost:{_webServer.Port}");
        }

        private object _modStatusLock = new object();
        private void HandleModStatus(HttpListenerContext context)
        {
            lock (_modStatusLock)
            {
                var req = context.Request;
                var resp = context.Response;
                try
                {
                    /*THIS IS TEST CODE FOR EMULATOR, REMOVE IT FOR DEVICE
                     * 
                     */
                    //resp.Serialize(new ModStatus()
                    //{
                    //    IsBeatSaberInstalled = true,
                    //    CurrentStatus = ModStatusType.ModInstalled
                    //});
                    //return;

                    var model = new ModStatus()
                    {
                        IsBeatSaberInstalled = _mod.IsBeatSaberInstalled
                    };

                    if (model.IsBeatSaberInstalled)
                    {
                        if (_mod.IsInstalledBeatSaberModded)
                        {
                            model.CurrentStatus = ModStatusType.ModInstalled;
                        }
                        else if (!_mod.IsBeatSaberInstalled && _mod.DoesTempApkExist)
                        {
                            if (_mod.IsTempApkModded)
                            {
                                model.CurrentStatus = ModStatusType.ReadyForInstall;
                            }
                            else
                            {
                                model.CurrentStatus = ModStatusType.ReadyForModApply;
                            }
                        }
                    }
                    else
                    {
                        if (_mod.DoesTempApkExist)
                        {
                            if (_mod.IsTempApkModded)
                                model.CurrentStatus = ModStatusType.ReadyForInstall;
                            else
                                model.CurrentStatus = ModStatusType.ReadyForModApply;
                        }
                    }
                    resp.Serialize(model);
                }
                catch (Exception ex)
                {
                    Log.LogErr("Exception handling mod status!", ex);
                    resp.StatusCode = 500;
                }
            }
        }

        private object _reloadSongsLock = new object();
        private void HandleReloadSongFolders(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;

            if (!Monitor.TryEnter(_reloadSongsLock))
            {
                resp.Error("Song reload already in progress");
                return;
            }
            try
            { 
                try
                {
                    if (!_mod.IsBeatSaberInstalled || !_mod.IsInstalledBeatSaberModded)
                    {
                        resp.BadRequest("Modded Beat Saber is not installed!");
                        ShowToast("Can't reload song folders.", "Modded Beat Saber is not installed!");
                        return;
                    }
                    var sls = new StatusUpdateLogSink((msg) =>
                    {
                        SendStatusMessage(msg);
                    });
                    try
                    {
                        
                        _suppressConfigChangeMessage = true;
                        var folders = BeatOnUtils.GetCustomSongsFromPath(Path.Combine(Constants.ROOT_BEAT_ON_DATA_PATH, _qaeConfig.SongsPath));
                        if (folders.Count < 1)
                        {
                            Log.LogErr("Request to reload songs folder, but didn't find any songs!");
                            //not found probably isn't the right response code for this, but meh
                            resp.NotFound();
                            return;
                        }
                        Log.LogMsg($"Starting to reload custom songs from folders.  Found {folders} folders to evaluate");
                        //todo: probably don't just grab this one
                        var playlist = CurrentConfig.Config.Playlists.FirstOrDefault(x => x.PlaylistID == "CustomSongs");
                        if (playlist == null)
                        {
                            playlist = new BeatSaberPlaylist()
                            {
                                PlaylistID = "CustomSongs",
                                PlaylistName = "Custom Songs"
                            };
                            CurrentConfig.Config.Playlists.Add(playlist);
                        }
                        int addedCtr = 0;

                        foreach (var folder in folders)
                        {
                            string songId = folder.Replace("/", "");
                            songId = songId.Replace(" ", "");
                            if (CurrentConfig.Config.Playlists.SelectMany(x => x.SongList).Any(x => x.SongID?.ToLower() == songId.ToLower()))
                            {
                                SendStatusMessage($"Folder {folder} already loaded");
                                Log.LogMsg($"Custom song in folder {folder} appears to already be loaded, skipping it.");
                                continue;
                            }
                            SendStatusMessage($"Adding song in {folder}");
                            Log.LogMsg($"Adding custom song in folder {folder} to playlist ID {playlist.PlaylistID}");
                            playlist.SongList.Add(new BeatSaberSong()
                            {
                                SongID = songId,
                                CustomSongPath = Path.Combine(_qaeConfig.SongsPath, folder)
                            });
                            addedCtr++;
                            //maybe limit how many
                            //if (addedCtr > 200)
                            //{
                            //    ShowToast("Too Many Songs", "That's too many at once.  After these finish and you 'Sync to Beat Saber', then try 'Reload Songs Folder' again to load more.", ToastType.Warning, 10);
                            //    break;
                            //}
                        }
                        if (addedCtr > 0)
                        {
                            SendStatusMessage($"{addedCtr} songs will be added");
                            SendStatusMessage($"Updating configuration...");
                            Log.LogMsg("Updating config with loaded song folders");
                            Log.SetLogSink(sls);
                            Engine.UpdateConfig(CurrentConfig.Config);
                            Log.RemoveLogSink(sls);
                            ShowToast("Folder Load Complete", $"{addedCtr} folders were scanned and added to {playlist.PlaylistName}", ToastType.Success, 3);
                        }
                        else
                        {
                            SendStatusMessage($"No new songs found");
                            Log.LogMsg("No new songs were found to load.");
                            ShowToast("Folder Load Complete", "No additional songs were found to add", ToastType.Warning, 3);
                        }
                    }
                    finally
                    {
                        _suppressConfigChangeMessage = false;
                        Log.RemoveLogSink(sls);
                    }
                    SendConfigChangeMessage();
                    resp.Ok();
                }
                catch (Exception ex)
                {
                    Log.LogErr("Exception reloading song folders!", ex);
                    resp.StatusCode = 500;
                }
            }
            finally
            {
                Monitor.Exit(_reloadSongsLock);
            }
        }

        private void HandleGetNetInfo(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;
            try
            {
                resp.Serialize(new NetInfo() {
                    Url = _webServer.ListeningOnUrl,
                    WebSocketUrl = _webServer.WebSocketUrl
                });
            }
            catch (Exception ex)
            {
                Log.LogErr("Exception handling get net info!", ex);
                resp.StatusCode = 500;
            }
        }

        private object _modInstallLock = new object();
        private void HandleModInstallStep1(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;
            if (!Monitor.TryEnter(_modInstallLock))
                resp.BadRequest("Another install request is in progress.");
            try
            {

                try
                {
                    if (!_mod.IsBeatSaberInstalled)
                    {
                        resp.BadRequest("Beat Saber is not installed!");
                        SendMessageToClient(new HostSetupEvent() { SetupEvent = SetupEventType.StatusMessage, Message = "Beat Saber is not installed!  Install Beat Saber and come back." });
                        SendMessageToClient(new HostSetupEvent() { SetupEvent = SetupEventType.Error, Message = "Beat Saber is not installed!"});
                        return;
                    }
                    _mod.CopyOriginalBeatSaberApkAndTriggerUninstall();
                    SendMessageToClient(new HostSetupEvent() { SetupEvent = SetupEventType.Step1Complete });
                    resp.Ok();
                }
                catch (Exception ex)
                {
                    Log.LogErr("Exception handling mod install step 1!", ex);
                    resp.StatusCode = 500;
                }
            }
            finally
            {
                Monitor.Exit(_modInstallLock);
            }
        }

        private void HandleFileUpload(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;
            
            try
            {
                if (!_mod.IsBeatSaberInstalled || !_mod.IsInstalledBeatSaberModded)
                {
                    resp.BadRequest("Modded Beat Saber is not installed!");
                    ShowToast("Can't upload.", "Modded Beat Saber is not installed!");
                    return;
                }
                var ct = req.ContentType;
                if (!ct.StartsWith("multipart/form-data"))
                {
                    resp.BadRequest("Expected content-type of multipart/form-data");
                    return;
                }

                Dictionary<string, MemoryStream> files = new Dictionary<string, MemoryStream>();
                var parser = new HttpMultipartParser.StreamingMultipartFormDataParser(req.InputStream);
                parser.FileHandler = (name, fileName, type, disposition, buffer, bytes) =>
                {
                    if (name != "file")
                    {
                        Log.LogMsg($"Got extra form value named {name}, ignoring it");
                        return;
                    }
                    if (type != "application/x-zip-compressed")
                        throw new NotSupportedException($"Data for file {fileName} isn't a zip");
                    MemoryStream s = null;
                    if (files.ContainsKey(fileName))
                    {
                        s = files[fileName];
                    }
                    else {
                        s = new MemoryStream();
                        files.Add(fileName, s);
                    }
                    s.Write(buffer, 0, bytes);
                };
                parser.Run();
                if (files.Count < 1)
                {
                    resp.BadRequest("Didn't get any useable files.");
                    return;
                }
                foreach (var file in files.Keys.ToList())
                {
                    var s = files[file];
                    byte[] b = s.ToArray();
                    files.Remove(file);
                    s.Dispose();
                    _SongDownloadManager.ProcessFile(b, file);
                }
                resp.Ok();
            }
            catch (Exception ex)
            {
                Log.LogErr("Exception handling mod install step 1!", ex);
                resp.StatusCode = 500;
            }
        }

        private void HandleCommitConfig(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;
            
            try
            {
                if (!_mod.IsBeatSaberInstalled || !_mod.IsInstalledBeatSaberModded)
                {
                    resp.BadRequest("Modded Beat Saber is not installed!");
                    ShowToast("Can't commit config.", "Modded Beat Saber is not installed!");
                    return;
                }
                ShowToast("Saving Config", "Do not turn off the Quest or exit the app!", ToastType.Warning, 8);
                //todo: decide if this really needs to be called again, or if things are going to do their own updates as they go along
                Engine.UpdateConfig(CurrentConfig.Config);
                Engine.Save();
                CurrentConfig.IsCommitted = true;
                SendConfigChangeMessage();
                resp.Ok();
            }
            catch (Exception ex)
            {
                Log.LogErr("Exception handling mod install step 1!", ex);
                resp.StatusCode = 500;
            }
        }

        private void HandleModInstallStep2(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;
            if (!Monitor.TryEnter(_modInstallLock))
                resp.BadRequest("Another install request is in progress.");
            try
            {
                try
                {
                    if (!_mod.DoesTempApkExist)
                    {
                        resp.BadRequest("Step 1 has not completed, temporary APK does not exist!");
                        return;
                    }
                    _mod.ApplyModToTempApk();
                    SendMessageToClient(new HostSetupEvent() { SetupEvent = SetupEventType.Step2Complete });
                    resp.Ok();
                }
                catch (Exception ex)
                {
                    Log.LogErr("Exception handling mod install step 2!", ex);
                    resp.StatusCode = 500;
                }
            }
            finally
            {
                Monitor.Exit(_modInstallLock);
            }
        }

        private void HandleModInstallStep3(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;
            if (!Monitor.TryEnter(_modInstallLock))
                resp.BadRequest("Another install request is in progress.");
            try
            {
                try
                {
                    if (!_mod.DoesTempApkExist)
                    {
                        resp.BadRequest("Step 2 has not completed, temporary APK does not exist!");
                        return;
                    }
                    _mod.TriggerPackageInstall();
                    SendMessageToClient(new HostSetupEvent() { SetupEvent = SetupEventType.Step3Complete });
                    resp.Ok();
                }
                catch (Exception ex)
                {
                    Log.LogErr("Exception handling mod install step 2!", ex);
                    resp.StatusCode = 500;
                }
            }
            finally
            {
                Monitor.Exit(_modInstallLock);
            }
        }

        private object _playlistCoverLock = new object();
        private void HandleGetPlaylistCover(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;
            lock (_playlistCoverLock)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(req.Url.Query))
                    {
                        resp.BadRequest("Expected playlistid");
                        return;
                    }
                    string playlistid = null;
                    foreach (string kvp in req.Url.Query.TrimStart('?').Split("&"))
                    {
                        var split = kvp.Split('=');
                        if (split.Count() < 1)
                            continue;
                        if (split[0].ToLower() == "playlistid")
                        {
                            playlistid = Java.Net.URLDecoder.Decode(split[1]);
                            break;
                        }
                    }
                    if (string.IsNullOrEmpty(playlistid))
                    {
                        resp.BadRequest("Expected playlistid");
                        return;
                    }
                    var playlist = CurrentConfig.Config.Playlists.FirstOrDefault(x => x.PlaylistID == playlistid);
                    if (playlist == null)
                    {
                        resp.NotFound();
                        return;
                    }
                    var imgBytes = playlist.TryGetCoverPngBytes();
                    if (imgBytes == null)
                    {
                        resp.Error();
                        return;
                    }
                    resp.StatusCode = 200;
                    resp.ContentType = MimeMap.GetMimeType("test.png");
                    using (MemoryStream ms = new MemoryStream(imgBytes))
                    {
                        ms.CopyTo(resp.OutputStream);
                    }
                    
                }
                catch (Exception ex)
                {
                    Log.LogErr("Exception handling get playlist cover!", ex);
                    resp.StatusCode = 500;
                }
            }
        }

        private object _configLock = new object();
        private void HandleGetConfig(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;
            lock (_configLock)
            {
                try
                {
                    resp.Serialize(CurrentConfig);
                }
                catch (Exception ex)
                {
                    Log.LogErr("Exception getting config!", ex);
                    resp.StatusCode = 500;
                }
            }
        }
        
        private void HandlePutConfig(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;
            lock (_configLock)
            {
                try
                {
                    BeatSaberQuestomConfig config;
                    //todo: check content type header
                    using (JsonTextReader jtr = new JsonTextReader(new StreamReader(req.InputStream)))
                    {
                        config = (new JsonSerializer()).Deserialize<BeatSaberQuestomConfig>(jtr);
                    }
                    Log.LogMsg("Read new config on put config");
                    if (config == null)
                    {
                        throw new Exception("Error deserializing config!");
                    }
                    UpdateConfig(config);
                }
                catch (Exception ex)
                {
                    Log.LogErr("Exception getting config!", ex);
                    resp.StatusCode = 500;
                }
            }
        }

        private const int MAX_SONG_COVER_REQS = 15;
        private object _songCounterLock = new object();
        private int _songRequestCounter = 0;
        private void HandleGetSongCover(HttpListenerContext context)
        {
            try
            {
                var req = context.Request;
                var resp = context.Response;
                lock (_songCounterLock)
                {
                    _songRequestCounter++;
                    if (_songRequestCounter > MAX_SONG_COVER_REQS)
                    {
                        resp.StatusCode = 429;
                        return;
                    }
                }


                lock (_playlistCoverLock)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(req.Url.Query))
                        {
                            resp.BadRequest("Expected songid");
                            return;
                        }
                        string songid = null;
                        foreach (string kvp in req.Url.Query.TrimStart('?').Split("&"))
                        {
                            var split = kvp.Split('=');
                            if (split.Count() < 1)
                                continue;
                            if (split[0].ToLower() == "songid")
                            {
                                songid = Java.Net.URLDecoder.Decode(split[1]);
                                break;
                            }
                        }
                        if (string.IsNullOrEmpty(songid))
                        {
                            resp.BadRequest("Expected songid");
                            return;
                        }
                        var song = CurrentConfig.Config.Playlists.SelectMany(x => x.SongList).FirstOrDefault(x => x.SongID == songid);
                        if (song == null)
                        {
                            resp.NotFound();
                            return;
                        }
                        var imgBytes = song.TryGetCoverPngBytes();
                        if (imgBytes == null)
                        {
                            resp.Error();
                            return;
                        }
                        resp.StatusCode = 200;
                        resp.ContentType = MimeMap.GetMimeType("test.png");
                        resp.AppendHeader("Cache-Control", "max-age=86400, public");
                        using (MemoryStream ms = new MemoryStream(imgBytes))
                        {
                            ms.CopyTo(resp.OutputStream);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.LogErr("Exception handling get song cover!", ex);
                        resp.StatusCode = 500;
                    }
                }
            }
            finally
            {
                lock(_songCounterLock)
                {
                    _songRequestCounter--;
                }
            }
        }

        private void HandleUninstallBeatSaber(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;
            if (!Monitor.TryEnter(_modInstallLock))
                resp.BadRequest("Another mod request is in progress.");
            try
            {

                try
                {
                    if (!_mod.IsBeatSaberInstalled)
                    {
                        ShowToast("Beat Saber Not Installed", "Beat Saber doesn't seem to be installed.", ToastType.Error, 8);
                        resp.BadRequest("Beat Saber isn't installed.");
                        return;
                    }
                    _mod.UninstallBeatSaber();
                    resp.Ok();
                }
                catch (Exception ex)
                {
                    Log.LogErr("Exception handling mod install step 1!", ex);
                    resp.StatusCode = 500;
                }
            }
            finally
            {
                Monitor.Exit(_modInstallLock);
            }
        }

        private void HandleResetAssets(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;
            if (!Monitor.TryEnter(_modInstallLock))
                resp.BadRequest("Another mod request is in progress.");
            try
            {

                try
                {
                    if (!_mod.IsBeatSaberInstalled && !_mod.IsInstalledBeatSaberModded)
                    {
                        ShowToast("Mod Not Installed", "The mod does not appear to be installed correctly.", ToastType.Error, 8);
                        resp.BadRequest("The mod does not appear to be installed correctly.");
                        return;
                    }
                    _currentConfig = null;
                    _qae.Dispose();
                    _qae = null;
                    _mod.ResetAssets();
                    SendConfigChangeMessage();
                    resp.Ok();
                }
                catch (Exception ex)
                {
                    Log.LogErr("Exception handling mod install step 1!", ex);
                    resp.StatusCode = 500;
                }
            }
            finally
            {
                Monitor.Exit(_modInstallLock);
            }
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

