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
using QuestomAssets.AssetOps;
using QuestomAssets.Models;

namespace BeatOn.ClientModels
{
    public class ClientChangeColor : MessageBase
    {
        public override MessageType Type => MessageType.ChangeColor;

        public BeatSaberColor Color { get; set; }

        public ColorType ColorType { get; set; }
    }

}