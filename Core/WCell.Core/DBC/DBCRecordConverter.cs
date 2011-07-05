/*************************************************************************
 *
 *   file		: DBCRecordConverter.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-12-23 20:07:17 +0100 (on, 23 dec 2009) $
 
 *   revision		: $Rev: 1151 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using WCell.Constants;
using WCell.Util;
using WCell.Util.Data;

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

    public abstract class DBCRecordConverter : IDisposable
    {
        private byte[] m_stringTable;

        public void Init(byte[] stringTable)
        {
            m_stringTable = stringTable;
		}

		public virtual void Convert(byte[] rawData)
		{
		}

		protected static int CopyTo(byte[] bytes, object obj, int index)
		{
// ReSharper disable RedundantEnumerableCastCall
			var fieldsAndProps = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Cast<MemberInfo>();
			fieldsAndProps = fieldsAndProps.Concat(obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public));
// ReSharper restore RedundantEnumerableCastCall
			foreach (var prop in fieldsAndProps)
			{
				if (prop.IsReadonly() ||
					prop.GetCustomAttributes(typeof(NotPersistentAttribute), true).Length > 0)
					continue; // Skip this property

				var val = GetInt32(bytes, index++);
				var objVal = Utility.ChangeType(val, prop.GetVariableType());
				prop.SetUnindexedValue(obj, objVal);
			}
			return index;
		}

    	/// <summary>
		/// Copies the next count fields into obj, starting from offset.
		/// Keep in mind, that one field has a length of 4 bytes.
		/// </summary>
		protected static void CopyTo(byte[] bytes, int fromOffset, int length, object target, int toOffset)
		{
			if (length % 4 != 0)
			{
				throw new Exception("Cannot copy to object " + target + " because it's size is not a multiple of 4.");
			}

			try
			{
				var handle = GCHandle.Alloc(target, GCHandleType.Pinned);
				try
				{
					Marshal.Copy(bytes, fromOffset * 4, new IntPtr(handle.AddrOfPinnedObject().ToInt64() + toOffset), length);
				}
				finally
				{
					handle.Free();
				}
			}
			catch (Exception e)
			{
				throw new Exception(string.Format("Unable to copy bytes to object {0} of type {1}", target, target.GetType()), e);
			}
		}

        protected static uint GetUInt32(byte[] data, int field)
        {
            int startIndex = field * 4;
            if (startIndex + 4 > data.Length)
                throw new IndexOutOfRangeException();

            return BitConverter.ToUInt32(data, startIndex);
        }

        protected static int GetInt32(byte[] data, int field)
        {
            int startIndex = field * 4;
            if (startIndex + 4 > data.Length)
                throw new IndexOutOfRangeException();

            return BitConverter.ToInt32(data, startIndex);
        }

        protected static float GetFloat(byte[] data, int field)
        {
            int startIndex = field * 4;
            if (startIndex + 4 > data.Length)
                throw new IndexOutOfRangeException();

            return BitConverter.ToSingle(data, startIndex);
        }

        protected static ulong GetUInt64(byte[] data, int startingField)
        {
            int startIndex = startingField * 4;
            if (startIndex + 8 > data.Length)
                throw new IndexOutOfRangeException();

            return BitConverter.ToUInt64(data, startIndex);
        }

        public string GetString(byte[] data, int stringOffset)
        {
        	return GetString(data, WCellConstants.DefaultLocale, stringOffset);
		}

		public string[] GetStrings(byte[] data, int stringOffset)
		{
			var strings = new string[(int) ClientLocale.End];
			for (var l = 0; l < (int)ClientLocale.End; l++)
			{
				strings[l] = GetString(data, (ClientLocale) l, stringOffset);
			}
			return strings;
		}

		public string GetString(byte[] data, ref int offset)
		{
			var ret = GetString(data, offset);
			offset += 17;
			return ret;
		}

		public string GetString(byte[] data, ClientLocale locale, int stringOffset)
		{
			var startOffset = GetInt32(data, stringOffset + (int)locale);
			var len = 0;
			
			while (m_stringTable[(startOffset + len++)] != 0)
			{
			}

			return WCellConstants.DefaultEncoding.GetString(m_stringTable, startOffset, len-1) ?? "";
		}

        public void Dispose()
        {
            m_stringTable = null;
        }
    }
}