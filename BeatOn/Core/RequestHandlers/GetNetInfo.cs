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
    public class GetNetInfo : IHandleRequest
    {
        WebServer _webServer;
        public GetNetInfo(WebServer webServer)
        {
            _webServer = webServer;
        }

        public void HandleRequest(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;
            try
            {
                resp.Serialize(new NetInfo()
                {
                    Url = _webServer.ListeningOnUrl,
                    WebSocketUrl = _webServer.WebSocketUrl
                });
            }
            catch (Exception ex)
            {
                Log.LogErr("Exception handling get net info!", ex);
                resp.StatusCode = 500;
            }
        }
    }
}