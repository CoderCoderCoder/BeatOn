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
using static BeatOn.SyncManager;

namespace BeatOn.Core.MessageHandlers
{
    [MessageHandler(MessageType.SyncSaberSync)]
    public class ClientSyncSaberSyncHandler : IMessageHandler
    {
        private Func<SyncManager> _getSyncManager;
        public ClientSyncSaberSyncHandler(Func<SyncManager> getSyncManager)
        {
            _getSyncManager = getSyncManager;
        }

        public void HandleMessage(MessageBase message, SendHostMessageDelegate sendHostMessage)
        {
            var msg = message as ClientSyncSaber;
            if (msg == null)
                throw new ArgumentException("Message is not the right type");
            List<FeedSyncStatus> feedStatus = null;
            EventHandler<FeedSyncStatus> handler = null;
            handler = new EventHandler<FeedSyncStatus>((s, f) =>
            {
                //rare chance, but rather it not crash.  probably dumb.
                if (feedStatus != null)
                {
                    if (!_getSyncManager().ActiveSyncs.Any(x=> feedStatus.Contains(x)))
                    {
                        var ar = new ActionResponse();
                        ar.ResponseToMessageID = msg.MessageID;
                        //well, maybe not success
                        ar.Success = true;
                        sendHostMessage(ar);
                        _getSyncManager().SyncStatusUpdate -= handler;
                    }
                }
            });
            _getSyncManager().SyncStatusUpdate += handler;
            feedStatus = _getSyncManager().Sync(msg.SyncOnlyID);
        }
    }
}