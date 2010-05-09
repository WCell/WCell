/*************************************************************************
 *
 *   file		: TokenResolver.cs
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
using System.Reflection;

namespace DynamicMethods
{
    public interface ITokenResolver
    {
        MethodBase AsMethod(int token);
        FieldInfo AsField(int token);
        Type AsType(int token);
        String AsString(int token);
        MemberInfo AsMember(int token);
        byte[] AsSignature(int token);
    }

    public class ModuleScopeTokenResolver : ITokenResolver
    {
        private MethodBase m_enclosingMethod;
        private Type[] m_methodContext;
        private Module m_module;
        private Type[] m_typeContext;

        public ModuleScopeTokenResolver(MethodBase method)
        {
            m_enclosingMethod = method;
            m_module = method.Module;
            m_methodContext = (method is ConstructorInfo) ? null : method.GetGenericArguments();
            m_typeContext = (method.DeclaringType == null) ? null : method.DeclaringType.GetGenericArguments();
        }

        #region ITokenResolver Members

        public MethodBase AsMethod(int token)
        {
            return m_module.ResolveMethod(token, m_typeContext, m_methodContext);
        }

        public FieldInfo AsField(int token)
        {
            return m_module.ResolveField(token, m_typeContext, m_methodContext);
        }

        public Type AsType(int token)
        {
            return m_module.ResolveType(token, m_typeContext, m_methodContext);
        }

        public MemberInfo AsMember(int token)
        {
            return m_module.ResolveMember(token, m_typeContext, m_methodContext);
        }

        public string AsString(int token)
        {
            return m_module.ResolveString(token);
        }

        public byte[] AsSignature(int token)
        {
            return m_module.ResolveSignature(token);
        }

        #endregion
    }
}