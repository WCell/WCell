/*************************************************************************
 *
 *   file		: Skills.DBC.cs
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

using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.Core;
using WCell.Core.DBC;
using WCell.RealmServer.Spells;
using WCell.Util;

namespace WCell.RealmServer.Skills
{
	public partial class SkillHandler
	{
		#region SkillLine.dbc
		public class SkillLineConverter : AdvancedDBCRecordConverter<SkillLine>
		{
			public override SkillLine ConvertTo(byte[] rawData, ref int id)
			{
				SkillLine skill = new SkillLine();

				int index = 0;
				id = (int)(skill.Id = (SkillId)GetUInt32(rawData, index++));

				skill.Category = (SkillCategory)GetInt32(rawData, index++);
				skill.SkillCostsDataId = GetInt32(rawData, index++);
				//skill.Name = GetString(rawData, 3);
				skill.Name = GetString(rawData, ref index);
				string m_description_langString = GetString(rawData, ref index);
				int spellIconId = GetInt32(rawData, index++);
				string m_alternateVerb_lang = GetString(rawData, ref index);
				int m_canLink = GetInt32(rawData, index);// TODO: this is present on professions and secondary skills

				if (skill.Category == SkillCategory.Profession)
				{
					skill.Abandonable = 1;
				}


				return skill;
			}
		}
		#endregion

		#region SkillLineAbility.dbc
		public class SkillAbilityConverter : AdvancedDBCRecordConverter<SkillAbility>
		{
			public override SkillAbility ConvertTo(byte[] rawData, ref int id)
			{
				var ability = new SkillAbility();
				var index = 0;
				id = (int)(ability.AbilityId = GetUInt32(rawData, index++));	// 0
				ability.Skill = ById[GetUInt32(rawData, index++)];				// 1
				var spellId = (SpellId)GetUInt32(rawData, index++);

				if (spellId > 0)
				{
					Spell spell = SpellHandler.Get(spellId);
					// apparently some abilities are outdated
					if (spell != null)
					{
						ability.Spell = spell;
					}
				}

				ability.RaceMask = (RaceMask)GetUInt32(rawData, index++);
				ability.ClassMask = (ClassMask)GetUInt32(rawData, index++);//4
				var excludeRace = (RaceMask)GetUInt32(rawData, index++);
				var excludeClass = (ClassMask)GetUInt32(rawData, index++);

				int minSkillLineRank = GetInt32(rawData, index++);

				ability.NextSpellId = (SpellId)GetUInt32(rawData, index++);//8

				ability.AcquireMethod = (SkillAcquireMethod)GetInt32(rawData, index++);//9
				ability.GreyValue = GetUInt32(rawData, index++);//10 m_trivialSkillLineRankHigh
				ability.YellowValue = GetUInt32(rawData, index);//11 m_trivialSkillLineRankLow
				// 12 - 13  m_characterPoints[2], but all are 0

				var diff = ability.GreyValue - ability.YellowValue;
				var red = (int)ability.YellowValue - (int)(diff / 2);
				ability.RedValue = red < 0 ? 0 : (uint)red;
				ability.GreenValue = ability.YellowValue + (diff / 2);

				ability.CanGainSkill = ability.GreenValue > 0;

				return ability;
			}
		}
		#endregion

		#region SkillTier.dbc

		public class SkillTierConverter : AdvancedDBCRecordConverter<SkillTiers>
		{
			public override SkillTiers ConvertTo(byte[] rawData, ref int id)
			{
				const int maxTiersPerSkill = 16;

				var tier = new SkillTiers();

				var index = 0;
				id = (int)(tier.Id = GetUInt32(rawData, index++));

				tier.Id = (uint)id;
				var cost = new uint[maxTiersPerSkill];
				var value = new uint[maxTiersPerSkill];

				for (int i = 0; i < maxTiersPerSkill; i++)
				{
					cost[i] = GetUInt32(rawData, index + i);
					value[i] = GetUInt32(rawData, index + i + maxTiersPerSkill);
				}

				tier.MaxValues = value.Where(i => i != 0).ToArray();
				tier.Costs = cost.Take(tier.MaxValues.Length).ToArray();


				//tier.SkillLine = (SkillId)GetUInt32(rawData, currentIndex++);

				/*List<uint> maxVals = new List<uint>(5);
				// 5 values but the 6th is always 0
				for (uint i = 0; i < 6; i++)
				{
					uint maxVal = GetUInt32(rawData, i + 17);
					if (maxVal == 0)
					{
						tier.TierLimits = maxVals.ToArray<uint>();
						break;
					}
					maxVals.Add(maxVal);
				}
                */

				return tier;
			}
		}

		#endregion

		#region SkillRaceClassInfo.dbc

		public class SkillRaceClassInfoConverter : AdvancedDBCRecordConverter<SkillRaceClassInfo>
		{
			public override SkillRaceClassInfo ConvertTo(byte[] rawData, ref int id)
			{
				id = GetInt32(rawData, 0);

				int currentIndex = 0;

				var info = new SkillRaceClassInfo();

				info.Id = GetUInt32(rawData, currentIndex++);

				var skillId = (SkillId)GetUInt32(rawData, currentIndex++);

				info.RaceMask = (RaceMask)GetUInt32(rawData, currentIndex++);
				info.ClassMask = (ClassMask)GetUInt32(rawData, currentIndex++);
				info.Flags = (SkillRaceClassFlags)GetUInt32(rawData, currentIndex++);
				info.MinimumLevel = GetUInt32(rawData, currentIndex++);

				int skillTierId = GetInt32(rawData, currentIndex++);
				if (skillTierId > 0)
				{
					TierReader.Entries.TryGetValue(skillTierId, out info.Tiers);
				}

				info.SkillCostIndex = GetUInt32(rawData, currentIndex);

				// there are outdated skills referenced by this DBC (which do not exist anymore)
				info.SkillLine = ById.Get((uint)skillId);
				if (info.SkillLine != null)
				{
					foreach (var classId in WCellConstants.AllClassIds)
					{
						if (classId >= ClassId.End)
						{
							continue;
						}
						var classMask = classId.ToMask();
						foreach (var raceMask in WCellConstants.RaceTypesByMask.Keys)
						{
							RaceId raceId = WCellConstants.GetRaceType(raceMask);
							if (info.RaceMask.HasAnyFlag(raceMask) && info.ClassMask.HasAnyFlag(classMask))
							{
								Dictionary<SkillId, SkillRaceClassInfo>[] byClass = RaceClassInfos[(int)raceId];
								if (byClass == null)
								{
									RaceClassInfos[(int)raceId] = byClass = new Dictionary<SkillId, SkillRaceClassInfo>[WCellConstants.ClassTypeLength];
								}

								Dictionary<SkillId, SkillRaceClassInfo> infos = byClass[(int)classId];
								if (infos == null)
								{
									byClass[(int)classId] = infos = new Dictionary<SkillId, SkillRaceClassInfo>();
								}

								SkillRaceClassInfo oldInf;
								if (infos.TryGetValue(skillId, out oldInf))
								{
									// double skill entry: Update races and classes, everything else is the same
									info.RaceMask |= oldInf.RaceMask;
									info.ClassMask |= oldInf.ClassMask;
								}
								else
								{
									// we can do this here because SkillTiers are the same for all races/classes
									if (info.SkillLine.Tiers.Id == 0 && info.Tiers.Id != 0)
									{
										info.SkillLine.Tiers = info.Tiers;
									}
								}

								infos[skillId] = info;
							}
						}
					}
				}

				return info;
			}
		}

		#endregion

		#region SkillCostsData.dbc

		public struct SkillCostsData
		{

		}

		public class SkillCostsDataConverter : AdvancedDBCRecordConverter<SkillCostsData>
		{
			public override SkillCostsData ConvertTo(byte[] rawData, ref int id)
			{
				return base.ConvertTo(rawData, ref id);
			}
		}

		#endregion
	}
}