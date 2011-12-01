using System;
using System.IO;
using System.Runtime.InteropServices;
using WCell.Constants;
using WCell.Constants.GameObjects;
using WCell.Constants.Updates;
using WCell.Core.Network;
using WCell.Util;

namespace WCell.Core
{
	public enum HighId : ushort
	{
		Unit = 0xF130,
		UnitPet = 0xF140,
		//Unit = 0xF550,
		Unit_F4 = 0xF430,
		Unit_F7 = 0xF730,

		Unit_F1_Vehicle = 0xF150,
		Unit_F4_Vehicle = 0xF450,
		Unit_F7_Vehicle = 0xF750,

		Pet = 0xF140,

        Group = 0x1F50,
        Guild = 0x1FF6,

		MoTransport = 0x1FC0,
		Transport = 0xF120,
		Vehicle = 0xF550,

		GameObject = 0xF110,
		GameObject_F4 = 0xF410,
		GameObject_F7 = 0xF710,
		GameObject4 = 0xF009,
		/// <summary>
		/// Or Container
		/// </summary>
		Item = 0x4000,
		Player = 0x0000,
		DynamicObject = 0xF100,
		DynamicObject_2 = 0xF500,

		Corpse = 0xF101, // TODO: actually should be 0xF000, but can't due to duplicate entries in some dictionaries
		Corpse2 = 0xF400,
		Corpse3 = 0xF700,
	}

	public enum HighGuid8 : byte
	{
		Flag_1F = 0x1F,
		Item = 0x40,
		Flag_F1 = 0xF1,
		Flag_F4 = 0xF4,
		Flag_F5 = 0xF5,
		Flag_F7 = 0xF7,
	}

	public enum HighGuidType : byte
	{
		/// <summary>
		/// Also Player, Corpse, or DynamicObject
		/// </summary>
		NoEntry = 0x00,
		GameObject = 0x10,
		Transport = 0x20,
		Unit = 0x30,
		Pet = 0x40,
		Vehicle = 0x50,
        Guild = 0xF6,

		MapObjectTransport = 0xC0,
	}

	[StructLayout(LayoutKind.Explicit, Size = 8)]
	public struct EntityId : IComparable, IEquatable<EntityId>, IConvertible
	{
		const uint LowMask = 0xFFFFFF;
		const uint EntryMask = 0xFFFFFF;
		const uint HighMask = 0xFFFF0000;
		const uint High7Mask = 0x00FF0000;
		const uint High8Mask = 0xFF000000;

		public static readonly EntityId Zero = new EntityId(0);
		public static readonly byte[] ZeroRaw = new byte[8];

		[FieldOffset(0)]
		public ulong Full;
		[FieldOffset(0)]
		private uint m_low;
		[FieldOffset(3)]
		private uint m_entry;
		[FieldOffset(4)]
		private uint m_high;

		public EntityId(byte[] fullRaw)
		{
			m_low = 0;
			m_high = 0;
			m_entry = 0;
			Full = BitConverter.ToUInt64(fullRaw, 0);
		}

		public EntityId(ulong full)
		{
			m_low = 0;
			m_high = 0;
			m_entry = 0;
			Full = full;
		}

		public EntityId(uint low, uint high)
		{
			Full = 0;
			m_entry = 0;
			m_low = low;
			m_high = high;
		}

		public EntityId(uint low, HighId high)
		{
			Full = 0;
			m_high = 0;
			m_entry = 0;
			m_low = low;
			High = high;
		}

		public EntityId(uint low, uint entry, HighId high)
		{
			Full = 0;
			m_high = 0;
			m_low = low;
			m_entry = entry;
			High = high;
		}

		public uint Low
		{
			get
			{
				return m_low & LowMask;
			}

			private set
			{
				m_low &= ~LowMask;
				m_low |= (value & LowMask);
			}
		}

		public uint Entry
		{
			get
			{
				return m_entry & EntryMask;
			}
		}

		public HighId High
		{
			get
			{
				return (HighId)(m_high >> 16);
			}
			private set
			{
				m_high &= ~HighMask;
				m_high |= ((uint)value) << 16;
			}
		}

		public bool HasEntry
		{
			get
			{
				//return ((m_high >> 16) & 0xFF) != 0;
				return SeventhByte != HighGuidType.NoEntry;
			}
		}

		public uint LowRaw
		{
			get { return m_low; }
		}

		public uint HighRaw
		{
			get { return m_high; }
		}

		public bool IsItem
		{
			get { return EighthByte == HighGuid8.Item; }
		}

		public HighGuidType SeventhByte
		{
			get
			{
				return (HighGuidType)((m_high & High7Mask) >> 16);
			}
		}

		public HighGuid8 EighthByte
		{
			get
			{
				return (HighGuid8)((m_high & High8Mask) >> 24);
			}
		}

