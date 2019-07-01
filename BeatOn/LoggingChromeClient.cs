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

namespace BeatOn
{
    public class LoggingChromeClient : WebChromeClient
    {
        public override bool OnConsoleMessage(ConsoleMessage consoleMessage)
        {
            Android.Util.Log.Info("BeatOn", $"WebView Console: {consoleMessage.Message()} (line {consoleMessage.LineNumber()} of {consoleMessage.SourceId()}");
            return base.OnConsoleMessage(consoleMessage);
        }
    }
}