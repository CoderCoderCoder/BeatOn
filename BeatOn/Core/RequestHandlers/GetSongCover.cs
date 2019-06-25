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
    public class GetSongCover : IHandleRequest
    {
        private GetBeatOnConfigDelegate _getConfig;
        public GetSongCover(GetBeatOnConfigDelegate getConfig)
        {
            _getConfig = getConfig;
        }

        private const int MAX_SONG_COVER_REQS = 15;
        private object _songCounterLock = new object();
        private int _songRequestCounter = 0;
        private object _songCoverLock = new object();
        public void HandleRequest(HttpListenerContext context)
        {
            try
            {
                var req = context.Request;
                var resp = context.Response;
                lock (_songCounterLock)
                {
                    _songRequestCounter++;
                    if (_songRequestCounter > MAX_SONG_COVER_REQS)
                    {
                        resp.StatusCode = 429;
                        return;
                    }
                }


                lock (_songCoverLock)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(req.Url.Query))
                        {
                            resp.BadRequest("Expected songid");
                            return;
                        }
                        string songid = null;
                        foreach (string kvp in req.Url.Query.TrimStart('?').Split("&"))
                        {
                            var split = kvp.Split('=');
                            if (split.Count() < 1)
                                continue;
                            if (split[0].ToLower() == "songid")
                            {
                                songid = Java.Net.URLDecoder.Decode(split[1]);
                                break;
                            }
                        }
                        if (string.IsNullOrEmpty(songid))
                        {
                            resp.BadRequest("Expected songid");
                            return;
                        }
                        var song = _getConfig().Config.Playlists.SelectMany(x => x.SongList).FirstOrDefault(x => x.SongID == songid);
                        if (song == null)
                        {
                            resp.NotFound();
                            return;
                        }
                        var imgBytes = song.TryGetCoverPngBytes();
                        if (imgBytes == null)
                        {
                            resp.Error();
                            return;
                        }
                        resp.StatusCode = 200;
                        resp.ContentType = MimeMap.GetMimeType("test.png");
                        resp.AppendHeader("Cache-Control", "max-age=86400, public");
                        using (MemoryStream ms = new MemoryStream(imgBytes))
                        {
                            ms.CopyTo(resp.OutputStream);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.LogErr("Exception handling get song cover!", ex);
                        resp.StatusCode = 500;
                    }
                }
            }
            finally
            {
                lock (_songCounterLock)
                {
                    _songRequestCounter--;
                }
            }
        }
    }
}