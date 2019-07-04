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
                    return _downloads.Count(x => x.Status != DownloadStatus.Processed && x.Status != DownloadStatus.Failed);
                }
            }
        }
        private ImportManager _importManager;

        private int _maxConcurrentDownloads;
        
        public DownloadManager(ImportManager importManager, int maxConcurrentDownloads = 5)
        {
            _importManager = importManager;
            _maxConcurrentDownloads = maxConcurrentDownloads;
        }

        public Download DownloadFile(string url, bool processAfterDownload = true)
        {
            //prevent the same URL from going in the queue twice.
            var exists = false;
            lock (_downloads)
            {
                exists = _downloads.Any(x => x.DownloadUrl == new Uri(url));
            }

            if (exists)
            {
                var deadDl = new Download(url);
                deadDl.StatusChanged += StatusChangeHandler;
                deadDl.SetStatus(DownloadStatus.Failed, "File is already being downloaded.");
                return deadDl;
            }            
            
            Download dl = new Download(url, processAfterDownload);
            lock (_downloads)
                _downloads.Add(dl);

            dl.StatusChanged += StatusChangeHandler;
            dl.SetStatus(DownloadStatus.NotStarted);
            return dl;
        }

        public event EventHandler<DownloadStatusChangeArgs> StatusChanged;

        private void StatusChangeHandler(object sender, DownloadStatusChangeArgs args)
        {
            //this method is the main "loop" of the download manager.  When a download is queued, its status is set to "not started", which triggers
            //  and event that lands it here.  This function handles all of the state changes, starting of downloads, clearing of succeeded or failed downloads
            //  and the triggering of processing downloads that have completed
            lock (_downloads)
            {
                var dl = sender as Download;
                switch (args.Status)
                {
                    case DownloadStatus.Failed:
                        _downloads.Remove(dl);
                        break;
                    case DownloadStatus.Processed:
                        _downloads.Remove(dl);
                        break;
                    case DownloadStatus.Downloaded:
                        if (dl.ProcessAfterDownload)
                        {
                            dl.SetStatus(DownloadStatus.Processing);
                            Task.Run(() =>
                            {
                                try
                                {
                                    //TODO: duplicate code on determining file type with what's in file upload... need another method in importmanager to determine file
                                    if (dl.DownloadedFilename.ToLower().EndsWith("json") || dl.DownloadedFilename.ToLower().EndsWith("bplist"))
                                    {
                                        _importManager.ImportFile(dl.DownloadedFilename, "application/json", dl.DownloadedData);
                                    }
                                    else
                                    {
                                        //load the downloaded data into a zip file provider and have the import manager try importing it
                                        MemoryStream ms = new MemoryStream(dl.DownloadedData);
                                        try
                                        {
                                            var provider = new ZipFileProvider(ms, dl.DownloadedFilename, FileCacheMode.None, true, QuestomAssets.Utils.FileUtils.GetTempDirectory());
                                            try
                                            {
                                                _importManager.ImportFromFileProvider(provider, () =>
                                                {
                                                    provider.Dispose();
                                                    ms.Dispose();
                                                });
                                            }
                                            catch
                                            {
                                                provider.Dispose();
                                                throw;
                                            }
                                        }
                                        catch
                                        {
                                            ms.Dispose();
                                            throw;
                                        }
                                    }
                                    //if the import manager succeeds, mark the download status as processed.
                                    // The status change events will land it in this parent event handler again, and it'll be cleared from the download list.
                                    dl.SetStatus(DownloadStatus.Processed);
                                }
                                catch (ImportException iex)
                                {
                                    Log.LogErr($"Exception processing downloaded file from {dl.DownloadUrl}", iex);
                                    dl.SetStatus(DownloadStatus.Failed, $"Failed to process {dl.DownloadUrl}: {iex.FriendlyMessage}");
                                }
                                catch (Exception ex)
                                {
                                    Log.LogErr($"Exception processing downloaded file from {dl.DownloadUrl}", ex);
                                    dl.SetStatus(DownloadStatus.Failed, $"Unable to process downloaded file from {dl.DownloadUrl}");
                                }
                            });
                        }
                        else
                        {
                            dl.SetStatus(DownloadStatus.Processed);
                        }
                        break;
                }

                if (_downloads.Count(x => x.Status == DownloadStatus.Downloading) < _maxConcurrentDownloads)
                {
                    var next = _downloads.FirstOrDefault(x => x.Status == DownloadStatus.NotStarted);
                    if (next != null)
                        next.Start();
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