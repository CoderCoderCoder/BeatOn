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
    public class ClientGetOpsHandler : IMessageHandler
    {
        private List<AssetOp> _opList;
        private Action<MessageBase> _messageSender;
        //actual op list being used, items will be removed from it
        public ClientGetOpsHandler(List<AssetOp> opList, Action<MessageBase> messageSender)
        {
            _opList = opList;
            _messageSender = messageSender;
        }

        public MessageType HandlesType => MessageType.GetOps;

        public void HandleMessage(MessageBase message, SendHostMessageDelegate sendHostMessage)
        {
            List<AssetOp> opCopy;
            lock (_opList)
            {
                var msg = message as ClientGetOps;
                if (msg.ClearFailedOps)
                {
                    _opList.RemoveAll(x => x.Status == OpStatus.Failed);
                }
                opCopy = _opList.ToList();
            }

            HostOpStatus opstat = new HostOpStatus();

            foreach (var op in opCopy)
            {
                opstat.Ops.Add(new HostOp() { ID = op.ID, OpDescription = op.GetType().Name, Status = op.Status, Error = op?.Exception?.Message });
            }

            _messageSender(opstat);
        }
    }
}