using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using QuestomAssets;
using QuestomAssets.Utils;
using Android.Support.V4.Content;

namespace BeatOn
{
    public class Mod
    {
        
        public const string APK_ASSETS_PATH = "assets/bin/Data/";
        public const string LIBMODLOADER_TARGET_FILE = "lib/armeabi-v7a/libmodloader.so";
        public const string MOD_TAG_FILE = "beaton.modded";

        public event EventHandler<string> StatusUpdated;
        private Context _context;
        private string _tempApk;

        public Mod(Context context)
        {
            _context = context;
        }

        private string TempApk
        {
            get
            {
                if (_tempApk == null)
                {
                    TryFindTempApk();
                }
                return _tempApk;
            }
            set
            {
                _tempApk = value;
            }
        }

        public bool DoesTempApkExist
        {
            get
            {
                return TempApk != null && File.Exists(TempApk);
            }
        }

        public bool IsTempApkModded
        {
            get
            {
                if (TempApk == null)
                {
                    Log.LogErr("IsTempApkModded was called, but the TempApk does not exist!");
                    throw new ModException("IsTempApkModded was called, but the TempApk does not exist!");
                }
                return CheckApkHasModTagFile(TempApk);
            }
        }

        public bool IsInstalledBeatSaberModded
        {
            get
            {
                string bsApk = FindBeatSaberApk();
                if (bsApk == null)
                {
                    Log.LogErr($"Tried to call {nameof(IsInstalledBeatSaberModded)} when beat saber isn't installed.");
                    throw new ModException("Beat saber is not installed, cannot check if it is modded.");
                }
                try
                {
                    return CheckApkHasModTagFile(bsApk);
                }
                catch (Exception ex)
                {
                    Log.LogErr($"Exception in {nameof(IsInstalledBeatSaberModded)} when trying to check if it is modded.", ex);
                    throw new ModException("Error checking if installed beat saber is modded.", ex);
                }
            }
        }

        public bool IsBeatSaberInstalled
        {
            get
            {
                return FindBeatSaberApk() != null;
            }
        }

        public bool CheckIsTempApkReadyForInstall()
        {
            try
            {
                if (TempApk == null)
                    return false;
                return CheckApkHasModTagFile(TempApk);
            }
            catch (Exception ex)
            {
                Log.LogErr("Error checking if temp apk is ready for install.", ex);
                throw new ModException("Error checking if temp apk is ready for install.", ex);
            }
        }

        public void CopyOriginalBeatSaberApkAndTriggerUninstall()
        {
            UpdateStatus("Locating installed Beat Saber app...");
            string bsApkPath = FindBeatSaberApk();
            if (bsApkPath == null)
            {
                UpdateStatus("Unable to find installed Beat Saber app!");
                throw new ModException("Beat Saber does not seem to be installed, could not find its APK.");
            }
            UpdateStatus("Copying original Beat Saber APK to temporary location...");
            TempApk = Path.Combine(_context.ExternalCacheDir.AbsolutePath, "beatsabermod.apk");
            try
            {
                File.Copy(bsApkPath, TempApk, true);
                UpdateStatus("APK copied successfully!");
                UpdateStatus("Prompting user to uninstall Beat Saber...");
                TriggerPackageUninstall(bsApkPath);
            }
            catch (Exception ex)
            {
                UpdateStatus("There was an error copying the original Beat Saber APK!");
                try
                {
                    File.Delete(TempApk);
                }
                catch
                { }
                throw new ModException("Problem copying original APK to temporary location.", ex);
            }
        }

