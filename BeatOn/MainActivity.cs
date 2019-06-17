using System;
using System.IO;
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

namespace BeatOn
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private WebView _webView;
        private JSWebViewClient _webViewClient;
        private WebServer _webServer;
        private Mod _mod;

        private QaeConfig _qaeConfig = new QaeConfig()
        {
            FileProvider = new FolderFileProvider(Constants.ROOT_BEAT_ON_DATA_PATH, false),
            PlaylistArtPath = "Art",
            AssetsPath = "BeatSaberAssets",
            ModsPath = "Mods",
            SongsPath = "CustomSongs"
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

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
            _webView.Download += _webView_Download;
            SetupWebApp();
        }

        private void _mod_StatusUpdated(object sender, string e)
        {
            _webViewClient.SendMessage(e);
        }

        private void _webView_Download(object sender, DownloadEventArgs e)
        {

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
            _webServer.Router.AddRoute("GET", "beatsaber/songcovers", HandleGetSongCover);
            _webServer.Router.AddRoute("GET", "mod/status", HandleModStatus);
            _webServer.Router.AddRoute("POST", "mod/install/step1", HandleModInstallStep1);
            _webServer.Router.AddRoute("POST", "mod/install/step2", HandleModInstallStep2);
            _webServer.Router.AddRoute("POST", "mod/install/step3", HandleModInstallStep3);
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
                    /*THIS IS TEST CODE FOR EMULATOR, REMOVE IT
                     * 
                     */
                    resp.Serialize(new ModStatus()
                    {
                        IsBeatSaberInstalled = true,
                        CurrentStatus = ModStatusType.ModInstalled
                    });
                    return;

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
                        return;
                    }
                    _mod.CopyOriginalBeatSaberApkAndTriggerUninstall();
                    _webViewClient.SendEvent(ClientEventType.Step1Complete);
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
                    _webViewClient.SendEvent(ClientEventType.Step2Complete);
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
                    _webViewClient.SendEvent(ClientEventType.Step3Complete);
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
    }


    public static class ResponseExtensions
    {
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

