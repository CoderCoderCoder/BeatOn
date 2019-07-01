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

namespace BeatOn
{
    public static class Constants
    {
        public const string CONFIG_FILE = ROOT_BEAT_ON_DATA_PATH + "beatonconfig.json";
        public const string ROOT_BEAT_ON_DATA_PATH = "/sdcard/BeatOnData/";
        public const string ASSETS_RELOC_PATH = ROOT_BEAT_ON_DATA_PATH + "BeatSaberAssets/";
        public const string MODLOADER_MODS_PATH = "/sdcard/Android/data/com.beatgames.beatsaber/files/mods/";
        public const string LOGFILE = "/sdcard/beaton.log";
        public const string SERVICE_LOGFILE = "/sdcard/beatonservice.log";

        public const string JS_INTERFACE_NAME = "BeatOnAppInterface";
    }
}