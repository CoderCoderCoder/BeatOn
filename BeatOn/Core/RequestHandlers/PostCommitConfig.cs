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
    public class PostCommitConfig : IHandleRequest
    {
        private ShowToastDelegate _showToast;
        private BeatSaberModder _mod;
        private SendHostMessageDelegate _sendMessage;
        private GetQaeDelegate _getQae;
        private GetBeatOnConfigDelegate _getConfig;
        private Action _triggerConfigChanged;
        private Func<bool> _canSync;
        public PostCommitConfig(BeatSaberModder mod, ShowToastDelegate showToast, SendHostMessageDelegate sendMessage, GetQaeDelegate getQae, GetBeatOnConfigDelegate getConfig, Action triggerConfigChanged, Func<bool> canSync)
        {
            _mod = mod;
            _showToast = showToast;
            _sendMessage = sendMessage;
            _getQae = getQae;
            _getConfig = getConfig;
            _triggerConfigChanged = triggerConfigChanged;
            _canSync = canSync;
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
                if (!_canSync())
                {
                    Log.LogErr("Attempted to sync while operations are in progress!");
                    _showToast("Can't Sync Yet", "Operations are still in progress, wait a moment and try again.", ToastType.Warning, 4);
                    resp.BadRequest("Operations in progress, can't sync");
                    return;
                }
                _showToast("Saving Config", "Do not turn off the Quest or exit the app!", ToastType.Warning, 3);
                _getQae().Save();
                _getConfig().IsCommitted = (!_getQae().HasChanges);
                _triggerConfigChanged();
                resp.Ok();
            }
            catch (Exception ex)
            {
                Log.LogErr("Exception handling commit!", ex);
                _sendMessage(new HostShowToast() { Message = "There was an error saving changes.", Title = "Unable to save changes to Beat Saber!", Timeout = 8, ToastType = ToastType.Error });
                resp.StatusCode = 500;
            }
        }
    }
}