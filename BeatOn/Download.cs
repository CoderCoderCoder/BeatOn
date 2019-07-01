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

        public Download(string url)
        {
            DownloadUrl = new Uri(url);
            Status = DownloadStatus.NotStarted;
        }

        //public Download(QaeConfig config, byte[] downloadedData, string downloadedFilename, Func<QuestomAssets.QuestomAssetsEngine> engineFactory)
        //{
        //    DownloadUrl = new Uri($"file://{downloadedFilename}");
        //    Status = DownloadStatus.NotStarted;
        //    _qaeConfig = config;
        //    DownloadedData = downloadedData;
        //    DownloadedFilename = downloadedFilename;
        //    _engineFactory = engineFactory;
        //}

        public void Start()
        {
            if (DownloadUrl.IsAbsoluteUri)
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
                            StatusChange(DownloadStatus.Downloaded);
                        }
                        catch (Exception ex)
                        {
                            Log.LogErr($"Exception downloading file from {DownloadUrl}!", ex);
                            StatusChange(DownloadStatus.Failed, "Unable to extract file");
                        }
                    });
                };
                StatusChange(DownloadStatus.Downloading);
                _client.DownloadProgressChanged += (sender, e) =>
                {
                    PercentCompleteChange(e.ProgressPercentage);
                };
                _client.DownloadDataAsync(DownloadUrl);
            }
        }
        
        //private void ProcessDownloadedData()
        //{
        //    try
        //    {
        //        using (MemoryStream ms = new MemoryStream(DownloadedData))
        //        {
        //            Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read(ms, new Ionic.Zip.ReadOptions() { Encoding = System.Text.Encoding.UTF8 });

        //            DownloadType = IdentifyFileType(zip);
        //            switch (DownloadType)
        //            {
        //                case DownloadType.ModFile:
        //                    ProcessDownloadedMod(zip);
        //                    break;
        //                case DownloadType.OldSongFile:
        //                    throw new NotImplementedException("Old format songs aren't supported yet");
        //                case DownloadType.SongFile:
        //                    ProcessDownloadedSong(zip);                            
        //                    break;
        //                default:
        //                    break;
        //            }


        //            if (PercentageComplete != 100)
        //                PercentCompleteChange(100);

        //            StatusChange(DownloadStatus.Downloaded);
        //        }
        //    } 
        //    catch (Exception ex)
        //    {
        //        Log.LogErr($"Exception processing downoaded file.", ex);
        //        StatusChange(DownloadStatus.Failed, "Unable to extract file");
        //    }
        //}

        public void SetStatus(DownloadStatus status, string message = null)
        {
            StatusChange(status, message);
        }            

        public string DownloadedFilename { get; set; }
        public byte[] DownloadedData { get; private set; }
        public Uri DownloadUrl { get; private set; }
        public DownloadStatus Status { get; private set; }
    }
}