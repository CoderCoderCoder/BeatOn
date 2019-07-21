using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BeatOn.ClientModels;

namespace BeatOn.Core.MessageHandlers
{
    [MessageHandler(ClientModels.MessageType.StopDownloads)]
    public class ClientStopDownloadsHandler : IMessageHandler
    {
        Func<DownloadManager> _getDownloadManager;
        public ClientStopDownloadsHandler(Func<DownloadManager> getDownloadManager)
        {
            _getDownloadManager = getDownloadManager;
        }

        public void HandleMessage(MessageBase message, SendHostMessageDelegate sendHostMessage)
        {
            var msg = message as ClientStopDownloads;
            if (msg == null)
                throw new ArgumentException("Message is not the right type");
            var dlmgr = _getDownloadManager();
            foreach (var dl in dlmgr.Downloads)
            {
                if ((msg.DownloadsToStop == null || msg.DownloadsToStop.Count < 1 || msg.DownloadsToStop.Contains(dl.ID)) && (dl.Status == DownloadStatus.Downloading || dl.Status == DownloadStatus.NotStarted || dl.Status == DownloadStatus.Downloaded))
                    dl.SetStatus(DownloadStatus.Aborted);
            }
           
        }
    }
}