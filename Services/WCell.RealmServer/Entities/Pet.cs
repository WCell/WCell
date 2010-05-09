/*************************************************************************
 *
 *   file		: Pet.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-11-21 09:08:00 +0100 (fr, 21 nov 2008) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 672 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core;
using WCell.Constants;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Items;
using WCell.Constants.Updates;
using WCell.Constants.Spells;
using WCell.Util;
using WCell.RealmServer.Factions;

namespace WCell.RealmServer.Entities
{
    public class Pet : NPC
	{

		#region Base Unit Fields Overrides
		public override uint DisplayId
		{
			get
			{
				return base.DisplayId;
			}
			set
			{
				base.DisplayId = value;
				//Update Group Update flags
				if (m_master != null && m_master is Character)
					((Character)m_master).GroupUpdateFlags |= GroupUpdateFlags.PetDisplayId;
			}
		}

		public override int Health
		{
			get
			{
				return base.Health;
			}
			set
			{
				base.Health = value;
				//Update Group Update flags
				if (m_master != null && m_master is Character)
					((Character)m_master).GroupUpdateFlags |= GroupUpdateFlags.PetHealth;
			}
		}

		public override uint MaxHealth
		{
			get
			{
				return base.MaxHealth;
			}
			internal set
			{
				base.MaxHealth = value;
				
				//Update Group Update flags
				if (m_master != null && m_master is Character)
					((Character)m_master).GroupUpdateFlags |= GroupUpdateFlags.PetMaxHealth;
			}
		}

		public override int Power
		{
			get
			{
				return base.Power;
			}
			set
			{
				base.Power = value;
				//Update Group Update flags
				if (m_master != null && m_master is Character)
					((Character)m_master).GroupUpdateFlags |= GroupUpdateFlags.PetPower;
			}
		}

		public override PowerType PowerType
		{
			get
			{
				return base.PowerType;
			}
			set
			{
				base.PowerType = value;
				//Update Group Update flags
                if (m_master != null && m_master is Character)
                {
                    // Since we're updating the power type, we also must trigger power and maxpower
                    ((Character)m_master).GroupUpdateFlags |= GroupUpdateFlags.PetPowerType | GroupUpdateFlags.PetPower | GroupUpdateFlags.PetMaxPower;
                }
			}
		}

		public override uint MaxPower
		{
			get
			{
				return base.MaxPower;
			}
			internal set
			{
				base.MaxPower = value;
				//Update Group Update flags
				if (m_master != null && m_master is Character)
					((Character)m_master).GroupUpdateFlags |= GroupUpdateFlags.PetMaxPower;
			}
		}
		#endregion
	}
}
