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
using QuestomAssets;
using BeatOn.Core;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace BeatOn
{
    [Service(Label="BeatOnService", Name ="com.emulamer.BeatOnService")]
    public class BeatOnService : Service
    {
        private const int BROADCAST_STATUS_INTERVAL_MS = 5000;

        private Timer _broadcastStatusTimer;
        private BeatOnServiceTransceiver _transciever;
        private bool _isBrowserOpen = false;

        private void StartStatusTimer()
        {
            if (_broadcastStatusTimer != null)
                StopStatusTimer();
            _broadcastStatusTimer = new Timer((o) =>
            {
                if (_core == null)
                {
                    if (CheckSelfPermission(Android.Manifest.Permission.WriteExternalStorage) == Android.Content.PM.Permission.Granted)
                    {
                        //Toast.makeText(getApplicationContext(), "Hello Javatpoint", Toast.LENGTH_SHORT).show();
                        _core = new BeatOnCore(this, _transciever.SendPackageInstall, _transciever.SendPackageUninstall);
                        _core.Start();
                    }
                }

                if (_core != null)
                {
                    _transciever.SendServerStatusInfo(new ServiceStatusInfo() { Url = _core.Url });
                }
            }, null, 0, BROADCAST_STATUS_INTERVAL_MS);
        }

        private void StopStatusTimer()
        {
            if (_broadcastStatusTimer != null)
            {
                _broadcastStatusTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _broadcastStatusTimer.Dispose();
                _broadcastStatusTimer = null;
            }
        }

        private BeatOnCore _core;

        public BeatOnService()
        {
            Log.ClearSinksOfType<AndroidLogger>( (x) => true);
            //prevent duplicate log sinks of these types from getting registered.
            Log.ClearSinksOfType<FileLogger>( x=> x.LogFile == Constants.SERVICE_LOGFILE);
            Log.SetLogSink(new AndroidLogger());
            _fileLogger = new FileLogger(Constants.SERVICE_LOGFILE);
            Log.SetLogSink(_fileLogger);
            Log.LogMsg($"Beat On Service v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()} starting up");
            QuestomAssets.Utils.FileUtils.GetTempDirectoryOverride = () =>
            {
                try
                {
                    var appCache = Path.Combine(Application.Context.GetExternalFilesDir(null).AbsolutePath, "cache");
                    if (!Directory.Exists(appCache))
                        Directory.CreateDirectory(appCache);
                    return appCache;
                }
                catch (Exception ex)
                {
                    Log.LogErr("Exception trying to get/make external cache folder!", ex);
                    throw ex;
                }
            };
        }
        public IBinder Binder { get; private set; }


        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            // This method executes on the main thread of the application.
            Log.LogMsg("BeatOnService started");
            

            return StartCommandResult.Sticky;
        }

        private FileLogger _fileLogger;
        public override void OnCreate()
        {
            //hook up handlers for all avenues of otherwise uncaught exceptions to make sure they get logged.
            AppDomain.CurrentDomain.UnhandledException += (sender, ex) => {
                var castEx = ex.ExceptionObject as Exception;
                if (castEx != null)
                {
                    Log.LogErr("------------------->  AppDomain.CurrentDomain.UnhandledException!", castEx);
                }
                else
                {
                    Log.LogErr("------------------->  AppDomain.CurrentDomain.UnhandledException!  Exception Object was not an exception or was null!");
                }
                try
                {
                    if (_fileLogger != null)
                        _fileLogger.Flush();
                }
                catch { }
            };
            TaskScheduler.UnobservedTaskException += (sender, ex) =>
            {
                Log.LogErr("------------------->  TaskScheduler.UnobservedTaskException!", ex.Exception);
                try
                {
                    if (_fileLogger != null)
                        _fileLogger.Flush();
                }
                catch { }
            };

            AndroidEnvironment.UnhandledExceptionRaiser += (sender, ex) =>
            {
                Log.LogErr("------------------->  AndroidEnvironment.UnhandledExceptionRaiser!", ex.Exception);
                try
                {
                    if (_fileLogger != null)
                        _fileLogger.Flush();
                }
                catch { }
            };


            Log.LogMsg("BeatOnService OnCreate called");

            // This method is optional to implement
            base.OnCreate();

            _transciever = new BeatOnServiceTransceiver(this);
            _transciever.RegisterContextForIntents(BeatOnIntent.DownloadUrl, BeatOnIntent.ImportConfigFile, BeatOnIntent.RestartCore);
            _transciever.RestartCoreReceived+= (e, a) =>
                { 
                    //TODO:
                };
            _transciever.DownloadUrlReceived += (e, a) =>
                {
                    if (_core != null)
                    {
                        _core.DownloadUrl(a.Url, a.MimeType);
                    }
                };
            //TODO: import config file
            

            StartStatusTimer();
        }


        public override IBinder OnBind(Intent intent)
        {
            // I'm not sure what happens here, is this where things should be set up?
            // what if multiple clients bind, is this called multiple times?
            Log.LogMsg("BeatOnService OnBind called");
            return null;
        }

        public override bool OnUnbind(Intent intent)
        {
            Log.LogMsg("BeatOnService OnUnbind called");

            return false;
        }

        public override void OnDestroy()
        {
            Log.LogMsg("BeatOnService OnDestroy called");
            StopStatusTimer();
            _transciever.UnregisterIntents();
            _transciever.Dispose();
            _transciever = null;

            //todo: shut down stuff here
            _core.Dispose();
            _core = null;
            base.OnDestroy();
        }

    }
}