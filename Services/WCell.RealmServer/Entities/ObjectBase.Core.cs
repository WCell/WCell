/*************************************************************************
 *
 *   file		: ObjectBase.Core.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-12-29 05:43:08 +0100 (ti, 29 dec 2009) $

 *   revision		: $Rev: 1160 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using WCell.Util.Logging;
using WCell.Constants.Updates;
using WCell.Core;

namespace WCell.RealmServer.Entities
{
    public partial class ObjectBase
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        #region Get[...]

        public float GetFloat(UpdateFieldId field)
        {
            return m_updateValues[field.RawId].Float;
        }

        public short GetInt16Low(UpdateFieldId field)
        {
            return m_updateValues[field.RawId].Int16Low;
        }

        public short GetInt16Low(int field)
        {
            return m_updateValues[field].Int16Low;
        }

        public short GetInt16High(UpdateFieldId field)
        {
            return m_updateValues[field.RawId].Int16High;
        }

        public short GetInt16High(int field)
        {
            return m_updateValues[field].Int16High;
        }

        public ushort GetUInt16Low(UpdateFieldId field)
        {
            return m_updateValues[field.RawId].UInt16Low;
        }

        public ushort GetUInt16Low(int field)
        {
            return m_updateValues[field].UInt16Low;
        }

        public ushort GetUInt16High(UpdateFieldId field)
        {
            return m_updateValues[field.RawId].UInt16High;
        }

        public ushort GetUInt16High(int field)
        {
            return m_updateValues[field].UInt16High;
        }

        #region GetInt32

        public int GetInt32(int field)
        {
            return m_updateValues[field].Int32;
        }

        public int GetInt32(ObjectFields field)
        {
            return m_updateValues[(int)field].Int32;
        }

        public int GetInt32(UnitFields field)
        {
            return m_updateValues[(int)field].Int32;
        }

        public int GetInt32(PlayerFields field)
        {
            return m_updateValues[(int)field].Int32;
        }

        public int GetInt32(ItemFields field)
        {
            return m_updateValues[(int) field].Int32;
        }
        public int GetInt32(ContainerFields field)
        {
            return m_updateValues[(int) field].Int32;
        }

        public int GetInt32(GameObjectFields field)
        {
            return m_updateValues[(int) field].Int32;
        }

        public int GetInt32(CorpseFields field)
        {
            return m_updateValues[(int)field].Int32;
        }

        public int GetInt32(DynamicObjectFields field)
        {
            return m_updateValues[(int)field].Int32;
        }

        #endregion

        #region GetUInt32

        public uint GetUInt32(int field)
        {
            return m_updateValues[field].UInt32;
        }

        public uint GetUInt32(ObjectFields field)
        {
            return m_updateValues[(int)field].UInt32;
        }

        public uint GetUInt32(UnitFields field)
        {
            return m_updateValues[(int)field].UInt32;
        }

        public uint GetUInt32(PlayerFields field)
        {
            return m_updateValues[(int)field].UInt32;
        }

        public uint GetUInt32(GameObjectFields field)
        {
            return m_updateValues[(int)field].UInt32;
        }

        public uint GetUInt32(ItemFields field)
        {
            return m_updateValues[(int)field].UInt32;
        }

        public uint GetUInt32(ContainerFields field)
        {
            return m_updateValues[(int)field].UInt32;
        }

        public uint GetUInt32(DynamicObjectFields field)
        {
            return m_updateValues[(int)field].UInt32;
        }

        public uint GetUInt32(CorpseFields field)
        {
            return m_updateValues[(int)field].UInt32;
        }

        #endregion

        public ulong GetUInt64(int field)
        {
            uint low = m_updateValues[field].UInt32;
            uint high = m_updateValues[field + 1].UInt32;
			return low | ((ulong)high << 32);
        }

        public ulong GetUInt64(UpdateFieldId field)
        {
            uint low = m_updateValues[field.RawId].UInt32;
            uint high = m_updateValues[field.RawId + 1].UInt32;
            return low | ((ulong)high << 32);
        }

        public EntityId GetEntityId(UpdateFieldId field)
        {
            return GetEntityId(field.RawId);
        }

        public EntityId GetEntityId(int field)
        {
            return new EntityId(m_updateValues[field].UInt32, m_updateValues[field + 1].UInt32);
		}

		public byte[] GetByteArray(UpdateFieldId field)
		{
			return m_updateValues[field.RawId].ByteArray;
		}

        public byte GetByte(int field, int index)
        {
            return m_updateValues[field].GetByte(index);
        }

        public byte GetByte(UpdateFieldId field, int index)
        {
            return m_updateValues[field.RawId].GetByte(index);
        }

        #endregion

        #region Set[...]

        public void SetFloat(UpdateFieldId field, float value)
        {
            SetFloat(field.RawId, value);
        }

        public void SetFloat(int field, float value)
        {
            if (m_updateValues[field].Float == value)
                return;


            m_updateValues[field].Float = value;

            MarkUpdate(field);
        }

        public void SetInt16Low(UpdateFieldId field, short value)
        {
            SetInt16Low(field.RawId, value);
        }

        public void SetInt16Low(int field, short value)
        {
            if (m_updateValues[field].Int16Low == value)
                return;

            m_updateValues[field].Int16Low = value;

            MarkUpdate(field);
        }

        public void SetInt16High(UpdateFieldId field, short value)
        {
            SetInt16High(field.RawId, value);
        }

        public void SetInt16High(int field, short value)
        {
            if (m_updateValues[field].Int16High == value)
                return;


            m_updateValues[field].Int16High = value;

            MarkUpdate(field);
        }

        public void SetUInt16Low(UpdateFieldId field, ushort value)
        {
            SetUInt16Low(field.RawId, value);
        }

        public void SetUInt16Low(int field, ushort value)
        {
            if (m_updateValues[field].UInt16Low == value)
                return;


            m_updateValues[field].UInt16Low = value;

            MarkUpdate(field);
        }

        public void SetUInt16High(UpdateFieldId field, ushort value)
        {
            SetUInt16High(field.RawId, value);
        }

        public void SetUInt16High(int field, ushort value)
        {
            if (m_updateValues[field].UInt16High == value)
                return;


            m_updateValues[field].UInt16High = value;

            MarkUpdate(field);
        }

        public void SetInt32(UpdateFieldId field, int value)
        {
            SetInt32(field.RawId, value);
        }

        public void SetInt32(int field, int value)
        {
            if (m_updateValues[field].Int32 == value)
                return;


            m_updateValues[field].Int32 = value;

            MarkUpdate(field);
        }

        public void SetUInt32(UpdateFieldId field, uint value)
        {
            SetUInt32(field.RawId, value);
        }

        public void SetUInt32(int field, uint value)
        {
            if (m_updateValues[field].UInt32 == value)
                return;

            m_updateValues[field].UInt32 = value;

            MarkUpdate(field);
        }

        public void SetInt64(int field, long value)
        {
            SetInt32(field, (int) (value & 0xFFFFFFFF));
            SetInt32(field + 1, (int) (value >> 32));
        }

        public void SetInt64(UpdateFieldId field, long value)
        {
            SetInt64(field.RawId, value);
        }

        public void SetUInt64(UpdateFieldId field, ulong value)
        {
            SetUInt64(field.RawId, value);
        }

        public void SetUInt64(int field, ulong value)
        {
            SetUInt32(field, (uint)(value & 0xFFFFFFFF));
            SetUInt32(field + 1, (uint)(value >> 32));
        }

        public void SetEntityId(UpdateFieldId field, EntityId id)
        {
            SetEntityId(field.RawId, id);
        }

        public void SetEntityId(int field, EntityId id)
        {
            SetUInt64(field, id.Full);
        }

        public void SetByteArray(UpdateFieldId field, byte[] value)
        {
            SetByteArray(field.RawId, value);
        }

        public unsafe void SetByteArray(int field, byte[] value)
        {
            if (value.Length != 4)
            {
                // this better not be happening
                LogUtil.ErrorException(new Exception("Invalid length"), "Tried to set a byte array with invalid length: ");
                return;
            }

            fixed (byte* pBuffer = value)
            {
                SetUInt32(field, *(uint*)pBuffer);
            }
        }

        /// <summary>
        /// Sets a specified byte of an updatefield to the specified value
        /// </summary>
        /// <param name="field">The field to set</param>
        /// <param name="index">The index of the byte in the 4-byte field. (Ranges from 0-3)</param>
        /// <param name="value">The value to set</param>
        public void SetByte(UpdateFieldId field, int index, byte value)
        {
            SetByte(field.RawId, index, value);
        }

        /// <summary>
        /// Sets a specified byte of an updatefield to the specified value
        /// </summary>
        /// <param name="field">The field to set</param>
        /// <param name="value">The value to set</param>
        /// <param name="index">The index of the byte in the 4-byte field. (Ranges from 0-3)</param>
        public void SetByte(int field, int index, byte value)
        {
            if (m_updateValues[field].GetByte(index) == value)
                return;


            m_updateValues[field].SetByte(index, value);

            MarkUpdate(field);
        }

        /// <summary>
        /// Is called whenever a field has changed.
        /// Adds the given index to the corresponding UpdateMasks.
        /// </summary>
        protected internal void MarkUpdate(int index)
        {
            m_privateUpdateMask.SetBit(index);

            if (index > m_highestUsedUpdateIndex)
            {
                m_highestUsedUpdateIndex = index;
            }

            if (m_publicUpdateMask != m_privateUpdateMask && IsUpdateFieldPublic(index))
            {
                m_publicUpdateMask.SetBit(index);
            }
#if DEBUG
            //var str = FieldRenderUtil.GetFieldStr(ObjectTypeId, index);
            //str = string.Format("{0} updating: {1}", this is INamed ? ((INamed)this).Name : this.ToString(), str);
            //if (this is Character)
            //{
            //    DebugUtil.Log(((Character)this).Account, str);
            //}
            //else
            //{
            //    Console.WriteLine(str);
            //}
#endif
            if (!m_requiresUpdate && IsInWorld)
            {
                RequestUpdate();
            }
        }


        /// <summary>
        /// Marks the given UpdateField for an Update.
        /// Marked UpdateFields will be re-sent to all surrounding Characters.
        /// </summary>
        protected internal void MarkUpdate(UpdateFieldId index)
        {
            MarkUpdate(index.RawId);
        }

        #endregion
    }
}