/*************************************************************************
 *
 *   file		: CasterInfo.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-02-17 05:08:19 +0100 (on, 17 feb 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1256 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using WCell.Core;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Entities
{
	/// <summary>
	/// Wraps a WorldObject.
	/// This is primarily used for Auras and other things that are allowed to persist after
	/// a Character or object might be gone, but still require basic information about the original
	/// object.
	/// </summary>
	public class ObjectReference : IEntity
	{
		public static ObjectReference GetOrCreate(Region rgn, EntityId id)
		{
			var caster = rgn.GetObject(id);
			if (caster != null)
			{
				return caster.SharedReference;
			}
			return new ObjectReference(id, 1);
		}

		public EntityId EntityId
		{
			get;
			private set;
		}
		//public readonly ObjectTypes ObjectType;
		//public readonly Faction Faction;

		WorldObject m_Object;

		public ObjectReference(WorldObject obj)
		{
			EntityId = obj.EntityId;
			Level = obj.CasterLevel;
			m_Object = obj;
			//Faction = caster.Faction;
			//ObjectType = caster.Type;
		}

		public ObjectReference(EntityId entityId, int level)
		{
			EntityId = entityId;
			Level = level;
		}

		public ObjectReference(int level)
		{
			Level = level;
		}

		public ObjectReference()
		{
		}

		public int Level
		{
			get;
			internal set;
		}

		public WorldObject Object
		{
			get { return (m_Object != null && m_Object.IsInWorld) ? m_Object : null; }
			internal set { m_Object = value; }
		}

		/// <summary>
		/// Returns the Unit behind this object (if exists)
		/// </summary>
		public Unit UnitMaster
		{
			get { return m_Object != null ? m_Object.UnitMaster : null; }
		}

		//public DynamicObject CasterObject
		//{
		//    get
		//    {
		//        return WorldMgr.GetObject(Id) as DynamicObject;
		//    }
		//}
	}
}