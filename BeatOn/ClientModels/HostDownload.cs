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

namespace BeatOn.ClientModels
{
    public class HostDownload
    {
        public Guid ID { get; set; }
        public string Url { get; set; }
        public DownloadStatus Status { get; set; }
        public int PercentageComplete { get; set; }
    }
}