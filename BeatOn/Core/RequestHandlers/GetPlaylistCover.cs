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
using QuestomAssets;

namespace BeatOn.Core.RequestHandlers
{
    public class GetPlaylistCover : IHandleRequest
    {
        private GetBeatOnConfigDelegate _getConfig;
        public GetPlaylistCover(GetBeatOnConfigDelegate getConfig)
        {
            _getConfig = getConfig;
        }

        private object _playlistCoverLock = new object();
        public void HandleRequest(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;
            lock (_playlistCoverLock)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(req.Url.Query))
                    {
                        resp.BadRequest("Expected playlistid");
                        return;
                    }
                    string playlistid = null;
                    foreach (string kvp in req.Url.Query.TrimStart('?').Split("&"))
                    {
                        var split = kvp.Split('=');
                        if (split.Count() < 1)
                            continue;
                        if (split[0].ToLower() == "playlistid")
                        {
                            playlistid = Java.Net.URLDecoder.Decode(split[1]);
                            break;
                        }
                    }
                    if (string.IsNullOrEmpty(playlistid))
                    {
                        resp.BadRequest("Expected playlistid");
                        return;
                    }
                    var playlist = _getConfig().Config.Playlists.FirstOrDefault(x => x.PlaylistID == playlistid);
                    if (playlist == null)
                    {
                        resp.NotFound();
                        return;
                    }
                    var imgBytes = playlist.TryGetCoverPngBytes();
                    if (imgBytes == null)
                    {
                        resp.Error();
                        return;
                    }
                    resp.StatusCode = 200;
                    resp.ContentType = MimeMap.GetMimeType("test.png");
                    using (MemoryStream ms = new MemoryStream(imgBytes))
                    {
                        ms.CopyTo(resp.OutputStream);
                    }

                }
                catch (Exception ex)
                {
                    Log.LogErr("Exception handling get playlist cover!", ex);
                    resp.StatusCode = 500;
                }
            }
        }

    }
}