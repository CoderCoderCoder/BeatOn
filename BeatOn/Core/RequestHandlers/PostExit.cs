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
    public class PostExit : IHandleRequest
    {
        private Action _triggerHardQuit;
        public PostExit(Action triggerHardQuit)
        {
            _triggerHardQuit = triggerHardQuit;
        }

        public void HandleRequest(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;

            try
            {
                Log.LogErr("Hard quitting..");
                _triggerHardQuit();
            }
            catch (Exception ex)
            {
                Log.LogErr("Exception handling Post config restore!", ex);
                resp.Serialize(500, new { BeatSaberNeedsUninstall = false, ErrorMessage = ex.FullMessage() });
            }

        }
    }
}