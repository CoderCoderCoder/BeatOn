using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
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
    [Activity(Name= "com.emulamer.beaton.MainActivity", Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
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
        private BeatSaberQuestomConfig _currentConfig;

        private QuestomAssetsEngine Engine
        {
            get
            {
                if (_qae == null)
                    _qae = new QuestomAssetsEngine(_qaeConfig);
                return _qae;
            }
        }

        private BeatSaberQuestomConfig CurrentConfig
        {
            get
            {
                if (_currentConfig == null)
                    _currentConfig = Engine.GetCurrentConfig();

                return _currentConfig;
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

        private void SaveCurrentConfig()
        {
            if (_currentConfig == null)
                return;

            try
            {
                Engine.UpdateConfig(CurrentConfig);
            }
            catch (Exception ex)
            {
                Log.LogErr("Exception updating config", ex);
                ShowToast("Unable to save configuration", "There was an error saving the configuration!", ToastType.Error, 5);
            }
            //todo: not sure if I really need to do this, it's faster without forcing a reload
            // _currentConfig = null;
            // _qae = null;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            QuestomAssets.Log.SetLogSink(new AndroidLogger());
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
            //TODO: check that we actually got these

            //has to be the activity context to do the package manager stuff
            _mod = new Mod(this);
            _mod.StatusUpdated += _mod_StatusUpdated;
            _webView = FindViewById<WebView>(Resource.Id.webView1);
            _SongDownloadManager = new DownloadManager(() => { return Engine; }, () => { return CurrentConfig; }, _qaeConfig.SongFileProvider, _qaeConfig.SongsPath);
            _SongDownloadManager.StatusChanged += _SongDownloadManager_StatusChanged;
            _webView.Download += _webView_Download;
            SetupWebApp();
        }

        private void _SongDownloadManager_StatusChanged(object sender, DownloadStatusChangeArgs e)
        {
            RunOnUiThread(() =>
            {
                var dl = sender as Download;
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
            });
        }

        private void _mod_StatusUpdated(object sender, string e)
        {
            _webViewClient.SendHostMessage(new HostSetupEvent() { SetupEvent = SetupEventType.StatusMessage, Message = e });
        }

        private void ShowToast(string title, string message, ToastType type = ToastType.Info, float durationSec = 3.0F)
        {
            _webViewClient.SendHostMessage(new HostShowToast() { Title = title, Message = message, ToastType = type, Timeout = (int)(durationSec * 1000) });
        }

        private void _webView_Download(object sender, DownloadEventArgs e)
        {
            if (e.Mimetype != "application/zip")
            {
                ShowToast("Unable to Download", "File isn't a zip file!  Not downloading it.", ToastType.Error, 8);
                return;
            }
            var uri = new Uri(e.Url);
            ShowToast("Starting Download...", uri.ToString(), ToastType.Info, 2);

            var fileName = Path.GetFileNameWithoutExtension(uri.LocalPath);
            if (_qaeConfig.FileProvider.FileExists(Path.Combine(_qaeConfig.SongsPath, fileName)))
            {
                ShowToast("Unable to Download", "A custom song folder with the name of this zip already exists.  Not downloading it.", ToastType.Error, 8);
                return;
            }
            _SongDownloadManager.DownloadFile(e.Url);
        }

        

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void JavascriptAction(string data)
        {

        }

        private void SetupWebApp()
        {
            _webServer = new WebServer(Assets, "www");
            _webServer.Router.AddRoute("GET", "beatsaber/config", HandleGetConfig);
            _webServer.Router.AddRoute("PUT", "beatsaber/config", HandlePutConfig);
            _webServer.Router.AddRoute("GET", "beatsaber/songcover", HandleGetSongCover);
            _webServer.Router.AddRoute("GET", "beatsaber/playlistcover", HandleGetPlaylistCover);
            _webServer.Router.AddRoute("GET", "mod/status", HandleModStatus);
            _webServer.Router.AddRoute("POST", "mod/install/step1", HandleModInstallStep1);
            _webServer.Router.AddRoute("POST", "mod/install/step2", HandleModInstallStep2);
            _webServer.Router.AddRoute("POST", "mod/install/step3", HandleModInstallStep3);
            _webServer.Router.AddRoute("POST", "/mod/resetassets", HandleResetAssets);
            _webServer.Router.AddRoute("POST", "/mod/uninstallbeatsaber", HandleUninstallBeatSaber);
            
            _webServer.Start();
            _webViewClient = new JSWebViewClient(this, _webView, JavascriptAction);
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
                        else if (_mod.DoesTempApkExist)
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
                        _webViewClient.SendHostMessage(new HostSetupEvent() { SetupEvent = SetupEventType.StatusMessage, Message = "Beat Saber is not installed!  Install Beat Saber and come back." });
                        _webViewClient.SendHostMessage(new HostSetupEvent() { SetupEvent = SetupEventType.Error, Message = "Beat Saber is not installed!"});
                        return;
                    }
                    _mod.CopyOriginalBeatSaberApkAndTriggerUninstall();
                    _webViewClient.SendHostMessage(new HostSetupEvent() { SetupEvent = SetupEventType.Step1Complete });
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
                    _webViewClient.SendHostMessage(new HostSetupEvent() { SetupEvent = SetupEventType.Step2Complete });
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
                    _webViewClient.SendHostMessage(new HostSetupEvent() { SetupEvent = SetupEventType.Step3Complete });
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
                    var playlist = CurrentConfig.Playlists.FirstOrDefault(x => x.PlaylistID == playlistid);
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
                    Log.LogErr("Exception handling mod install step 2!", ex);
                    resp.StatusCode = 500;
                }
            }
        }

        private object _qaeLock = new object();
        private void HandleGetConfig(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;
            lock (_qaeLock)
            {
                try
                {
                    var qae = new QuestomAssetsEngine(_qaeConfig);
                    var config = qae.GetCurrentConfig();
                    resp.Serialize(config);
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

        }

        private void HandleGetSongCover(HttpListenerContext context)
        {

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

