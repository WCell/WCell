/*************************************************************************
 *
 *   file		: DBCRecordConverter.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-12-23 20:07:17 +0100 (on, 23 dec 2009) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1151 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Text;
using WCell.Constants;

namespace WCell.Core.DBC
{
	public class AdvancedDBCRecordConverter<T> : DBCRecordConverter
	{
		public virtual T ConvertTo(byte[] rawData, ref int id)
		{
			id = int.MinValue;
			return default(T);
		}
	}

    public class DBCRecordConverter : IDisposable
    {
        private byte[] m_stringTable;

        public void Init(byte[] stringTable)
        {
            m_stringTable = stringTable;
		}

		public virtual void Convert(byte[] rawData)
		{
		}

        protected static unsafe uint GetUInt32(byte[] data, uint field)
        {
            uint startIndex = field * 4;
            if (startIndex + 4 > data.Length)
                return uint.MaxValue;

            fixed (byte* pData = &data[startIndex])
            {
                return *(uint*)pData;
            }
        }

        protected static unsafe int GetInt32(byte[] data, uint field)
        {
            uint startIndex = field * 4;
            if (startIndex + 4 > data.Length)
                return int.MaxValue;

            fixed (byte* pData = &data[startIndex])
            {
                return *(int*)pData;
            }
        }

        protected static unsafe float GetFloat(byte[] data, uint field)
        {
            uint startIndex = field * 4;
            if (startIndex + 4 > data.Length)
                return float.NaN;

            fixed (byte* pData = &data[startIndex])
            {
                return *(float*)pData;
            }
        }

        protected static unsafe ulong GetUInt64(byte[] data, uint startingField)
        {
            uint startIndex = startingField * 4;
            if (startIndex + 8 > data.Length)
                return ulong.MaxValue;

            fixed (byte* pData = &data[startIndex])
            {
                return *(ulong*)pData;
            }
        }

        public string GetString(byte[] data, uint stringOffset)
        {
        	return GetString(data, WCellDef.DefaultLocale, stringOffset);
		}

		public string[] GetStrings(byte[] data, uint stringOffset)
		{
			var strings = new string[(int) ClientLocale.End];
			for (var l = 0; l < (int) ClientLocale.End; l++)
			{
				strings[l] = GetString(data, (ClientLocale) l, stringOffset);
			}
			return strings;
		}

		public string GetString(byte[] data, ref uint offset)
		{
			var ret = GetString(data, offset);
			offset += 17;
			return ret;
		}

		public string GetString(byte[] data, ClientLocale locale, uint stringOffset)
		{
			var startOffset = GetInt32(data, stringOffset + (uint)locale);
			var len = 0;
			
			while (m_stringTable[(startOffset + len++)] != 0)
			{
			}

			return WCellDef.DefaultEncoding.GetString(m_stringTable, startOffset, len-1);
		}

        public void Dispose()
        {
            m_stringTable = null;
        }
    }
}
