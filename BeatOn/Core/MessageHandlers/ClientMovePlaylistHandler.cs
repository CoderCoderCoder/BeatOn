using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BeatOn.ClientModels;
using QuestomAssets;
using QuestomAssets.AssetOps;

namespace BeatOn.Core.MessageHandlers
{
    public class ClientMovePlaylistHandler : IMessageHandler
    {
        private GetQaeDelegate _getQae;
        private GetBeatOnConfigDelegate _getConfig;
        public ClientMovePlaylistHandler(GetQaeDelegate getQae, GetBeatOnConfigDelegate getConfig)
        {
            _getQae = getQae;
            _getConfig = getConfig;
        }

        public MessageType HandlesType => MessageType.MovePlaylist;

        public void HandleMessage(MessageBase message)
        {
            var msg = message as ClientMovePlaylist;
            if (msg == null)
                throw new ArgumentException("Message is not the right type");
            var op = new MovePlaylistOp(msg.PlaylistID, msg.Index);
            Stopwatch sw = new Stopwatch();
            
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