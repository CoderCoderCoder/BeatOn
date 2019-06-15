package com.emulamer.beaton;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.content.pm.ResolveInfo;
import android.net.Uri;
import android.util.Base64;
import android.util.Log;

import com.google.common.io.Files;

import org.jf.dexlib2.DexFileFactory;
import org.jf.dexlib2.Opcode;
import org.jf.dexlib2.iface.*;
import org.jf.dexlib2.immutable.ImmutableMethod;
import org.jf.dexlib2.immutable.ImmutableMethodImplementation;
import org.jf.dexlib2.immutable.ImmutableMethodParameter;
import org.jf.dexlib2.immutable.instruction.ImmutableInstruction;
import org.jf.dexlib2.immutable.instruction.ImmutableInstruction21c;
import org.jf.dexlib2.immutable.instruction.ImmutableInstruction35c;
import org.jf.dexlib2.immutable.reference.*;
import org.jf.dexlib2.dexbacked.DexBackedClassDef;
import org.jf.dexlib2.dexbacked.DexBackedDexFile;
import org.jf.dexlib2.dexbacked.DexBackedMethod;
import org.jf.dexlib2.rewriter.DexRewriter;
import org.jf.dexlib2.rewriter.RewriterModule;
import beatonlib.beatonlib.BeatOnCore;
import org.jf.dexlib2.rewriter.*;
import java.io.ByteArrayInputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.zip.ZipEntry;
import java.util.zip.ZipInputStream;
import org.jf.dexlib2.writer.builder.BuilderAnnotationSet;
import org.jf.dexlib2.writer.io.MemoryDataStore;
import org.jf.dexlib2.writer.pool.DexPool;
import javax.annotation.Nonnull;
import javax.annotation.Nullable;


public class BeatOnInstaller {
    private static String TAG = "BeatOnInstaller";
    private PackageManager _packageManager;
    private Context _context;
    private File _tempApk;

    private static String makeExMessage(String message, Exception ex, BeatOnCore core)
    {
        String msg = message;
        if (ex != null)
        {
            msg = msg + ex.getMessage() + " " + ex.getStackTrace();
        }
        if (core != null)
        {
            String logs = core.getLog();
            if (logs != null && logs.length() > 0)
            {
                msg += " Core logs: " + logs;
            }
            else
            {
                msg += " (no Core logs)";
            }
        }
        if (msg.length() < 1)
            msg = "Unknown error";
        Log.e(TAG, msg);
        return msg;
    }

    private void updateStatus(String message)
    {
        Log.i(TAG, "Status: "+message);
    }

    public BeatOnInstaller(Context context)
    {
        _context = context;
        _packageManager = context.getPackageManager();
    }

