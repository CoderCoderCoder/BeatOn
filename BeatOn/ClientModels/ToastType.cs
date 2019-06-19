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
using Newtonsoft.Json.Converters;

namespace BeatOn.ClientModels
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ToastType
    {
        Error,
        Warning,
        Info,
        Success
    }
}