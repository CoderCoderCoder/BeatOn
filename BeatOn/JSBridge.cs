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
using Java.Interop;

namespace BeatOn
{
    public class JSBridge : Java.Lang.Object
    {
        private Action<string> _invoker;

        public JSBridge(Action<string> invoker)
        {
            _invoker = invoker;
        }

        [JavascriptInterface]
        [Export("invokeNative")]
        public void invokeNative(string data)
        {
            _invoker.Invoke(data);
        }
    }
}