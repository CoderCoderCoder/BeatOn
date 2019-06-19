package com.emulamer.installerhelper;

import org.jf.dexlib2.Opcode;
import org.jf.dexlib2.iface.ClassDef;
import org.jf.dexlib2.iface.Method;
import org.jf.dexlib2.iface.MethodParameter;
import org.jf.dexlib2.immutable.ImmutableMethod;
import org.jf.dexlib2.immutable.ImmutableMethodImplementation;
import org.jf.dexlib2.immutable.ImmutableMethodParameter;
import org.jf.dexlib2.immutable.instruction.ImmutableInstruction;
import org.jf.dexlib2.immutable.instruction.*;
import org.jf.dexlib2.immutable.instruction.ImmutableInstruction21c;
import org.jf.dexlib2.immutable.instruction.ImmutableInstruction35c;
import org.jf.dexlib2.immutable.instruction.ImmutableInstructionFactory;
import org.jf.dexlib2.immutable.reference.ImmutableMethodReference;
import org.jf.dexlib2.immutable.reference.*;
import org.jf.dexlib2.rewriter.ClassDefRewriter;
import org.jf.dexlib2.rewriter.Rewriters;
import org.jf.dexlib2.writer.InstructionFactory;
import org.jf.dexlib2.writer.builder.BuilderAnnotationSet;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

