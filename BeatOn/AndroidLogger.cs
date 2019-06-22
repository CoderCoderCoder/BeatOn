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
using QuestomAssets;

namespace BeatOn
{
    public class AndroidLogger : ILog
    {
        public void LogErr(string message, Exception ex)
        {
            Android.Util.Log.Error("BeatOn", $"{message} {ex.Message} {ex.StackTrace} {ex?.InnerException?.Message} {ex?.InnerException?.StackTrace}");
        }

        public void LogErr(string message, params object[] args)
        {
            Android.Util.Log.Error("BeatOn", String.Format(message, args));
        }

        public void LogMsg(string message, params object[] args)
        {
            string logStr = message;
            if (args.Length > 0 && !(args[0] is object[] && ((object[])args[0]).Length < 1))
            {
                try
                {
                    logStr = string.Format(logStr, args);
                }
                catch
                { }
            }
            Android.Util.Log.Info("BeatOn", logStr);
        }
    }
}