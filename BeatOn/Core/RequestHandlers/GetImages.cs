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
    public class GetImages : IHandleRequest
    {
        private QaeConfig _config;
        public GetImages(QaeConfig config)
        {
            _config = config;
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
                var files = _config.RootFileProvider.FindFiles(_config.PlaylistsPath.CombineFwdSlash("*.png")).Select(x => x.GetFilenameFwdSlash()).ToList();
                resp.SerializeOk(files);
                return;
            }
            catch (Exception ex)
            {
                Log.LogErr("Exception handling get images!", ex);
                resp.StatusCode = 500;
            }
        }
    }
}