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
using QuestomAssets.BeatSaber;

namespace BeatOn.ClientModels
{
    [Message(MessageType.SortPlaylist)]
    public class ClientSortPlaylist : MessageBase
    {
        public string PlaylistID { get; set; }
        public PlaylistSortMode SortMode { get; set; }
        public bool Reverse { get; set; }
    }
}