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
using Newtonsoft.Json.Converters;

namespace BeatOn.ClientModels
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ModSetupStatusType
    {
        /// <summary>
        /// The mod is not installed and there is no progress towards installing it
        /// </summary>
        ModInstallNotStarted,

        /// <summary>
        /// The APK has been copied out to a temp file and is ready to be modded
        /// </summary>
        ReadyForModApply,

        /// <summary>
        /// The temp APK has been modded and is ready for installation
        /// </summary>
        ReadyForInstall,

        /// <summary>
        /// The modded APK is already installed and everything is ready to go
        /// </summary>
        ModInstalled
    }

    public class ModSetupStatus
    {
        public ModSetupStatusType CurrentStatus { get; set; }

        public bool IsBeatSaberInstalled { get; set; }

        public string BeatOnVersion { get; set; }
        public bool HasGoodBackup { get; set; }
        public bool HasHalfAssBackup { get; set; }
    }
}