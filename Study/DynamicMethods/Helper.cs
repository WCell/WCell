/*************************************************************************
 *
 *   file		: Helper.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-01-31 12:35:36 +0100 (to, 31 jan 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 87 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace DynamicMethods
{
    public class DynamicMethodHelper
    {
        public static DynamicMethod ConvertFrom(MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            if (!method.IsStatic)
            {
                throw new InvalidOperationException("the method is expected to be static");
            }

            if (method.IsGenericMethod)
            {
                throw new InvalidOperationException("currently dynamic method cannot be generic");
            }

            MethodBody body = method.GetMethodBody();

            if (body == null)
            {
                throw new InvalidOperationException("the method does not have method body");
            }

            DynamicMethod dm = new DynamicMethod(
                method.Name,
                method.ReturnType,
                GetParameterTypes(method),
                typeof (DynamicMethodHelper));

            DynamicILInfo ilInfo = dm.GetDynamicILInfo();

            SetLocalSignature(body, ilInfo);
            SetCode(method, body, ilInfo);
            SetExceptions(body, ilInfo);

            return dm;
        }

        private static void SetCode(MethodInfo method, MethodBody body, DynamicILInfo ilInfo)
        {
            byte[] code = body.GetILAsByteArray();
            ILReader reader = new ILReader(method);
            ILInfoGetTokenVisitor visitor = new ILInfoGetTokenVisitor(ilInfo, code);
            reader.Accept(visitor);

            ilInfo.SetCode(code, body.MaxStackSize);
        }

        private static void SetLocalSignature(MethodBody body, DynamicILInfo ilInfo)
        {
            SignatureHelper sig = SignatureHelper.GetLocalVarSigHelper();
            foreach (LocalVariableInfo lvi in body.LocalVariables)
            {
                sig.AddArgument(lvi.LocalType, lvi.IsPinned);
            }
            ilInfo.SetLocalSignature(sig.GetSignature());
        }

        private static void SetExceptions(MethodBody body, DynamicILInfo ilInfo)
        {
            IList<ExceptionHandlingClause> ehcs = body.ExceptionHandlingClauses;
            int ehCount = ehcs.Count;
            if (ehCount == 0) return;

            // Let us do FAT exception header
            int size = 4 + 24*ehCount;
            byte[] exceptions = new byte[size];

            exceptions[0] = 0x01 | 0x40; //Offset: 0, Kind: CorILMethod_Sect_EHTable | CorILMethod_Sect_FatFormat
            OverwriteInt32(size, 1, exceptions); // Offset: 1, DataSize: n * 24 + 4

            int pos = 4;
            foreach (ExceptionHandlingClause ehc in ehcs)
            {
                // 
                // Flags, TryOffset, TryLength, HandlerOffset, HandlerLength, 
                //
                OverwriteInt32((int) ehc.Flags, pos, exceptions);
                pos += 4;
                OverwriteInt32(ehc.TryOffset, pos, exceptions);
                pos += 4;
                OverwriteInt32(ehc.TryLength, pos, exceptions);
                pos += 4;
                OverwriteInt32(ehc.HandlerOffset, pos, exceptions);
                pos += 4;
                OverwriteInt32(ehc.HandlerLength, pos, exceptions);
                pos += 4;

                //
                // ClassToken or FilterOffset
                //
                switch (ehc.Flags)
                {
                    case ExceptionHandlingClauseOptions.Clause:
                        int token = ilInfo.GetTokenFor(ehc.CatchType.TypeHandle);
                        OverwriteInt32(token, pos, exceptions);
                        break;
                    case ExceptionHandlingClauseOptions.Filter:
                        OverwriteInt32(ehc.FilterOffset, pos, exceptions);
                        break;
                    case ExceptionHandlingClauseOptions.Fault:
                        throw new NotSupportedException("dynamic method does not support fault clause");
                    case ExceptionHandlingClauseOptions.Finally:
                        break;
                }
                pos += 4;
            }

            ilInfo.SetExceptions(exceptions);
        }

        public static void OverwriteInt32(int value, int pos, byte[] array)
        {
            array[pos++] = (byte) value;
            array[pos++] = (byte) (value >> 8);
            array[pos++] = (byte) (value >> 16);
            array[pos++] = (byte) (value >> 24);
        }

        private static Type[] GetParameterTypes(MethodInfo method)
        {
            ParameterInfo[] pia = method.GetParameters();
            Type[] types = new Type[pia.Length];

            for (int i = 0; i < pia.Length; i++)
            {
                types[i] = pia[i].ParameterType;
            }
            return types;
        }

        #region Nested type: ILInfoGetTokenVisitor

        private class ILInfoGetTokenVisitor : ILInstructionVisitor
        {
            private byte[] code;
            private DynamicILInfo ilInfo;

            public ILInfoGetTokenVisitor(DynamicILInfo ilinfo, byte[] code)
            {
                ilInfo = ilinfo;
                this.code = code;
            }

            public override void VisitInlineMethodInstruction(InlineMethodInstruction inlineMethodInstruction)
            {
                OverwriteInt32(ilInfo.GetTokenFor(
                                   inlineMethodInstruction.Method.MethodHandle,
                                   inlineMethodInstruction.Method.DeclaringType.TypeHandle),
                               inlineMethodInstruction.Offset + inlineMethodInstruction.OpCode.Size);
            }

            public override void VisitInlineSigInstruction(InlineSigInstruction inlineSigInstruction)
            {
                OverwriteInt32(ilInfo.GetTokenFor(inlineSigInstruction.Signature),
                               inlineSigInstruction.Offset + inlineSigInstruction.OpCode.Size);
            }

            public override void VisitInlineFieldInstruction(InlineFieldInstruction inlineFieldInstruction)
            {
                //CLR BUG: 
                //OverwriteInt32(ilInfo.GetTokenFor(inlineFieldInstruction.Field.FieldHandle, inlineFieldInstruction.Field.DeclaringType.TypeHandle),
                //    inlineFieldInstruction.Offset + inlineFieldInstruction.OpCode.Size);

                OverwriteInt32(ilInfo.GetTokenFor(inlineFieldInstruction.Field.FieldHandle),
                               inlineFieldInstruction.Offset + inlineFieldInstruction.OpCode.Size);
            }

            public override void VisitInlineStringInstruction(InlineStringInstruction inlineStringInstruction)
            {
                OverwriteInt32(ilInfo.GetTokenFor(inlineStringInstruction.String),
                               inlineStringInstruction.Offset + inlineStringInstruction.OpCode.Size);
            }

            public override void VisitInlineTypeInstruction(InlineTypeInstruction inlineTypeInstruction)
            {
                OverwriteInt32(ilInfo.GetTokenFor(inlineTypeInstruction.Type.TypeHandle),
                               inlineTypeInstruction.Offset + inlineTypeInstruction.OpCode.Size);
            }

            public override void VisitInlineTokInstruction(InlineTokInstruction inlineTokInstruction)
            {
                MemberInfo mi = inlineTokInstruction.Member;
                int token = 0;
                if (mi.MemberType == MemberTypes.TypeInfo || mi.MemberType == MemberTypes.NestedType)
                {
                    Type type = mi as Type;
                    token = ilInfo.GetTokenFor(type.TypeHandle);
                }
                else if (mi.MemberType == MemberTypes.Method || mi.MemberType == MemberTypes.Constructor)
                {
                    MethodBase m = mi as MethodBase;
                    token = ilInfo.GetTokenFor(m.MethodHandle, m.DeclaringType.TypeHandle);
                }
                else if (mi.MemberType == MemberTypes.Field)
                {
                    FieldInfo f = mi as FieldInfo;
                    //CLR BUG: token = ilInfo.GetTokenFor(f.FieldHandle, f.DeclaringType.TypeHandle);
                    token = ilInfo.GetTokenFor(f.FieldHandle);
                }

                OverwriteInt32(token,
                               inlineTokInstruction.Offset + inlineTokInstruction.OpCode.Size);
            }

            private void OverwriteInt32(int value, int pos)
            {
                code[pos++] = (byte) value;
                code[pos++] = (byte) (value >> 8);
                code[pos++] = (byte) (value >> 16);
                code[pos++] = (byte) (value >> 24);
            }
        }

        #endregion
    }
}