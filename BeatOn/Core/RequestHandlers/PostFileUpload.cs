using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using QuestomAssets;

namespace BeatOn.Core.RequestHandlers
{
    public class PostFileUpload : IHandleRequest
    {
        private BeatSaberModder _mod;
        private ShowToastDelegate _showToast;
        private Func<ImportManager> _getImportManager;

        public PostFileUpload(BeatSaberModder mod, ShowToastDelegate showToast, Func<ImportManager> getImportManager)
        {
            _mod = mod;
            _showToast = showToast;
            _getImportManager = getImportManager;
        }

        public void HandleRequest(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;

            try
            {
                if (!_mod.IsBeatSaberInstalled || !_mod.IsInstalledBeatSaberModded)
                {
                    resp.BadRequest("Modded Beat Saber is not installed!");
                    _showToast("Can't upload.", "Modded Beat Saber is not installed!");
                    return;
                }
                var ct = req.ContentType;
                if (!ct.StartsWith("multipart/form-data"))
                {
                    resp.BadRequest("Expected content-type of multipart/form-data");
                    return;
                }

                Dictionary<string, MemoryStream> files = new Dictionary<string, MemoryStream>();
                var parser = new HttpMultipartParser.StreamingMultipartFormDataParser(req.InputStream);
                parser.FileHandler = (name, fileName, type, disposition, buffer, bytes) =>
                {
                    if (name != "file")
                    {
                        Log.LogMsg($"Got extra form value named {name}, ignoring it");
                        return;
                    }
                    if (type != "application/x-zip-compressed")
                        throw new NotSupportedException($"Data for file {fileName} isn't a zip");
                    MemoryStream s = null;
                    if (files.ContainsKey(fileName))
                    {
                        s = files[fileName];
                    }
                    else
                    {
                        s = new MemoryStream();
                        files.Add(fileName, s);
                    }
                    s.Write(buffer, 0, bytes);
                };
                parser.Run();
                if (files.Count < 1)
                {
                    resp.BadRequest("Didn't get any useable files.");
                    return;
                }
                foreach (var file in files.Keys.ToList())
                {
                    var s = files[file];
                    byte[] b = s.ToArray();
                    files.Remove(file);
                    s.Dispose();
                    try
                    {
                        MemoryStream ms = new MemoryStream(b);
                        try
                        {
                            var provider = new ZipFileProvider(ms, file, FileCacheMode.None, true, QuestomAssets.Utils.FileUtils.GetTempDirectory());
                            try
                            {
                                _getImportManager().ImportFromFileProvider(provider, () =>
                                {
                                    provider.Dispose();
                                    ms.Dispose();
                                });
                            }
                            catch
                            {
                                provider.Dispose();
                                throw;
                            }
                        }
                        catch
                        {
                            ms.Dispose();
                            throw;
                        }
                    }
                    catch (ImportException iex)
                    {
                        _showToast($"Unable to import file", $"There was an error importing the file {file}: {iex.FriendlyMessage}", ClientModels.ToastType.Error, 5);
                    }
                    catch (Exception ex)
                    {
                        _showToast($"Unable to process file", $"There was an error processing the file {file}.", ClientModels.ToastType.Error, 5);
                    }
                }
                resp.Ok();
            }
            catch (Exception ex)
            {
                Log.LogErr("Exception handling mod install step 1!", ex);
                resp.StatusCode = 500;
            }
        }
    }
}