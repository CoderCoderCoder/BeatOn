using System;
using System.Collections.Generic;
using System.Text;

namespace BeatOn.Core.Mods.Assets
{
    public abstract class AssetAction
    {
        public int StepNumber { get; set; }
        public abstract AssetActionType Type { get; }
    }
}
