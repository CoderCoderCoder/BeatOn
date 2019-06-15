package com.emulamer.beaton;

import org.jf.dexlib2.Opcode;
import org.jf.dexlib2.iface.ClassDef;
import org.jf.dexlib2.iface.Method;
import org.jf.dexlib2.iface.MethodParameter;
import org.jf.dexlib2.immutable.ImmutableMethod;
import org.jf.dexlib2.immutable.ImmutableMethodImplementation;
import org.jf.dexlib2.immutable.ImmutableMethodParameter;
import org.jf.dexlib2.immutable.instruction.ImmutableInstruction;
import org.jf.dexlib2.immutable.instruction.ImmutableInstruction21c;
import org.jf.dexlib2.immutable.instruction.ImmutableInstruction35c;
import org.jf.dexlib2.immutable.reference.ImmutableMethodReference;
import org.jf.dexlib2.immutable.reference.ImmutableStringReference;
import org.jf.dexlib2.rewriter.ClassDefRewriter;
import org.jf.dexlib2.rewriter.Rewriters;
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
                    new ImmutableInstruction35c(Opcode.INVOKE_STATIC, 1, 0, 0, 0, 0, 0, new ImmutableMethodReference("Ljava/lang/System", "loadLibrary()", Arrays.asList(parameter), "V"))
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