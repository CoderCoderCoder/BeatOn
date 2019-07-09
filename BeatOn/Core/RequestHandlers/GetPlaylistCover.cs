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
        private QaeConfig _config;
        public GetPlaylistCover(GetBeatOnConfigDelegate getConfig, QaeConfig config)
        {
            _getConfig = getConfig;
            _config = config;
        }

        private object _playlistCoverLock = new object();
        public void HandleRequest(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;

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
                var pngName = _config.PlaylistsPath.CombineFwdSlash(playlistid + ".png");
                bool coverWasLoaded = playlist.IsCoverLoaded;
                //if the playlist cover is already loaded, there's a good chance it's from an updated asset which hasn't written the PNG to disk yet, so
                //  that should skip the filesystem.
                if (!coverWasLoaded)
                {                    
                    try
                    {
                        if (_config.RootFileProvider.FileExists(pngName))
                        {
                            var pngData = _config.RootFileProvider.Read(pngName);
                            resp.StatusCode = 200;
                            resp.ContentType = MimeMap.GetMimeType("test.png");
                            resp.AppendHeader("Cache-Control", "no-cache");
                            resp.OutputStream.Write(pngData, 0, pngData.Length);
                            Log.LogMsg($"Returned playlist cover art from file {pngName} for id {playlistid}");
                            return;
                        }
                        else
                        {
                            Log.LogMsg($"Playlist image {pngName} didn't exist, falling back to assets files.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.LogErr($"Exception loading png for playlist {playlistid}, falling back to assets files.", ex);
                    }
                }
                lock (_playlistCoverLock)
                {
                    var imgBytes = playlist.TryGetCoverPngBytes();
                    if (imgBytes == null)
                    {
                        resp.Error();
                        return;
                    }
                    try
                    {
                        if (!coverWasLoaded && !_config.RootFileProvider.FileExists(pngName))
                        {
                            Log.LogMsg($"Trying to create cover art file at {pngName} for playlist ID {playlistid} so it's available next time");
                            _config.RootFileProvider.MkDir(_config.PlaylistsPath, true);
                            _config.RootFileProvider.Write(pngName, imgBytes, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.LogErr($"Exception creating cover art file at {pngName} for playlist ID {playlistid}", ex);
                    }
                    resp.StatusCode = 200;
                    resp.ContentType = MimeMap.GetMimeType("test.png");
                    resp.AppendHeader("Cache-Control", "max-age=86400, public");
                    using (MemoryStream ms = new MemoryStream(imgBytes))
                    {
                        ms.CopyTo(resp.OutputStream);
                    }
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