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

namespace BeatOn.Core.MessageHandlers
{
    [MessageHandler(MessageType.UpdateFeedReader)]
    public class ClientUpdateFeedReaderHandler : IMessageHandler
    {
        private Func<SyncManager> _getSyncManager;
        public ClientUpdateFeedReaderHandler(Func<SyncManager> getSyncManager)
        {
            _getSyncManager = getSyncManager;
        }

        public void HandleMessage(MessageBase message, SendHostMessageDelegate sendHostMessage)
        {
            var msg = message as ClientUpdateFeedReader;
            if (msg == null)
                throw new ArgumentException("Message is not the right type");
            var mgr = _getSyncManager();

            var feedCfg = mgr.SyncConfig.FeedReaders.FirstOrDefault(x => x.ID == msg.ID);
            if (feedCfg == null)
            {
                Log.LogErr($"Got message to update feed config with ID {msg.ID} but it wasn't found in the config!");
                return;
            }

            try
            {
                var typedConfig = msg.FeedConfig.ToObject(feedCfg.GetType());
                int idx = mgr.SyncConfig.FeedReaders.IndexOf(feedCfg);
                mgr.SyncConfig.FeedReaders.Remove(feedCfg);
                if (idx >= mgr.SyncConfig.FeedReaders.Count)
                    idx--;
                mgr.SyncConfig.FeedReaders.Add(typedConfig as FeedConfig);
                mgr.Save();
            }
            catch (Exception ex)
            {
                Log.LogErr($"Exception deserializing JObject into proper type of {feedCfg.GetType().Name} for ID {msg.ID}", ex);
            }
        }
    }
}