/*************************************************************************
 *
 *   file		: SkillLine.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-24 01:12:44 +0100 (sø, 24 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1213 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Skills
{
	public class SkillLine
	{
		public SkillId Id;

		public SkillCategory Category;
		public int SkillCostsDataId;

		/// <summary>
		/// The Skill's "challenge levels"
		/// </summary>
		public SkillTier Tier;

		/// <summary>
		/// The name of this Skill
		/// </summary>
		public string Name;

		/// <summary>
		/// The language that this Skill represents (if any).
		/// </summary>
		public ChatLanguage Language = ChatLanguage.Universal;

		/// <summary>
		/// 1 for professions, else 0 - needed by packets
		/// Also allows the skill to be unlearned
		/// </summary>
		public ushort Abandonable;

		public List<SkillAbility> InitialAbilities = new List<SkillAbility>(5);

		public Spell ApprenticeSpell
		{
			get
			{
				foreach (var spell in TeachingSpells)
				{
					if (spell.GetEffect(SpellEffectType.Skill).BasePoints == 0)
					{
						return spell;
					}
				}
				return null;
			}
		}

		/// <summary>
		/// The Spells that give the different tiers of this Skill
		/// </summary>
		public List<Spell> TeachingSpells = new List<Spell>(1);

		public bool HasTier(uint tier)
		{
			return Tier.Values != null && tier < Tier.Values.Length;
		}

		/// <summary>
		/// The initial value of this skill, when it has just been learnt
		/// </summary>
		public uint InitialValue
		{
			get
			{
				if (Tier.Values != null && Tier.Values.Length == 1)
				{
					return Tier.Values[0];
				}
				return 1;
			}
		}

		/// <summary>
		/// The max-value of the skill when it has just been learnt
		/// </summary>
		public uint InitialLimit
		{
			get
			{
				if (Tier.Values == null)
				{
					return 1;
				}

				// The first entry is the first initial limit
				return Tier.Values[0];
			}
		}

		/// <summary>
		/// The highest available value for this skill.
		/// </summary>
		public uint MaxValue
		{
			get
			{
				if (Tier.Values != null)
				{
					return Math.Max(1, Tier.Values[Tier.Values.Length - 1]);
				}

				if(Category == SkillCategory.WeaponProficiency)
				{
					return 400;
				}
				return 1;
			}
		}


		public override string ToString()
		{
			return Name + " (" + (uint)Id + ", " + Category + ", Tier: " + Tier + ")";
		}
	}
}
