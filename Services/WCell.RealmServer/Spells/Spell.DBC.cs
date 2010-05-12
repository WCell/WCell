/*************************************************************************
 *
 *   file		: Spell.DBC.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-04-23 15:13:50 +0200 (fr, 23 apr 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1282 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Spells;
using WCell.Core.DBC;
using WCell.RealmServer.Content;
using WCell.RealmServer.Items;
using WCell.Util;
using WCell.Util.Data;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells
{
	/// <summary>
	/// Represents a Spell (which -in fact- is any kind of effect or action) in WoW.
	/// 
	/// TODO: Spell-Crafting through XML overrides
	/// </summary>
	public partial class Spell
	{
		[NotPersistent]
		public static MappedDBCReader<DurationEntry, DBCDurationConverter> mappeddbcDurationReader;
		[NotPersistent]
		public static MappedDBCReader<float, DBCRadiusConverter> mappeddbcRadiusReader;
		[NotPersistent]
		public static MappedDBCReader<uint, DBCCastTimeConverter> mappeddbcCastTimeReader;
		[NotPersistent]
		public static MappedDBCReader<SimpleRange, DBCRangeConverter> mappeddbcRangeReader;
		[NotPersistent]
		public static MappedDBCReader<string, DBCMechanicConverter> mappeddbcMechanicReader;
        [NotPersistent]
        public static MappedDBCReader<RuneCostEntry, DBCSpellRuneCostConverter> mappeddbcRuneCostReader;

		internal static void InitDbcs()
		{
			mappeddbcDurationReader = new MappedDBCReader<DurationEntry, DBCDurationConverter>(RealmServerConfiguration.GetDBCFile("SpellDuration.dbc"));
			mappeddbcRadiusReader = new MappedDBCReader<float, DBCRadiusConverter>(RealmServerConfiguration.GetDBCFile("SpellRadius.dbc"));
			mappeddbcCastTimeReader = new MappedDBCReader<uint, DBCCastTimeConverter>(RealmServerConfiguration.GetDBCFile("SpellCastTimes.dbc"));
			mappeddbcRangeReader = new MappedDBCReader<SimpleRange, DBCRangeConverter>(RealmServerConfiguration.GetDBCFile("SpellRange.dbc"));
			//DBCMechanicReader = new DBCReader<SpellMechanic, DBCMechanicConverter>(RealmServerConfiguration.GetDBCFile("SpellMechanic.dbc"));
			mappeddbcMechanicReader = new MappedDBCReader<string, DBCMechanicConverter>(RealmServerConfiguration.GetDBCFile("SpellMechanic.dbc"));
		    mappeddbcRuneCostReader = new MappedDBCReader<RuneCostEntry, DBCSpellRuneCostConverter>(RealmServerConfiguration.GetDBCFile("SpellRuneCost.dbc"));
		}

		#region SpellDuration.dbc
		public struct DurationEntry
		{
			public int Min;
			/// <summary>
			/// The amount the duration increases per caster-level
			/// </summary>
			public int LevelDelta;
			public int Max;

			public int Random()
			{
				return Utility.Random(Min, Max);
			}
		}

		public class DBCDurationConverter : AdvancedDBCRecordConverter<DurationEntry>
		{
			public override DurationEntry ConvertTo(byte[] rawData, ref int id)
			{
				var durations = new DurationEntry();

				id = (int)GetUInt32(rawData, 0);
				durations.Min = GetInt32(rawData, 1);
				durations.LevelDelta = GetInt32(rawData, 2);
				durations.Max = GetInt32(rawData, 3);

				return durations;
			}
		}
		#endregion

		#region SpellRadius.dbc
		public class DBCRadiusConverter : AdvancedDBCRecordConverter<float>
		{
			public override float ConvertTo(byte[] rawData, ref int id)
			{
				id = (int)GetUInt32(rawData, 0);
				return GetFloat(rawData, 1);
			}
		}
		#endregion

		#region SpellCastTimes.dbc
		public class DBCCastTimeConverter : AdvancedDBCRecordConverter<uint>
		{
			public override uint ConvertTo(byte[] rawData, ref int id)
			{
				id = (int)GetUInt32(rawData, 0);
				return GetUInt32(rawData, 1);
			}
		}
		#endregion

		#region SpellRange.dbc
		public class DBCRangeConverter : AdvancedDBCRecordConverter<SimpleRange>
		{
			public override SimpleRange ConvertTo(byte[] rawData, ref int id)
			{
				var range = new SimpleRange();

				id = GetInt32(rawData, 0);
				range.MinDist = (uint)GetFloat(rawData, 1);
				// another min ?
				range.MaxDist = (uint)GetFloat(rawData, 3);
				// another max?

				return range;
			}
		}
		#endregion

		#region SpellMechanic.dbc
		public class DBCMechanicConverter : AdvancedDBCRecordConverter<string>
		{
			public override string ConvertTo(byte[] rawData, ref int id)
			{
				id = GetInt32(rawData, 0);
				return GetString(rawData, 1);
			}
		}
		#endregion

		#region SpellFocusObject.dbc
		public struct SpellFocusEntry
		{
			public uint Id;
			public string Name;
		}

		public class DBCSpellFocusConverter : AdvancedDBCRecordConverter<SpellFocusEntry>
		{
			public override SpellFocusEntry ConvertTo(byte[] rawData, ref int id)
			{
			    var entry = new SpellFocusEntry
			                    {
			                        Id = (uint) (id = GetInt32(rawData, 0)),
			                        Name = GetString(rawData, 1)
			                    };

			    return entry;
			}
		}
		#endregion

        #region SpellRuneCost.dbc
        public class RuneCostEntry
        {
            public uint Id;                                                                              
            public uint BloodCost;
            public uint FrostCost;
            public uint UnholyCost;
            public uint PowerGain;                                  
        }

        public class DBCSpellRuneCostConverter : AdvancedDBCRecordConverter<RuneCostEntry>
        {
            public override RuneCostEntry ConvertTo(byte[] rawData, ref int id)
            {
                var entry = new RuneCostEntry
                                {
                                    Id = (uint) (id = GetInt32(rawData, 0)),
                                    BloodCost = (uint) (GetInt32(rawData, 1)),
                                    FrostCost = (uint) (GetInt32(rawData, 2)),
                                    UnholyCost = (uint) (GetInt32(rawData, 3)),
                                    PowerGain = (uint) (GetInt32(rawData, 4))
                                };
                return entry;
            }
        }
        #endregion

		#region Spell.DBC
		public class SpellDBCConverter : DBCRecordConverter
		{
			public override void Convert(byte[] rawData)
			{
				#region Parsing
				int currentIndex = 0;

				var spell = new Spell {
					Id = GetUInt32(rawData, currentIndex++),
					SpellId = (SpellId)GetInt32(rawData, 0)
				};

				try
				{
					spell.Category = GetUInt32(rawData, currentIndex++);                                   // 1
					spell.DispelType = (DispelType)GetUInt32(rawData, currentIndex++);                     // 2
					spell.Mechanic = (SpellMechanic)GetUInt32(rawData, currentIndex++);                    // 3
					spell.Attributes = (SpellAttributes)GetUInt32(rawData, currentIndex++);                // 4
					spell.AttributesEx = (SpellAttributesEx)GetUInt32(rawData, currentIndex++);            // 5
					spell.AttributesExB = (SpellAttributesExB)GetUInt32(rawData, currentIndex++);          // 6
					spell.AttributesExC = (SpellAttributesExC)GetUInt32(rawData, currentIndex++);          // 7
                    spell.AttributesExD = (SpellAttributesExD)GetUInt32(rawData, currentIndex++);          // 8
                    spell.AttributesExE = (SpellAttributesExE)GetUInt32(rawData, currentIndex++);          // 9
                    spell.AttributesExF = (SpellAttributesExF)GetUInt32(rawData, currentIndex++);          // 10
                    spell.ShapeshiftMask = (ShapeShiftMask)GetUInt32(rawData, currentIndex++);             // 11
                    spell.Unk_322_1 = GetUInt32(rawData, currentIndex++);                                  // 12
                    spell.ExcludeShapeshiftMask = (ShapeShiftMask)GetUInt32(rawData, currentIndex++);      // 13
                    spell.Unk_322_2 = GetUInt32(rawData, currentIndex++);                                  // 14
                    spell.TargetFlags = (SpellTargetFlags)GetUInt32(rawData, currentIndex++);              // 15
                    spell.Unk_322_3 = GetUInt32(rawData, currentIndex++);                                  // 16
                    spell.TargetCreatureTypes = (TargetCreatureMask)GetUInt32(rawData, currentIndex++);    // 17
					spell.RequiredSpellFocus = (SpellFocus)GetUInt32(rawData, currentIndex++);              // 18
					spell.FacingFlags = (SpellFacingFlags)GetUInt32(rawData, currentIndex++);               // 19
					spell.RequiredCasterAuraState = (AuraState)GetUInt32(rawData, currentIndex++);          // 20
					spell.RequiredTargetAuraState = (AuraState)GetUInt32(rawData, currentIndex++);          // 21
					spell.ExcludeCasterAuraState = (AuraState)GetUInt32(rawData, currentIndex++);           // 22
					spell.ExcludeTargetAuraState = (AuraState)GetUInt32(rawData, currentIndex++);           // 23
                    spell.RequiredCasterAuraId = GetUInt32(rawData, currentIndex++);                        // 24
					spell.RequiredTargetAuraId = GetUInt32(rawData, currentIndex++);                        // 25
                    spell.ExcludeCasterAuraId = GetUInt32(rawData, currentIndex++);                         // 26
					spell.ExcludeTargetAuraId = GetUInt32(rawData, currentIndex++);                         // 27

					int castTimeIndex = GetInt32(rawData, currentIndex++);                                  // 28
					if (castTimeIndex > 0)
					{
						if (!mappeddbcCastTimeReader.Entries.TryGetValue(castTimeIndex, out spell.CastDelay))
						{
							ContentHandler.OnInvalidClientData("DBC Spell \"{0}\" referred to invalid CastTime-Entry: {1}", spell.Name, castTimeIndex);
						}
					}

					spell.CooldownTime = Math.Max(0, GetInt32(rawData, currentIndex++) - (int)spell.CastDelay);     // 29
					spell.categoryCooldownTime = GetInt32(rawData, currentIndex++);                                 // 30
					spell.InterruptFlags = (InterruptFlags)GetUInt32(rawData, currentIndex++);                      // 31
					spell.AuraInterruptFlags = (AuraInterruptFlags)GetUInt32(rawData, currentIndex++);              // 32
					spell.ChannelInterruptFlags = (ChannelInterruptFlags)GetUInt32(rawData, currentIndex++);        // 33
					spell.ProcTriggerFlags = (ProcTriggerFlags)GetUInt32(rawData, currentIndex++);                  // 34
					spell.ProcChance = GetUInt32(rawData, currentIndex++);                                          // 35
					spell.ProcCharges = GetInt32(rawData, currentIndex++);                                          // 36
                    spell.MaxLevel = GetInt32(rawData, currentIndex++);                                             // 37
                    spell.BaseLevel = GetInt32(rawData, currentIndex++);                                            // 38
                    spell.Level = GetInt32(rawData, currentIndex++);                                                // 30

                    var durationIndex = GetInt32(rawData, currentIndex++);                                          // 40
					if (durationIndex > 0)
					{
						if (!mappeddbcDurationReader.Entries.TryGetValue(durationIndex, out spell.Durations))
						{
							ContentHandler.OnInvalidClientData("DBC Spell \"{0}\" referred to invalid Duration-Entry: {1}", spell.Name, durationIndex);
						}
					}

					spell.PowerType = (PowerType)GetUInt32(rawData, currentIndex++);        // 41
					spell.PowerCost = GetInt32(rawData, currentIndex++);                    // 42
					spell.PowerCostPerlevel = GetInt32(rawData, currentIndex++);            // 43
					spell.PowerPerSecond = GetInt32(rawData, currentIndex++);               // 44
					spell.PowerPerSecondPerLevel = GetInt32(rawData, currentIndex++);       // 45

					var rangeIndex = GetInt32(rawData, currentIndex++);                     // 46
					if (rangeIndex > 0)
					{
						if (!mappeddbcRangeReader.Entries.TryGetValue(rangeIndex, out spell.Range))
						{
							ContentHandler.OnInvalidClientData("DBC Spell \"{0}\" referred to invalid Range-Entry: {1}", spell.Name, rangeIndex);
						}
					}

					spell.ProjectileSpeed = GetFloat(rawData, currentIndex++);              // 47
					spell.ModalNextSpell = (SpellId)GetUInt32(rawData, currentIndex++);     // 48

					spell.MaxStackCount = GetInt32(rawData, currentIndex++);                // 49

					spell.RequiredToolIds = new uint[2];                                    // 50-51
					for (var i = 0; i < spell.RequiredToolIds.Length; i++)
					{
						spell.RequiredToolIds[i] = GetUInt32(rawData, currentIndex++);
					}

					List<ItemStackDescription> reagents = null;
					int reagentStart = currentIndex;
					for (int i = 0; i < 8; i++)                                             //52-59
					{
						ReadReagent(rawData, reagentStart, i, out currentIndex, ref reagents);
					}
					if (reagents != null)
					{
						spell.Reagents = reagents.ToArray();
					}
					else
					{
						spell.Reagents = ItemStackDescription.EmptyArray;
					}
                    spell.RequiredItemClass = (ItemClass)GetUInt32(rawData, currentIndex++);   //68
					if (spell.RequiredItemClass < 0)
					{
						spell.RequiredItemClass = ItemClass.None;
					}

					spell.RequiredItemSubClassMask = (ItemSubClassMask)GetUInt32(rawData, currentIndex++); // 69
					if (spell.RequiredItemSubClassMask < 0)
					{
						spell.RequiredItemSubClassMask = ItemSubClassMask.None;
					}

					spell.RequiredItemInventorySlotMask = (InventorySlotTypeMask)GetUInt32(rawData, currentIndex++); // 70
					if (spell.RequiredItemInventorySlotMask < 0)
					{
						spell.RequiredItemInventorySlotMask = InventorySlotTypeMask.None;
					}

					var effects = new List<SpellEffect>(3);     // 71 - 127
					int effectStart = currentIndex;
					for (int i = 0; i < 3; i++)
					{
						SpellEffect effect = ReadEffect(spell, rawData, effectStart, i, out currentIndex);
						if (effect != null && effect.EffectType != SpellEffectType.None)
						{
							effects.Add(effect);
						}
					}
					spell.Effects = effects.ToArray();

					spell.Visual = GetUInt32(rawData, currentIndex++);              // 128
					spell.Visual2 = GetUInt32(rawData, currentIndex++);             // 129
					spell.SpellbookIconId = GetUInt32(rawData, currentIndex++);     // 130
					spell.BuffIconId = GetUInt32(rawData, currentIndex++);          // 131
					spell.Priority = GetUInt32(rawData, currentIndex++);            // 132

				    spell.Name = GetString(rawData, ref currentIndex);              // 133
				    spell.RankDesc = GetString(rawData, ref currentIndex);          // 124
                    spell.Description = GetString(rawData, ref currentIndex);       // 125
                    spell.BuffDescription = GetString(rawData, ref currentIndex);   // 126

					spell.PowerCostPercentage = GetInt32(rawData, currentIndex++);  // 127
                    spell.StartRecoveryTime = GetInt32(rawData, currentIndex++);    // 128
					spell.StartRecoveryCategory = GetInt32(rawData, currentIndex++);    // 129
					spell.MaxTargetLevel = GetUInt32(rawData, currentIndex++);          // 130
					spell.SpellClassSet = (SpellClassSet)GetUInt32(rawData, currentIndex++);    // 131

					spell.SpellClassMask[0] = GetUInt32(rawData, currentIndex++);   // 132
					spell.SpellClassMask[1] = GetUInt32(rawData, currentIndex++);   // 133
					spell.SpellClassMask[2] = GetUInt32(rawData, currentIndex++);   // 134

					spell.MaxTargets = GetUInt32(rawData, currentIndex++);          // 135
					spell.DefenseType = (SpellDefenseType)GetUInt32(rawData, currentIndex++);   // 136
					spell.PreventionType = (SpellPreventionType)GetUInt32(rawData, currentIndex++); // 137
					spell.StanceBarOrder = GetInt32(rawData, currentIndex++);  // 138

					for (int i = 0; i < spell.DamageMultipliers.Length; i++) // 139-141
					{
						spell.DamageMultipliers[i] = GetFloat(rawData, currentIndex++);
					}

					spell.MinFactionId = GetUInt32(rawData, currentIndex++);        // 142
					spell.MinReputation = GetUInt32(rawData, currentIndex++);       // 143
					spell.RequiredAuraVision = GetUInt32(rawData, currentIndex++);  // 144

					spell.RequiredTotemCategories = new TotemCategory[2];       // 145-146
					for (int i = 0; i < spell.RequiredTotemCategories.Length; i++)
					{
						spell.RequiredTotemCategories[i] = (TotemCategory)GetUInt32(rawData, currentIndex++);
					}

					spell.AreaGroupId = GetUInt32(rawData, currentIndex++);                     // 147
					spell.SchoolMask = (DamageSchoolMask)GetUInt32(rawData, currentIndex++);    // 148

                    spell.RuneCostId = GetUInt32(rawData, currentIndex++);          // 149
					spell.MissileId = GetUInt32(rawData, currentIndex++);           // 150

                    // New 3.1.0. Id from PowerDisplay.dbc
				    spell.PowerDisplayId = GetInt32(rawData, currentIndex++);       // 151

                    // 3.2.2 unk float (array?)
                    spell.Unk_322_4_1 = GetUInt32(rawData, currentIndex++);         // 152
                    spell.Unk_322_4_2 = GetUInt32(rawData, currentIndex++);         // 153
                    spell.Unk_322_4_3 = GetUInt32(rawData, currentIndex++);         // 154

                    // 3.2.2
                    spell.spellDescriptionVariablesID = GetUInt32(rawData, currentIndex++);
				}
				catch (Exception e)
				{
					throw new Exception(string.Format("Unable to parse Spell from DBC file. Index: " + currentIndex), e);
				}
				#endregion

				ArrayUtil.Set(ref SpellHandler.ById, spell.Id, spell);
			}

			private void ReadReagent(byte[] rawData, int reagentStart, int reagentNum, out int currentIndex, ref List<ItemStackDescription> list)
			{
				currentIndex = reagentStart + reagentNum;
				var id = (ItemId)GetUInt32(rawData, currentIndex);
				currentIndex += 8;
				var count = GetInt32(rawData, currentIndex);
				currentIndex += 8 - reagentNum;

				if (id > 0 && count > 0)
				{
					if (list == null)
					{
						list = new List<ItemStackDescription>();
					}
					var reagent = new ItemStackDescription {ItemId = id, Amount = count};
				    list.Add(reagent);
				}
			}

			private SpellEffect ReadEffect(Spell spell, byte[] rawData, int effectStartIndex, int effectNum, out int currentIndex)
			{
				var effect = new SpellEffect(spell, effectNum);

				currentIndex = effectStartIndex + effectNum;

				effect.EffectType = (SpellEffectType)GetUInt32(rawData, currentIndex);  // 71
				currentIndex += 3;

                effect.DiceSides = GetInt32(rawData, currentIndex);                    // 80
                currentIndex += 3;

				effect.RealPointsPerLevel = GetFloat(rawData, currentIndex);            // 77
				currentIndex += 3;

				effect.BasePoints = GetInt32(rawData, currentIndex);                    // 80
				currentIndex += 3;

				effect.Mechanic = (SpellMechanic)GetUInt32(rawData, currentIndex);      // 83
				currentIndex += 3;

				effect.ImplicitTargetA = (ImplicitTargetType)GetUInt32(rawData, currentIndex);      // 86
				currentIndex += 3;

				effect.ImplicitTargetB = (ImplicitTargetType)GetUInt32(rawData, currentIndex);      // 89
				currentIndex += 3;

				// Fix: This is a default AoE effect, thus doesn't have a fact at destination
				if (effect.ImplicitTargetA == ImplicitTargetType.AllEnemiesAroundCaster &&
					effect.ImplicitTargetB == ImplicitTargetType.AllEnemiesInArea)
				{
					effect.ImplicitTargetB = ImplicitTargetType.None;
				}

				int radiusIndex = GetInt32(rawData, currentIndex);                                  // 92
				if (radiusIndex > 0)
				{
					mappeddbcRadiusReader.Entries.TryGetValue(radiusIndex, out effect.Radius);
				}
				//if (effect.Radius < 1) {
				//    effect.Radius = 5;
				//}
				currentIndex += 3;

				effect.AuraType = (AuraType)GetUInt32(rawData, currentIndex);           // 95
				currentIndex += 3;

				effect.Amplitude = GetInt32(rawData, currentIndex);     // 98
				currentIndex += 3;

				effect.ProcValue = GetFloat(rawData, currentIndex);     // 101
				currentIndex += 3;

				effect.ChainTargets = GetInt32(rawData, currentIndex);  // 104
				currentIndex += 3;

				effect.ItemId = GetUInt32(rawData, currentIndex);       // 107
				currentIndex += 3;

				effect.MiscValue = GetInt32(rawData, currentIndex);     // 110
				currentIndex += 3;

				effect.MiscValueB = GetInt32(rawData, currentIndex);    // 113
				currentIndex += 3;

                effect.TriggerSpellId = (SpellId)GetUInt32(rawData, currentIndex);      // 116
                currentIndex += 3;

				effect.PointsPerComboPoint = GetFloat(rawData, currentIndex);       // 119
				currentIndex += 3 - effectNum;


				// since the masks are stored congruently instead of indexed
				currentIndex += effectNum * 3;

				effect.AffectMask[0] = GetUInt32(rawData, currentIndex++);
				effect.AffectMask[1] = GetUInt32(rawData, currentIndex++);
				effect.AffectMask[2] = GetUInt32(rawData, currentIndex++);

				// skip ahead 6 for index 0, 3 for index 1, and 0 for index 2
				currentIndex += (2 - effectNum) * 3;

				return effect;
			}
		}

		#endregion

		public SpellEffect GetAuraEffect(AuraType aura)
		{
			foreach (SpellEffect effect in Effects)
			{
				if (effect.AuraType == aura)
				{
					return effect;
				}
			}
			return null;
		}

		#region Verbose / Debug
		public void PrintEffects(TextWriter writer)
		{
			foreach (SpellEffect effect in Effects)
			{
				effect.DumpInfo(writer, "");
			}
		}
		#endregion
	}
}