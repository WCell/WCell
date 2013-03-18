/*************************************************************************
 *
 *   file		: Skill.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-28 05:18:24 +0100 (to, 28 jan 2010) $

 *   revision		: $Rev: 1229 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using WCell.Constants.Achievements;
using WCell.RealmServer.Database.Entities;
using WCell.Util.Logging;
using WCell.Constants.Skills;
using WCell.Constants.Updates;
using WCell.RealmServer.Database;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.Spells;
using WCell.Util;

namespace WCell.RealmServer.Skills
{
	/// <summary>
	/// Represents a Player's progress with a certain skill
	/// </summary>
	public class Skill
	{
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

			SetCurrentValueSilently(record.CurrentValue);
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

			m_record.CreateLater();
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
				SetCurrentValueSilently(value);
				m_skills.Owner.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.ReachSkillLevel,
				                                                            (uint) m_record.SkillId, m_record.CurrentValue);
			}
		}

		protected void SetCurrentValueSilently(ushort value)
		{
			m_skills.Owner.SetUInt16Low(PlayerField + 1, value);
			m_record.CurrentValue = value;
			if (SkillLine.Id == SkillId.Defense)
			{
				m_skills.Owner.UpdateDefense();
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
				if (SkillLine.Id == SkillId.Defense)
				{
					m_skills.Owner.UpdateDefense();
				}
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

		/// <summary>
		/// The persistant record that can be saved to/loaded from DB
		/// </summary>
		internal SkillRecord Record
		{
			get { return m_record; }
		}

		public SkillTierId CurrentTier
		{
			get
			{
				if (CurrentTierSpell != null)
				{
					return CurrentTierSpell.SkillTier;
				}
				// added skill without a spell (means that a GM or Dev was playing around)
				return SkillLine.GetTierForLevel(CurrentValue);
			}
		}

		private Spell _currentTierSpell;

		/// <summary>
		/// The spell that represents the current tier
		/// </summary>
		public Spell CurrentTierSpell
		{
			get { return _currentTierSpell; }
			internal set
			{
				_currentTierSpell = value;
				var skillId = value.Ability.Skill.Id;
				var tier = value.SkillTier;
				m_skills.m_owner.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.LearnSkillLevel, (uint)skillId, (uint)tier);
			}
		}

		/// <summary>
		/// Checks whether the given tier can be learned
		/// </summary>
		public bool CanLearnTier(SkillTierId tier)
		{
			if (SkillLine.HasTier(tier))
			{
				uint tierLimit = SkillLine.Tiers.GetMaxValue(tier);
				if (CurrentValue >= (int)tierLimit - 100)
				{
					// cannot be learnt if we have less than max - 100 in that skill
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Gains up to maxGain skill points with the given chance.
		/// </summary>
		public void GainRand(int chance, int maxGain)
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
			foreach (var ability in SkillHandler.GetAbilities(SkillLine.Id))
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