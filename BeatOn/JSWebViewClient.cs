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
using QuestomAssets;

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
            WebView.Settings.CacheMode = CacheModes.Default;
         //   WebView.Clickable = true;
           // WebView.Focusable = false;
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
            //code to intercept all requests and remove unfriendly iframe headers.  seems to slow things down a bit.
            //try
            //{
            //    string uri = request.Url.ToString();
            //    //todo: more betterer
            //    if (request.IsForMainFrame || request.IsRedirect || uri.ToLower().EndsWith(".zip"))
            //        return base.ShouldInterceptRequest(view, request);
            //    HttpWebRequest req = new HttpWebRequest(new Uri(request.Url.ToString()));
            //    req.AllowAutoRedirect = false;
            //    req.Method = request.Method;
            //    foreach (var header in request.RequestHeaders)
            //    {
            //        try
            //        {
            //            req.Headers.Add(header.Key, header.Value);
            //        }
            //        catch
            //        {
            //            Log.LogMsg($"failed to add header {header.Key}");
            //        }
            //    }
            //    HttpWebResponse resp;
            //    try
            //    {
            //        resp = req.GetResponse() as HttpWebResponse;
            //    }
            //    catch (WebException wex)
            //    {
            //        Log.LogMsg("Request intercept got a bad response");
            //        resp = wex.Response as HttpWebResponse;
            //    }
            //    var contentType = resp.ContentType.Split(';')[0];
            //    if (contentType =="application/zip" || contentType == "application/octet-stream" || contentType == "application/x-zip-compressed" || contentType == "multipart/x-zip")
            //    {
            //        Log.LogMsg("Oops, intercepted a download.  Aborting and handing back to the browser.");
            //        resp.Close();
            //        return base.ShouldInterceptRequest(view, request);
            //    }
            //    Log.LogMsg($"contettype: {resp.ContentType} encoding:{resp.Headers[HttpRequestHeader.ContentEncoding]}");
            //    var responseHeaders = new Dictionary<string, string>();
            //    for (int i = 0; i < resp.Headers.Count; i++)
            //    {
            //        try
            //        {
            //            var key = resp.Headers.GetKey(i);
            //            if (key.ToLower().StartsWith("x-frame"))
            //            {
            //                Log.LogMsg("Skipped x-frame header!  muahaha");
            //                continue;
            //            }


            //            var val = resp.Headers.Get(i);

            //            responseHeaders.Add(key, val);
            //        }
            //        catch (Exception ex)
            //        {
            //            Log.LogErr("exception with header", ex);
            //            try
            //            {
            //                Log.LogErr($"key was {resp.Headers.GetKey(i)}");
            //            }
            //            catch
            //            {

            //            }
            //        }
            //    }


            //    var response = new WebResourceResponse(resp.ContentType.Split(';')[0], resp.ContentEncoding, (int)resp.StatusCode, resp.StatusDescription, responseHeaders, resp.GetResponseStream());

            //    return response;

            //}
            //catch (Exception ex)
            //{
            //    Log.LogErr("Request intercept exception!", ex);
            //    return base.ShouldInterceptRequest(view, request);
            //}
        }

        public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
        {
            return base.ShouldOverrideUrlLoading(view, request);
        }
    }
}