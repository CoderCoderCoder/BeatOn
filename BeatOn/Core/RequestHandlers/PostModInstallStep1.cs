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
using Newtonsoft.Json;
using QuestomAssets;

namespace BeatOn.Core.RequestHandlers
{
    public class PostModInstallStep1 : IHandleRequest
    {
        private BeatSaberModder _mod;
        private SendHostMessageDelegate _sendMessage;

        public PostModInstallStep1(BeatSaberModder mod, SendHostMessageDelegate sendMessage)
        {
            _mod = mod;
            _sendMessage = sendMessage;
        }

        public static object MOD_INSTALL_LOCK { get; } = new object();

        public void HandleRequest(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;
            if (!Monitor.TryEnter(MOD_INSTALL_LOCK))
            {
                resp.BadRequest("Another install request is in progress.");
                return;
            }

            try
            {
                if (!_mod.IsBeatSaberInstalled)
                {
                    resp.BadRequest("Beat Saber is not installed!");
                    _sendMessage(new HostSetupEvent() { SetupEvent = SetupEventType.StatusMessage, Message = "Beat Saber is not installed!  Install Beat Saber and come back." });
                    _sendMessage(new HostSetupEvent() { SetupEvent = SetupEventType.Error, Message = "Beat Saber is not installed!" });
                    return;
                }
                var skipUninstall = req.Url?.Query?.TrimStart('?')?.Split("&")?.Any(x => x.ToLower() == "skipuninstall")??false;
                _mod.CopyOriginalBeatSaberApk(!skipUninstall);
                _sendMessage(new HostSetupEvent() { SetupEvent = SetupEventType.Step1Complete });
                resp.SerializeOk(new { BeatSaberNeedsUninstall = skipUninstall });
            }
            catch (Exception ex)
            {
                Log.LogErr("Exception handling mod install step 1!", ex);
                resp.Serialize(500, new { BeatSaberNeedsUninstall = false, ErrorMessage = ex.FullMessage() });
            }

            finally
            {
                Monitor.Exit(MOD_INSTALL_LOCK);
            }
        }
    }
}