/*************************************************************************
 *
 *   file		: Corpse.Fields.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-06-25 18:16:31 +0200 (to, 25 jun 2009) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1027 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.Constants.Updates;
using WCell.Core;

namespace WCell.RealmServer.Entities
{
	public partial class Corpse
	{
		Character m_owner;

		public Character Owner
		{
			get
			{
				return m_owner;
			}
			set
			{
				m_owner = value;
				if (m_owner != null)
				{
					SetEntityId(CorpseFields.OWNER, value.EntityId);
				}
				else
				{
					SetEntityId(CorpseFields.OWNER, EntityId.Zero);
				}
			}
		}

		public uint DisplayId
		{
			get { return GetUInt32(CorpseFields.DISPLAY_ID); }
			set { SetUInt32(CorpseFields.DISPLAY_ID, value); }
		}

		/// <summary>
		/// Array of 19 uints
		/// TODO: Set the equipment of the player
		/// </summary>
		public uint ItemBase
		{
			get { return GetUInt32(CorpseFields.ITEM); }
			set { SetUInt32(CorpseFields.ITEM, value); }
		}

		#region BYTES_1

		public byte[] Bytes1
		{
			get { return GetByteArray(CorpseFields.BYTES_1); }
			set { SetByteArray(CorpseFields.BYTES_1, value); }
		}

		public byte Bytes1_0
		{
			get { return GetByte(CorpseFields.BYTES_1, 0); }
			set { SetByte(CorpseFields.BYTES_1, 0, value); }
		}

		public RaceId Race
		{
			get { return (RaceId)GetByte(CorpseFields.BYTES_1, 1); }
			set { SetByte(CorpseFields.BYTES_1, 1, (byte)value); }
		}

		public GenderType Gender
		{
			get { return (GenderType)GetByte(CorpseFields.BYTES_1, 2); }
			set { SetByte(CorpseFields.BYTES_1, 2, (byte)value); }
		}

		public byte Skin
		{
			get { return GetByte(CorpseFields.BYTES_1, 3); }
			set { SetByte(CorpseFields.BYTES_1, 3, value); }
		}

		#endregion

		#region CORPSE_BYTES_2

		public byte[] Bytes2
		{
			get { return GetByteArray(CorpseFields.BYTES_2); }
			set { SetByteArray(CorpseFields.BYTES_2, value); }
		}

		public byte Face
		{
			get { return GetByte(CorpseFields.BYTES_2, 0); }
			set { SetByte(CorpseFields.BYTES_2, 0, value); }
		}

		public byte HairStyle
		{
			get { return GetByte(CorpseFields.BYTES_2, 1); }
			set { SetByte(CorpseFields.BYTES_2, 1, value); }
		}

		public byte HairColor
		{
			get { return GetByte(CorpseFields.BYTES_2, 2); }
			set { SetByte(CorpseFields.BYTES_2, 2, value); }
		}

		public byte FacialHair
		{
			get { return GetByte(CorpseFields.BYTES_2, 3); }
			set { SetByte(CorpseFields.BYTES_2, 3, value); }
		}

		#endregion

		public CorpseFlags Flags
		{
			get { return (CorpseFlags)GetUInt32(CorpseFields.FLAGS); }
			set { SetUInt32(CorpseFields.FLAGS, (uint)value); }
		}

		public CorpseDynamicFlags DynamicFlags
		{
			get { return (CorpseDynamicFlags)GetUInt32(CorpseFields.DYNAMIC_FLAGS); }
			set { SetUInt32(CorpseFields.DYNAMIC_FLAGS, (uint)value); }
		}


		public override ObjectTypeCustom CustomType
		{
			get
			{
				return ObjectTypeCustom.Object | ObjectTypeCustom.Corpse;
			}
		}
	}
}