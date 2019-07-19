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
using Newtonsoft.Json.Linq;

namespace BeatOn.ClientModels
{
    [Message(MessageType.SetBeastSaberUsername)]
    public class ClientSetBeastSaberUsername : MessageBase
    {
        public string BeastSaberUsername { get; set; }
    }
}