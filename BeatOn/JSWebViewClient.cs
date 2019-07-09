using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using BeatOn.ClientModels;
using Java.Interop;
using Newtonsoft.Json;
using QuestomAssets;

namespace BeatOn
{
    public class BeatOnWebViewClient : WebViewClient
    {
        private const string JS_TRIGGER_BUTTON_EVENT = @"(function() {{ var event = new CustomEvent('appbutton', {{ detail: {{ button: '{0}', x: {1}, y: {2} }} }}); window.dispatchEvent(event); }})();";
        private Activity _activity;
        public WebView WebView { get; private set; }

        public BeatOnWebViewClient(Activity activity, WebView webView)
        {
            _activity = activity;
            webView.SetWebChromeClient(new LoggingChromeClient());
            WebView = webView;
            SetupWebView();
        }

        public JSInterface JSInterface;
        private void SetupWebView()
        {
            JSInterface = new JSInterface();
            WebView.SetWebViewClient(this);
            WebView.AddJavascriptInterface(JSInterface, Constants.JS_INTERFACE_NAME);
            WebView.Settings.JavaScriptEnabled = true;
            WebView.Settings.AllowContentAccess = true;
            WebView.Settings.CacheMode = CacheModes.Default;
            WebView.Settings.MediaPlaybackRequiresUserGesture = false;
            WebView.Focusable = true;
        }

        public enum ButtonType
        {
            Up,
            Down,
            Left,
            Right,
            Center
        }

        public void SendButtonToClient(ButtonType button, float x, float y)
        {
            WebView.EvaluateJavascript(string.Format(JS_TRIGGER_BUTTON_EVENT, button, x, y), null);
        }

        public override void OnPageFinished(WebView view, string url)
        {
            base.OnPageFinished(view, url);
            view.EvaluateJavascript("window.isQuestHosted = function() { return true; }", null);
        }

        public override void OnLoadResource(WebView view, string url)
        {
            Android.Util.Log.WriteLine(Android.Util.LogPriority.Info, "TAG", $"LOADRESOURCE: {url}");
            base.OnLoadResource(view, url);
        }

        public override void OnPageStarted(WebView view, string url, Bitmap favicon)
        {
            base.OnPageStarted(view, url, favicon);
        }

        public override void OnReceivedHttpError(WebView view, IWebResourceRequest request, WebResourceResponse errorResponse)
        {
            base.OnReceivedHttpError(view, request, errorResponse);
        }

        public override void OnReceivedError(WebView view, IWebResourceRequest request, WebResourceError error)
        {
            base.OnReceivedError(view, request, error);
        }

        public override WebResourceResponse ShouldInterceptRequest(WebView view, IWebResourceRequest request)
        {
            return base.ShouldInterceptRequest(view, request);
        }

        public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
        {
            //TODO: check for external links and do something else with them
            return base.ShouldOverrideUrlLoading(view, request);
        }
    }
}