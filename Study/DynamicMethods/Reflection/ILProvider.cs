/*************************************************************************
 *
 *   file		: ILProvider.cs
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
    public interface IILProvider
    {
        Byte[] GetByteArray();
    }

    public class MethodBaseILProvider : IILProvider
    {
        private byte[] m_byteArray;
        private MethodBase m_method;

        public MethodBaseILProvider(MethodBase method)
        {
            m_method = method;
        }

        #region IILProvider Members

        public byte[] GetByteArray()
        {
            if (m_byteArray == null)
            {
                MethodBody methodBody = m_method.GetMethodBody();
                m_byteArray = (methodBody == null) ? new Byte[0] : methodBody.GetILAsByteArray();
            }
            return m_byteArray;
        }

        #endregion
    }
}