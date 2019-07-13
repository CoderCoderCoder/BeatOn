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
    public class GetModCover : IHandleRequest
    {
        private GetBeatOnConfigDelegate _getConfig;
        private QaeConfig _config;
        public GetModCover(QaeConfig config, GetBeatOnConfigDelegate getConfig)
        {
            _getConfig = getConfig;
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
                    resp.BadRequest("Expected modid");
                    return;
                }
                string modid = null;
                foreach (string kvp in req.Url.Query.TrimStart('?').Split("&"))
                {
                    var split = kvp.Split('=');
                    if (split.Count() < 1)
                        continue;
                    if (split[0].ToLower() == "modid")
                    {
                        modid = Java.Net.URLDecoder.Decode(split[1]);
                        break;
                    }
                }
                if (string.IsNullOrEmpty(modid))
                {
                    resp.BadRequest("Expected modid");
                    return;
                }
                var mod = _getConfig().Config.Mods.Where(x => x.ID == modid).FirstOrDefault();
                if (mod == null)
                {
                    Log.LogErr($"mod id {mod.id} doesn't seem to be loaded, so can't find image file for it");
                    resp.NotFound();
                    return;
                }
                if (string.IsNullOrEmpty(mod.CoverImageFilename) || !_config.RootFileProvider.FileExists(Constants.MODS_FOLDER_NAME.CombineFwdSlash(modid).CombineFwdSlash(mod.CoverImageFilename)))
                {
                    Log.LogErr($"mod ID {mod.ID} doesn't seem to have a cover image, or it doesn't exist.");
                    resp.NotFound();
                    return;
                }
                var mime = MimeMap.GetMimeType(mod.CoverImageFilename);
                var data = _config.RootFileProvider.Read(Constants.MODS_FOLDER_NAME.CombineFwdSlash(modid).CombineFwdSlash(mod.CoverImageFilename));
                resp.StatusCode = 200;
                resp.ContentType = mime;                
                resp.OutputStream.Write(data, 0, data.Length);                
            }
            catch (Exception ex)
            {
                Log.LogErr("Exception handling get mod cover!", ex);
                resp.StatusCode = 500;
            }
        }
    }
}