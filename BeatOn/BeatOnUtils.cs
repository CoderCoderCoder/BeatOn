using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using QuestomAssets;
using QuestomAssets.AssetsChanger;

namespace BeatOn
{
    public static class BeatOnUtils
    {
        /// <summary>
        /// Gets a list of relative custom song folders within a base folder
        /// </summary>
        public static List<string> GetCustomSongsFromPath(string path)
        {
            List<string> customSongsFolders = new List<string>();

            List<string> foundSongs = Directory.EnumerateDirectories(path).Where(y => File.Exists(Path.Combine(y, "Info.dat"))).ToList();
            Log.LogMsg($"Found {foundSongs.Count()} custom songs to inject");
            customSongsFolders.AddRange(foundSongs.Select(x=> Path.GetFileName(x)));
            return customSongsFolders;
        }
    }
}