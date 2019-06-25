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
    public class ClientDeletePlaylistHandler : IMessageHandler
    {
        private GetQaeDelegate _getQae;
        private GetBeatOnConfigDelegate _getConfig;
        public ClientDeletePlaylistHandler(GetQaeDelegate getQae, GetBeatOnConfigDelegate getConfig)
        {
            _getQae = getQae;
            _getConfig = getConfig;
        }

        public void HandleMessage(MessageBase message)
        {
            var msg = message as ClientDeletePlaylist;
            if (msg == null)
                throw new ArgumentException("Message is not the right type");
            var op = new DeletePlaylistOp(msg.PlaylistID);
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