/*************************************************************************
 *
 *   file		: DynamicObject.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-14 13:00:53 +0100 (to, 14 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1192 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using WCell.Constants.Factions;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Global;
using WCell.RealmServer.Network;
using WCell.RealmServer.Spells;
using WCell.RealmServer.UpdateFields;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Entities
{
	/// <summary>
	/// This -contrary to what its name suggests- is a static animation or decoration in the world
	/// </summary>
	public partial class DynamicObject : WorldObject
	{
		public static readonly UpdateFieldCollection UpdateFieldInfos = UpdateFieldMgr.Get(ObjectTypeId.DynamicObject);

		protected override UpdateFieldCollection _UpdateFieldInfos
		{
			get { return UpdateFieldInfos; }
		}

		public override UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUpdateFieldHandlers
		{
			get { return UpdateFieldHandler.DynamicDOFieldHandlers; }
		}

		internal static uint lastId;
		internal Unit m_creator;

		public Unit Caster
		{
			get { return m_creator; }
		}

		internal DynamicObject()
		{
		}

		public DynamicObject(SpellCast cast, float radius)
			: this(cast.Caster,
			cast.Spell.SpellId, radius,
			cast.Map,
			cast.TargetLoc)
		{
		}

		public DynamicObject(Unit creator, SpellId spellId, float radius, Region region, Vector3 pos)
		{
			if (creator == null)
				throw new ArgumentNullException("creator", "creator must not be null");

			Master = m_creator = creator;
			EntityId = EntityId.GetDynamicObjectId(++lastId);
			Type |= ObjectTypes.DynamicObject;
			SetEntityId(DynamicObjectFields.CASTER, Caster.EntityId);
			SpellId = spellId;
			Radius = radius;
			Bytes = 0x01EEEEEE;
			ScaleX = 1;

			m_position = pos;
			region.AddObjectLater(this);
		}

		public override int CasterLevel
		{
			get { return m_creator.Level; }
		}

		public override string Name
		{
			get { return m_creator + "'s " + SpellId + " - Effect"; }
			set
			{
				// does nothing
			}
		}

		#region Factions/Hostility
		public override Faction Faction
		{
			get
			{
				return m_creator.Faction;
			}
			set
			{
				//throw new Exception("Faction of DynamicObject cannot be set.");
			}
		}

		public override FactionId FactionId
		{
			get
			{
				return m_creator.Faction != null ? m_creator.Faction.Id : FactionId.None;
			}
			set
			{
				//throw new Exception("Faction of DynamicObject cannot be set.");
			}
		}

		public override bool IsHostileWith(IFactionMember opponent)
		{
			return m_creator.IsHostileWith(opponent);
		}

		public override bool IsAlliedWith(IFactionMember opponent)
		{
			return m_creator.IsAlliedWith(opponent);
		}

		public override bool IsFriendlyWith(IFactionMember opponent)
		{
			return m_creator.IsFriendlyWith(opponent);
		}
		#endregion

		public override ObjectTypeId ObjectTypeId
		{
			get { return ObjectTypeId.DynamicObject; }
		}

		// 0x48 in 3.1
		public override UpdateFlags UpdateFlags
		{
			get { return UpdateFlags.StationaryObject | UpdateFlags.Flag_0x10 | UpdateFlags.Flag_0x8; }
		}

		protected override void WriteMovementUpdate(PrimitiveWriter writer, UpdateFieldFlags relation)
		{
			// UpdateFlag.StationaryObject
			writer.Write(Position);
			writer.WriteFloat(Orientation);
		}
	}
}