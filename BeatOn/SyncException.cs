using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace BeatOn
{
    public class SyncException : Exception
    {
        public SyncException(string message) : base(message)
        { }
        public SyncException (string message, Exception innerException) : base(message, innerException)
        { }
    }
}