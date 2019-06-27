using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

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
    public class PostCommitConfig : IHandleRequest
    {
        private ShowToastDelegate _showToast;
        private Mod _mod;
        private SendHostMessageDelegate _sendMessage;
        private GetQaeDelegate _getQae;
        private GetBeatOnConfigDelegate _getConfig;
        private Action _triggerConfigChanged;
        public PostCommitConfig(Mod mod, ShowToastDelegate showToast, SendHostMessageDelegate sendMessage, GetQaeDelegate getQae, GetBeatOnConfigDelegate getConfig, Action triggerConfigChanged)
        {
            _mod = mod;
            _showToast = showToast;
            _sendMessage = sendMessage;
            _getQae = getQae;
            _getConfig = getConfig;
            _triggerConfigChanged = triggerConfigChanged;
        }

        public void HandleRequest(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;

            try
            {
                if (!_mod.IsBeatSaberInstalled || !_mod.IsInstalledBeatSaberModded)
                {
                    resp.BadRequest("Modded Beat Saber is not installed!");
                    _showToast("Can't commit config.", "Modded Beat Saber is not installed!");
                    return;
                }
                _showToast("Saving Config", "Do not turn off the Quest or exit the app!", ToastType.Warning, 3);
                _getQae().Save();
                _getConfig().IsCommitted = true;
                _triggerConfigChanged();
                resp.Ok();
            }
            catch (Exception ex)
            {
                Log.LogErr("Exception handling mod install step 1!", ex);
                resp.StatusCode = 500;
            }
        }
    }
}