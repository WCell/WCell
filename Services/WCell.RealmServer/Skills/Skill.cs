/*************************************************************************
 *
 *   file		: Skill.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-28 05:18:24 +0100 (to, 28 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1229 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using NLog;
using WCell.Constants.Updates;
using WCell.RealmServer.Database;
using WCell.Util;

namespace WCell.RealmServer.Skills
{
	/// <summary>
	/// Represents a Player's progress with a certain skill
	/// </summary>
	public class Skill
	{
		private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

		public readonly PlayerFields PlayerField;
		public readonly SkillLine SkillLine;

		/// <summary>
		/// The containing SkillCollection
		/// </summary>
		readonly SkillCollection m_skills;
	    readonly SkillRecord m_record;

		public Skill(SkillCollection skills, PlayerFields field, SkillRecord record, SkillLine skillLine)
		{
			PlayerField = field;
			m_skills = skills;
			m_record = record;

			SkillLine = skillLine;
			m_skills.Owner.SetUInt16Low(field, (ushort)skillLine.Id);
			m_skills.Owner.SetUInt16High(field, skillLine.Abandonable);

			CurrentValue = record.CurrentValue;
			MaxValue = record.MaxValue;
		}

		public Skill(SkillCollection skills, PlayerFields field, SkillLine skill, uint value, uint max)
		{
			m_record = new SkillRecord { SkillId = skill.Id, OwnerId = skills.Owner.Record.Guid };

			m_skills = skills;
			PlayerField = field;
			SkillLine = skill;

			m_skills.Owner.SetUInt16Low(field, (ushort)skill.Id);
			m_skills.Owner.SetUInt16High(field, skill.Abandonable);


			CurrentValue = (ushort)value;
			MaxValue = (ushort)max;

			m_record.CreateAndFlush();
		}

		/// <summary>
		/// Checks whether the given tier can be activated
		/// </summary>
		public bool IsTierActivated(uint tier)
		{
			if (SkillLine.HasTier(tier))
			{
				// TODO: Correct tier-value calculation
				// So far: You must have Max - 15 of the tier before to activate the next one
				// (which is the tier that you are in right now)
				uint tierLimit = SkillLine.Tier.Values[tier];
				if (CurrentValue >= tierLimit - 15)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// The current value of this skill
		/// </summary>
		public ushort CurrentValue
		{
			get
			{
				return m_record.CurrentValue;
			}
			set
			{
				m_skills.Owner.SetUInt16Low(PlayerField + 1, value);
				m_record.CurrentValue = value;
			}
		}

		/// <summary>
		/// The maximum possible value of this skill not including modifiers
		/// </summary>
		public ushort MaxValue
		{
			get { return m_record.MaxValue; }
			set
			{
				m_skills.Owner.SetUInt16High(PlayerField + 1, value);
				m_record.MaxValue = value;
			}
		}

		/// <summary>
		/// Returns CurrentValue + Modifier
		/// </summary>
		public uint ActualValue
		{
			get
			{
				return (uint)(CurrentValue + Modifier);
			}
		}

		/// <summary>
		/// Either the original max of this skill or the owner's level * 5, whatever comes first
		/// </summary>
		public int ActualMax
		{
			get
			{
				return Math.Min(MaxValue, m_skills.Owner.Level * 5);
			}
		}

		/// <summary>
		/// The modifier to this skill
		/// Will be red if negative, green if positive
		/// </summary>
		public short Modifier
		{
			get { return m_skills.Owner.GetInt16Low(PlayerField + 2); }
			set
			{
				m_skills.Owner.SetInt16Low(PlayerField + 2, value);
			}
		}

		/// <summary>
		/// Apparently a flat skill-bonus without colored text
		/// </summary>
		public short ModifierValue
		{
			get { return m_skills.Owner.GetInt16High(PlayerField + 2); }
			set { m_skills.Owner.SetInt16High(PlayerField + 2, value); }
		}

		internal SkillRecord Record
		{
			get
			{
				return m_record;
			}
		}

		/// <summary>
		/// Gains up to maxGain with the given chance.
		/// </summary>
		public void Gain(int chance, int maxGain)
		{
			int current = CurrentValue;
			int maxPossGain = MaxValue - current;
			if (maxPossGain > 0)
			{
				maxGain = Math.Min(maxGain, maxPossGain);
				var rand = Utility.Random(0, 100);
				if (chance > rand)
				{
					var gain = (int)Math.Ceiling((maxGain / 100f) * (100 - rand));
					CurrentValue += (ushort)gain;
				}
			}
		}

		/// <summary>
		/// Gains max value of this skill.
		/// </summary>
		public void LearnMax()
		{
			MaxValue = (ushort)SkillLine.MaxValue;
			CurrentValue = (ushort)SkillLine.MaxValue;
		}

		/// <summary>
		/// The player learns all abilities of this skill.
		/// </summary>
		public void LearnAllAbilities()
		{
			foreach (SkillAbility ability in SkillHandler.GetAbilities(SkillLine.Id))
			{
				if (ability != null)
				{
					m_skills.Owner.Spells.AddSpell(ability.Spell);
				}
			}
		}

		/// <summary>
		/// The player unlearns all abilities of this skill.
		/// </summary>
		public void RemoveAllAbilities()
		{
			foreach (SkillAbility ability in SkillHandler.GetAbilities(SkillLine.Id))
			{
				if (ability != null)
				{
					m_skills.Owner.Spells.Remove(ability.Spell);
				}
			}
		}

		/// <summary>
		/// Saves all recent changes to this Skill to the DB
		/// </summary>
		public void Save()
		{
			m_record.SaveAndFlush();
		}

		/// <summary>
		/// Sends this skill instantly to the owner
		/// </summary>
		public void Push()
		{
			//m_skills.Owner.PushFieldUpdateToPlayer(m_skills.Owner, PlayerField, m_skills.Owner.GetUInt32(PlayerField));
			//m_skills.Owner.PushFieldUpdateToPlayer(m_skills.Owner, PlayerField + 1, m_skills.Owner.GetUInt32(PlayerField + 1));
			//m_skills.Owner.PushFieldUpdateToPlayer(m_skills.Owner, PlayerField + 2, m_skills.Owner.GetUInt32(PlayerField + 2));
		}
	}
}
