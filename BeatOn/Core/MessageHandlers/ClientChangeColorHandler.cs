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
    [MessageHandler(MessageType.ChangeColor)]
    public class ClientChangeColorHandler : IMessageHandler
    {
        private GetQaeDelegate _getQae;
        private GetBeatOnConfigDelegate _getConfig;
        public ClientChangeColorHandler(GetQaeDelegate getQae, GetBeatOnConfigDelegate getConfig)
        {
            _getQae = getQae;
            _getConfig = getConfig;
        }

        public void HandleMessage(MessageBase message, SendHostMessageDelegate sendHostMessage)
        {
            var msg = message as ClientChangeColor;
            if (msg == null)
                throw new ArgumentException("Message is not the right type");
            var op = new ChangeColorOp(msg.ColorType, msg.Color);
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