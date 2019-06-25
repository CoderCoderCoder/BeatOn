using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BeatOn.ClientModels;
using QuestomAssets;

namespace BeatOn.Core.RequestHandlers
{
    public class PostResetAssets : IHandleRequest
    {
        private Mod _mod;
        private ShowToastDelegate _showToast;
        private Action _triggerConfigChanged;
        private Action _triggerFullEngineReset;
        public PostResetAssets(Mod mod, ShowToastDelegate showToast, Action triggerConfigChanged, Action triggerFullEngineReset)
        {
            _mod = mod;
            _showToast = showToast;
            _triggerConfigChanged = triggerConfigChanged;
            _triggerFullEngineReset = triggerFullEngineReset;
        }

        public void HandleRequest(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;
            if (!Monitor.TryEnter(PostModInstallStep1.MOD_INSTALL_LOCK))
                resp.BadRequest("Another mod request is in progress.");
            try
            {

                try
                {
                    if (!_mod.IsBeatSaberInstalled && !_mod.IsInstalledBeatSaberModded)
                    {
                        _showToast("Mod Not Installed", "The mod does not appear to be installed correctly.", ToastType.Error, 8);
                        resp.BadRequest("The mod does not appear to be installed correctly.");
                        return;
                    }
                    _triggerFullEngineReset();
                    _mod.ResetAssets();
                    _triggerConfigChanged();
                    resp.Ok();
                }
                catch (Exception ex)
                {
                    Log.LogErr("Exception handling mod install step 1!", ex);
                    resp.StatusCode = 500;
                }
            }
            finally
            {
                Monitor.Exit(PostModInstallStep1.MOD_INSTALL_LOCK);
            }
        }
    }
}