public class InjectorClassDefRewrite extends ClassDefRewriter
{
    public InjectorClassDefRewrite( Rewriters rewriters) {
        super(rewriters);

    }
    @Override
    public ClassDef rewrite(ClassDef classDef) {
        return new InjectorRewrittenClassDef(classDef);
    }
    public void test()
    {/*************** BEGIN method definition for <clinit> **********/
        List<ImmutableMethodParameter> parameters0 = null;
        List<ImmutableInstruction> instructions0 = Arrays.asList(new ImmutableInstruction21c(Opcode.CONST_STRING, 0, new ImmutableStringReference("modloader")), new ImmutableInstruction35c(Opcode.INVOKE_STATIC, 1,0, 0, 0, 0, 0, new ImmutableMethodReference("Ljava/lang/System;", "loadLibrary", Arrays.asList("Ljava/lang/String;"), "V")), new ImmutableInstruction10x(Opcode.RETURN_VOID));
        ImmutableMethodImplementation methodImplementation0 = new ImmutableMethodImplementation(1, instructions0, null, null);
        Method method0 = new ImmutableMethod("Lcom/unity3d/player/UnityPlayerActivity;", "<clinit>", parameters0, "V", 65544, null, methodImplementation0);
/*************** END method definition for <clinit> **********/

/*************** BEGIN method definition for <init> **********/
        List<ImmutableMethodParameter> parameters1 = null;
        List<ImmutableInstruction> instructions1 = Arrays.asList(new ImmutableInstruction35c(Opcode.INVOKE_DIRECT, 1,0, 0, 0, 0, 0, new ImmutableMethodReference("Landroid/app/Activity;", "<init>", null, "V")), new ImmutableInstruction10x(Opcode.RETURN_VOID));
        ImmutableMethodImplementation methodImplementation1 = new ImmutableMethodImplementation(1, instructions1, null, null);
        Method method1 = new ImmutableMethod("Lcom/unity3d/player/UnityPlayerActivity;", "<init>", parameters1, "V", 65537, null, methodImplementation1);
/*************** END method definition for <init> **********/

/*************** BEGIN method definition for continueLoad **********/
        List<ImmutableMethodParameter> parameters2 = null;
        List<ImmutableInstruction> instructions2 = Arrays.asList(new ImmutableInstruction22c(Opcode.IGET_OBJECT, 0, 1, new ImmutableFieldReference("Lcom/unity3d/player/UnityPlayerActivity;", "mUnityPlayer", "Lcom/unity3d/player/UnityPlayer;")), new ImmutableInstruction35c(Opcode.INVOKE_VIRTUAL, 1,0, 0, 0, 0, 0, new ImmutableMethodReference("Lcom/unity3d/player/UnityPlayer;", "start", null, "V")), new ImmutableInstruction10x(Opcode.RETURN_VOID));
        ImmutableMethodImplementation methodImplementation2 = new ImmutableMethodImplementation(2, instructions2, null, null);
        Method method2 = new ImmutableMethod("Lcom/unity3d/player/UnityPlayerActivity;", "continueLoad", parameters2, "V", 2, null, methodImplementation2);
/*************** END method definition for continueLoad **********/

/*************** BEGIN method definition for dispatchKeyEvent **********/
        List<ImmutableMethodParameter> parameters3 = Arrays.asList(new ImmutableMethodParameter("Landroid/view/KeyEvent;",null,null));
        List<ImmutableInstruction> instructions3 = Arrays.asList(new ImmutableInstruction35c(Opcode.INVOKE_VIRTUAL, 1,3, 0, 0, 0, 0, new ImmutableMethodReference("Landroid/view/KeyEvent;", "getAction", null, "I")), new ImmutableInstruction11x(Opcode.MOVE_RESULT, 0), new ImmutableInstruction11n(Opcode.CONST_4, 1, 2), new ImmutableInstruction22t(Opcode.IF_NE, 0, 1, 9), new ImmutableInstruction22c(Opcode.IGET_OBJECT, 0, 2, new ImmutableFieldReference("Lcom/unity3d/player/UnityPlayerActivity;", "mUnityPlayer", "Lcom/unity3d/player/UnityPlayer;")), new ImmutableInstruction35c(Opcode.INVOKE_VIRTUAL, 2,0, 3, 0, 0, 0, new ImmutableMethodReference("Lcom/unity3d/player/UnityPlayer;", "injectEvent", Arrays.asList("Landroid/view/InputEvent;"), "Z")), new ImmutableInstruction11x(Opcode.MOVE_RESULT, 0), new ImmutableInstruction11x(Opcode.RETURN, 0), new ImmutableInstruction35c(Opcode.INVOKE_SUPER, 2,2, 3, 0, 0, 0, new ImmutableMethodReference("Landroid/app/Activity;", "dispatchKeyEvent", Arrays.asList("Landroid/view/KeyEvent;"), "Z")), new ImmutableInstruction11x(Opcode.MOVE_RESULT, 0), new ImmutableInstruction10t(Opcode.GOTO, -5));
        ImmutableMethodImplementation methodImplementation3 = new ImmutableMethodImplementation(4, instructions3, null, null);
        Method method3 = new ImmutableMethod("Lcom/unity3d/player/UnityPlayerActivity;", "dispatchKeyEvent", parameters3, "Z", 1, null, methodImplementation3);
/*************** END method definition for dispatchKeyEvent **********/

/*************** BEGIN method definition for onConfigurationChanged **********/
        List<ImmutableMethodParameter> parameters4 = Arrays.asList(new ImmutableMethodParameter("Landroid/content/res/Configuration;",null,null));
        List<ImmutableInstruction> instructions4 = Arrays.asList(new ImmutableInstruction35c(Opcode.INVOKE_SUPER, 2,1, 2, 0, 0, 0, new ImmutableMethodReference("Landroid/app/Activity;", "onConfigurationChanged", Arrays.asList("Landroid/content/res/Configuration;"), "V")), new ImmutableInstruction22c(Opcode.IGET_OBJECT, 0, 1, new ImmutableFieldReference("Lcom/unity3d/player/UnityPlayerActivity;", "mUnityPlayer", "Lcom/unity3d/player/UnityPlayer;")), new ImmutableInstruction35c(Opcode.INVOKE_VIRTUAL, 2,0, 2, 0, 0, 0, new ImmutableMethodReference("Lcom/unity3d/player/UnityPlayer;", "configurationChanged", Arrays.asList("Landroid/content/res/Configuration;"), "V")), new ImmutableInstruction10x(Opcode.RETURN_VOID));
        ImmutableMethodImplementation methodImplementation4 = new ImmutableMethodImplementation(3, instructions4, null, null);
        Method method4 = new ImmutableMethod("Lcom/unity3d/player/UnityPlayerActivity;", "onConfigurationChanged", parameters4, "V", 1, null, methodImplementation4);
/*************** END method definition for onConfigurationChanged **********/

/*************** BEGIN method definition for onCreate **********/
        List<ImmutableMethodParameter> parameters5 = Arrays.asList(new ImmutableMethodParameter("Landroid/os/Bundle;",null,null));
        List<ImmutableInstruction> instructions5 = Arrays.asList(new ImmutableInstruction11n(Opcode.CONST_4, 0, 1), new ImmutableInstruction35c(Opcode.INVOKE_VIRTUAL, 2,1, 0, 0, 0, 0, new ImmutableMethodReference("Lcom/unity3d/player/UnityPlayerActivity;", "requestWindowFeature", Arrays.asList("I"), "Z")), new ImmutableInstruction35c(Opcode.INVOKE_SUPER, 2,1, 2, 0, 0, 0, new ImmutableMethodReference("Landroid/app/Activity;", "onCreate", Arrays.asList("Landroid/os/Bundle;"), "V")), new ImmutableInstruction21c(Opcode.NEW_INSTANCE, 0, new ImmutableTypeReference("Lcom/unity3d/player/UnityPlayer;")), new ImmutableInstruction35c(Opcode.INVOKE_DIRECT, 2,0, 1, 0, 0, 0, new ImmutableMethodReference("Lcom/unity3d/player/UnityPlayer;", "<init>", Arrays.asList("Landroid/content/Context;"), "V")), new ImmutableInstruction22c(Opcode.IPUT_OBJECT, 0, 1, new ImmutableFieldReference("Lcom/unity3d/player/UnityPlayerActivity;", "mUnityPlayer", "Lcom/unity3d/player/UnityPlayer;")), new ImmutableInstruction22c(Opcode.IGET_OBJECT, 0, 1, new ImmutableFieldReference("Lcom/unity3d/player/UnityPlayerActivity;", "mUnityPlayer", "Lcom/unity3d/player/UnityPlayer;")), new ImmutableInstruction35c(Opcode.INVOKE_VIRTUAL, 2,1, 0, 0, 0, 0, new ImmutableMethodReference("Lcom/unity3d/player/UnityPlayerActivity;", "setContentView", Arrays.asList("Landroid/view/View;"), "V")), new ImmutableInstruction22c(Opcode.IGET_OBJECT, 0, 1, new ImmutableFieldReference("Lcom/unity3d/player/UnityPlayerActivity;", "mUnityPlayer", "Lcom/unity3d/player/UnityPlayer;")), new ImmutableInstruction35c(Opcode.INVOKE_VIRTUAL, 1,0, 0, 0, 0, 0, new ImmutableMethodReference("Lcom/unity3d/player/UnityPlayer;", "requestFocus", null, "Z")), new ImmutableInstruction10x(Opcode.RETURN_VOID));
        ImmutableMethodImplementation methodImplementation5 = new ImmutableMethodImplementation(3, instructions5, null, null);
        Method method5 = new ImmutableMethod("Lcom/unity3d/player/UnityPlayerActivity;", "onCreate", parameters5, "V", 4, null, methodImplementation5);
/*************** END method definition for onCreate **********/

/*************** BEGIN method definition for onDestroy **********/
        List<ImmutableMethodParameter> parameters6 = null;
        List<ImmutableInstruction> instructions6 = Arrays.asList(new ImmutableInstruction22c(Opcode.IGET_OBJECT, 0, 1, new ImmutableFieldReference("Lcom/unity3d/player/UnityPlayerActivity;", "mUnityPlayer", "Lcom/unity3d/player/UnityPlayer;")), new ImmutableInstruction35c(Opcode.INVOKE_VIRTUAL, 1,0, 0, 0, 0, 0, new ImmutableMethodReference("Lcom/unity3d/player/UnityPlayer;", "destroy", null, "V")), new ImmutableInstruction35c(Opcode.INVOKE_SUPER, 1,1, 0, 0, 0, 0, new ImmutableMethodReference("Landroid/app/Activity;", "onDestroy", null, "V")), new ImmutableInstruction10x(Opcode.RETURN_VOID));
        ImmutableMethodImplementation methodImplementation6 = new ImmutableMethodImplementation(2, instructions6, null, null);
        Method method6 = new ImmutableMethod("Lcom/unity3d/player/UnityPlayerActivity;", "onDestroy", parameters6, "V", 4, null, methodImplementation6);
/*************** END method definition for onDestroy **********/

/*************** BEGIN method definition for onGenericMotionEvent **********/
        List<ImmutableMethodParameter> parameters7 = Arrays.asList(new ImmutableMethodParameter("Landroid/view/MotionEvent;",null,null));
        List<ImmutableInstruction> instructions7 = Arrays.asList(new ImmutableInstruction22c(Opcode.IGET_OBJECT, 0, 1, new ImmutableFieldReference("Lcom/unity3d/player/UnityPlayerActivity;", "mUnityPlayer", "Lcom/unity3d/player/UnityPlayer;")), new ImmutableInstruction35c(Opcode.INVOKE_VIRTUAL, 2,0, 2, 0, 0, 0, new ImmutableMethodReference("Lcom/unity3d/player/UnityPlayer;", "injectEvent", Arrays.asList("Landroid/view/InputEvent;"), "Z")), new ImmutableInstruction11x(Opcode.MOVE_RESULT, 0), new ImmutableInstruction11x(Opcode.RETURN, 0));
        ImmutableMethodImplementation methodImplementation7 = new ImmutableMethodImplementation(3, instructions7, null, null);
        Method method7 = new ImmutableMethod("Lcom/unity3d/player/UnityPlayerActivity;", "onGenericMotionEvent", parameters7, "Z", 1, null, methodImplementation7);
/*************** END method definition for onGenericMotionEvent **********/

/*************** BEGIN method definition for onKeyDown **********/
        List<ImmutableMethodParameter> parameters8 = Arrays.asList(new ImmutableMethodParameter("I",null,null), new ImmutableMethodParameter("Landroid/view/KeyEvent;",null,null));
        List<ImmutableInstruction> instructions8 = Arrays.asList(new ImmutableInstruction22c(Opcode.IGET_OBJECT, 0, 1, new ImmutableFieldReference("Lcom/unity3d/player/UnityPlayerActivity;", "mUnityPlayer", "Lcom/unity3d/player/UnityPlayer;")), new ImmutableInstruction35c(Opcode.INVOKE_VIRTUAL, 2,0, 3, 0, 0, 0, new ImmutableMethodReference("Lcom/unity3d/player/UnityPlayer;", "injectEvent", Arrays.asList("Landroid/view/InputEvent;"), "Z")), new ImmutableInstruction11x(Opcode.MOVE_RESULT, 0), new ImmutableInstruction11x(Opcode.RETURN, 0));
        ImmutableMethodImplementation methodImplementation8 = new ImmutableMethodImplementation(4, instructions8, null, null);
        Method method8 = new ImmutableMethod("Lcom/unity3d/player/UnityPlayerActivity;", "onKeyDown", parameters8, "Z", 1, null, methodImplementation8);
/*************** END method definition for onKeyDown **********/

/*************** BEGIN method definition for onKeyUp **********/
        List<ImmutableMethodParameter> parameters9 = Arrays.asList(new ImmutableMethodParameter("I",null,null), new ImmutableMethodParameter("Landroid/view/KeyEvent;",null,null));
        List<ImmutableInstruction> instructions9 = Arrays.asList(new ImmutableInstruction22c(Opcode.IGET_OBJECT, 0, 1, new ImmutableFieldReference("Lcom/unity3d/player/UnityPlayerActivity;", "mUnityPlayer", "Lcom/unity3d/player/UnityPlayer;")), new ImmutableInstruction35c(Opcode.INVOKE_VIRTUAL, 2,0, 3, 0, 0, 0, new ImmutableMethodReference("Lcom/unity3d/player/UnityPlayer;", "injectEvent", Arrays.asList("Landroid/view/InputEvent;"), "Z")), new ImmutableInstruction11x(Opcode.MOVE_RESULT, 0), new ImmutableInstruction11x(Opcode.RETURN, 0));
        ImmutableMethodImplementation methodImplementation9 = new ImmutableMethodImplementation(4, instructions9, null, null);
        Method method9 = new ImmutableMethod("Lcom/unity3d/player/UnityPlayerActivity;", "onKeyUp", parameters9, "Z", 1, null, methodImplementation9);
/*************** END method definition for onKeyUp **********/

/*************** BEGIN method definition for onLowMemory **********/
        List<ImmutableMethodParameter> parameters10 = null;
        List<ImmutableInstruction> instructions10 = Arrays.asList(new ImmutableInstruction35c(Opcode.INVOKE_SUPER, 1,1, 0, 0, 0, 0, new ImmutableMethodReference("Landroid/app/Activity;", "onLowMemory", null, "V")), new ImmutableInstruction22c(Opcode.IGET_OBJECT, 0, 1, new ImmutableFieldReference("Lcom/unity3d/player/UnityPlayerActivity;", "mUnityPlayer", "Lcom/unity3d/player/UnityPlayer;")), new ImmutableInstruction35c(Opcode.INVOKE_VIRTUAL, 1,0, 0, 0, 0, 0, new ImmutableMethodReference("Lcom/unity3d/player/UnityPlayer;", "lowMemory", null, "V")), new ImmutableInstruction10x(Opcode.RETURN_VOID));
        ImmutableMethodImplementation methodImplementation10 = new ImmutableMethodImplementation(2, instructions10, null, null);
        Method method10 = new ImmutableMethod("Lcom/unity3d/player/UnityPlayerActivity;", "onLowMemory", parameters10, "V", 1, null, methodImplementation10);
/*************** END method definition for onLowMemory **********/

/*************** BEGIN method definition for onNewIntent **********/
        List<ImmutableMethodParameter> parameters11 = Arrays.asList(new ImmutableMethodParameter("Landroid/content/Intent;",null,null));
        List<ImmutableInstruction> instructions11 = Arrays.asList(new ImmutableInstruction35c(Opcode.INVOKE_VIRTUAL, 2,0, 1, 0, 0, 0, new ImmutableMethodReference("Lcom/unity3d/player/UnityPlayerActivity;", "setIntent", Arrays.asList("Landroid/content/Intent;"), "V")), new ImmutableInstruction10x(Opcode.RETURN_VOID));
        ImmutableMethodImplementation methodImplementation11 = new ImmutableMethodImplementation(2, instructions11, null, null);
        Method method11 = new ImmutableMethod("Lcom/unity3d/player/UnityPlayerActivity;", "onNewIntent", parameters11, "V", 4, null, methodImplementation11);
/*************** END method definition for onNewIntent **********/

/*************** BEGIN method definition for onPause **********/
        List<ImmutableMethodParameter> parameters12 = null;
        List<ImmutableInstruction> instructions12 = Arrays.asList(new ImmutableInstruction35c(Opcode.INVOKE_SUPER, 1,1, 0, 0, 0, 0, new ImmutableMethodReference("Landroid/app/Activity;", "onPause", null, "V")), new ImmutableInstruction22c(Opcode.IGET_OBJECT, 0, 1, new ImmutableFieldReference("Lcom/unity3d/player/UnityPlayerActivity;", "mUnityPlayer", "Lcom/unity3d/player/UnityPlayer;")), new ImmutableInstruction35c(Opcode.INVOKE_VIRTUAL, 1,0, 0, 0, 0, 0, new ImmutableMethodReference("Lcom/unity3d/player/UnityPlayer;", "pause", null, "V")), new ImmutableInstruction10x(Opcode.RETURN_VOID));
        ImmutableMethodImplementation methodImplementation12 = new ImmutableMethodImplementation(2, instructions12, null, null);
        Method method12 = new ImmutableMethod("Lcom/unity3d/player/UnityPlayerActivity;", "onPause", parameters12, "V", 4, null, methodImplementation12);
/*************** END method definition for onPause **********/

/*************** BEGIN method definition for onRequestPermissionsResult **********/
        List<ImmutableMethodParameter> parameters13 = Arrays.asList(new ImmutableMethodParameter("I",null,"requestCode"), new ImmutableMethodParameter("[Ljava/lang/String;",null,"permissions"), new ImmutableMethodParameter("[I",null,"grantResults"));
        List<ImmutableInstruction> instructions13 = Arrays.asList(new ImmutableInstruction35c(Opcode.INVOKE_SUPER, 4,2, 3, 4, 5, 0, new ImmutableMethodReference("Landroid/app/Activity;", "onRequestPermissionsResult", Arrays.asList("I", "[Ljava/lang/String;", "[I"), "V")), new ImmutableInstruction12x(Opcode.ARRAY_LENGTH, 0, 5), new ImmutableInstruction11n(Opcode.CONST_4, 1, 0), new ImmutableInstruction21t(Opcode.IF_LEZ, 0, 10), new ImmutableInstruction23x(Opcode.AGET, 0, 5, 1), new ImmutableInstruction21t(Opcode.IF_NEZ, 0, 6), new ImmutableInstruction35c(Opcode.INVOKE_DIRECT, 1,2, 0, 0, 0, 0, new ImmutableMethodReference("Lcom/unity3d/player/UnityPlayerActivity;", "continueLoad", null, "V")), new ImmutableInstruction10t(Opcode.GOTO, 4), new ImmutableInstruction35c(Opcode.INVOKE_STATIC, 1,1, 0, 0, 0, 0, new ImmutableMethodReference("Ljava/lang/System;", "exit", Arrays.asList("I"), "V")), new ImmutableInstruction10x(Opcode.RETURN_VOID));
        ImmutableMethodImplementation methodImplementation13 = new ImmutableMethodImplementation(6, instructions13, null, null);
        Method method13 = new ImmutableMethod("Lcom/unity3d/player/UnityPlayerActivity;", "onRequestPermissionsResult", parameters13, "V", 1, null, methodImplementation13);
/*************** END method definition for onRequestPermissionsResult **********/

/*************** BEGIN method definition for onResume **********/
        List<ImmutableMethodParameter> parameters14 = null;
        List<ImmutableInstruction> instructions14 = Arrays.asList(new ImmutableInstruction35c(Opcode.INVOKE_SUPER, 1,1, 0, 0, 0, 0, new ImmutableMethodReference("Landroid/app/Activity;", "onResume", null, "V")), new ImmutableInstruction22c(Opcode.IGET_OBJECT, 0, 1, new ImmutableFieldReference("Lcom/unity3d/player/UnityPlayerActivity;", "mUnityPlayer", "Lcom/unity3d/player/UnityPlayer;")), new ImmutableInstruction35c(Opcode.INVOKE_VIRTUAL, 1,0, 0, 0, 0, 0, new ImmutableMethodReference("Lcom/unity3d/player/UnityPlayer;", "resume", null, "V")), new ImmutableInstruction10x(Opcode.RETURN_VOID));
        ImmutableMethodImplementation methodImplementation14 = new ImmutableMethodImplementation(2, instructions14, null, null);
        Method method14 = new ImmutableMethod("Lcom/unity3d/player/UnityPlayerActivity;", "onResume", parameters14, "V", 4, null, methodImplementation14);
/*************** END method definition for onResume **********/

/*************** BEGIN method definition for onStart **********/
        List<ImmutableMethodParameter> parameters15 = null;
        List<ImmutableInstruction> instructions15 = Arrays.asList(new ImmutableInstruction35c(Opcode.INVOKE_SUPER, 1,2, 0, 0, 0, 0, new ImmutableMethodReference("Landroid/app/Activity;", "onStart", null, "V")), new ImmutableInstruction21c(Opcode.CONST_STRING, 0, new ImmutableStringReference("android.permission.WRITE_EXTERNAL_STORAGE")), new ImmutableInstruction35c(Opcode.INVOKE_VIRTUAL, 2,2, 0, 0, 0, 0, new ImmutableMethodReference("Lcom/unity3d/player/UnityPlayerActivity;", "checkSelfPermission", Arrays.asList("Ljava/lang/String;"), "I")), new ImmutableInstruction11x(Opcode.MOVE_RESULT, 1), new ImmutableInstruction21t(Opcode.IF_NEZ, 1, 6), new ImmutableInstruction35c(Opcode.INVOKE_DIRECT, 1,2, 0, 0, 0, 0, new ImmutableMethodReference("Lcom/unity3d/player/UnityPlayerActivity;", "continueLoad", null, "V")), new ImmutableInstruction10t(Opcode.GOTO, 9), new ImmutableInstruction35c(Opcode.FILLED_NEW_ARRAY, 1,0, 0, 0, 0, 0, new ImmutableTypeReference("[Ljava/lang/String;")), new ImmutableInstruction11x(Opcode.MOVE_RESULT_OBJECT, 0), new ImmutableInstruction11n(Opcode.CONST_4, 1, 1), new ImmutableInstruction35c(Opcode.INVOKE_VIRTUAL, 3,2, 0, 1, 0, 0, new ImmutableMethodReference("Lcom/unity3d/player/UnityPlayerActivity;", "requestPermissions", Arrays.asList("[Ljava/lang/String;", "I"), "V")), new ImmutableInstruction10x(Opcode.RETURN_VOID));
        ImmutableMethodImplementation methodImplementation15 = new ImmutableMethodImplementation(3, instructions15, null, null);
        Method method15 = new ImmutableMethod("Lcom/unity3d/player/UnityPlayerActivity;", "onStart", parameters15, "V", 4, null, methodImplementation15);
/*************** END method definition for onStart **********/

/*************** BEGIN method definition for onStop **********/
        List<ImmutableMethodParameter> parameters16 = null;
        List<ImmutableInstruction> instructions16 = Arrays.asList(new ImmutableInstruction35c(Opcode.INVOKE_SUPER, 1,1, 0, 0, 0, 0, new ImmutableMethodReference("Landroid/app/Activity;", "onStop", null, "V")), new ImmutableInstruction22c(Opcode.IGET_OBJECT, 0, 1, new ImmutableFieldReference("Lcom/unity3d/player/UnityPlayerActivity;", "mUnityPlayer", "Lcom/unity3d/player/UnityPlayer;")), new ImmutableInstruction35c(Opcode.INVOKE_VIRTUAL, 1,0, 0, 0, 0, 0, new ImmutableMethodReference("Lcom/unity3d/player/UnityPlayer;", "stop", null, "V")), new ImmutableInstruction10x(Opcode.RETURN_VOID));
        ImmutableMethodImplementation methodImplementation16 = new ImmutableMethodImplementation(2, instructions16, null, null);
        Method method16 = new ImmutableMethod("Lcom/unity3d/player/UnityPlayerActivity;", "onStop", parameters16, "V", 4, null, methodImplementation16);
/*************** END method definition for onStop **********/

/*************** BEGIN method definition for onTouchEvent **********/
        List<ImmutableMethodParameter> parameters17 = Arrays.asList(new ImmutableMethodParameter("Landroid/view/MotionEvent;",null,null));
        List<ImmutableInstruction> instructions17 = Arrays.asList(new ImmutableInstruction22c(Opcode.IGET_OBJECT, 0, 1, new ImmutableFieldReference("Lcom/unity3d/player/UnityPlayerActivity;", "mUnityPlayer", "Lcom/unity3d/player/UnityPlayer;")), new ImmutableInstruction35c(Opcode.INVOKE_VIRTUAL, 2,0, 2, 0, 0, 0, new ImmutableMethodReference("Lcom/unity3d/player/UnityPlayer;", "injectEvent", Arrays.asList("Landroid/view/InputEvent;"), "Z")), new ImmutableInstruction11x(Opcode.MOVE_RESULT, 0), new ImmutableInstruction11x(Opcode.RETURN, 0));
        ImmutableMethodImplementation methodImplementation17 = new ImmutableMethodImplementation(3, instructions17, null, null);
        Method method17 = new ImmutableMethod("Lcom/unity3d/player/UnityPlayerActivity;", "onTouchEvent", parameters17, "Z", 1, null, methodImplementation17);
/*************** END method definition for onTouchEvent **********/

/*************** BEGIN method definition for onTrimMemory **********/
        List<ImmutableMethodParameter> parameters18 = Arrays.asList(new ImmutableMethodParameter("I",null,null));
        List<ImmutableInstruction> instructions18 = Arrays.asList(new ImmutableInstruction35c(Opcode.INVOKE_SUPER, 2,1, 2, 0, 0, 0, new ImmutableMethodReference("Landroid/app/Activity;", "onTrimMemory", Arrays.asList("I"), "V")), new ImmutableInstruction21s(Opcode.CONST_16, 0, 15), new ImmutableInstruction22t(Opcode.IF_NE, 2, 0, 7), new ImmutableInstruction22c(Opcode.IGET_OBJECT, 0, 1, new ImmutableFieldReference("Lcom/unity3d/player/UnityPlayerActivity;", "mUnityPlayer", "Lcom/unity3d/player/UnityPlayer;")), new ImmutableInstruction35c(Opcode.INVOKE_VIRTUAL, 1,0, 0, 0, 0, 0, new ImmutableMethodReference("Lcom/unity3d/player/UnityPlayer;", "lowMemory", null, "V")), new ImmutableInstruction10x(Opcode.RETURN_VOID));
        ImmutableMethodImplementation methodImplementation18 = new ImmutableMethodImplementation(3, instructions18, null, null);
        Method method18 = new ImmutableMethod("Lcom/unity3d/player/UnityPlayerActivity;", "onTrimMemory", parameters18, "V", 1, null, methodImplementation18);
/*************** END method definition for onTrimMemory **********/

/*************** BEGIN method definition for onWindowFocusChanged **********/
        List<ImmutableMethodParameter> parameters19 = Arrays.asList(new ImmutableMethodParameter("Z",null,null));
        List<ImmutableInstruction> instructions19 = Arrays.asList(new ImmutableInstruction35c(Opcode.INVOKE_SUPER, 2,1, 2, 0, 0, 0, new ImmutableMethodReference("Landroid/app/Activity;", "onWindowFocusChanged", Arrays.asList("Z"), "V")), new ImmutableInstruction22c(Opcode.IGET_OBJECT, 0, 1, new ImmutableFieldReference("Lcom/unity3d/player/UnityPlayerActivity;", "mUnityPlayer", "Lcom/unity3d/player/UnityPlayer;")), new ImmutableInstruction35c(Opcode.INVOKE_VIRTUAL, 2,0, 2, 0, 0, 0, new ImmutableMethodReference("Lcom/unity3d/player/UnityPlayer;", "windowFocusChanged", Arrays.asList("Z"), "V")), new ImmutableInstruction10x(Opcode.RETURN_VOID));
        ImmutableMethodImplementation methodImplementation19 = new ImmutableMethodImplementation(3, instructions19, null, null);
        Method method19 = new ImmutableMethod("Lcom/unity3d/player/UnityPlayerActivity;", "onWindowFocusChanged", parameters19, "V", 1, null, methodImplementation19);
/*************** END method definition for onWindowFocusChanged **********/

    }
    protected class InjectorRewrittenClassDef extends RewrittenClassDef
    {
        public InjectorRewrittenClassDef(ClassDef classdef) {
            super(classdef);
        }

