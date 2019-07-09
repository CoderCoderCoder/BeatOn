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
    public class ClientAutoCreatePlaylists : MessageBase
    {
        public override MessageType Type => MessageType.AutoCreatePlaylists;
        public PlaylistSortMode SortMode { get; set; }
        public int? MaxPerNamePlaylist { get; set; }
        public bool RemoveEmptyPlaylists { get; set; }
    }
}