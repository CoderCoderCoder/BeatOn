using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json.Linq;
using QuestomAssets;

namespace BeatOn
{
    public static class BeatSaverUtils
    {
        public static string GetDownloadUrl(string apiURL)
        {
            WebClient client = new WebClient();
            try
            {
                var bsaver = client.DownloadString(apiURL);
                JToken jt = JToken.Parse(bsaver);
                return jt.Value<string>("downloadURL");
            }
            catch  (WebException wex)
            {
                Log.LogMsg($"BeatSaver failed to get info for {apiURL}, status {wex.Status}");
                return null;
            }
        }
    }
}