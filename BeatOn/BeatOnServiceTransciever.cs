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
using Newtonsoft.Json;
using QuestomAssets;

namespace BeatOn
{
    public class ServiceStatusInfo
    {
        public string Url { get; set; }
    }

    public class DownloadUrlInfo
    {
        public string Url { get; set; }
        public string MimeType { get; set; }
    }

    public class ImportConfigInfo
    {
        public string ConfigFile { get; set; }
    }

    public class PackageInfo
    {
        public string PackageUrl { get; set; }
    }
    public class IntentAction
    {
        public IntentActionType Type { get; set; }
        public string PackageName { get; set; }
    }
    public enum IntentActionType
    {
        Launch,
        Exit
    }

    public enum BeatOnIntent
    {
        ServerStatusInfo,
        DownloadUrl,
        RestartCore,
        ImportConfigFile,
        InstallPackage,
        UninstallPackage,
        IntentAction
    }

    public class BeatOnServiceTransceiver : BroadcastReceiver
    {

        public bool IsRegistered { get; private set; }
        private const string ACTIVITY_PREFIX = "com.emulamer.beatonservice";
        Context _context;
        public BeatOnServiceTransceiver(Context context)
        {
            _context = context;

        }
        public event EventHandler<ServiceStatusInfo> ServiceStatusInfoReceived;
        public event EventHandler RestartCoreReceived;
        public event EventHandler<DownloadUrlInfo> DownloadUrlReceived;
        public event EventHandler<PackageInfo> InstallPackageReceived;
        public event EventHandler<PackageInfo> UninstallPackageReceived;
        public event EventHandler<IntentAction> IntentActionReceived;

        public void RegisterContextForIntents(params BeatOnIntent[] intents)
        {
            IntentFilter filter = new IntentFilter();
            intents.ToList().ForEach(x => filter.AddAction($"{ACTIVITY_PREFIX}.{x}"));
            _context.RegisterReceiver(this, filter);
            IsRegistered = true;
        }

        public void UnregisterIntents()
        {
            _context.UnregisterReceiver(this);
            IsRegistered = false;
        }

        private void SendIntent(BeatOnIntent intent, string json)
        {
            Intent broadCastIntent = new Intent();
            broadCastIntent.SetAction($"{ACTIVITY_PREFIX}.{intent}");


            broadCastIntent.PutExtra("json", json);

            _context.SendBroadcast(broadCastIntent);
        }

        public void SendServerStatusInfo(ServiceStatusInfo info)
        {
            SendIntent(BeatOnIntent.ServerStatusInfo, JsonConvert.SerializeObject(info));
        }

        public void SendDownloadUrl(DownloadUrlInfo info)
        {
            SendIntent(BeatOnIntent.DownloadUrl, JsonConvert.SerializeObject(info));
        }
        public void SendPackageInstall(string package)
        {
            SendIntent(BeatOnIntent.InstallPackage, JsonConvert.SerializeObject(new PackageInfo() { PackageUrl = package }));
        }

        public void SendPackageUninstall(string package)
        {
            SendIntent(BeatOnIntent.UninstallPackage, JsonConvert.SerializeObject(new PackageInfo() { PackageUrl = package }));
        }

        public void SendRestartCore()
        {
            SendIntent(BeatOnIntent.RestartCore, "");  
        }

        public void SendImportConfig(ImportConfigInfo info)
        {
            SendIntent(BeatOnIntent.ImportConfigFile, JsonConvert.SerializeObject(info));
        }

        public void SendIntentAction(IntentAction action)
        {
            SendIntent(BeatOnIntent.IntentAction, JsonConvert.SerializeObject(action));
        }

        public override void OnReceive(Context context, Intent intent)
        {
            try
            {
                var type = intent.Action.Substring(intent.Action.LastIndexOf(".") + 1);
                BeatOnIntent intentType;
                if (!Enum.TryParse<BeatOnIntent>(type, out intentType))
                {
                    Log.LogErr($"Received intent {type} but can't parse it into BeatOnIntent enum!");
                    return;
                }

                var json = intent.GetStringExtra("json");
                Log.LogMsg($"Handling intent {intentType} with json payload {json}");

                switch (intentType)
                {
                    case BeatOnIntent.DownloadUrl:
                        DownloadUrlReceived?.Invoke(this, JsonConvert.DeserializeObject<DownloadUrlInfo>(json));
                        break;
                    case BeatOnIntent.RestartCore:
                        RestartCoreReceived?.Invoke(this, new EventArgs());
                        break;
                    case BeatOnIntent.ServerStatusInfo:
                        ServiceStatusInfoReceived?.Invoke(this, JsonConvert.DeserializeObject<ServiceStatusInfo>(json));
                        break;
                    case BeatOnIntent.UninstallPackage:
                        InstallPackageReceived?.Invoke(this, JsonConvert.DeserializeObject<PackageInfo>(json));
                        break;
                    case BeatOnIntent.InstallPackage:
                        UninstallPackageReceived?.Invoke(this, JsonConvert.DeserializeObject<PackageInfo>(json));
                        break;
                    case BeatOnIntent.IntentAction:
                        IntentActionReceived?.Invoke(this, JsonConvert.DeserializeObject<IntentAction>(json));
                        break;
                    default:
                        Log.LogErr($"Unhandled enum type in OnReceive: {intentType}");
                        break;
                }

            }
            catch (Exception ex)
            {
                Log.LogErr($"Exception handling OnReceive intent of action {intent?.Action}", ex);
            }
        }
    }


}