using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using QuestomAssets;
using QuestomAssets.AssetOps;
using QuestomAssets.AssetsChanger;
using QuestomAssets.Models;

namespace BeatOn
{
    public class DownloadManager
    {
        public int InProgress
        {
            get
            {
                lock (_downloads)
                {
                    return _downloads.Count(x => x.Status != DownloadStatus.Installed && x.Status != DownloadStatus.Failed);
                }
            }
        }
        private Func<QuestomAssets.QuestomAssetsEngine> _engineFactory;
        private Func<QuestomAssets.Models.BeatSaberQuestomConfig> _configFactory;
        private IAssetsFileProvider _provider;
        private string _defaultSavePath;
        private int _maxConcurrentDownloads;
        public DownloadManager(Func<QuestomAssets.QuestomAssetsEngine> engineFactory, Func<QuestomAssets.Models.BeatSaberQuestomConfig> configFactory, IAssetsFileProvider provider, string defaultSavePath, int maxConcurrentDownloads = 5)
        {
            _engineFactory = engineFactory;
            _configFactory = configFactory;                
            _provider = provider;
            _defaultSavePath = defaultSavePath;
            _maxConcurrentDownloads = maxConcurrentDownloads;
        }
        private object _installLock = new object();
        private Task _currentInstall;

        public Download DownloadFile(string url, string savePath = null)
        {
            savePath = savePath ?? _defaultSavePath;
            
            //prevent the same URL from going in the queue twice.
            var exists = false;
            lock (_downloads)
            {
                exists = _downloads.Any(x => x.DownloadUrl == new Uri(url));
            }

            if (exists)
            {
                var deadDl = new Download(url, _provider, savePath);
                deadDl.StatusChanged += StatusChangeHandler;
                deadDl.SetStatus(DownloadStatus.Failed, "File is already being downloaded.");
                return deadDl;
            }            
            
            Download dl = new Download(url, _provider, savePath);
            lock (_downloads)
                _downloads.Add(dl);
            dl.StatusChanged += StatusChangeHandler;
            dl.SetStatus(DownloadStatus.NotStarted);
            return dl;
        }

        public Download ProcessFile(byte[] fileData, string fileName, string savePath = null)
        {
            savePath = savePath ?? _defaultSavePath;

            Download dl = new Download(_provider, savePath, fileData, fileName);
            lock(_downloads)
                _downloads.Add(dl);
            dl.StatusChanged += StatusChangeHandler;
            dl.SetStatus(DownloadStatus.NotStarted);
            return dl;
        }
        public event EventHandler<DownloadStatusChangeArgs> StatusChanged;

        private void StatusChangeHandler(object sender, DownloadStatusChangeArgs args)
        {
            lock (_downloads)
            {
                var dl = sender as Download;
                switch (args.Status)
                {
                    case DownloadStatus.Downloaded:

                        break;
                    case DownloadStatus.Failed:
                        _downloads.Remove(dl);
                        break;
                    case DownloadStatus.Installed:
                        _downloads.Remove(dl);
                        break;
                }
                if (_downloads.Count(x => x.Status == DownloadStatus.Downloading) < _maxConcurrentDownloads)
                {
                    var next = _downloads.FirstOrDefault(x => x.Status == DownloadStatus.NotStarted);
                    if (next != null)
                        next.Start();
                }
                Task newTask = null;
                lock (_installLock)
                {
                    if (_currentInstall == null)
                    {
                        var doneDownloads = _downloads.Where(x => x.Status == DownloadStatus.Downloaded).ToList();
                        if (doneDownloads.Count > 0)
                        {
                            newTask = new Task(() =>
                            {
                                try
                                {
                                    var qae = _engineFactory();
                                    var currentConfig = _configFactory();
                                    var pl = currentConfig.Playlists.FirstOrDefault(x => x.PlaylistID == "CustomSongs");
                                    Task<BeatSaberPlaylist> playlistTask;
                                    if (pl == null)
                                    {
                                        playlistTask = new Task<BeatSaberPlaylist>(() =>
                                        {
                                            pl = new BeatSaberPlaylist()
                                            {
                                                PlaylistID = "CustomSongs",
                                                PlaylistName = "Custom Songs"
                                            };
                                            var addPlOp = new AddOrUpdatePlaylistOp(pl);
                                            qae.OpManager.QueueOp(addPlOp);
                                            addPlOp.FinishedEvent.WaitOne();
                                            if (addPlOp.Status != OpStatus.Complete)
                                            {
                                                Log.LogErr($"Unable to create CustomSongs playlist!");
                                                return null;
                                            }
                                            currentConfig.Playlists.Add(pl);
                                            return pl;
                                        });
                                    }
                                    else
                                    {
                                        playlistTask = new Task<BeatSaberPlaylist>(() => { return pl; });
                                    }
                                    playlistTask.ContinueWith((pt) =>
                                    {
                                        var playlist = pt.Result;
                                        foreach (var toInst in doneDownloads)
                                        {
                                            var bsSong = new BeatSaberSong()
                                            {
                                                SongID = Path.GetFileName(toInst.DownloadPath),
                                                CustomSongPath = toInst.DownloadPath
                                            };
                                            var addOp = new AddNewSongToPlaylistOp(bsSong, playlist.PlaylistID);

                                            addOp.OpFinished += (s, op) =>
                                             {
                                                 playlist.SongList.Add(bsSong);
                                                 
                                                 if (op.Status == OpStatus.Complete)
                                                     toInst.SetStatus(DownloadStatus.Installed);
                                                 else
                                                     toInst.SetStatus(DownloadStatus.Failed);
                                             };
                                            toInst.SetStatus(DownloadStatus.Installing);
                                            qae.OpManager.QueueOp(addOp);
                                        }
                                    });
                                    playlistTask.Start();
                                }
                                finally
                                {
                                    lock (_installLock)
                                        _currentInstall = null;
                                }
                            });
                            _currentInstall = newTask;
                        }
                    }
                }
                if (newTask != null)
                {
                    newTask.Start();
                }
            }
            StatusChanged?.Invoke(sender, args);
        }

        private List<Download> _downloads = new List<Download>();

        public List<Download> Downloads
        {
            get
            {
                lock (_downloads)
                    return _downloads.ToList();
            }
        }
        
    }
}