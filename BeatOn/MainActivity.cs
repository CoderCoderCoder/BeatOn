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
using Android.Hardware.Display;
using Android.Animation;
using Android.Views.Animations;

namespace BeatOn
{
    [Activity(Name = "com.emulamer.beaton.MainActivity", Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : Activity
    {
        public MainActivity()
        {
            //prevent duplicate log sinks of these types from getting registered.
            Log.ClearSinksOfType<AndroidLogger>((x) => true);
            Log.SetLogSink(new AndroidLogger());

            QuestomAssets.Utils.FileUtils.GetTempDirectoryOverride = () =>
            {
                try
                {
                    var appCache = Path.Combine(Application.Context.GetExternalFilesDir(null).AbsolutePath, "cache");
                    if (!Directory.Exists(appCache))
                        Directory.CreateDirectory(appCache);
                    return appCache;
                }
                catch (Exception ex)
                {
                    Log.LogErr("Exception trying to get/make external cache folder!", ex);
                    throw ex;
                }
            };

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

        private void ShowToast(string title, string message, ToastType type = ToastType.Info, float durationSec = 3.0F)
        {
            _toastInjector.ShowToast(title, message, type, durationSec);
        }


        WebView _browserView;
        ToastInjectorWebViewClient _toastInjector;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);


            base.OnCreate(savedInstanceState);
            _webView = FindViewById<WebView>(Resource.Id.webView1);
            _browserView= FindViewById<WebView>(Resource.Id.webView2);
            _toastInjector = new ToastInjectorWebViewClient(_browserView);
            _browserView.SetWebViewClient(_toastInjector);
            _browserView.SetWebChromeClient(new LoggingChromeClient());
            _browserView.Settings.JavaScriptEnabled = true;
            _browserView.Settings.AllowContentAccess = true;
            _browserView.Settings.CacheMode = CacheModes.Default;
            _browserView.Focusable = true;
            _browserView.Settings.MediaPlaybackRequiresUserGesture = false;

            _browserView.Download += _webView_Download;
            _toastInjector.Download += _toastInjector_Download;

            _webViewClient = new JSWebViewClient(this, _webView);
            _webViewClient.JSInterface.OnBrowserGoBack += JSInterface_OnBrowserGoBack;
            _webViewClient.JSInterface.OnHideBrowser += JSInterface_OnHideBrowser;
            _webViewClient.JSInterface.OnNavigateBrowser += JSInterface_OnNavigateBrowser;
            _webViewClient.JSInterface.OnRefreshBrowser += JSInterface_OnRefreshBrowser;
            _webViewClient.JSInterface.OnShowBrowser += JSInterface_OnShowBrowser;
            _webViewClient.JSInterface.OnToast += JSInterface_OnToast;
            

            if (CheckSelfPermission(Android.Manifest.Permission.WriteExternalStorage)
                != Android.Content.PM.Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new String[] { Android.Manifest.Permission.WriteExternalStorage }, 1);
            }
            else
            {
                ContinueLoad();
            }
        }

        private void _toastInjector_Download(object sender, string e)
        {
            _broadcastReceiver.SendDownloadUrl(new DownloadUrlInfo() { Url = e, MimeType = "" });
        }

        private void JSInterface_OnToast(object sender, JSInterface.ToastMsg e)
        {
            Log.LogMsg("got toast from JSInterface");
            RunOnUiThread(() => { 
                ShowToast(e.Title, e.Message, e.Type, e.Duration);
            });
        }

        public override bool DispatchKeyEvent(KeyEvent e)
        {
            if (e.Action == KeyEventActions.Down && e.KeyCode == Keycode.DpadDown)
            {
                SmoothScrollBrowser(true);
                return true;
            }
            else if (e.Action == KeyEventActions.Down && e.KeyCode == Keycode.DpadUp)
            {
                SmoothScrollBrowser(false);
                return true;
            }
            else
            {
                return base.DispatchKeyEvent(e);
            }
        }
        private ObjectAnimator _currentScroll = null;
        private void SmoothScrollBrowser(bool down)
        {
            ObjectAnimator anim = ObjectAnimator.OfInt(_browserView, "scrollY", _browserView.ScrollY, _browserView.ScrollY + (down ? 300 : -300));
            if (_currentScroll != null)
            {
                _currentScroll.End();
                anim.SetInterpolator(new DecelerateInterpolator());
            }
            else
            {
                anim.SetInterpolator(new AccelerateDecelerateInterpolator());
            }
            anim.SetDuration(500).Start();
            _currentScroll = anim;
        }

        public override bool DispatchGenericMotionEvent(MotionEvent ev)
        {
            //if (ev.Action == MotionEventActions.PointerIndexShift)
            //{
            //    var vscroll = ev.GetAxisValue(Axis.Vscroll, ev.ActionIndex);
            //    if (Math.Abs(vscroll) > 0.04F)
            //    {
            //        //scrolled
            //    }
            //}
            return base.DispatchGenericMotionEvent(ev);
        }



        private void JSInterface_OnShowBrowser(object sender, int e)
        {
            RunOnUiThread(() => {
                int dp = 0;
                Log.LogMsg("Trying to show browser.. checking position");
                try
                {
                    Android.Util.DisplayMetrics metrics = new Android.Util.DisplayMetrics();
                    WindowManager.DefaultDisplay.GetMetrics(metrics);
                    float density = metrics.Density;
                    Log.LogMsg($"density is {density}");
                    dp = (int)Math.Ceiling(e*density );
                } catch (Exception ex)
                {
                    Log.LogErr("couldn't get dp");
                    return;
                }
                try
                {
                    _webView.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, dp);
                }
                catch (Exception ex)
                {
                    Log.LogErr("coudln't set _webView layout", ex);
                    return;
                }
                Log.LogMsg($"Putting browser at {dp}");
            });
        }


        private void JSInterface_OnRefreshBrowser(object sender, EventArgs e)
        {
            RunOnUiThread(() =>
            {
                _browserView.Reload();
            });
        }

        private void JSInterface_OnNavigateBrowser(object sender, string e)
        {
            RunOnUiThread(() =>
            {
                _browserView.LoadUrl(e);
            });
        }

        private void JSInterface_OnHideBrowser(object sender, EventArgs e)
        {
            RunOnUiThread(() =>
            {
                _webView.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.MatchParent);
            });
        }

        private void JSInterface_OnBrowserGoBack(object sender, EventArgs e)
        {
            RunOnUiThread(() =>
            {
                _browserView.GoBack();
            });
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
            Log.LogMsg("Starting service");
            StartService(serviceToStart);
            Log.LogMsg("Started service");
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

