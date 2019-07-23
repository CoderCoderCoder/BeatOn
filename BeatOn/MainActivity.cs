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

            Log.LogMsg($"Beat On MainActivity v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()} starting up");
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
        private BeatOnWebViewClient _webViewClient;
        private BeatOnServiceTransceiver _broadcastReceiver;
        private bool _isBrowserShown = false;
        private float _lastX;
        private float _lastY;

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
            _browserView.Settings.UserAgentString += " BeatOn_Quest/" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            _browserView.SetWebChromeClient(new WebChromeClient());
            _browserView.Settings.JavaScriptEnabled = true;
            _browserView.Settings.AllowContentAccess = true;
            _browserView.Settings.CacheMode = CacheModes.Default;
            _browserView.Focusable = true;
            _browserView.Settings.MediaPlaybackRequiresUserGesture = false;
            _browserView.Settings.DomStorageEnabled = true;
            _browserView.Settings.DatabaseEnabled = true;
            _browserView.Settings.DatabasePath = "/data/data/" + _browserView.Context.PackageName + "/databases/";

            _browserView.Download += _webView_Download;
            _toastInjector.Download += _toastInjector_Download;

            _webViewClient = new BeatOnWebViewClient(this, _webView);
            _webViewClient.JSInterface.OnBrowserGoBack += JSInterface_OnBrowserGoBack;
            _webViewClient.JSInterface.OnHideBrowser += JSInterface_OnHideBrowser;
            _webViewClient.JSInterface.OnNavigateBrowser += JSInterface_OnNavigateBrowser;
            _webViewClient.JSInterface.OnRefreshBrowser += JSInterface_OnRefreshBrowser;
            _webViewClient.JSInterface.OnShowBrowser += JSInterface_OnShowBrowser;
            _webViewClient.JSInterface.OnToast += JSInterface_OnToast;
            _webView.Settings.UserAgentString += " BeatOn_Quest/" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
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
            //_webView.
            if (e.Action == KeyEventActions.Down && e.KeyCode == Keycode.DpadDown)
            {
                if (_isBrowserShown)
                {
                    SmoothScrollBrowser(true);
                }
                else
                {
                    _webViewClient.SendButtonToClient(BeatOnWebViewClient.ButtonType.Down, _lastX, _lastY);
                }
                return true;
            }
            else if (e.Action == KeyEventActions.Down && e.KeyCode == Keycode.DpadUp)
            {
                if (_isBrowserShown)
                {
                    SmoothScrollBrowser(false);
                }
                else
                {
                    _webViewClient.SendButtonToClient(BeatOnWebViewClient.ButtonType.Up, _lastX, _lastY);
                }
                return true;
            }
            else
            {
                return base.DispatchKeyEvent(e);
            }
        }
        const int SCROLL_ACCEL_INCREMENT = 50;
        const int SCROLL_ACCEL_DELAY_MS = 300;
        private bool? scrollLastDirection;
        private int scrollAccel = 0;
        private DateTime scrollLastTime = DateTime.Now;
        private ObjectAnimator _currentScroll = null;
        private void SmoothScrollBrowser(bool down)
        {
            var delay = (DateTime.Now - scrollLastTime).TotalMilliseconds;
            if (scrollLastDirection.HasValue && scrollLastDirection == down && SCROLL_ACCEL_DELAY_MS > delay)
                scrollAccel += SCROLL_ACCEL_INCREMENT;
            else
                scrollAccel = 0;

            scrollLastTime = DateTime.Now;
            scrollLastDirection = down;
            ObjectAnimator anim = ObjectAnimator.OfInt(_browserView, "scrollY", _browserView.ScrollY, _browserView.ScrollY + (down ? 300+ scrollAccel : -300- scrollAccel));
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
            if (ev.Action == MotionEventActions.HoverMove)
            {
                _lastX = ev.GetX();
                _lastY = ev.GetY();
            }
            
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
                    _isBrowserShown = true;
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
        DateTime lastNav = DateTime.Now;
        private void JSInterface_OnNavigateBrowser(object sender, string e)
        {
            RunOnUiThread(() =>
            {
                if ((DateTime.Now-lastNav).TotalMilliseconds < 800)
                    return;
                lastNav = DateTime.Now;
                _browserView.StopLoading();
                _browserView.LoadUrl(e);
            });
        }

        private void JSInterface_OnHideBrowser(object sender, EventArgs e)
        {
            RunOnUiThread(() =>
            {
                _webView.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.MatchParent);
                _isBrowserShown = false;
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
            try
            {
                ActivityManager am = (ActivityManager)GetSystemService(Context.ActivityService);
                am.KillBackgroundProcesses("com.beatgames.beatsaber");
            }
            catch (Exception ex)
            {
                Log.LogErr("Exception trying to kill background process for beatsaber.", ex);
            }
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
                    _broadcastReceiver.HardQuitReceived += (s, i) =>
                    {
                        Intent serviceToStart = new Intent(this, typeof(BeatOnService));
                        Log.LogMsg("Stopping service");
                        StopService(serviceToStart);
                        Log.LogMsg("Service Stopped");
                        Log.LogMsg("Killing app");
                        Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
                        Java.Lang.JavaSystem.Exit(0);
                        Log.LogMsg("Should be dead");
                    };
                    _broadcastReceiver.IntentActionReceived += (s, i) =>
                     {
                         if (i.Type == IntentActionType.Exit)
                         {
                             var intent = new Intent("com.oculus.system_activity");
                             intent.SetPackage(i.PackageName);
                             intent.PutExtra("intent_pkg", "com.oculus.vrshell");
                             intent.PutExtra("intent_cmd", "{\"Command\":\"exitToHome\", \"PlatformUIVersion\":3, \"ToPackage\":\""+ i.PackageName+"\"}");
                             SendBroadcast(intent);
                             intent.PutExtra("intent_cmd", "{\"Command\":\"returnToLauncher\", \"PlatformUIVersion\":3, \"ToPackage\":\""+ i.PackageName+"\"}");
                             SendBroadcast(intent);
                         }
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

