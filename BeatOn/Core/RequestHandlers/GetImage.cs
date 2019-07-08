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
    public class GetImage : IHandleRequest
    {
        private QaeConfig _config;
        public GetImage(QaeConfig config)
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
                if (string.IsNullOrWhiteSpace(req.Url.Query))
                {
                    resp.BadRequest("Expected filename");
                    return;
                }
                string filename = null;
                foreach (string kvp in req.Url.Query.TrimStart('?').Split("&"))
                {
                    var split = kvp.Split('=');
                    if (split.Count() < 1)
                        continue;
                    if (split[0].ToLower() == "filename")
                    {
                        filename = Java.Net.URLDecoder.Decode(split[1]);
                        break;
                    }
                }
                if (string.IsNullOrEmpty(filename))
                {
                    resp.BadRequest("Expected filename");
                    return;
                }
                //yes, I am aware this can leak relative paths, I just care less about fixing it in this scenario than I do about typing this comment
                var fullfile = _config.PlaylistsPath.CombineFwdSlash(filename);
                if (!_config.RootFileProvider.FileExists(fullfile))
                {
                    resp.NotFound();
                    return;
                }

                byte[] fileBytes = _config.SongFileProvider.Read(fullfile);
                string mimeType = MimeMap.GetMimeType(fullfile.GetFilenameFwdSlash());
                resp.StatusCode = 200;
                resp.ContentType = mimeType;
                resp.AppendHeader("Cache-Control", "max-age=86400, public");
                resp.OutputStream.Write(fileBytes, 0, fileBytes.Length);
                return;
            }
            catch (Exception ex)
            {
                Log.LogErr("Exception handling get image!", ex);
                resp.StatusCode = 500;
            }
        }
    }
}