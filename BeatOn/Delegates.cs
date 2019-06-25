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
using QuestomAssets.Models;

namespace BeatOn
{
    public delegate void ShowToastDelegate(string title, string message, ToastType type = ToastType.Info, float durationSec = 3.0F);

    public delegate void SendHostMessageDelegate(ClientModels.MessageBase message);

    public delegate QuestomAssetsEngine GetQaeDelegate();

    public delegate BeatOnConfig GetBeatOnConfigDelegate();

    public delegate DownloadManager GetDownloadManagerDelegate();

    public delegate void UpdateConfigDelegate(BeatSaberQuestomConfig config);

    public delegate void SetSuppressConfigChangeMessages(bool suppress);
    
}