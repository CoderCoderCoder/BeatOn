package com.emulamer.installerhelper;

import org.jf.dexlib2.DexFileFactory;
import org.jf.dexlib2.iface.*;
import org.jf.dexlib2.dexbacked.DexBackedClassDef;
import org.jf.dexlib2.dexbacked.DexBackedDexFile;
import org.jf.dexlib2.dexbacked.DexBackedMethod;
import org.jf.dexlib2.rewriter.DexRewriter;
import org.jf.dexlib2.rewriter.RewriterModule;
import org.jf.dexlib2.rewriter.*;
import java.io.File;
import javax.annotation.Nonnull;


public class DexHelper  {

    public boolean checkDexInjection(File classesDexFilename) throws Exception
    {
        DexBackedDexFile dexFile = DexFileFactory.loadDexFile(classesDexFilename, null);
        return checkDexInjected(dexFile);
    }

    private boolean checkDexInjected(DexBackedDexFile dexFile ) throws Exception
    {
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
        return foundStaticInit;
    }

    public boolean injectDex (File classesDexFilename, File moddedOutputFilename) throws Exception {
        DexBackedDexFile dexFile = DexFileFactory.loadDexFile(classesDexFilename, null);

        if (checkDexInjected(dexFile)) {
            return false;
        }
        DexRewriter rewriter = new DexRewriter(new RewriterModule() {

            @Nonnull @Override public Rewriter<ClassDef> getClassDefRewriter(@Nonnull Rewriters rewriters) {
                return new InjectorClassDefRewrite(rewriters);
            }
        });
        DexFile rewritten = rewriter.rewriteDexFile(dexFile);
        DexFileFactory.writeDexFile(moddedOutputFilename.getAbsolutePath(), rewritten);
        return true;
    }


}