    public boolean modAndInstallBeatSaberApk() throws Exception
    {
        if (_tempApk == null)
        {
            Log.e(TAG, "_tempApk was null calling modAndInstallBeatSaberApk, don't know where the apk is.");
            return false;
        }

        if (findBeatSaberApk() != null)
        {
            updateStatus("The Beat Saber app is still installed, it must be removed before modding and reinstalling.");
            Log.e(TAG, "modAndInstallBeatSaberApk: Beat Saber is still installed.  It needs to be gone to continue.");
            // return false;
        }

        //keep track of any temp files that may have been used so we can clean them up
        ArrayList<File> tempFiles = new ArrayList<File>();

        BeatOnCore core = new BeatOnCore(_tempApk.getAbsolutePath());
        String targetAssetsPath = "/sdcard/Android/data/com.beatgames.beatsaber/files/assets/";

        ///////modify classes.dex and inject the loadlibrary call for libmodloader.so
        updateStatus("Getting classes.dex from APK...");
        File classesDexTempFile = getFromApkToFile(_tempApk.getAbsolutePath(),"classes.dex", null);
        tempFiles.add(classesDexTempFile);
        File moddedClassesDexTempFile = File.createTempFile("moddedclasses.dex", "", _context.getCacheDir());
        tempFiles.add(moddedClassesDexTempFile);
        if (!injectDex(classesDexTempFile, moddedClassesDexTempFile))
        {
            Log.i(TAG, "APK appears to already be modified for libmodloader injection.");
        }
        else
        {
            updateStatus("Writing modded classes.dex to the APK...");
            if (!core.saveFromFileToApk(_tempApk.getAbsolutePath(), "classes.dex", moddedClassesDexTempFile.getAbsolutePath()))
            {
                throw new Exception(makeExMessage("Failed to save classes.dex to APK.  ", null, core));
            }
        }

        ///// add libmodloader.so to the apk

        updateStatus("Adding the libmodloader.so file to the APK...");
        try {

            InputStream inp =_context.getResources().openRawResource(R.raw.libmodloader);
            byte[] data = new byte[inp.available()];
            inp.read(data);
            inp.close();
            File modloaderTempFile = File.createTempFile("libmodloader", "", _context.getCacheDir());
            tempFiles.add(modloaderTempFile);
            FileOutputStream outp = new FileOutputStream(modloaderTempFile, false);
            outp.write(data);
            outp.close();
            core.saveFromFileToApk(_tempApk.getAbsolutePath(), "lib/armeabi-v7a/libmodloader.so", modloaderTempFile.getAbsolutePath());
        }
        catch (Exception ex)
        {
            throw new Exception(makeExMessage("Error adding libmodloader.so to APK.", ex, core));
        }


        ///// copy asset files from APK to /sdcard/wherever
        updateStatus("Extracting assets files from the APK to external storage...");
        FileInputStream tempApkStream = new FileInputStream(_tempApk);
        ZipInputStream zipIs = new ZipInputStream(tempApkStream);
        ZipEntry ze = null;
        String assetsApkPath = "assets/bin/Data";
        while ((ze = zipIs.getNextEntry()) != null) {
            String fileName = ze.getName();
            if (!fileName.startsWith(assetsApkPath))
                continue;

            fileName = fileName.substring(assetsApkPath.length()+1);
            File targetFile = new File(targetAssetsPath, fileName);
            if (!targetFile.getParentFile().exists())
            {
                if (!targetFile.getParentFile().mkdirs())
                    throw new Exception("Unable to make directory " + targetFile.getParentFile().getAbsolutePath());
            }
            FileOutputStream fout = new FileOutputStream(targetAssetsPath + fileName,false);

            byte[] buffer = new byte[1024];
            int length = 0;

            while ((length = zipIs.read(buffer))>0) {
                fout.write(buffer, 0, length);
            }
            zipIs.closeEntry();
            fout.close();
        }
        zipIs.close();
        tempApkStream.close();

        //// copy libassetredirect.so to the mods folder
        updateStatus("Copying libassetredirect.so to the mods folder...");
        File libRedirectTarget = new File("/sdcard/Android/data/com.beatgames.beatsaber/files/mods/libassetredirect.so");
        InputStream inp2 =_context.getResources().openRawResource(R.raw.libassetredirect);
        byte[] data2 = new byte[inp2.available()];
        inp2.read(data2);
        inp2.close();
        if (!libRedirectTarget.getParentFile().exists())
        {
            if (!libRedirectTarget.getParentFile().mkdirs())
                throw new Exception("Unable to make directory " + libRedirectTarget.getParentFile().getAbsolutePath());
        }

        FileOutputStream outp = new FileOutputStream(libRedirectTarget,false);
        outp.write(data2);
        outp.close();


        //// re-sign the APK
        updateStatus("Re-signing the modded APK...");
        try {
            if (!core.signApk(_tempApk.getAbsolutePath()))
                throw new Exception("signApk returned false.");

        } catch (Exception ex)
        {
            throw new Exception(makeExMessage("Failed to sign the APK", ex, core), ex);
        }

        updateStatus("Prompting the user to install the modded APK...");

        Intent intent = new Intent(Intent.ACTION_VIEW);
        intent.setDataAndType(Uri.fromFile(_tempApk), "application/vnd.android.package-archive");
        _context.startActivity(intent);
        return true;
    }

