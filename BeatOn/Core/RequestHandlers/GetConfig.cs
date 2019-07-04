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
using QuestomAssets;

namespace BeatOn.Core.RequestHandlers
{
    public class GetConfig : IHandleRequest
    {
        private GetBeatOnConfigDelegate _getConfig;
        public GetConfig(GetBeatOnConfigDelegate getConfig)
        {
            _getConfig = getConfig;
        }

        /// <summary>
        /// Used from places that need to lock configuration modification
        /// </summary>
        public static object CONFIG_LOCK { get; } = new object();
        public void HandleRequest(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;
            lock (CONFIG_LOCK)
            {
                try
                {
                    resp.SerializeOk(_getConfig());
                }
                catch (Exception ex)
                {
                    Log.LogErr("Exception getting config!", ex);
                    resp.StatusCode = 500;
                }
            }
        }
    }
}