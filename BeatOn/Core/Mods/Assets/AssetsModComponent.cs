﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BeatOn.Core.Mods.Assets
{
    public class AssetsModComponent : ModComponent
    {
        public override ModComponentType Type => ModComponentType.AssetsMod;

        public AssetsModActionGroup InstallAction { get; set; }

        public AssetsModActionGroup UninstallAction { get; set; }
    }
}
