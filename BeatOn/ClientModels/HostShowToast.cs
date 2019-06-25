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
    public class HostShowToast : MessageBase
    {
        public override MessageType Type => MessageType.Toast;

        public ToastType ToastType { get; set; }

        public int Timeout { get; set; } = 4000;

        public string Title { get; set; }

        public string Message { get; set; }
    }
}