        public void ApplyModToTempApk()
        {
            if (TempApk == null)
            {
                Log.LogErr("TempApk was null calling ModAndInstallBeatSaberApk, don't know where the temp apk is.");
                UpdateStatus("Unable to find the temporary APK!");
                throw new ModException("Unable to find the temporary APK.");
            }

            if (FindBeatSaberApk() != null)
            {
                UpdateStatus("The Beat Saber app is still installed, it must be removed before modding and reinstalling.");
                Log.LogErr("ModAndInstallBeatSaberApk: Beat Saber is still installed.  It needs to be gone to continue.");
                //throw new ModException("Beat Saber is still installed.  It needs to be uninstalled.");
            }

            bool tempApkModified = false;
            bool modFailed = false;
            //keep track of any temp files that may have been used so we can clean them up
            List<string> tempFiles = new List<string>();
            try
            {
                //// copy asset files from APK to /sdcard/wherever
                ExtractAssetsFromApkToExternalStorage(TempApk, new List<string>() {
                    "Managed",
                    "boot.config" });

                //// copy libassetredirect.so to the mods folder
                InstallAssetRedirectMod();

                //from this point on, the APK has been modified and isn't definitively recoverable if something goes wrong
                tempApkModified = true;

                //// modify classes.dex and inject the loadlibrary call for libmodloader.so
                InjectModLoaderToApk(TempApk, tempFiles);

                //// add libmodloader.so to the apk
                AddModLoaderToApk(TempApk);

                //// fix the manifest
                AddManifestModToApk(TempApk);

                //// add a 1 byte file to the APK so we know it's been modded to make verifying it later easier
                AddTagFileToApk(TempApk);

                //// re-sign the APK
                UpdateStatus("Re-signing the modded APK (this takes a minute)...");
                SignApk(TempApk);
                
            }
            catch (Exception ex)
            {
                Log.LogErr("Exception modding temp APK.", ex);
                UpdateStatus("Something has gone wrong modding the APK!  You will need to reinstall beat saber and try again!");
                modFailed = true;
                throw ex;
            }
            finally
            {
                tempFiles.ForEach(x =>
                {
                    try
                    {
                        File.Delete(x);
                    }
                    catch (Exception ex)
                    {
                        Log.LogErr($"Could not remove temp file '{x}'!", ex);
                    }
                });
                if (modFailed && tempApkModified)
                {
                    Log.LogErr("The TempApk file is being deleted because something went wrong modding.");
                    try
                    {
                        var tempApk = TempApk;
                        TempApk = null;
                        File.Delete(tempApk);                        
                    }
                    catch (Exception ex)
                    {
                        Log.LogErr("Could not delete the TempApk file!", ex);
                    }
                }
            }
            UpdateStatus("Modding has completed!");
        }

        public void TriggerPackageInstall()
        {
            if (TempApk == null)
                throw new Exception("TempApk is null, can't install it!");
            PackageManager pkgMgr = _context.PackageManager;
            Intent intent = new Intent(Intent.ActionView);
            Android.Net.Uri apkURI = FileProvider.GetUriForFile(
                         _context,
                         _context.PackageName + ".provider", new Java.IO.File(TempApk));
            intent.SetDataAndType(apkURI, "application/vnd.android.package-archive");
            intent.AddFlags(ActivityFlags.GrantReadUriPermission);
            //intent.SetDataAndType(Android.Net.Uri.FromFile(new Java.IO.File(packageApkPath)), "application/vnd.android.package-archive");
            _context.StartActivity(intent);
        }

        public void ResetAssets()
        {
            UpdateStatus("Locating installed Beat Saber app...");
            string bsApkPath = FindBeatSaberApk();
            if (bsApkPath == null)
            {
                UpdateStatus("Unable to find installed Beat Saber app!");
                throw new ModException("Beat Saber does not seem to be installed, could not find its APK.");
            }
            UpdateStatus("Deleting existing external assets...");
            if (Directory.Exists(Constants.ASSETS_RELOC_PATH))
                Directory.Delete(Constants.ASSETS_RELOC_PATH);
            else
                UpdateStatus("External assets didn't seem to exist already");

            ExtractAssetsFromApkToExternalStorage(bsApkPath, new List<string>() {
                    "Managed",
                    "boot.config" });
        }

        public void UninstallBeatSaber()
        {
            UpdateStatus("Locating installed Beat Saber app...");
            string bsApkPath = FindBeatSaberApk();
            if (bsApkPath == null)
            {
                UpdateStatus("Unable to find installed Beat Saber app!");
                throw new ModException("Beat Saber does not seem to be installed, could not find its APK.");
            }

            UpdateStatus("Triggering uninstall...");
            TriggerPackageUninstall(bsApkPath);
        }

        private void SignApk(string apkFilename)
        {
            try
            {
                using (var apk = new ApkAssetsFileProvider(apkFilename, FileCacheMode.None, false))
                {
                    ApkSigner signer = new ApkSigner(QuestomAssets.BeatSaber.BSConst.DebugCertificatePEM);
                    signer.Sign(apk);
                }
                UpdateStatus("APK signed!");
            }
            catch (Exception ex)
            {
                Log.LogErr($"Exception signing the APK {apkFilename}!", ex);
                UpdateStatus("Error re-signing the APK!");
                throw new ModException($"Exception signing the APK {apkFilename}!", ex);
            }
        }

        public void CleanupTempApk()
        {
            if (TempApk == null)
                return;

            try
            {
                File.Delete(TempApk);
                TempApk = null;
            }
            catch (Exception ex)
            {
                Log.LogErr("Unable to delete temp APK.", ex);
                throw new ModException("Unable to delete temp APK.", ex);
            }

        }

