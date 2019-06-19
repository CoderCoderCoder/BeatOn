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

        private const string JS_FUNC = "function invokeNative(data){beatOnJSBridge.invokeNative(data);}";

        private const string JS_SEND_MESSAGE = "window.dispatchEvent(new MessageEvent('host-message', {{ data: {0}}}));";
        

        private JSBridge _bridge;
        private Activity _activity;
        public WebView WebView { get; private set; }

        public void SendHostMessage(HostMessage message)
        {
            _activity.RunOnUiThread(() =>
            {
                var msg = string.Format(JS_SEND_MESSAGE, JsonConvert.SerializeObject(message));
                WebView.EvaluateJavascript(msg, null);
            });

        }

        public JSWebViewClient(Activity activity, WebView webView, Action<string> jsInvoker)
        {
            _activity = activity;
            _bridge = new JSBridge(jsInvoker);
            WebView = webView;
            SetupWebView();
        }

        private void SetupWebView()
        {
            WebView.SetWebViewClient(this);
            this.WebView.AddJavascriptInterface(_bridge, "beatOnJSBridge");
            WebView.Settings.JavaScriptEnabled = true;
            WebView.Settings.AllowContentAccess = true;
           // WebView.Settings.SafeBrowsingEnabled = false;
        }

        public override void OnPageFinished(WebView view, string url)
        {
            base.OnPageFinished(view, url);
            view.EvaluateJavascript($"javascript: {JS_FUNC}", null);
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