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
using Android.Content;
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
using Android.Gestures;
using Android.AccessibilityServices;


namespace BeatOn
{
    [Activity(Name = "com.emulamer.beaton.MainActivity", Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : Activity
    {
        public MainActivity()
        {
            
            QuestomAssets.Utils.ImageUtils.Instance = new ImageUtilsDroid();
        }



        private WebView _webView;
        private JSWebViewClient _webViewClient;
        private BeatOnServiceTransceiver _broadcastReceiver;

        private void _webView_Download(object sender, DownloadEventArgs e)
        {
            _broadcastReceiver.SendDownloadUrl(new DownloadUrlInfo() { Url = e.Url, MimeType = e.Mimetype });
        }

        //public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        //{
        //    var displayMetrics = Resources.DisplayMetrics;
        //    int midY = displayMetrics.HeightPixels / 2;
        //    int midX = displayMetrics.WidthPixels / 2;
        //    var builder = new GestureDescription.Builder();
        //    Android.Graphics.Path p = new Android.Graphics.Path();
        //    p.MoveTo(midX, midY);
        //    p.LineTo(midX, displayMetrics.HeightPixels);
        //    builder.AddStroke(new GestureDescription.StrokeDescription(p, 100, 50));
        //    var handler = new Handler();
            
        //    // _webView.DispatchKeyEvent(new KeyEvent( KeyEventActions.Down, Keycode.PageDown)
        //    switch (e.KeyCode)
        //    {
               
        //        case Keycode.DpadDown:
        //            DispatchGenericMotionEvent(MotionEvent.Obtain(SystemClock.UptimeMillis(), SystemClock.UptimeMillis(), MotionEventActions.Scroll, 50, 50, 0));
        //            return base.OnKeyDown(keyCode, e);
        //            //handler.PostDelayed(() =>
        //            //{
        //            //    _webView.DispatchGenericMotionEvent(MotionEvent.Obtain(SystemClock.UptimeMillis(), SystemClock.UptimeMillis(), (int)MotionEventActions.Move, midX, midY, 0));
        //            //}, 50);
        //            //handler.PostDelayed(() =>
        //            //{
        //            //    _webView.DispatchGenericMotionEvent(MotionEvent.Obtain(SystemClock.UptimeMillis(), SystemClock.UptimeMillis(), (int)MotionEventActions.Move, midX, midY + 200, 0));
        //            //}, 100);
        //            //handler.PostDelayed(() =>
        //            //{
        //            //    _webView.DispatchGenericMotionEvent(MotionEvent.Obtain(SystemClock.UptimeMillis(), SystemClock.UptimeMillis(), (int)MotionEventActions.Move, midX, midY + 400, 0));
        //            //}, 150);
                   


        //            break;
        //        case Keycode.DpadUp:
                    
        //            break;
        //        case Keycode.ButtonX:
        //            _webView.PerformClick();
        //            break;
        //        default:
                    
        //            break;
        //    }
        //    return base.OnKeyDown(keyCode, e);
        //}

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Log.SetLogSink(new AndroidLogger());
            Log.SetLogSink(new FileLogger(Constants.LOGFILE));
            base.OnCreate(savedInstanceState);
            _webView = FindViewById<WebView>(Resource.Id.webView1);
            _webView.Download += _webView_Download;
            //has to be the activity context to do the package manager stuff

            //don't force the config to load
            //var x = CurrentConfig;

            _webViewClient = new JSWebViewClient(this, _webView);
            
            //_webView.LoadUrl($"http://localhost:50000");

            if (CheckSelfPermission(Android.Manifest.Permission.WriteExternalStorage)
                != Android.Content.PM.Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new String[] { Android.Manifest.Permission.WriteExternalStorage }, 1);
            }
            else
            {
                ContinueLoad();
            }
            //if (CheckSelfPermission(Android.Manifest.Permission.ReadExternalStorage)
            //        != Android.Content.PM.Permission.Granted)
            //{
            //    ActivityCompat.RequestPermissions(this, new String[] { Android.Manifest.Permission.ReadExternalStorage }, 1);
            //}


        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (grantResults.Length > 0 && grantResults[0] != Android.Content.PM.Permission.Granted)
            {
                Android.App.AlertDialog.Builder dlgAlert = new Android.App.AlertDialog.Builder(this);
                dlgAlert.SetMessage("You must give Beat On write access to your external storage or it cannot function.");
                dlgAlert.SetTitle("Beat On Permissions");
                dlgAlert.SetPositiveButton("Try Again", (o, e) =>
                {
                    ActivityCompat.RequestPermissions(this, new String[] { Android.Manifest.Permission.WriteExternalStorage }, 1);
                });
                dlgAlert.SetNegativeButton("Close", (o, e) =>
                {
                    Java.Lang.JavaSystem.Exit(0);
                });
                dlgAlert.SetCancelable(true);

                dlgAlert.Create().Show();
            }
            else
            {
                ContinueLoad();
            }            
        }

        private void ContinueLoad()
        {
            Intent serviceToStart = new Intent(this, typeof(BeatOnService));



            Log.LogMsg("Starting service!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            StartService(serviceToStart);
            Log.LogMsg("Started service!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        }

        protected override void OnStop()
        {            
            //if (_broadcastReceiver != null && _broadcastReceiver.IsRegistered)
            //{
            //    _broadcastReceiver.UnregisterIntents();
            //}
            base.OnStop();
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        //private BeatOnServiceConnection _serviceConnection;
        protected override void OnStart()
        {
            base.OnStart();
            if (_broadcastReceiver == null)
            {
                _broadcastReceiver = new BeatOnServiceTransceiver(this);
                _broadcastReceiver.ServiceStatusInfoReceived += (o, e) =>
                {
                    //_webView.LoadUrl($"http://localhost:50000");
                    _webView.LoadUrl(e.Url);
                    _broadcastReceiver.UnregisterIntents();
                    _broadcastReceiver.RegisterContextForIntents(BeatOnIntent.InstallPackage, BeatOnIntent.UninstallPackage);
                    _broadcastReceiver.UninstallPackageReceived+= (s, p) => {
                        BeatSaberModder m = new BeatSaberModder(this, null, null);
                        m.TriggerPackageUninstall(p.PackageUrl);
                    };
                    _broadcastReceiver.InstallPackageReceived += (s, p) =>
                     {
                         BeatSaberModder m = new BeatSaberModder(this, null, null);
                         m.TriggerPackageInstall(p.PackageUrl);
                     };
                };
            }
            if (!_broadcastReceiver.IsRegistered)
            { 
                _broadcastReceiver.RegisterContextForIntents(BeatOnIntent.ServerStatusInfo);
            }


            //if (_serviceConnection == null)
            //{
            //    this._serviceConnection = new BeatOnServiceConnection(this);
            //}

            //string s = _serviceConnection.WebUrl;

        }


    }


}

