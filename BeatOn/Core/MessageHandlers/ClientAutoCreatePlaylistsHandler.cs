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
    [MessageHandler(MessageType.AutoCreatePlaylists)]
    public class ClientAutoCreatePlaylistsHandler : IMessageHandler
    {
        private GetQaeDelegate _getQae;
        private GetBeatOnConfigDelegate _getConfig;
        public ClientAutoCreatePlaylistsHandler(GetQaeDelegate getQae, GetBeatOnConfigDelegate getConfig)
        {
            _getQae = getQae;
            _getConfig = getConfig;
        }

        public void HandleMessage(MessageBase message, SendHostMessageDelegate sendHostMessage)
        {
            var msg = message as ClientAutoCreatePlaylists;
            if (msg == null)
                throw new ArgumentException("Message is not the right type");
            if (msg.MaxPerNamePlaylist.HasValue && msg.MaxPerNamePlaylist.Value < 5)
                throw new Exception("You're insane. less than 5 per playlist?");
            var op = new AutoCreatePlaylistsOp(msg.SortMode, msg.MaxPerNamePlaylist??50, msg.RemoveEmptyPlaylists);
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