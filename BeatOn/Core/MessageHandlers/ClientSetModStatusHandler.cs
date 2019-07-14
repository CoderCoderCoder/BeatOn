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
using QuestomAssets;
using QuestomAssets.AssetOps;

namespace BeatOn.Core.MessageHandlers
{
    public class ClientSetModStatusHandler : IMessageHandler
    {
        private GetQaeDelegate _getQae;
        private GetBeatOnConfigDelegate _getConfig;
        private SendHostMessageDelegate _sendHostMessage;
        public ClientSetModStatusHandler(GetQaeDelegate getQae, GetBeatOnConfigDelegate getConfig, SendHostMessageDelegate sendHostMessage)
        {
            _getQae = getQae;
            _getConfig = getConfig;
            _sendHostMessage = sendHostMessage;
        }

        public MessageType HandlesType => MessageType.SetModStatus;

        public void HandleMessage(MessageBase message, SendHostMessageDelegate sendHostMessage)
        {
            var msg = message as ClientSetModStatus;
            if (msg == null)
                throw new ArgumentException("Message is not the right type");
            try
            {
                Log.LogMsg($"ClientSetModStatusHandler got message to set mod ID {msg.ModID} to {msg.Status}");
                var qae = _getQae();
                var mod = qae.ModManager.Mods.FirstOrDefault(x => x.ID == msg.ModID);
                if (mod == null)
                {
                    throw new Exception($"Unable to locate mod ID {msg.ModID} to set its status to {msg.Status}!");
                }
                if (mod.Status == msg.Status)
                {
                    throw new Exception($"Message to set mod status, but mod ID {msg.ModID} is already set to status {msg.Status}!");
                    
                }
                List<AssetOp> ops = null;
                switch (msg.Status)
                {
                    case QuestomAssets.Mods.ModStatusType.Installed:
                        ops = qae.ModManager.GetInstallModOps(mod);
                        break;
                    case QuestomAssets.Mods.ModStatusType.NotInstalled:
                        ops = qae.ModManager.GetUninstallModOps(mod);
                        break;
                    default:
                        Log.LogErr($"Unsupported mod status '{msg.Status}' was set for mod ID {mod.ID}");
                        throw new NotImplementedException();
                }
                Log.LogMsg($"Queueing {ops.Count} ops to set mod id {mod.ID} to status {msg.Status}");
                if (ops.Count < 1)
                {
                    throw new Exception("No ops resulted from setting mod status!");
                    return;
                }
                ops.Last().OpFinished += (s, e) =>
                {
                    _getConfig().Config = qae.GetCurrentConfig();
                    if (e.Status == OpStatus.Complete)
                    {
                        var respMsg = new ActionResponse() { ResponseToMessageID = msg.MessageID, Success = true };
                        Log.LogMsg($"Sending host action success response to original message ID {msg.MessageID} for successfully setting mod ID {mod.ID} to status {msg.Status}.");
                        _sendHostMessage(respMsg);
                    }
                    if (e.Status == OpStatus.Failed)
                    {
                        var respMsg = new ActionResponse() { ResponseToMessageID = msg.MessageID, Success = false, ErrorMessage = e?.Exception?.Message };
                        Log.LogErr($"Sending host action FAILED response to original message ID {msg.MessageID} for failing setting mod ID {mod.ID} to status {msg.Status}.", e?.Exception);
                        _sendHostMessage(respMsg);
                    }
                };
                ops.ForEach(x => qae.OpManager.QueueOp(x));
            }
            catch (Exception ex)
            {
                Log.LogErr($"Exception processing client set mod status handler for mod ID {msg.ModID} with intended status {msg.Status}!", ex);
                var respMsg = new ActionResponse() { ResponseToMessageID = msg.MessageID, Success = false, ErrorMessage = $"Failed to set mod id {msg.ModID} to {msg.Status}" };
                Log.LogErr($"Sending host action FAILED response to original message ID {msg.MessageID} for successfully setting mod ID {msg.ModID} to status {msg.Status}.", ex);
                _sendHostMessage(respMsg);
            }
        }
    }
}