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
    public class ClientDeleteModHandler : IMessageHandler
    {
        private GetQaeDelegate _getQae;
        private GetBeatOnConfigDelegate _getConfig;
        private SendHostMessageDelegate _sendMessage;
        public ClientDeleteModHandler(GetQaeDelegate getQae, GetBeatOnConfigDelegate getConfig, SendHostMessageDelegate sendMessage)
        {
            _getQae = getQae;
            _getConfig = getConfig;
            _sendMessage = sendMessage;
        }

        public MessageType HandlesType => MessageType.DeleteMod;

        public void HandleMessage(MessageBase message, SendHostMessageDelegate sendHostMessage)
        {
            var msg = message as ClientDeleteMod;
            if (msg == null)
                throw new ArgumentException("Message is not the right type");

            try
            {
                
                var qae = _getQae();

                var modDef = qae.ModManager.Mods.FirstOrDefault(x => x.ID == msg.ModID);
                if (modDef == null)
                    throw new ArgumentException("Mod ID does not exist!");

                qae.ModManager.DeleteMod(modDef);
                _getConfig().Config = qae.GetCurrentConfig();
                _sendMessage(new ActionResponse() { ResponseToMessageID = msg.MessageID, Success = true });
            }
            catch (Exception ex)
            {
                Log.LogErr($"Exception deleting mod id {msg.ModID}", ex);
                _sendMessage(new ActionResponse() { ResponseToMessageID = msg.MessageID, Success = false, ErrorMessage = "Failed to delete mod" });
            }
        }
    }
}