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
    public class GetSongCover : IHandleRequest
    {
        private GetBeatOnConfigDelegate _getConfig;
        private QaeConfig _config;
        public GetSongCover(QaeConfig config, GetBeatOnConfigDelegate getConfig)
        {
            _getConfig = getConfig;
            _config = config;
        }

        /// <summary>
        /// Attempt to locate the cover art file based on the song ID and info.dat
        /// </summary>
        public string GetCoverImageFilename(string songID)
        {
            var infodatfile = _config.SongsPath.CombineFwdSlash(songID.CombineFwdSlash("info.dat"));
            using (new LogTiming("getting song cover filename"))
            {
                if (!_config.SongFileProvider.FileExists(infodatfile))
                {
                    Log.LogErr($"Trying to find original song cover, could not find {infodatfile} for level ID {songID}");
                    return null;
                }

                //I hope this is a faster way than deserializing the entire object and worth the double code paths for parsing the same serialized type
                string imageFilename = null;
                try
                {
                    using (var jr = new JsonTextReader(new StreamReader(_config.SongFileProvider.GetReadStream(infodatfile))))
                    {
                        while (jr.Read())
                        {
                            if (jr.TokenType == JsonToken.PropertyName && jr.Value?.ToString() == "_coverImageFilename")
                            {
                                if (jr.Read() && jr.TokenType == JsonToken.String)
                                {
                                    imageFilename = jr.Value?.ToString();
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.LogErr($"Exception trying to read info.dat to get _coverImageFilename for level ID {songID} from '{infodatfile}'.", ex);
                    return null;
                }
                return songID.CombineFwdSlash(imageFilename);
            }
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
                        string originalFile = _config.SongsPath.CombineFwdSlash(GetCoverImageFilename(song.SongID));
                        if (originalFile != null)
                        {
                            try
                            {
                                byte[] fileBytes = _config.SongFileProvider.Read(originalFile);
                                string mimeType = MimeMap.GetMimeType(originalFile.GetFilenameFwdSlash());
                                resp.StatusCode = 200;
                                resp.ContentType = mimeType;
                                resp.AppendHeader("Cache-Control", "max-age=86400, public");
                                resp.OutputStream.Write(fileBytes, 0, fileBytes.Length);                                
                                return;
                            }
                            catch (Exception ex)
                            {
                                Log.LogErr($"Exception trying to load original song cover for song ID {song.SongID} from disk, will fall back to assets", ex);
                            }
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