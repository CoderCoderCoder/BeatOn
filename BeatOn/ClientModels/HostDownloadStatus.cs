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
    public class HostDownloadStatus : HostMessage
    {
        public override MessageType Type => MessageType.DownloadStatus;
        public List<HostDownload> Downloads { get; set; } = new List<HostDownload>();
    }
}