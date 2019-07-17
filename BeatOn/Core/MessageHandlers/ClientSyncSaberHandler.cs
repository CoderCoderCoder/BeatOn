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
    public class ClientSyncSaberHandler : IMessageHandler
    {
        private Func<SyncManager> _getSyncManager;
        public ClientSyncSaberHandler(Func<SyncManager> getSyncManager)
        {
            _getSyncManager = getSyncManager;
        }
        public MessageType HandlesType => MessageType.SyncSaber;

        public void HandleMessage(MessageBase message, SendHostMessageDelegate sendHostMessage)
        {
            var msg = message as ClientSyncSaber;
            if (msg == null)
                throw new ArgumentException("Message is not the right type");

            _getSyncManager().Sync(msg.SyncOnlyID);            
        }
    }
}