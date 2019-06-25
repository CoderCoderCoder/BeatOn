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
using Android.Webkit;
using Android.Widget;
using QuestomAssets;
using QuestomAssets.AssetsChanger;

namespace BeatOn
{
    public class Download
    {
        public int PercentageComplete { get; private set; }
        public Guid ID { get; } = Guid.NewGuid();
        private WebClient _client = new WebClient();
        public event EventHandler<DownloadStatusChangeArgs> StatusChanged;

        private void StatusChange(DownloadStatus status, string message = null)
        {
            Status = status;
            StatusChanged?.Invoke(this, new DownloadStatusChangeArgs(status, message));
        }
        private const int NOTIFY_EVERY_PERCENT = 10;
        int lastNotifyPercent = 0;
        private void PercentCompleteChange(int percentComplete)
        {
            
            this.PercentageComplete = percentComplete;
            if (percentComplete == 0 || percentComplete == 100 || (percentComplete - lastNotifyPercent) > NOTIFY_EVERY_PERCENT)
            {
                lastNotifyPercent = percentComplete;
                StatusChanged?.Invoke(this, new DownloadStatusChangeArgs(percentComplete));
            }
        }

        private IAssetsFileProvider _provider;
        private string _savePath;

        public Download(string url, IAssetsFileProvider provider, string savePath)
        {
            DownloadUrl = new Uri(url);
            Status = DownloadStatus.NotStarted;
            _provider = provider;
            _savePath = savePath;
        }

        public Download(IAssetsFileProvider provider, string savePath, byte[] downloadedData, string downloadedFilename)
        {
            DownloadUrl = new Uri($"file://{downloadedFilename}");
            Status = DownloadStatus.NotStarted;
            _provider = provider;
            _savePath = savePath;
            DownloadedData = downloadedData;
            DownloadedFilename = downloadedFilename;
        }

        public void Start()
        {
            if (DownloadedData != null && DownloadedFilename != null)
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    StatusChange(DownloadStatus.Downloading);
                    ProcessDownloadedData();
                });
            }
            else if (DownloadUrl.IsAbsoluteUri)
            {
                var fileName = Path.GetFileNameWithoutExtension(DownloadUrl.LocalPath);
                
                _client.DownloadDataCompleted += (s, dlArgs) =>
                {
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        try
                        {
                            if (dlArgs.Error != null)
                            {
                                Log.LogErr($"Error downloading file '{DownloadUrl}'", dlArgs.Error);
                                StatusChange(DownloadStatus.Failed, "Download file failed!");
                                return;
                            }
                            if (dlArgs.Cancelled)
                            {
                                Log.LogErr($"Download of '{DownloadUrl}' was cancelled.");
                                StatusChange(DownloadStatus.Failed, "Download was cancelled");
                                return;
                            }
                            DownloadedData = dlArgs.Result;
                            DownloadedFilename = fileName;
                            ProcessDownloadedData();
                        }
                        catch (Exception ex)
                        {
                            Log.LogErr($"Exception downloading file from {DownloadUrl}!", ex);
                            StatusChange(DownloadStatus.Failed, "Unable to extract file");
                        }
                    });
                };
                StatusChange(DownloadStatus.Downloading);
                _client.DownloadDataAsync(DownloadUrl);
                _client.DownloadProgressChanged += (sender, e) =>
                {
                    PercentCompleteChange(e.ProgressPercentage);
                };
            }
        }

        private void ProcessDownloadedData()
        {
            try
            {
                if (!(_provider is FolderFileProvider))
                    throw new NotImplementedException("This will only work with a FolderFileProvider.");
                var fp = _provider as FolderFileProvider;
                using (MemoryStream ms = new MemoryStream(DownloadedData))
                {
                    Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read(ms, new Ionic.Zip.ReadOptions() { Encoding = System.Text.Encoding.UTF8 });
                    var targetDir = Path.Combine(_savePath, DownloadedFilename);
                    _provider.MkDir(targetDir);
                    var firstInfoDat = zip.EntryFileNames.FirstOrDefault(x => x.ToLower() == "info.dat");
                    if (firstInfoDat == null)
                    {
                        StatusChange(DownloadStatus.Failed, "Zip file doesn't seem to be a song (no info.dat).");
                        return;
                    }
                    var infoDatPath = Path.GetDirectoryName(firstInfoDat);
                    foreach (var ze in zip.Entries)
                    {
                        string targetName = null;
                        try
                        {
                            //i've seen nested zip files, don't waste space with those
                            if (Path.GetExtension(ze.FileName).ToLower() == "zip")
                            {
                                Log.LogMsg($"Skipped {ze.FileName} because it looks like a nested zip file.");
                                continue;
                            }
                            //if the file isn't in the same path as the located info.dat, skip it
                            if (Path.GetDirectoryName(ze.FileName) != infoDatPath)
                            {
                                Log.LogMsg($"Skipped zip file {ze.FileName} because it wasn't in the path with info.dat at {infoDatPath}");
                                continue;
                            }
                            targetName = Path.Combine(targetDir, Path.GetFileName(ze.FileName));

                            using (Stream fs = fp.GetWriteStream(targetName))
                            {
                                ze.Extract(fs);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.LogErr($"Error writing zip entry {ze.FileName} to '{targetName ?? "(null)"}'", ex);
                            throw;
                        }
                    }
                    DownloadPath = targetDir;

                    if (PercentageComplete != 100)
                        PercentCompleteChange(100);

                    StatusChange(DownloadStatus.Downloaded);
                }
            } 
            catch (Exception ex)
            {
                Log.LogErr($"Exception processing downoaded file.", ex);
                StatusChange(DownloadStatus.Failed, "Unable to extract file");
            }
        }
        public void SetStatus(DownloadStatus status, string message = null)
        {
            StatusChange(status, message);
        }            

        private string DownloadedFilename { get; set; }
        private byte[] DownloadedData { get; set; }
        public Uri DownloadUrl { get; private set; }
        public DownloadStatus Status { get; private set; }
        public string DownloadPath { get; private set; }
    }
}