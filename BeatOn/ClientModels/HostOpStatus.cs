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
    public class HostOpStatus : MessageBase
    {
        public override MessageType Type => MessageType.OpStatus;

        public List<HostOp> Ops { get; set; } = new List<HostOp>();
    }
}