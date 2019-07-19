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
using BeatOn.ClientModels;

namespace BeatOn.Core.MessageHandlers
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MessageHandlerAttribute : Attribute
    {
        public MessageType MessageType { get; private set; }
        public MessageHandlerAttribute(MessageType messageType)
        {
            MessageType = messageType;
        }
    }
}