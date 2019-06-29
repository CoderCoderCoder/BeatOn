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
    public class PostUninstallBeatSaber : IHandleRequest
    {
        private BeatSaberModder _mod;
        private ShowToastDelegate _showToast;
        public PostUninstallBeatSaber(BeatSaberModder mod, ShowToastDelegate showToast)
        {
            _mod = mod;
            _showToast = showToast;
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
                    if (!_mod.IsBeatSaberInstalled)
                    {
                        _showToast("Beat Saber Not Installed", "Beat Saber doesn't seem to be installed.", ToastType.Error, 8);
                        resp.BadRequest("Beat Saber isn't installed.");
                        return;
                    }
                    _mod.UninstallBeatSaber();
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