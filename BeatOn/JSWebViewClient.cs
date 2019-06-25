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
using Newtonsoft.Json;

namespace BeatOn
{
    public class JSWebViewClient : WebViewClient
    {

        public class LoggingChromeClient : WebChromeClient
        {
            public override bool OnConsoleMessage(ConsoleMessage consoleMessage)
            {
                Android.Util.Log.Info("BeatOn", $"WebView Console: {consoleMessage.Message()} (line {consoleMessage.LineNumber()} of {consoleMessage.SourceId()}");
                return base.OnConsoleMessage(consoleMessage);
            }
        }

        private Activity _activity;
        public WebView WebView { get; private set; }

        public JSWebViewClient(Activity activity, WebView webView)
        {
            _activity = activity;
            webView.SetWebChromeClient(new LoggingChromeClient());
            WebView = webView;
            SetupWebView();
        }

        private void SetupWebView()
        {
            WebView.SetWebViewClient(this);
            WebView.Settings.JavaScriptEnabled = true;
            WebView.Settings.AllowContentAccess = true;
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
            return base.ShouldOverrideUrlLoading(view, request);
        }
    }
}