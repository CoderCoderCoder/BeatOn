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
        private WebClient _client = new WebClient();
        public event EventHandler<DownloadStatusChangeArgs> StatusChanged;

        private void StatusChange(DownloadStatus status, string message = null)
        {
            Status = status;
            StatusChanged?.Invoke(this, new DownloadStatusChangeArgs(status, message));
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

        public void Start()
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
                        if (!(_provider is FolderFileProvider))
                            throw new NotImplementedException("This will only work with a FolderFileProvider.");
                        var fp = _provider as FolderFileProvider;
                        using (MemoryStream ms = new MemoryStream(dlArgs.Result))
                        {
                            Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read(ms, new Ionic.Zip.ReadOptions() { Encoding = System.Text.Encoding.UTF8 });
                            var targetDir = Path.Combine(_savePath, fileName);
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
                            StatusChange(DownloadStatus.Downloaded);
                        }
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
        }

        public void SetStatus(DownloadStatus status, string message = null)
        {
            StatusChange(status, message);
        }            

        public Uri DownloadUrl { get; private set; }
        public DownloadStatus Status { get; private set; }
        public string DownloadPath { get; private set; }
    }
}