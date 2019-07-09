using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BeatOn.ClientModels;
using QuestomAssets;

namespace BeatOn.Core.RequestHandlers
{
    public class GetModStatus : IHandleRequest
    {
        private BeatSaberModder _mod;
        public GetModStatus(BeatSaberModder mod)
        {
            _mod = mod;
        }
        private object _modStatusLock = new object();
        public void HandleRequest(HttpListenerContext context)
        {
            lock (_modStatusLock)
            {
                var req = context.Request;
                var resp = context.Response;
                try
                {
#if EMULATOR
                    /*THIS IS TEST CODE FOR EMULATOR*/
                    resp.SerializeOk(new ModSetupStatus()
                    {
                        IsBeatSaberInstalled = true,
                        CurrentStatus = ModSetupStatusType.ModInstalled
                    });
                    return;
#else
                    var model = new ModSetupStatus()
                    {
                        IsBeatSaberInstalled = _mod.IsBeatSaberInstalled
                    };

                    if (model.IsBeatSaberInstalled)
                    {
                        if (_mod.IsInstalledBeatSaberModded)
                        {
                            model.CurrentStatus = ModSetupStatusType.ModInstalled;
                        }
                        else if (!_mod.IsBeatSaberInstalled && _mod.DoesTempApkExist)
                        {
                            if (_mod.IsTempApkModded)
                            {
                                model.CurrentStatus = ModSetupStatusType.ReadyForInstall;
                            }
                            else
                            {
                                model.CurrentStatus = ModSetupStatusType.ReadyForModApply;
                            }
                        }
                    }
                    else
                    {
                        if (_mod.DoesTempApkExist)
                        {
                            if (_mod.IsTempApkModded)
                                model.CurrentStatus = ModSetupStatusType.ReadyForInstall;
                            else
                                model.CurrentStatus = ModSetupStatusType.ReadyForModApply;
                        }
                    }
                    resp.SerializeOk(model);
#endif
                }
                catch (Exception ex)
                {
                    Log.LogErr("Exception handling mod status!", ex);
                    resp.StatusCode = 500;
                }
            }
        }
    }
}