        @Override
        public Iterable<? extends Method> getDirectMethods()
        {
            if (!classDef.getType().equals("Lcom/unity3d/player/UnityPlayerActivity;"))
                return super.getDirectMethods();

            List<Method> methods = new ArrayList<Method>();
            int accessFlags = 65544;
            BuilderAnnotationSet annotations = null;
            String definingClass = "Lcom/unity3d/player/UnityPlayerActivity;";
            String name = "<clinit>";
            Iterable<MethodParameter> parameters = new ArrayList<MethodParameter>();
            String returnType = "V";
            ImmutableMethodParameter parameter = new ImmutableMethodParameter("Ljava/lang/String;",null,null);
            List<ImmutableInstruction> instructions = Arrays.asList(
                    new ImmutableInstruction21c(Opcode.CONST_STRING, 0, new ImmutableStringReference("modloader")),
                    new ImmutableInstruction35c(Opcode.INVOKE_STATIC, 1, 0, 0, 0, 0, 0, new ImmutableMethodReference("Ljava/lang/System;", "loadLibrary", Arrays.asList(parameter), "V")),
                    new ImmutableInstruction10x(Opcode.RETURN_VOID)
            );


            ImmutableMethodImplementation methodImplementation = new ImmutableMethodImplementation(
                    1, instructions, null, null);

            Method staticInit = new ImmutableMethod(definingClass, name, parameters, returnType, accessFlags, annotations,
                    methodImplementation);
            methods.add(staticInit);
            for (Method m : this.classDef.getDirectMethods())
            {
                methods.add(m);
            }
            return methods;
        }
    }
}