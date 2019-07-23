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
    [Message(MessageType.StopDownloads)]
    public class ClientStopDownloads : MessageBase
    {
        /// <summary>
        /// null or empty = stop all downloads
        /// </summary>
        public List<Guid> DownloadsToStop { get; set; }
    }
}