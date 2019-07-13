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

        public const string LAST_COMMITTED_CONFIG = "config.json";
        public const string LAST_TEMP_CONFIG = "tempconfig.json";
        public const string LOGGLY_TOKEN = "271fee44-1ebb-4629-861e-e15d8e5a4659";
        public const string CONFIG_FILE = ROOT_BEAT_ON_DATA_PATH + "beatonconfig.json";
        public const string ROOT_BEAT_ON_DATA_PATH = "/sdcard/BeatOnData/";
        public const string BEATSABER_ASSETS_FOLDER_NAME = "BeatSaberAssets";
        public const string ASSETS_RELOC_PATH = ROOT_BEAT_ON_DATA_PATH + BEATSABER_ASSETS_FOLDER_NAME + "/";
        public const string MODLOADER_MODS_PATH = "/sdcard/Android/data/com.beatgames.beatsaber/files/mods/";
        public const string LOGFILE = "/sdcard/beaton.log";
        public const string SERVICE_LOGFILE = "/sdcard/beatonservice.log";
        public const string MODS_FOLDER_NAME = "Mods";
        public const string MODS_FULL_PATH = ROOT_BEAT_ON_DATA_PATH + MODS_FOLDER_NAME+"/";
        public const string CUSTOM_SONGS_FOLDER_NAME = "CustomSongs";
        public const string CUSTOM_SONGS_FULL_PATH = ROOT_BEAT_ON_DATA_PATH + CUSTOM_SONGS_FOLDER_NAME + "/";
        public const string BACKUP_FOLDER_NAME = "Backups";
        public const string BACKUP_FULL_PATH = ROOT_BEAT_ON_DATA_PATH + BACKUP_FOLDER_NAME + "/";
        public const string BEATSABER_APK_BACKUP_FILE = BACKUP_FULL_PATH + "beatsaber-unmodded.apk";
        public const string BEATSABER_APK_MODDED_BACKUP_FILE = BACKUP_FULL_PATH + "beatsaber-modded-donotuse.apk";

        public const string BEATSAVER_ROOT = "https://beatsaver.com/";
        public const string BEATSAVER_DOWNLOAD_API = BEATSAVER_ROOT + "api/download/key/{0}";
        public const string BEATSAVER_KEY_API = "api/maps/detail/{0}";
        public const string BEATSAVER_HASH_API = "api/maps/by-hash/{0}";


        public const string PLAYLISTS_FOLDER_NAME = "Playlists";
        public const string MOD_STATUS_FILE = "modstatus.json";

        public const string JS_INTERFACE_NAME = "BeatOnAppInterface";
    }
}