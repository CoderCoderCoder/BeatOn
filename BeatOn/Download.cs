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
        public string TargetPlaylistID { get; set; }
        public bool SuppressToast { get; set; } = false;
        private UrlWebClient _client = new UrlWebClient();
        internal bool ProcessAfterDownload { get; private set; }
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

        public Download(string url, bool processAfterDownload = true)
        {
            DownloadUrl = new Uri(url);
            Status = DownloadStatus.NotStarted;
            ProcessAfterDownload = processAfterDownload;
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

        private class UrlWebClient : WebClient
        {
            Uri _responseUrl;

            public Uri ResponseUrl
            {
                get { return _responseUrl; }
            }
            protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
            {
                WebResponse response = base.GetWebResponse(request, result);
                _responseUrl = response.ResponseUri;
                return response;
            }

            protected override WebResponse GetWebResponse(WebRequest request)
            {
                WebResponse response = base.GetWebResponse(request);
                _responseUrl = response.ResponseUri;
                return response;
            }
        }

        public void Start()
        {
            if (Status == DownloadStatus.Aborted)
                return;
            if (DownloadUrl.IsAbsoluteUri)
            {
                _client.DownloadDataCompleted += (s, dlArgs) =>
                {
                    if (Status == DownloadStatus.Aborted)
                        return;
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
                            DownloadedFilename = (s as UrlWebClient).ResponseUrl.LocalPath;
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
        
        public void SetStatus(DownloadStatus status, string message = null)
        {
            StatusChange(status, message);
            if (status == DownloadStatus.Aborted)
            {
                try
                {
                    _client.CancelAsync();
                }
                catch
                { }
            }
            if (status == DownloadStatus.Processed || status == DownloadStatus.Failed || status == DownloadStatus.Aborted)
                DownloadFinishedHandle.Set();
        }

        public ManualResetEvent DownloadFinishedHandle { get; } = new ManualResetEvent(false);
        public string DownloadedFilename { get; set; }
        public byte[] DownloadedData { get; private set; }
        public Uri DownloadUrl { get; private set; }
        public DownloadStatus Status { get; private set; }
    }
}