using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using QuestomAssets;
using QuestomAssets.AssetOps;
using QuestomAssets.BeatSaber;

namespace BeatOn.Core.RequestHandlers
{
    public class PostAutoCreatePlaylists : IHandleRequest
    {
        GetQaeDelegate _getQae;
        GetBeatOnConfigDelegate _getConfig;
        public PostAutoCreatePlaylists(GetQaeDelegate getQae,  GetBeatOnConfigDelegate getConfig)
        {
            _getQae = getQae;
            _getConfig = getConfig;
        }

        public static object PLAYLIST_LOCK { get; } = new object();

        public void HandleRequest(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;
            if (!Monitor.TryEnter(PLAYLIST_LOCK))
                resp.BadRequest("Another playlist request is in progress.");
            try
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(req.Url.Query))
                    {
                        resp.BadRequest("Expected sortorder");
                        return;
                    }
                    PlaylistSortMode? sortOrder = null;
                    int? maxNumPerNamePlaylist = null;
                    foreach (string kvp in req.Url.Query.TrimStart('?').Split("&"))
                    {
                        var split = kvp.Split('=');
                        if (split.Count() < 1)
                            continue;
                        if (split[0].ToLower() == "sortorder")
                        {
                            PlaylistSortMode ord;
                            if (!Enum.TryParse(split[1], out ord))
                            {
                                resp.BadRequest("sortorder is invalid");
                                return;
                            }
                            sortOrder = ord;
                        }
                        else if (split[0].ToLower() == "maxnumpernameplaylist")
                        {
                            int maxNum;
                            if (!Int32.TryParse(split[1], out maxNum) || maxNum < 5)
                            {
                                resp.BadRequest("maxnumpernameplaylist is invalid or must be at least 5");
                                return;
                            }
                            maxNumPerNamePlaylist = maxNum;
                        }
                    }
                    if (!sortOrder.HasValue)
                    {
                        resp.BadRequest("Expected sortorder");
                        return;
                    }

                    var op = new AutoCreatePlaylistsOp(sortOrder.Value, maxNumPerNamePlaylist ?? 50);

                    _getQae().OpManager.QueueOp(op);
                    op.FinishedEvent.WaitOne();
                    if (op.Status == OpStatus.Failed)
                    {
                        throw new Exception("Op failed", op.Exception);
                    }
                    _getConfig().Config = _getQae().GetCurrentConfig();
                    resp.Ok();
                }
                catch (Exception ex)
                {
                    Log.LogErr("Exception handling auto create playlist!", ex);
                    resp.StatusCode = 500;
                }
            }
            finally
            {
                Monitor.Exit(PLAYLIST_LOCK);
            }
        }
    }
}