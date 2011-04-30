/*************************************************************************
 *
 *   file		: SkillHandler.cs
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
using System.Collections.Generic;
using NLog;
using WCell.Constants;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Core.DBC;
using WCell.Core.Initialization;
using WCell.Core.Network;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Network;
using WCell.RealmServer.Spells;
using WCell.Util;
using WCell.RealmServer.Content;

namespace WCell.RealmServer.Skills
{
	// TODO: Send initial spells for profession skills
	// TODO: Handle improving of skills
	public static partial class SkillHandler
	{
		public const uint MaxSkillId = 2000;

		private static Logger log = LogManager.GetCurrentClassLogger();


		#region Global Skill-Settings
		/// <summary>
		/// The maximum amount of skills allowed (limited by the amount of PlayerUpdateFields for skills)
		/// </summary>
		public const int MaxAmount = 128;
		public const PlayerFields HighestField = PlayerFields.SKILL_INFO_1_1 + (MaxAmount * 3);

		/// <summary>
		/// The max amount of professions that every player may learn (Blizzlike: 2)
		/// </summary>
		public static uint MaxProfessionsPerChar = 2;

		/// <summary>
		/// Whether to automatically remove all spells that belong to a skill when removing it.
		/// </summary>
		public static bool RemoveAbilitiesWithSkill = true;
		#endregion


		#region Skill-Containers
		/// <summary>
		/// All skills, indexed by their id
		/// </summary>
		public static readonly SkillLine[] ById = new SkillLine[MaxSkillId];

		/// <summary>
		/// All lists of all Race/Class-specific skillinfos: Use RaceClassInfos[race][class]
		/// </summary>
		public static readonly Dictionary<SkillId, SkillRaceClassInfo>[][] RaceClassInfos =
			new Dictionary<SkillId, SkillRaceClassInfo>[WCellConstants.RaceTypeLength][];

		/// <summary>
		/// All SkillAbility-lists, indexed by their SkillId
		/// </summary>
		public static readonly SkillAbility[][] AbilitiesBySkill = new SkillAbility[MaxSkillId][];


		//public static readonly Dictionary<SpellId, List<SkillAbility>> AbilitiesBySpellId = new Dictionary<SpellId, List<SkillAbility>>(1000);
		// public static readonly Dictionary<SpellId, SkillLine> AbilitiesBySpellId = new Dictionary<SpellId, SkillLine>(1000);
		#endregion


		#region Init
		static MappedDBCReader<SkillTiers, SkillTierConverter> TierReader;
		static MappedDBCReader<SkillRaceClassInfo, SkillRaceClassInfoConverter> RaceClassReader;

		internal static void Initialize()
		{
            TierReader = new MappedDBCReader<SkillTiers, SkillTierConverter>(RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SKILLTIERS));

			var lineReader =
				new MappedDBCReader<SkillLine, SkillLineConverter>(RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SKILLLINE));

			// make sure that all these skill types have correct tiers
			foreach (var line in lineReader.Entries.Values)
			{
				ById[(uint)line.Id] = line;

				/*if ((line.Category == SkillCategory.WeaponSkill && line.Id != SkillId.DualWield) ||
					line.Category == SkillCategory.ClassSkill ||
					line.Category == SkillCategory.Profession ||
					line.Category == SkillCategory.SecondarySkill)
				{

					line.Tier = new SkillTier(SkillTier.DefaultTier, line.Id);
				}
				else*/
				if (line.Category == SkillCategory.Language)
				{
					var lang = LanguageHandler.GetLanguageDescBySkillType(line.Id);
					if (lang != null)
					{
						line.Language = lang.Language;
					}
				}
			}

			RaceClassReader = new MappedDBCReader<SkillRaceClassInfo, SkillRaceClassInfoConverter>(
                RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SKILLRACECLASSINFO));


		    var abilityReader =
		        new MappedDBCReader<SkillAbility, SkillAbilityConverter>(
		            RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SKILLLINEABILITY));

			var abilityLists = new List<SkillAbility>[MaxSkillId];
			foreach (var ability in abilityReader.Entries.Values)
			{
				// why would the spell be null?
				if (ability.Spell != null)
				{
					// some skills link the same spell multiple times
					//if (!AbilitiesBySpellId.ContainsKey((SpellId)ability.Spell.Id)) {
					//AbilitiesBySpellId.Add((SpellId)ability.Spell.Id, ability.SkillLine);
					//}
					ability.Spell.Ability = ability;
				}
				else
				{
					// Don't know whats wrong here?
                    log.Warn("Skill {0} with ability {1} has null Spell", ability.Skill.Name, ability.AbilityId);
				}

				// create a doubly linked list
				if (ability.NextSpellId > 0)
				{
					var nextSpell = SpellHandler.Get(ability.NextSpellId);
					if (nextSpell != null)
					{
						ability.NextAbility = nextSpell.Ability;
						if (nextSpell.Ability != null)
						{
							nextSpell.Ability.PreviousAbility = ability;
						}
					}
				}

				// initial ability to a skill
				//if (ability.AcquireMethod == SkillAcquireMethod.OnLearningSkill)
				//{
				//    ability.Skill.InitialAbilities.Add(ability);
				//}

				List<SkillAbility> abilities = abilityLists.Get((uint)ability.Skill.Id);

				if (abilities == null)
				{
					abilities = new List<SkillAbility>();

					ArrayUtil.Set(ref abilityLists, (uint)ability.Skill.Id, abilities);
				}

				abilities.Add(ability);
			}

			for (int i = 0; i < abilityLists.Length; i++)
			{
				if (abilityLists[i] != null)
				{
					AbilitiesBySkill[i] = abilityLists[i].ToArray();
				}
			}
		}

		public static void Initialize2()
		{
			// Find Profession Spells
			foreach (var list in AbilitiesBySkill)
			{
				if (list == null)
				{
					continue;
				}
				foreach (var ability in list)
				{
                    if(ability.Spell == null) continue;

					if (ability.Skill.Category == SkillCategory.Profession ||
						ability.Skill.Category == SkillCategory.SecondarySkill)
					{
						if (ability.Spell.HasEffect(SpellEffectType.Skill))
						{
							ability.Skill.TeachingSpells.Add(ability.Spell);
						}
					}
				}
			}

			// Add initial Recipes/Formulas
			foreach (var list in AbilitiesBySkill)
			{
				if (list == null)
				{
					continue;
				}
				foreach (var ability in list)
				{
					if (ability.Skill.Category == SkillCategory.Profession ||
						ability.Skill.Category == SkillCategory.SecondarySkill)
					{
						if (ability.AcquireMethod == SkillAcquireMethod.OnLearningSkill &&
							(ability.Spell.SpellLevels == null || ability.Spell.SpellLevels.BaseLevel == 0) &&
							ability.Spell.Rank == 0)
						{
							var spell = ability.Skill.GetSpellForTier(SkillTierId.Apprentice);
							if (spell != null)
							{
								spell.AdditionallyTaughtSpells.Add(ability.Spell);
							}
						}
					}
				}
			}
		}
		#endregion


		public static SkillLine Get(SkillId id)
		{
			if ((uint)id >= ById.Length)
			{
				return null;
			}
			return ById[(uint)id];
		}

		public static SkillLine Get(uint id)
		{
			if (id >= ById.Length)
			{
				return null;
			}
			return ById[id];
		}

		public static SkillAbility[] GetAbilities(SkillId id)
		{
			if ((uint)id >= AbilitiesBySkill.Length)
			{
				return null;
			}
			return AbilitiesBySkill[(uint)id];
		}

		public static SkillAbility[] GetAbilities(uint id)
		{
			if (id >= AbilitiesBySkill.Length)
			{
				return null;
			}
			return AbilitiesBySkill[id];
		}

		public static SkillAbility GetAbility(SkillId skill, SpellId spell)
		{
			var abilities = GetAbilities(skill);
			return Array.Find(abilities, ability => ability.Spell.SpellId == spell);
		}

		public static SkillId GetSkill(SkinningType skinType)
		{
			switch (skinType)
			{
				case SkinningType.Skinning:
					return SkillId.Skinning;
				case SkinningType.Engineering:
					return SkillId.Engineering;
				case SkinningType.Herbalism:
					return SkillId.Herbalism;
				case SkinningType.Mining:
					return SkillId.Mining;
			}
			return SkillId.Skinning;
		}

		#region Packets
		[ClientPacketHandler(RealmServerOpCode.CMSG_UNLEARN_SKILL)]
		public static void HandleUnlearnSkill(IRealmClient client, RealmPacketIn packet)
		{
			uint skillId = packet.ReadUInt32();

			client.ActiveCharacter.Skills.Remove((SkillId)skillId);
		}
		#endregion
	}
}