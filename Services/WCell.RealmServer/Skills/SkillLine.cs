/*************************************************************************
 *
 *   file		: SkillLine.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-24 01:12:44 +0100 (sø, 24 jan 2010) $

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
using System.Linq;
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
		public SkillTiers Tiers;

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

		/// <summary>
		/// The Spells that give the different tiers of this Skill
		/// </summary>
		public List<Spell> TeachingSpells = new List<Spell>(1);

		/// <summary>
		/// The initial value of this skill, when it has just been learnt
		/// </summary>
		public uint InitialValue
		{
			get
			{
				if (Tiers.MaxValues != null && Tiers.MaxValues.Length == 1)
				{
					return Tiers.MaxValues[0];
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
				if (Tiers.MaxValues == null)
				{
					return 1;
				}

				// The first entry is the first initial limit
				return Tiers.MaxValues[0];
			}
		}

		/// <summary>
		/// The highest available value for this skill.
		/// </summary>
		public uint MaxValue
		{
			get
			{
				if (Tiers.MaxValues != null)
				{
					return Math.Max(1, Tiers.MaxValues[Tiers.MaxValues.Length - 1]);
				}

				if (Category == SkillCategory.WeaponProficiency)
				{
					return 400;
				}
				return 1;
			}
		}


		public bool HasTier(SkillTierId tier)
		{
			return Tiers.MaxValues != null && (int)tier < Tiers.MaxValues.Length;
		}

		public SkillTierId GetTierForLevel(int value)
		{
			if (Tiers.MaxValues != null)
			{
				for (var t = 0; t < Tiers.MaxValues.Length; t++)
				{
					var max = Tiers.MaxValues[t];
					if (value < max)
					{
						return (SkillTierId)t;
					}
				}
			}
			return SkillTierId.End;
		}

		public Spell GetSpellForLevel(int skillLevel)
		{
			var tier = GetTierForLevel(skillLevel);
			return GetSpellForTier(tier);
		}

		public Spell GetSpellForTier(SkillTierId tier)
		{
			return TeachingSpells.FirstOrDefault(spell => spell.GetEffect(SpellEffectType.Skill).BasePoints == (int)tier);
		}

		public override string ToString()
		{
			return Name + " (" + (uint)Id + ", " + Category + ", Tier: " + Tiers + ")";
		}
	}
}