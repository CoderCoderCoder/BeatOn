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
    [Message(MessageType.MoveSongToPlaylist)]
    public class ClientMoveSongToPlaylist : MessageBase
    {
        public string SongID { get; set; }
        public string ToPlaylistID { get; set; }
        public int? Index { get; set; }

    }
}