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
    public class DeletePendingConfig : IHandleRequest
    {
        private Action _triggerConfigChanged;
        private Action _triggerFullEngineReset;
        public DeletePendingConfig(Action triggerConfigChanged, Action triggerFullEngineReset)
        {
            _triggerConfigChanged = triggerConfigChanged;
            _triggerFullEngineReset = triggerFullEngineReset;
        }

        public void HandleRequest(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;

            try
            {
                _triggerFullEngineReset();
                _triggerConfigChanged();
                resp.Ok();
            }
            catch (Exception ex)
            {
                Log.LogErr("Exception handling revert config!", ex);
                resp.StatusCode = 500;
            }
        }
    }
}