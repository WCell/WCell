/*************************************************************************
 *
 *   file		: DynamicObject.cs
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

using System;
using WCell.Constants.Factions;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Global;
using WCell.RealmServer.Spells;
using WCell.RealmServer.UpdateFields;
using WCell.Util;
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

        internal DynamicObject()
        {
        }

        public DynamicObject(SpellCast cast, float radius)
            : this(cast.CasterUnit,
            cast.Spell.SpellId, radius,
            cast.Map,
            cast.TargetLoc)
        {
        }

        public DynamicObject(Unit creator, SpellId spellId, float radius, Map map, Vector3 pos)
        {
            if (creator == null)
                throw new ArgumentNullException("creator", "creator must not be null");

            Master = creator;
            EntityId = EntityId.GetDynamicObjectId(++lastId);
            Type |= ObjectTypes.DynamicObject;
            SetEntityId(DynamicObjectFields.CASTER, creator.EntityId);
            SpellId = spellId;
            Radius = radius;
            Bytes = 0x01EEEEEE;
            ScaleX = 1;

            m_position = pos;
            map.AddObjectLater(this);
        }

        public override int CasterLevel
        {
            get { return m_master.Level; }
        }

        public override string Name
        {
            get { return m_master + "'s " + SpellId + " - Object"; }
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
                return m_master.Faction;
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
                return m_master.Faction != null ? m_master.Faction.Id : FactionId.None;
            }
            set
            {
                //throw new Exception("Faction of DynamicObject cannot be set.");
            }
        }

        #endregion Factions/Hostility

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

        public override string ToString()
        {
            return GetType() + " " + base.ToString();
        }
    }
}