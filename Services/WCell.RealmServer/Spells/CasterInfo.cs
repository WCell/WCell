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

using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Spells.Auras
{
	/// <summary>
	/// 
	/// </summary>
	public class CasterInfo
	{
		public static CasterInfo GetOrCreate(Region rgn, EntityId id)
		{
			var caster = rgn.GetObject(id);
			if (caster != null)
			{
				return caster.CasterInfo;
			}
			return new CasterInfo(id, 1);
		}

		public readonly EntityId CasterId;
		//public readonly ObjectTypes ObjectType;
		//public readonly Faction Faction;

		WorldObject m_caster;

		public CasterInfo(WorldObject caster)
		{
			CasterId = caster.EntityId;
			Level = caster.CasterLevel;
			m_caster = caster;
			//Faction = caster.Faction;
			//ObjectType = caster.Type;
		}

		public CasterInfo(EntityId casterId, int level)
		{
			CasterId = casterId;
			Level = level;
		}

		public CasterInfo(int level)
		{
			Level = level;
		}

		public CasterInfo()
		{
		}

		public bool IsItem
		{
			get { return CasterId.IsItem; }
		}

		public int Level
		{
			get;
			internal set;
		}

		public WorldObject Caster
		{
			get { return (m_caster != null && m_caster.IsInWorld) ? m_caster : null; }
			internal set { m_caster = value; }
		}

		/// <summary>
		/// Returns the Caster as a Unit (if exists)
		/// </summary>
		public Unit CasterUnit
		{
			get { return m_caster != null ? m_caster.UnitMaster : null; }
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