using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using QuestomAssets;

namespace BeatOn
{
    public static class BeatOnExtensions
    {
        public static void WaitForFinish(this IEnumerable<Download> downloads)
        {
            var dlCopy = downloads.ToList();
            using (new LogTiming($"waiting for {dlCopy.Count} downloads to complete"))
            {
                while (dlCopy.Count > 0)
                {
                    var waitOps = dlCopy.Take(64);
                    WaitHandle.WaitAll(waitOps.Select(x => x.DownloadFinishedHandle).ToArray());
                    dlCopy.RemoveAll(x => waitOps.Contains(x));
                }
            }
        }
    }
}