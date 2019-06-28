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
using QuestomAssets.BeatSaber;

namespace BeatOn
{
    //I really need to make this a more generic piece of code that isn't interwoven with what it's downloading
    //This whole class is a damn mess, inconsistent property values, meaningless names.... I need to refactor it
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

        private QaeConfig _qaeConfig;
        private QuestomAssetsEngine _qae;
        private Func<QuestomAssets.QuestomAssetsEngine> _engineFactory;
        public Download(string url, QaeConfig config, Func<QuestomAssets.QuestomAssetsEngine> engineFactory)
        {
            DownloadUrl = new Uri(url);
            Status = DownloadStatus.NotStarted;
            _qaeConfig = config;
            _engineFactory = engineFactory;
        }

        public Download(QaeConfig config, byte[] downloadedData, string downloadedFilename, Func<QuestomAssets.QuestomAssetsEngine> engineFactory)
        {
            DownloadUrl = new Uri($"file://{downloadedFilename}");
            Status = DownloadStatus.NotStarted;
            _qaeConfig = config;
            DownloadedData = downloadedData;
            DownloadedFilename = downloadedFilename;
            _engineFactory = engineFactory;
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

        
        private void ProcessDownloadedSong(Ionic.Zip.ZipFile zip)
        {           
            var targetDir = DownloadedFilename;
            _qaeConfig.SongFileProvider.MkDir(targetDir);

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

                    using (Stream fs = _qaeConfig.SongFileProvider.GetWriteStream(targetName))
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
            //todo: this isn't accurate, it's relative to the song file provider
            DownloadPath = targetDir;
        }

        private void ProcessDownloadedMod(Ionic.Zip.ZipFile zip)
        {
            var def = QuestomAssets.Mods.ModDefinition.LoadFromZipFile(zip);
            var outfile = MiscUtils.SanitizeName(def.ID) + ".zip";
            _qaeConfig.RootFileProvider.MkDir(_qaeConfig.ModsSourcePath);
            _qaeConfig.RootFileProvider.Write(_qaeConfig.ModsSourcePath + "/" + outfile, DownloadedData);
            DownloadedFilename = outfile;
            DownloadPath = _qaeConfig.ModsSourcePath;
        }

        private DownloadType IdentifyFileType(Ionic.Zip.ZipFile zip)
        {
            if (zip.EntryFileNames.Any(x => x.ToLower() == "beatonmod.json"))
                return DownloadType.ModFile;
            else if (zip.EntryFileNames.Any(x => x.ToLower() == "info.dat"))
                return DownloadType.SongFile;
            else if (zip.EntryFileNames.Any(x => x.ToLower() == "info.json"))
                return DownloadType.OldSongFile;
            else
                return DownloadType.Unknown;
        }

        private void ProcessDownloadedData()
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(DownloadedData))
                {
                    Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read(ms, new Ionic.Zip.ReadOptions() { Encoding = System.Text.Encoding.UTF8 });

                    DownloadType = IdentifyFileType(zip);
                    switch (DownloadType)
                    {
                        case DownloadType.ModFile:
                            ProcessDownloadedMod(zip);
                            break;
                        case DownloadType.OldSongFile:
                            throw new NotImplementedException("Old format songs aren't supported yet");
                        case DownloadType.SongFile:
                            ProcessDownloadedSong(zip);                            
                            break;
                        default:
                            break;
                    }


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

        public string DownloadedFilename { get; set; }
        private byte[] DownloadedData { get; set; }
        public Uri DownloadUrl { get; private set; }
        public DownloadStatus Status { get; private set; }
        public string DownloadPath { get; private set; }
        public DownloadType DownloadType { get; private set; }
    }
}