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
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MessageAttribute : Attribute
    {
        public MessageType MessageType { get; private set; }
        public MessageAttribute(MessageType messageType)
        {
            MessageType = messageType;
        }
    }
}