    public boolean prepAndDeleteOriginalBeatSaberApk() throws Exception
    {
        updateStatus("Locating installed Beat Saber app...");
        String bsApkPath = findBeatSaberApk();
        if (bsApkPath == null) {
            updateStatus("Unable to find installed Beat Saber app!");
            Log.e(TAG, "Beat Saber does not seem to be installed, could not find its APK.");
            return false;
        }
        updateStatus("Copying original Beat Saber APK to temporary location...");
        _tempApk = new File(_context.getCacheDir(), "beatsabermod.apk");
        try {
            Files.copy(new File(bsApkPath), _tempApk);

            updateStatus("Prompting uninstall of original BeatSaber app...");
            PackageManager pkgMgr = _context.getPackageManager();

            Intent intent = new Intent(Intent.ACTION_DELETE, Uri.fromParts("package",
                    pkgMgr.getPackageArchiveInfo(bsApkPath, 0).packageName,null));
            _context.startActivity(intent);

            return true;
        } catch (Exception ex)
        {
            Exception newE = new Exception(makeExMessage("Exception copying original APK to temporary location.", ex, null), ex);
            updateStatus("There was an error copying the original Beat Saber APK!");
            _tempApk.delete();
            throw newE;
        }
    }

    private boolean injectDex (File classesDexFilename, File moddedOutputFilename) throws Exception {
        updateStatus("Loading classes.dex to inject libmodloader...");
        DexBackedDexFile dexFile = DexFileFactory.loadDexFile(classesDexFilename, null);
        updateStatus("Checking if libmodloader is already injected...");
        boolean foundStaticInit = false;
        for (DexBackedClassDef dexClass : dexFile.getClasses()) {
            String type = dexClass.getType();
            if (type.equals("Lcom/unity3d/player/UnityPlayerActivity;")) {
                for (DexBackedMethod m : dexClass.getDirectMethods()) {
                    String name = m.getName();
                    if (name.equals("<clinit>")) {
                        //found a static class initializer, can't add another, means it's probably already injected
                        foundStaticInit = true;
                        break;
                    }
                }
                break;
            }
        }
        if (foundStaticInit) {
            updateStatus("Found that libmodloader is already injected.");
            return false;
        }
        updateStatus("Rewriting dex file with libmodloader injection...");
        DexRewriter rewriter = new DexRewriter(new RewriterModule() {

            @Nonnull @Override public Rewriter<ClassDef> getClassDefRewriter(@Nonnull Rewriters rewriters) {
                return new InjectorClassDefRewrite(rewriters);
            }
        });
        DexFile rewritten = rewriter.rewriteDexFile(dexFile);
        DexFileFactory.writeDexFile(moddedOutputFilename.getAbsolutePath(), rewritten);
        return true;
    }

    private File getFromApkToFile(String apkFileName, String getFilename, @Nullable File destinationFile) throws Exception
    {
        BeatOnCore core = new BeatOnCore(apkFileName);
        try {
            File tempFile;
            if (destinationFile != null)
            {
                tempFile = destinationFile;
            }
            else
            {
                tempFile = File.createTempFile(getFilename, "", _context.getCacheDir());
            }

            if (!core.saveFromApkToFile(apkFileName, getFilename, tempFile.getAbsolutePath()))
                throw new Exception ("Failed to extract file from APK to file.");
            return tempFile;
        }
        catch (Exception ex)
        {
            String logs = core.getLog();

            if (logs == null)
                logs = ex.getMessage();
            else
                logs = ex.getMessage() + " internal BeatOnCoreLog: " + logs;

            Log.e("Tag?", "Exception in getFromApkToFile"+logs);

            throw new Exception(logs, ex);
        }
    }

    private String findBeatSaberApk() throws Exception
    {
        Intent mainIntent = new Intent(Intent.ACTION_MAIN, null);
        mainIntent.addCategory(Intent.CATEGORY_INFO);
        List pkgAppsList = _packageManager.queryIntentActivities(mainIntent, 0);
        for (Object object : pkgAppsList) {
            ResolveInfo info = (ResolveInfo) object;
            if (info.activityInfo.packageName.equals("com.beatgames.beatsaber"))
            {
                //found beat saber
                return info.activityInfo.applicationInfo.publicSourceDir;
            }
        }
        return null;
    }

}
