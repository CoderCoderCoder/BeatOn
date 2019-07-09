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
using Newtonsoft.Json;
using QuestomAssets;

namespace BeatOn
{
    public class BPList
    {
        [JsonProperty("playlistTitle")]
        public string PlaylistTitle { get; set; }

        [JsonProperty("playlistAuthor")]
        public string PlaylistAuthor { get; set; }

        [JsonProperty("playlistDescription")]
        public string PlaylistDescription { get; set; }

        [JsonProperty("songs")]
        public List<BPSong> Songs { get; set; } = new List<BPSong>();

        [JsonProperty("image")]
        private string ImageString { get; set; }

        private byte[] _imageBytes = null;
        [JsonIgnore]
        public byte[] Image { get
            {
                if (_imageBytes != null)
                    return _imageBytes;
                try
                {
                    var idx = ImageString.IndexOf("base64,") + 7;
                    ImageString = ImageString.Substring(idx);
                    var retVal = Convert.FromBase64String(ImageString);
                    //let's give the GC something worthwhile to do
                    ImageString = null;
                    return retVal;
                }
                catch (Exception ex)
                {
                    Log.LogErr("Exception trying to get image bytes out of playlist base64", ex);
                    return null;
                }
            }
        }

    }

    public class BPSong
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("songName")]
        public string SongName { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

    }
}