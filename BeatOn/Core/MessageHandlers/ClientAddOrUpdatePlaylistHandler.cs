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
using QuestomAssets.AssetOps;
using QuestomAssets.Models;

namespace BeatOn.Core.MessageHandlers
{
    public class ClientAddOrUpdatePlaylistHandler : IMessageHandler
    {
        private GetQaeDelegate _getQae;
        private GetBeatOnConfigDelegate _getConfig;

        public ClientAddOrUpdatePlaylistHandler(GetQaeDelegate getQae, GetBeatOnConfigDelegate getConfig)
        {
            _getQae = getQae;
            _getConfig = getConfig;
        }

        public MessageType HandlesType => MessageType.AddOrUpdatePlaylist;

        public void HandleMessage(MessageBase message, SendHostMessageDelegate sendHostMessage)
        {
            var msg = message as ClientAddOrUpdatePlaylist;
            if (msg == null)
                throw new ArgumentException("Message is not the right type");
            var cfg = _getConfig();
            AddOrUpdatePlaylistOp op = new AddOrUpdatePlaylistOp(msg.Playlist);

            var qae = _getQae();
            op.OpFinished += (s, e) =>
            {
                if (e.Status == OpStatus.Complete)
                {
                    _getConfig().Config = qae.GetCurrentConfig();
                }
            };
            qae.OpManager.QueueOp(op);
        }

        
    }
}