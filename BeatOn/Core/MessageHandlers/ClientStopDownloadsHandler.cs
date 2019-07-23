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
            dlmgr.CancelDownloads(msg.DownloadsToStop);           
        }
    }
}