/*************************************************************************
 *
 *   file		: ObjectBase.Fields.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-14 13:00:53 +0100 (to, 14 jan 2010) $

 *   revision		: $Rev: 1192 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.Dynamic;
using WCell.Constants.Looting;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Entities
{
	public partial class ObjectBase
	{
		protected Loot m_loot;
		public readonly dynamic CustomData = new ExpandoObject();

		/// <summary>
		///  The entity ID of the object
		/// </summary>
		public EntityId EntityId
		{
			get { return GetEntityId(ObjectFields.GUID); }
			protected internal set
			{
				SetEntityId(ObjectFields.GUID, value);
			}
		}

		public ObjectTypes Type
		{
			get { return (ObjectTypes)GetUInt32(ObjectFields.TYPE); }
			protected set { SetUInt32(ObjectFields.TYPE, (uint)value); }
		}

		public uint EntryId
		{
			get { return GetUInt32(ObjectFields.ENTRY); }
			protected set { SetUInt32(ObjectFields.ENTRY, value); }
		}

		public float ScaleX
		{
			get { return GetFloat(ObjectFields.SCALE_X); }
			set
			{
				SetFloat(ObjectFields.SCALE_X, value);
				if (this is Unit && ((Unit)this).Model != null)
				{
					((Unit)this).UpdateModel();
				}
			}
		}

		public virtual ObjectTypeCustom CustomType
		{
			get
			{
				return ObjectTypeCustom.Object;
			}
		}

		#region Loot
		/// <summary>
		/// The current loot that can be looted of this object (if loot has been generated yet)
		/// </summary>
		public Loot Loot
		{
			get { return m_loot; }
			set { m_loot = value; }
		}

		public virtual uint GetLootId(LootEntryType type)
		{
			return 0;
		}

		public virtual uint LootMoney
		{
			get
			{
				// TODO: Customize
				return 0;
			}
		}

		#endregion
	}
}