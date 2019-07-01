using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using BeatOn.ClientModels;
using Java.Interop;
using QuestomAssets;

namespace BeatOn
{
    public class JSInterface : Java.Lang.Object
    {
        public class ToastMsg
        {
            public string Title { get; set; }
            public string Message { get; set; }
            public float Duration { get; set; }
            public ToastType Type { get; set; }
        }

        public event EventHandler<int> OnShowBrowser;
        public event EventHandler OnHideBrowser;
        public event EventHandler OnRefreshBrowser;
        public event EventHandler OnBrowserGoBack;
        public event EventHandler<string> OnNavigateBrowser;
        public event EventHandler<ToastMsg> OnToast;

        [JavascriptInterface]
        [Export]
        public void showBrowser(int top)
        {
            Log.LogMsg($"Got showBrowser {top}");
            OnShowBrowser?.Invoke(this, top);
        }

        [JavascriptInterface]
        [Export]
        public void hideBrowser()
        {
            Log.LogMsg("Got hideBrowser");
            OnHideBrowser?.Invoke(this, new EventArgs());
        }

        [JavascriptInterface]
        [Export]
        public void refreshBrowser()
        {
            Log.LogMsg("Got refreshBrowser");
            OnRefreshBrowser?.Invoke(this, new EventArgs());
        }

        [JavascriptInterface]
        [Export]
        public void browserGoBack()
        {
            Log.LogMsg("Got browserGoBack");
            OnBrowserGoBack?.Invoke(this, new EventArgs());
        }

        [JavascriptInterface]
        [Export]
        public void navigateBrowser(string url)
        {
            Log.LogMsg($"Got navigateBrowser {url}");
            OnNavigateBrowser?.Invoke(this, url);
        }

        [JavascriptInterface]
        [Export]
        public void showToast(string title, string message, string type, float duration)
        {
            ToastType tt = ToastType.Info;
            if (!Enum.TryParse(type, out tt))
                Log.LogErr($"Couldn't parse toast type from {type}");

            OnToast?.Invoke(this, new ToastMsg() { Title = title, Message = message, Type = tt, Duration = duration });
        }
    }
}