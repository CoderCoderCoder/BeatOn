using System;
using System.Net;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Views;
using Android.Webkit;
using Android.Widget;

namespace BeatOn
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private WebView _webView;
        private JSWebViewClient _webViewClient;
        private WebServer _webServer;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            if (CheckSelfPermission(Android.Manifest.Permission.WriteExternalStorage)
                != Android.Content.PM.Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new String[] { Android.Manifest.Permission.WriteExternalStorage}, 1);
            }
            if (CheckSelfPermission(Android.Manifest.Permission.ReadExternalStorage)
                    != Android.Content.PM.Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new String[] { Android.Manifest.Permission.ReadExternalStorage }, 1);
            }
            //TODO: check that we actually got these

            _webView = FindViewById<WebView>(Resource.Id.webView1);
            _webView.Download += _webView_Download;
            SetupWebApp();
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
            _webServer.Start();
            _webViewClient = new JSWebViewClient(_webView, JavascriptAction);
            _webView.LoadUrl($"http://localhost:{_webServer.Port}");
        }

        private void HandleGetConfig(HttpListenerContext context)
        {

        }

        private void HandlePutConfig(HttpListenerContext context)
        {

        }

        private void HandleGetSongCover(HttpListenerContext context)
        {

        }
    }


}

