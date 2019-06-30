using System;
using System.Collections.Generic;
using System.IO;
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
using BeatOn.ClientModels;
using QuestomAssets;
using QuestomAssets.BeatSaber;
using QuestomAssets.Models;

namespace BeatOn.Core.RequestHandlers
{
    public class PostReloadSongFolders : IHandleRequest
    {
        private ShowToastDelegate _showToast;
        private BeatSaberModder _mod;
        private SendHostMessageDelegate _sendMessage;
        private GetQaeDelegate _getQae;
        private GetBeatOnConfigDelegate _getConfig;
        private QaeConfig _qaeConfig;
        private Action _triggerConfigChanged;
        private SetSuppressConfigChangeMessages _setSuppressMsg;
        public PostReloadSongFolders(BeatSaberModder mod, QaeConfig qaeConfig, ShowToastDelegate showToast, SendHostMessageDelegate sendMessage, GetQaeDelegate getQae, GetBeatOnConfigDelegate getConfig, Action triggerConfigChanged, SetSuppressConfigChangeMessages setSuppressMsg)
        {
            _mod = mod;
            _qaeConfig = qaeConfig;
            _showToast = showToast;
            _sendMessage = sendMessage;
            _getQae = getQae;
            _getConfig = getConfig;
            _triggerConfigChanged = triggerConfigChanged;
            _setSuppressMsg = setSuppressMsg;
        }

        private void SendStatusMessage(string message)
        {
            _sendMessage(new HostSetupEvent() { SetupEvent = SetupEventType.StatusMessage, Message = message });
        }

        private object _reloadSongsLock = new object();
        public void HandleRequest(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;

            if (!Monitor.TryEnter(_reloadSongsLock))
            {
                resp.Error("Song reload already in progress");
                return;
            }
            try
            {
                try
                {
                    if (!_mod.IsBeatSaberInstalled || !_mod.IsInstalledBeatSaberModded)
                    {
                        resp.BadRequest("Modded Beat Saber is not installed!");
                        _showToast("Can't reload song folders.", "Modded Beat Saber is not installed!");
                        return;
                    }
                    var sls = new StatusUpdateLogSink((msg) =>
                    {
                        SendStatusMessage(msg);
                    });
                    try
                    {
                        _setSuppressMsg(true);
                        var folders = BeatOnUtils.GetCustomSongsFromPath(Path.Combine(Constants.ROOT_BEAT_ON_DATA_PATH, _qaeConfig.SongsPath));
                        if (folders.Count < 1)
                        {
                            Log.LogErr("Request to reload songs folder, but didn't find any songs!");
                            //not found probably isn't the right response code for this, but meh
                            resp.NotFound();
                            return;
                        }
                        Log.LogMsg($"Starting to reload custom songs from folders.  Found {folders.Count} folders to evaluate");
                        //todo: probably don't just grab this one
                        var playlist = _getConfig().Config.Playlists.FirstOrDefault(x => x.PlaylistID == "CustomSongs");
                        if (playlist == null)
                        {
                            playlist = new BeatSaberPlaylist()
                            {
                                PlaylistID = "CustomSongs",
                                PlaylistName = "Custom Songs"
                            };
                            _getConfig().Config.Playlists.Add(playlist);
                        }
                        int addedCtr = 0;

                        foreach (var folder in folders)
                        {
                            string songId = folder.Replace("/", "");
                            songId = songId.Replace(" ", "");
                            if (_getConfig().Config.Playlists.SelectMany(x => x.SongList).Any(x => x.SongID?.ToLower() == songId.ToLower()))
                            {
                                SendStatusMessage($"Folder {folder} already loaded");
                                Log.LogMsg($"Custom song in folder {folder} appears to already be loaded, skipping it.");
                                continue;
                            }
                            //safety check to make sure we aren't importing one with the same ID as a beatsaber song.  It could be re-ID'ed, but this is a stopgap
                            if (BSConst.KnownLevelIDs.Contains(songId))
                            {
                                Log.LogErr($"Song in path {folder} would conflict with a built in songID and will be skipped");
                                continue;
                            }
                            SendStatusMessage($"Adding song in {folder}");
                            Log.LogMsg($"Adding custom song in folder {folder} to playlist ID {playlist.PlaylistID}");
                            playlist.SongList.Add(new BeatSaberSong()
                            {
                                SongID = songId,
                                CustomSongPath = Path.Combine(_qaeConfig.SongsPath, folder)
                            });
                            addedCtr++;
                            //maybe limit how many
                            //if (addedCtr > 200)
                            //{
                            //    ShowToast("Too Many Songs", "That's too many at once.  After these finish and you 'Sync to Beat Saber', then try 'Reload Songs Folder' again to load more.", ToastType.Warning, 10);
                            //    break;
                            //}
                        }
                        Log.LogMsg("Reload song folders finished with main body");
                        if (addedCtr > 0)
                        {
                            SendStatusMessage($"{addedCtr} songs will be added");
                            SendStatusMessage($"Updating configuration...");
                            Log.LogMsg("Updating config with loaded song folders");
                            Log.SetLogSink(sls);
                            int failedOps = 0;
                            try
                            {
                                _getQae().UpdateConfig(_getConfig().Config);
                            }
                            catch (AssetOpsException aoe)
                            {
                                Log.LogErr($"Updating config completed, but {aoe.FailedOps?.Count} operations failed.");
                                failedOps = aoe.FailedOps?.Count ?? 0;
                            }
                            Log.RemoveLogSink(sls);
                            _showToast("Folder Load Complete", $"{addedCtr} folders were scanned and added to {playlist.PlaylistName}, {failedOps} failed to load.", ToastType.Success, 3);
                        }
                        else
                        {
                            SendStatusMessage($"No new songs found");
                            Log.LogMsg("No new songs were found to load.");
                            _showToast("Folder Load Complete", "No additional songs were found to add", ToastType.Warning, 3);
                        }
                    }
                    finally
                    {
                        _setSuppressMsg(false);
                        Log.RemoveLogSink(sls);
                    }
                    Log.LogMsg("Reload song folders sending change message");
                    _getConfig().Config = _getQae().GetCurrentConfig();
                    _triggerConfigChanged();
                    Log.LogMsg("Reload song folders responding OK");
                    resp.Ok();
                }
                catch (Exception ex)
                {
                    Log.LogErr("Exception reloading song folders!", ex);
                    resp.StatusCode = 500;
                }
            }
            finally
            {
                Monitor.Exit(_reloadSongsLock);
            }
        }
    }
}