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

namespace BeatOn.ClientModels
{
    [JsonConverter(typeof(MessageTypeConverter))]
    public abstract class MessageBase
    {
        public Guid MessageID { get; set; } = Guid.NewGuid();
        public Guid? ResponseToMessageID { get; set; }
        public abstract MessageType Type { get; }
    }
}