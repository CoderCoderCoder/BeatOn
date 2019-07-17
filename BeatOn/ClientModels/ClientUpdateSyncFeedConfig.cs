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
    public class ClientUpdateSyncFeedConfig : MessageBase
    {
        public override MessageType Type => MessageType.UpdateSyncFeedConfig;

        public Guid ID { get; set; }
        
        //will this work?
        public JObject FeedConfig { get; set; }

    }
}