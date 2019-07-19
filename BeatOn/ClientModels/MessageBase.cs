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
        public virtual MessageType Type
        {
            get
            {
                var attr = Attribute.GetCustomAttribute(this.GetType(), typeof(MessageAttribute)) as MessageAttribute;
                if (attr == null)
                    throw new Exception($"MessageBase.Type is not overridden in type {this.GetType()} and also does not have a MessageAttribute set.");
                return attr.MessageType;
            }
        }
    }
}