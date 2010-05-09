/*************************************************************************
 *
 *   file		: SkillCollection.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-28 16:34:36 +0100 (to, 28 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1231 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Linq;
using System.Collections.Generic;
using NLog;
using WCell.Constants;
using WCell.Constants.Skills;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.RealmServer.Formulas;
using WCell.Util;
using WCell.Constants.Items;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Skills
{
	// TODO: Also handle removal/adding of skill-spells
	// TODO: DB-support
	/// <summary>
	/// A collection of all of one <see cref="Character"/>'s skills.
	/// </summary>
	public class SkillCollection
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		readonly Dictionary<SkillId, Skill> m_skills = new Dictionary<SkillId, Skill>(40);
		readonly Dictionary<PlayerFields, Skill> ByField = new Dictionary<PlayerFields, Skill>();
		internal Character m_owner;

		public SkillCollection(Character chr)
		{
			m_owner = chr;
		}

		public bool CanDualWield
		{
			get { return Contains(SkillId.DualWield); }
		}

		public ItemSubClassMask WeaponProficiency
		{
			get;
			set;
		}

		public ItemSubClassMask ArmorProficiency
		{
			get;
			set;
		}

		/// <summary>
		/// Skill gains up chance formula
		/// </summary>
		/// <param name="targetLevel"></param>
		/// <param name="skill"></param>
		/// <returns></returns>
		private int GetSkillGainChance(int targetLevel, Skill skill)
		{
			int grayLevel = XpGenerator.GetGrayLevel(m_owner.Level);

			// no gain for gray skill levels
			if (targetLevel < grayLevel) return 0;

			var lvlDiff = targetLevel - grayLevel;
			var skillDiff = skill.MaxValue - skill.CurrentValue;

			if (skillDiff <= 0) return 0;

			var chance = 3 * lvlDiff * skillDiff;

			if (chance > 100) chance = 100;

			return chance;

		}


		public void GainWeaponSkill(int targetLevel, IWeapon weapon)
		{
			Skill skill;
			m_skills.TryGetValue(weapon.Skill, out skill);

			if (skill != null)
			{
				int chance = GetSkillGainChance(targetLevel, skill);

				skill.Gain(chance, 1);
			}
		}

		public void GainDefenseSkill(int attackerLevel)
		{
			var skill = m_skills[SkillId.Defense];

			int chance = GetSkillGainChance(attackerLevel, skill);
			if (skill != null)
				skill.Gain(chance, 1);
		}

		public void UpdateSkillsForLevel(int level)
		{
			foreach (var sk in m_skills.Values)
			{
				if (sk.SkillLine.Category == SkillCategory.WeaponProficiency)
				{
					sk.MaxValue = (ushort)(5 * level);
				}
			}
		}

		/// <summary>
		/// If this char is allowed to learn this skill (matching Race, Class and Level) on the given tier, 
		/// the correspdonding SkillLine will be returned. Returns null if skill cannot be learnt.
		/// </summary>
		public SkillLine GetLine(SkillId id, uint tier)
		{
			SkillRaceClassInfo info;
			if (!AvailableSkills.TryGetValue(id, out info) || m_owner.Level < info.MinimumLevel)
			{
				return null;
			}
			if (tier == 0 || (info.SkillLine.Tier.Values.Length >= tier))
			{
				Skill skill;
				if (m_skills.TryGetValue(id, out skill))
				{
					// TODO: Correct tier-value calculation
					uint tierLimit = skill.SkillLine.Tier.Values[tier];
					if (skill.CurrentValue < tierLimit - 15)
					{
						return null;
					}
				}
			}
			return info.SkillLine;
		}

		/// <summary>
		/// Tries to learn the given tier for the given skill (if allowed)
		/// </summary>
		/// <returns>Whether it succeeded</returns>
		public bool TryLearn(SkillId id)
		{
			return TryLearn(id, 0u);
		}

		/// <summary>
		/// Tries to learn the given tier for the given skill (if allowed)
		/// </summary>
		/// <returns>Whether it succeeded</returns>
		public bool TryLearn(SkillId id, uint tier)
		{
			Skill skill;
			if (!m_skills.TryGetValue(id, out skill))
			{
				SkillRaceClassInfo info;
				if (!AvailableSkills.TryGetValue(id, out info) || m_owner.Level < info.MinimumLevel)
				{
					return false;
				}
				skill = Add(info.SkillLine, false);
			}

			if (skill.IsTierActivated(tier))
			{
				skill.MaxValue = (ushort)skill.SkillLine.Tier.Values[tier];
			}
			return true;
		}

		/// <summary>
		/// Returns whether the given skill is known to the player
		/// </summary>
		public bool Contains(SkillId skill)
		{
			return m_skills.ContainsKey(skill);
		}

		/// <summary>
		/// Returns whether the owner has the given amount of the given skill
		/// </summary>
		public bool CheckSkill(SkillId skillId, int amount)
		{
			var skill = this[skillId];
			if (skill == null)
			{
				return false;
			}

			if (amount > 0 && skill.ActualValue < amount)
			{
				return false;
			}
			return true;
		}

		#region Props
		/// <summary>
		/// How many professions this character can learn
		/// </summary>
		public uint FreeProfessions
		{
			get { return m_owner.GetUInt32(PlayerFields.CHARACTER_POINTS2); }
			set { m_owner.SetUInt32(PlayerFields.CHARACTER_POINTS2, value); }
		}

		public Character Owner
		{
			get
			{
				return m_owner;
			}
		}

		public int Count
		{
			get
			{
				return m_skills.Count;
			}
		}

		/// <summary>
		/// Sets or overrides an existing skill
		/// </summary>
		public Skill this[SkillId key]
		{
			get
			{
				Skill skill;
				m_skills.TryGetValue(key, out skill);
				return skill;
			}

			set
			{
				if (m_skills.ContainsKey(key))
				{
					Remove(key);
				}

				Add(value, true);
			}
		}

		/// <summary>
		/// All skills that are available to the owner, restricted by Race/Class.
		/// </summary>
		public Dictionary<SkillId, SkillRaceClassInfo> AvailableSkills
		{
			get
			{
				return SkillHandler.RaceClassInfos[(int)m_owner.Race][(int)m_owner.Class];
			}
		}
		#endregion


		#region Add / Set
		/// <summary>
		/// Adds a new Skill to this SkillCollection if it is not added yet and allowed for this character (or ignoreRestrictions = true)
		/// </summary>
		/// <param name="ignoreRestrictions">Whether to ignore the race, class and level requirements of this skill</param>
		/// <returns>The existing or new skill or null</returns>
		public Skill GetOrCreate(SkillId id, bool ignoreRestrictions)
		{
			Skill skill;
			if (!m_skills.TryGetValue(id, out skill))
			{
				skill = Add(id, ignoreRestrictions);
			}
			return skill;
		}

		public uint GetValue(SkillId id)
		{
			Skill skill;
			if (!m_skills.TryGetValue(id, out skill))
			{
				return 0;
			}
			return skill.ActualValue;
		}

		/// <summary>
		/// Add a new Skill with initial values to this SkillCollection if it can be added
		/// </summary>
		/// <param name="ignoreRestrictions">Whether to ignore the race, class and level requirements of this skill</param>
		public Skill Add(SkillId id, bool ignoreRestrictions)
		{
			SkillLine line = ignoreRestrictions ? SkillHandler.ById.Get((uint)id) : GetLine(id, 0);

			if (line != null)
			{
				return Add(line, ignoreRestrictions);
			}
			return null;
		}

		/// <summary>
		/// Adds and returns the given Skill with initial values
		/// </summary>
		/// <param name="line"></param>
		public Skill Add(SkillLine line, bool ignoreRestrictions)
		{
			return Add(line, line.InitialValue, line.InitialLimit, ignoreRestrictions);
		}

		public Skill GetOrCreate(SkillId id, uint tier, bool ignoreRestrictions)
		{
			Skill skill = GetOrCreate(id, ignoreRestrictions);
			if (skill != null)
			{
				if (skill.SkillLine.HasTier(tier))
				{
					skill.MaxValue = (ushort)skill.SkillLine.Tier.Values[tier];
				}
			}
			return skill;
		}

		public Skill GetOrCreate(SkillId id, uint value, uint max)
		{
			Skill skill = GetOrCreate(id, false);
			if (skill != null)
			{
				skill.CurrentValue = (ushort)value;
				skill.MaxValue = (ushort)max;
			}
			return skill;
		}

		/// <summary>
		/// Adds and returns a skill with max values
		/// </summary>
		public void LearnMax(SkillId id)
		{
			LearnMax(SkillHandler.Get(id));
		}

		public void LearnMax(SkillLine skillLine)
		{
			GetOrCreate(skillLine.Id, (uint)skillLine.Tier.Values.Length - 1, skillLine.MaxValue);
			//Add(skill, 375, 375, instant);
		}

		/// <summary>
		/// Add a new Skill to this SkillCollection if its not a profession or the character still has professions left
		/// </summary>
		public Skill Add(SkillId skill, uint value, uint max, bool ignoreRestrictions)
		{
			return Add(SkillHandler.Get(skill), value, max, ignoreRestrictions);
		}

		/// <summary>
		/// Add a new Skill to this SkillCollection if its not a profession or the character still has professions left (or ignoreRestrictions is true)
		/// </summary>
		public Skill Add(SkillLine skillLine, uint value, uint max, bool ignoreRestrictions)
		{
			if (ignoreRestrictions || skillLine.Category != SkillCategory.Profession || FreeProfessions > 0)
			{
				var skill = CreateNew(skillLine, value, max);

				Add(skill, true);

				if (skillLine.Category == SkillCategory.Profession)
				{
					FreeProfessions--;
				}
				return skill;
			}
			return null;
		}

		/// <summary>
		/// Adds the skill without any checks
		/// </summary>
		protected void Add(Skill skill, bool isNew)
		{
			m_skills.Add(skill.SkillLine.Id, skill);
			ByField.Add(skill.PlayerField, skill);

			if (skill.SkillLine.Category == SkillCategory.Language)
			{
				m_owner.KnownLanguages.Add(skill.SkillLine.Language);
			}
			else if (skill.SkillLine.Category == SkillCategory.ArmorProficiency)
			{
				CharacterHandler.SendProfiency(m_owner, ItemClass.Armor, ArmorProficiency);
			}
			else if (skill.SkillLine.Category == SkillCategory.WeaponProficiency)
			{
				CharacterHandler.SendProfiency(m_owner, ItemClass.Armor, WeaponProficiency);
			}

			if (isNew)
			{
				skill.Push();
				//for (var i = 0; i < skill.SkillLine.InitialAbilities.Count; i++)
				//{
				//    var ability = skill.SkillLine.InitialAbilities[i];
				//    if (!m_owner.Spells.Contains(ability.Spell.Id))
				//    {
				//        m_owner.Spells.Add(ability.Spell);
				//    }
				//}
			}
		}
		#endregion


		#region Remove
		/// <summary>
		/// Removes a skill from this character's SkillCollection
		/// </summary>
		public bool Remove(SkillId id)
		{
			Skill skill;
			if (m_skills.TryGetValue(id, out skill))
			{
				Remove(skill);
				return true;
			}
			return false;
		}

		public void Remove(Skill skill)
		{
			m_skills.Remove(skill.SkillLine.Id);
			OnRemove(skill);
		}

		internal void OnRemove(Skill skill)
		{
			ByField.Remove(skill.PlayerField);

			if (skill.SkillLine.Category == SkillCategory.Profession && FreeProfessions < SkillHandler.MaxProfessionsPerChar)
			{
				FreeProfessions++;
			}

			// reset the skill field
			m_owner.SetUInt32(skill.PlayerField, 0);
			m_owner.SetUInt32(skill.PlayerField + 1, 0);
			m_owner.SetUInt32(skill.PlayerField + 2, 0);

			// remove all skill-related spells
			if (SkillHandler.RemoveAbilitiesWithSkill)
			{
				skill.RemoveAllAbilities();
			}
			skill.Record.DeleteAndFlush();
		}

		/// <summary>
		/// Returns a new Skill object
		/// </summary>
		protected Skill CreateNew(SkillLine skillLine, uint value, uint max)
		{
			return new Skill(this, FindFreeField(), skillLine, value, max);
		}

		/// <summary>
		/// Returns the next free Player's skill-field
		/// </summary>
		public PlayerFields FindFreeField()
		{
			for (PlayerFields i = PlayerFields.SKILL_INFO_1_1; i < SkillHandler.HighestField; i += 3)
			{
				if (m_owner.GetUInt32(i) == 0)
				{
					return i;
				}
			}
			// should never happen
			throw new Exception("No more free skill-fields? Impossible!");
		}
		#endregion


		#region Fill/Empty methods
		/// <summary>
		/// Removes all skills (can also be considered a "reset")
		/// </summary>
		public void Clear()
		{
			foreach (var skill in m_skills.Values)
			{
				OnRemove(skill);
			}
			m_skills.Clear();
		}

		/// <summary>
		/// Adds all skills that are allowed for the owner's race/class combination with max value
		/// </summary>
		/// <param name="learnAbilities"></param>
		public void LearnAll(bool learnAbilities)
		{
			LearnAll(m_owner.Race, m_owner.Class, learnAbilities);
		}

		/// <summary>
		/// Adds all skills of that race/class combination with max value
		/// </summary>
		/// <param name="learnAbilities">Whether to also learn all abilities, related to the given skills.</param>
		public void LearnAll(RaceId race, ClassId clss, bool learnAbilities)
		{
			foreach (var info in SkillHandler.RaceClassInfos[(int)race][(int)clss].Values)
			{
				var skill = GetOrCreate(info.SkillLine.Id, true);
				if (skill != null)
				{
					skill.LearnMax();
					if (learnAbilities)
					{
						skill.LearnAllAbilities();
					}
				}
			}
		}
		#endregion

		public IEnumerator<Skill> GetEnumerator()
		{
			return m_skills.Values.GetEnumerator();
		}

		public void Load()
		{
			uint professions = 0;
			foreach (var record in m_owner.Record.LoadSkills())
			{
				var skillLine = SkillHandler.ById[(ushort)record.SkillId];
				if (skillLine == null)
				{
					log.Warn("Invalid Skill Id '{0}' in SkillRecord '{1}'", record.SkillId, record.Guid);
					// m_owner.Record.RemoveSkill(record);
				}
				else
				{
					if (skillLine.Category == SkillCategory.Profession)
					{
						professions++;
					}

					if (m_skills.ContainsKey(skillLine.Id))
					{
						log.Warn("Character {0} had Skill {1} more than once", m_owner, skillLine);
					}
					else
					{
						var skill = new Skill(this, FindFreeField(), record, skillLine);
						Add(skill, false);
					}
				}
			}
			FreeProfessions = Math.Max(SkillHandler.MaxProfessionsPerChar - professions, 0);
		}

		/*
			int weaponSkill;
			if (attacker is Owner) {
				weaponSkill = ((Owner)attacker).Skills.GetValue(SkillId.Unarmed);
			}
			else {
				weaponSkill = attacker.Level * 5;
			}
		 */
	}
}