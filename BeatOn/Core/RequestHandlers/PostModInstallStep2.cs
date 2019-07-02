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
    public class PostModInstallStep2 : IHandleRequest
    {
        private BeatSaberModder _mod;
        private SendHostMessageDelegate _sendMessage;

        public PostModInstallStep2(BeatSaberModder mod, SendHostMessageDelegate sendMessage)
        {
            _mod = mod;
            _sendMessage = sendMessage;
        }

        public void HandleRequest(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;
            if (!Monitor.TryEnter(PostModInstallStep1.MOD_INSTALL_LOCK))
            {
                resp.BadRequest("Another install request is in progress.");
                return;
            }

            try
            {
                if (!_mod.DoesTempApkExist)
                {
                    resp.BadRequest("Step 1 has not completed, temporary APK does not exist!");
                    return;
                }
                _mod.ApplyModToTempApk();
                _sendMessage(new HostSetupEvent() { SetupEvent = SetupEventType.Step2Complete });
                resp.SerializeOk(new { BeatSaberShouldInstall = true, ModdedApkToInstall = _mod.TempApk });
            }
            catch (Exception ex)
            {
                Log.LogErr("Exception handling mod install step 2!", ex);
                resp.Serialize(500, new { BeatSaberShouldInstall = false, ErrorMessage = ex.FullMessage() });
            }
            finally
            {
                Monitor.Exit(PostModInstallStep1.MOD_INSTALL_LOCK);
            }
        }
    }
}