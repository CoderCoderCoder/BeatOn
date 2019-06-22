﻿using System;
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
                                    var playlist = currentConfig.Playlists.FirstOrDefault(x => x.PlaylistID == "CustomSongs");

                                    if (playlist == null)
                                    {
                                        playlist = new BeatSaberPlaylist()
                                        {
                                            PlaylistID = "CustomSongs",
                                            PlaylistName = "Custom Songs"
                                        };
                                        currentConfig.Playlists.Add(playlist);
                                    }

                                    foreach (var toInst in doneDownloads)
                                    {
                                        playlist.SongList.Add(new BeatSaberSong()
                                        {
                                            SongID = Path.GetFileName(toInst.DownloadPath),
                                            CustomSongPath = toInst.DownloadPath
                                        });
                                        toInst.SetStatus(DownloadStatus.Installing);
                                    }
                                    try
                                    {
                                        //todo: probably want to move the save out to a user driven thing?
                                        qae.UpdateConfig(currentConfig);
                                        foreach (var toInst in doneDownloads)
                                        {
                                            //dot
                                            toInst.SetStatus(DownloadStatus.Installed);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        foreach (var toInst in doneDownloads)
                                        {
                                            Log.LogErr($"Unable to install song from {toInst.DownloadPath}!", ex);
                                            toInst.SetStatus(DownloadStatus.Failed, "Failed to install song!");
                                        }
                                    }
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