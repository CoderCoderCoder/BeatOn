using System;
using System.Collections.Generic;
using System.Text;

namespace BeatOn.Core.Mods
{
    public class HookModInstallAction : IModAction
    {
        /// <summary>
        /// The filename of the .so library file of the hook that should be installed
        /// </summary>
        public string InstallLibraryFile { get; set; }

    }
}
