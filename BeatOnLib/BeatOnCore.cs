using System;
using System.Collections.Generic;
using System.IO;
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
using QuestomAssets.AssetsChanger;
using QuestomAssets.BeatSaber;
using QuestomAssets.Utils;

namespace BeatOnLib
{

    public class BeatOnCore 
    {
        private string _rootAssetsFolder;

        public BeatOnCore(string rootAssetsFolder)
        {
            _logger = new StringLogger();
            Log.SetLogSink(_logger);
        }

        private StringLogger _logger;
        private IAssetsFileProvider _fileProvider;
        private QuestomAssetsEngine _engine;
        private QuestomAssetsEngine Engine
        {
            get
            {
                if (_engine == null)
                    InitEngine();

                return _engine;
            }
        }
        
        private void InitEngine()
        {
            if (_fileProvider == null)
                _fileProvider = new FolderFileProvider(_rootAssetsFolder, false);

            if (ImageUtils.Instance == null)
                ImageUtils.Instance = new ImageUtilsDroid();


            _engine = new QuestomAssetsEngine(_fileProvider, _rootAssetsFolder, false);
        }

        public string GetLog()
        {
            return _logger.GetString();
        }

        public void ClearLog()
        {
            _logger.ClearLog();
        }

        public string GetConfigJson()
        {
            try
            {
                return JsonConvert.SerializeObject(Engine.GetCurrentConfig(true));
            }
            catch (Exception ex)
            {
                Log.LogErr("Exception in BeatOnCore.GetConfig", ex);
            }
            return null;
        }

        public bool UpdateConfig(string configJson)
        {
            try
            {
                var config = JsonConvert.DeserializeObject<BeatSaberQuestomConfig>(configJson);
                Engine.UpdateConfig(config);
                return true;
            }
            catch (Exception ex)
            {
                Log.LogErr("Exception in BeatOnCore.UpdateConfig", ex);
            }
            return false;
        }

        public bool SignApk(string apkFile)
        {
            try
            {
                using (var fp = new ApkAssetsFileProvider(apkFile, FileCacheMode.Memory))
                {
                    ApkSigner signer = new ApkSigner();
                    signer.Sign(fp);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.LogErr("Error signing APK", ex);
            }
            return false;
        }

        public bool SaveFromApkToFile(string apkFile, string filenameToGet, string outputFile)
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(outputFile)))
                    throw new Exception($"The directory for {outputFile} does not exist!");

                using (var fp = new ApkAssetsFileProvider(apkFile, FileCacheMode.Memory))
                {
                    using (var fs = File.Open(outputFile, FileMode.Create, FileAccess.Write))
                    {
                        using (var stream = fp.GetReadStream(filenameToGet, true))
                        {
                            stream.CopyTo(fs);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.LogErr($"Error getting file {filenameToGet} from apk {apkFile}", ex);
            }
            return false;
        }

        public bool SaveFromFileToApk(string apkFile, string filenameToSave, string inputFile)
        {
            try
            {
                if (!File.Exists(inputFile))
                    throw new FileNotFoundException($"The input file {inputFile} was not found!");

                
                using (var fp = new ApkAssetsFileProvider(apkFile, FileCacheMode.Memory))
                {
                    if (fp.FileExists(filenameToSave))
                    {
                        fp.Delete(filenameToSave);
                    }
                    using (var fs = File.OpenRead(inputFile))
                    {
                        fp.QueueWriteStream(filenameToSave, fs, true, true);
                        fp.Save();
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.LogErr($"Error saving file {filenameToSave} to apk {apkFile} from {inputFile}", ex);
            }
            return false;
        }
    }
   
}