        private bool CheckApkHasModTagFile(string apkFilename)
        {
            using (var apk = new ApkAssetsFileProvider(apkFilename, FileCacheMode.None, true))
            {
                if (apk.FileExists(MOD_TAG_FILE))
                    return true;
            }
            return false;
        }

        private void UpdateStatus(string message)
        {
            StatusUpdated?.Invoke(this, message);
        }

        private void TriggerPackageUninstall(string packageApkPath)
        {
            PackageManager pkgMgr = _context.PackageManager;

            Intent intent = new Intent(Intent.ActionDelete, Android.Net.Uri.FromParts("package",
                    pkgMgr.GetPackageArchiveInfo(packageApkPath, 0).PackageName, null));
            _context.StartActivity(intent);
        }

        private string FindBeatSaberApk()
        {
            Intent mainIntent = new Intent(Intent.ActionMain, null);
            mainIntent.AddCategory(Intent.CategoryInfo);
            var pkgAppsList = _context.PackageManager.QueryIntentActivities(mainIntent, 0);
            foreach (var info in pkgAppsList)
            {

                if (info.ActivityInfo.PackageName == "com.beatgames.beatsaber")
                {
                    //found beat saber
                    return info.ActivityInfo.ApplicationInfo.PublicSourceDir;
                }
            }
            return null;
        }

        private string GetFromApkToFile(string apkFileName, string getFilename, string destinationFile = null)
        {
            string tempFile;
            if (destinationFile != null)
            {
                tempFile = destinationFile;
            }
            else
            {
                tempFile = Java.IO.File.CreateTempFile(getFilename, "", _context.ExternalCacheDir).AbsolutePath;
            }
            using (var apk = new ApkAssetsFileProvider(apkFileName, FileCacheMode.None, false))
            {
                using (var fs = File.Open(tempFile, FileMode.Create, FileAccess.ReadWrite))
                {
                    using (var readStream = apk.GetReadStream(getFilename, true))
                        readStream.CopyTo(fs);
                }
            }
            return tempFile;
        }

        private void SaveFileToApk(string apkFileName, string toFileName, string sourceFile)
        {
            using (var apk = new ApkAssetsFileProvider(apkFileName, FileCacheMode.None, false))
            {
                using (var fs = File.OpenRead(sourceFile))
                {
                    apk.QueueWriteStream(toFileName, fs, true, true);
                    apk.Save();
                }
            }
        }

        private void TryFindTempApk()
        {
            try
            {
                var tempApk = Path.Combine(_context.ExternalCacheDir.AbsolutePath, "beatsabermod.apk");
                if (File.Exists(tempApk))
                {
                    //TODO: more validation to make sure it isn't busted?
                    TempApk = tempApk;
                }
            }
            catch (Exception ex)
            {
                Log.LogErr("Exception trying to find the temp apk.", ex);
                _tempApk = null;
            }
        }

