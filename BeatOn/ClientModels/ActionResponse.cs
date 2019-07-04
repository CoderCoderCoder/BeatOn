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
    public class ActionResponse : MessageBase
    {
        public override MessageType Type => MessageType.ActionResponse;

        public virtual bool Success { get; set; }

        public virtual string ErrorMessage { get; set; }
    }
}