﻿using System;
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

namespace BeatOn.Core.MessageHandlers
{
    [MessageHandler(MessageType.SetBeastSaberUsername)]
    public class ClientSetBeastSaberUsernameHandler : IMessageHandler
    {
        private Func<SyncManager> _getSyncManager;
        public ClientSetBeastSaberUsernameHandler(Func<SyncManager> getSyncManager)
        {
            _getSyncManager = getSyncManager;
        }

        public void HandleMessage(MessageBase message, SendHostMessageDelegate sendHostMessage)
        {
            var msg = message as ClientSetBeastSaberUsername;
            if (msg == null)
                throw new ArgumentException("Message is not the right type");

            _getSyncManager().SyncConfig.BeastSaberUsername = msg.BeastSaberUsername;
            //todo: save here?
            _getSyncManager().Save();
        }
    }
}