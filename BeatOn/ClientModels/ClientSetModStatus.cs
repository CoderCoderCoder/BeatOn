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
using QuestomAssets.Mods;

namespace BeatOn.ClientModels
{
    [Message(MessageType.SetModStatus)]
    public class ClientSetModStatus : MessageBase
    {
        public string ModID { get; set; }
        public ModStatusType Status { get; set; }
    }
}