        private void InjectModLoaderToApk(string apkFilename, List<string> tempFiles)
        {
            try
            {
                UpdateStatus("Getting classes.dex from APK...");

                string classesDexTempFile = GetFromApkToFile(apkFilename, "classes.dex");
                tempFiles.Add(classesDexTempFile);
                string moddedClassesDexTempFile = Java.IO.File.CreateTempFile("moddedclasses.dex", "", _context.ExternalCacheDir).AbsolutePath;
                tempFiles.Add(moddedClassesDexTempFile);
                using (Com.Emulamer.Installerhelper.DexHelper dexHelper = new Com.Emulamer.Installerhelper.DexHelper())
                {
                    if (!dexHelper.InjectDex(new Java.IO.File(classesDexTempFile), new Java.IO.File(moddedClassesDexTempFile)))
                    {
                        UpdateStatus("classes.dex appears to already be modified");
                        Log.LogMsg("Tried to inject static constructor to classes.dex, but it seems to already have one.");
                    }
                    else
                    {
                        UpdateStatus("Writing modded classes.dex to the APK...");
                        SaveFileToApk(apkFilename, "classes.dex", moddedClassesDexTempFile);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogErr($"Exception in {nameof(InjectModLoaderToApk)}!", ex);
                UpdateStatus("Error injecting modloader!");
                throw new ModException("Unable to inject mod loader!", ex);
            }
        }

        private void ExtractAssetsFromApkToExternalStorage(string apkFilename, List<string> excludePaths = null)
        {
            UpdateStatus("Extracting assets files from the APK to external storage...");
            using (var apk = new ApkAssetsFileProvider(apkFilename, FileCacheMode.None, true))
            {
                foreach (var assetFilename in apk.FindFiles(APK_ASSETS_PATH + "*"))
                {
                    string relativeFilename = assetFilename.Substring(APK_ASSETS_PATH.Length);
                    if (excludePaths != null)
                    {
                        if (excludePaths.Any(x => relativeFilename.StartsWith(x)))
                        {
                            Log.LogMsg($"The asset file {assetFilename} ({relativeFilename}) is not included in assets that should be extracted, skipping.");
                            continue;
                        }
                    }
                    Log.LogMsg($"Extracting {assetFilename}...");
                    string targetFile = Path.Combine(Constants.ASSETS_RELOC_PATH, relativeFilename);
                    string dirName = Path.GetDirectoryName(targetFile);
                    try
                    {
                        if (!Directory.Exists(dirName))
                        {
                            Log.LogMsg($"Assets target directory doesn't exist, creating {dirName}");
                            Directory.CreateDirectory(dirName);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.LogErr($"Unable to create directory {dirName}!", ex);
                        UpdateStatus("Failed to create assets directory in external storage!");
                        throw new ModException($"Unable to create directory {dirName}!", ex);
                    }
                    try
                    {
                        using (var readStream = apk.GetReadStream(assetFilename, true))
                        {
                            using (var fs = File.Open(targetFile, FileMode.Create, FileAccess.Write))
                            {
                                readStream.CopyTo(fs);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.LogErr($"Failed to extract {assetFilename} to {targetFile}", ex);
                        UpdateStatus("Failed extracting an asset from the APK to external storage!");
                        throw new ModException($"Failed to extract {assetFilename} to {targetFile}", ex);
                    }
                }
            }
        }

        private void AddManifestModToApk(string apkFilename)
        {
            UpdateStatus("Modding the manifest in the APK...");
            try
            {
                using (var apk = new ApkAssetsFileProvider(apkFilename, FileCacheMode.None, false))
                {
                    using (var resStream = _context.Resources.OpenRawResource(Resource.Raw.manifestmod))
                    {
                        apk.QueueWriteStream("AndroidManifest.xml", resStream, true, true);
                        apk.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogErr("Error modding the manifest in the APK", ex);
                UpdateStatus("Error modding the manifest in the APK!");
                throw new ModException("Error modding the manifest in the APK", ex);
            }
        }

        private void AddModLoaderToApk(string apkFilename)
        {
            UpdateStatus("Adding the libmodloader.so file to the APK...");
            try
            {
                using (var apk = new ApkAssetsFileProvider(apkFilename, FileCacheMode.None, false))
                {
                    using (var resStream = _context.Resources.OpenRawResource(Resource.Raw.libmodloader))
                    {
                        apk.QueueWriteStream(LIBMODLOADER_TARGET_FILE, resStream, true, true);
                        apk.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogErr("Error adding libmodloader.so to APK", ex);
                UpdateStatus("Error adding modloader to the APK!");
                throw new ModException("Error adding libmodloader.so to APK", ex);
            }
        }

        private void InstallAssetRedirectMod()
        {
            UpdateStatus("Installing asset redirection mod ...");
            try
            {
                string dirName = Constants.MODLOADER_MODS_PATH;
                try
                {
                    if (!Directory.Exists(dirName))
                    {
                        Log.LogMsg($"Mods target directory doesn't exist, creating {dirName}");
                        Directory.CreateDirectory(dirName);
                    }
                }
                catch (Exception ex)
                {
                    Log.LogErr($"Unable to create directory {dirName}!", ex);
                    UpdateStatus("Failed to create mods directory in external storage!");
                    throw new ModException($"Unable to create directory {dirName}!", ex);
                }
                using (var resStream = _context.Resources.OpenRawResource(Resource.Raw.libbeatonmod))
                {
                    using (var fs = File.Open(Path.Combine(Constants.MODLOADER_MODS_PATH, "libbeatonmod.so"), FileMode.Create, FileAccess.Write))
                    {
                        resStream.CopyTo(fs);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogErr("Error copying libbeatonmod.so!", ex);
                UpdateStatus("Error installing asset redirection mod!");
                throw new ModException("Error copying libbeatonmod.so", ex);
            }
        }

        private void AddTagFileToApk(string apkFilename)
        {
            using (var apk = new ApkAssetsFileProvider(apkFilename, FileCacheMode.None, false))
            {
                if (apk.FileExists(MOD_TAG_FILE))
                {
                    Log.LogMsg("APK file already had the mod's tag file.");
                    return;
                }
                apk.Write(MOD_TAG_FILE, new byte[1], true, false);
                apk.Save();
            }
        }

    }
}