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

namespace BeatOn.Core.MessageHandlers
{
    public class ClientSortPlaylistHandler : IMessageHandler
    {
        private GetQaeDelegate _getQae;
        private GetBeatOnConfigDelegate _getConfig;
        public ClientSortPlaylistHandler(GetQaeDelegate getQae, GetBeatOnConfigDelegate getConfig)
        {
            _getQae = getQae;
            _getConfig = getConfig;
        }

        public MessageType HandlesType => MessageType.SortPlaylist;

        public void HandleMessage(MessageBase message)
        {
            var msg = message as ClientSortPlaylist;
            if (msg == null)
                throw new ArgumentException("Message is not the right type");
            var op = new SortPlaylistOp(msg.PlaylistID, msg.SortMode, msg.Reverse);
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