		public ObjectTypeId ObjectType
		{
			get
			{
				switch (SeventhByte)
				{
					case HighGuidType.NoEntry:
						{
							if (IsItem)
							{
								return ObjectTypeId.Item;
							}
							return ObjectTypeId.Player;
						}
					case HighGuidType.GameObject: return ObjectTypeId.GameObject;
					case HighGuidType.MapObjectTransport: return ObjectTypeId.GameObject;
					case HighGuidType.Pet: return ObjectTypeId.Unit;
					case HighGuidType.Transport: return ObjectTypeId.GameObject;
					case HighGuidType.Unit: return ObjectTypeId.Unit;
					case HighGuidType.Vehicle: return ObjectTypeId.Unit;
				}

				return ObjectTypeId.Object;
			}
		}


		public int WritePacked(BinaryWriter binWriter)
		{
			return binWriter.WritePackedUInt64(Full);
		}

		public override bool Equals(object obj)
		{
			return obj != null && obj is EntityId && Equals((EntityId)obj);
		}

		public int CompareTo(object obj)
		{
			if (obj is EntityId)
			{
				return Full.CompareTo(((EntityId)obj).Full);
			}
			if (obj is ulong)
			{
				return Full.CompareTo((ulong)obj);
			}
			return -1;
		}

		public bool Equals(EntityId other)
		{
			return other.Full == Full;
		}

		public static bool operator ==(EntityId left, EntityId right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(EntityId left, EntityId right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
		{
			if (Full == 0)
			{
				// empty
				return "<null>";
			}

			var type = ObjectType;
			if (HasEntry)
			{
				var entryStr = type.ToString(Entry);
				return string.Format("High: 0x{0} ({1}) - Low: {2} - Entry: {3}", ((ushort)High).ToString("X2"), type, Low, entryStr);
			}
			return string.Format("High: 0x{0:X4} ({1}) - Low: {2}", m_high, type, m_low);
		}


		public static EntityId ReadPacked(PacketIn packet)
		{
			var mask = Utility.GetSetIndices(packet.ReadByte());

			var rawId = new byte[8];
			foreach (var i in mask)
			{
				rawId[i] = packet.ReadByte();
			}
			return new EntityId(rawId);
		}

		public static implicit operator ulong(EntityId id)
		{
			return id.Full;
		}

		#region GetCorpseId

		public static EntityId GetCorpseId(uint id)
		{
			return new EntityId(id, 0, HighId.Corpse);
		}

		#endregion

		#region GetUnitId
		public static EntityId GetUnitId(uint id, uint entry)
		{
			return new EntityId(id, entry, HighId.Unit);
		}

		public static EntityId GetPetId(uint id, uint petNumber)
		{
			return new EntityId(id, petNumber, HighId.UnitPet);
		}

		#endregion

		public static EntityId GetPlayerId(uint low)
		{
			return new EntityId(low, 0, HighId.Player);
		}

		public static EntityId GetItemId(uint low)
		{
			return new EntityId(low, HighId.Item);
		}

		public static EntityId GetDynamicObjectId(uint low)
		{
			return new EntityId(low, 0, HighId.DynamicObject);
		}

		public static EntityId GetGameObjectId(uint low, GOEntryId entry)
		{
			return new EntityId(low, (uint)entry, HighId.GameObject);
		}

        public static EntityId GetMOTransportId(uint low, uint entry)
        {
            return new EntityId(low, entry, HighId.MoTransport);
        }

		//public static string ToString(ObjectTypeId type)
		//{
		//    string entryStr;
		//    if (type == ObjectTypeId.Item)
		//    {
		//        entryStr = (ItemId)Entry + " (" + Entry + ")";
		//    }
		//    else if (type == ObjectTypeId.Unit)
		//    {
		//        entryStr = (NPCId)Entry + " (" + Entry + ")";
		//    }
		//    else if (type == ObjectTypeId.GameObject)
		//    {
		//        entryStr = (GOEntryId)Entry + " (" + Entry + ")";
		//    }
		//    else
		//    {
		//        entryStr = Entry.ToString();
		//    }
		//}
		public override int GetHashCode()
		{
			return Full.GetHashCode();
		}

        #region IConvertible Members

        public TypeCode GetTypeCode()
        {
            return TypeCode.Object;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot cast EntityId to Boolean");
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot cast EntityId to Byte");
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot cast EntityId to Char");
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot cast EntityId to DateTime");
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot cast EntityId to Decimal");
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot cast EntityId to Double");
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot cast EntityId to Int16");
        }

        public int ToInt32(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot cast EntityId to Int32");
        }

        public long ToInt64(IFormatProvider provider)
        {
            return (long)Full;
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot cast EntityId to SByte");
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot cast EntityId to Single");
        }

        public string ToString(IFormatProvider provider)
        {
            return ToString();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return Convert.ChangeType(Full, conversionType);
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot cast EntityId to UInt16");
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot cast EntityId to UInt32");
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            return Full;
        }

        #endregion
    }
}