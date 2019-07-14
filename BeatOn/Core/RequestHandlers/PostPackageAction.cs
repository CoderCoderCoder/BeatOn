using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using QuestomAssets;
using QuestomAssets.BeatSaber;

namespace BeatOn.Core.RequestHandlers
{
    public class PostPackageAction : IHandleRequest
    {
        private QaeConfig _config;
        private Action<string> _startPackage;
        private Action<string> _stopPackage;
        public PostPackageAction(Action<string> startPackage, Action<string> stopPackage)
        {
            _startPackage = startPackage;
            _stopPackage = stopPackage;
        }

        private const int MAX_SONG_COVER_REQS = 15;
        private object _songCounterLock = new object();
        private int _songRequestCounter = 0;
        private object _songCoverLock = new object();
        public void HandleRequest(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;

            try
            {
                if (string.IsNullOrWhiteSpace(req.Url.Query))
                {
                    resp.BadRequest("Expected package and action");
                    return;
                }
                string package = null;
                string action = null;
                foreach (string kvp in req.Url.Query.TrimStart('?').Split("&"))
                {
                    var split = kvp.Split('=');
                    if (split.Count() < 1)
                        continue;
                    if (split[0].ToLower() == "package")
                    {
                        package= Java.Net.URLDecoder.Decode(split[1]);
                        break;
                    } else if (split[0].ToLower() == "action")
                    {
                        action = Java.Net.URLDecoder.Decode(split[1]);
                    }
                }
                if (string.IsNullOrEmpty(package))
                {
                    resp.BadRequest("Expected package");
                    return;
                }
                if (string.IsNullOrEmpty(action))
                {
                    resp.BadRequest("Expected action");
                    return;
                }
                action = action.ToLower();
                if (action == "start")
                {
                    _startPackage(package);
                    resp.Ok();
                    return;
                }
                else if (action == "stop")
                {
                    _stopPackage(package);
                    resp.Ok();
                    return;
                }
                else
                { 
                    resp.BadRequest("Expected action to be 'start' or 'stop'");
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.LogErr("Exception handling get image!", ex);
                resp.StatusCode = 500;
            }
        }
    }
}