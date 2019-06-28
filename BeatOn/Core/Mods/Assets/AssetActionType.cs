using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeatOn.Core.Mods.Assets
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AssetActionType
    {
        ReplaceAsset
    }
}
