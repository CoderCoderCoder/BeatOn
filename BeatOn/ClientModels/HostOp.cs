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

namespace BeatOn.ClientModels
{
    public class HostOp
    {
        public int ID { get; set; }
        public string OpDescription { get; set; }
        public OpStatus Status { get; set; }
        public string Error { get; set; }
    }
}