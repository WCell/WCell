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
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Core.DBC;
using WCell.RealmServer.Content;
using WCell.RealmServer.Items;
using WCell.Util;
using WCell.Util.Data;
using WCell.RealmServer.Misc;
using WCell.Core;

namespace WCell.RealmServer.Spells
{
	/// <summary>
	/// Represents a Spell (which -in fact- is any kind of effect or action) in WoW.
	/// </summary>
	public partial class Spell
	{
        [NotPersistent]
        public static MappedDBCReader<SpellScaling, DBCSpellScalingConverter> mappeddbcSpellScalingReader;
        [NotPersistent]
        public static MappedDBCReader<SpellShapeshift, DBCSpellShapeshiftConverter> mappeddbcShapeShiftReader;
        [NotPersistent]
        public static MappedDBCReader<SpellAuraOptions, DBCAuraOptionsConverter> mappeddbcAuraOptionsReader;
        [NotPersistent]
        public static MappedDBCReader<SpellAuraRestrictions, DBCAuraRestrictionsConverter> mappeddbcAuraRestrictionsReader;
        [NotPersistent]
        public static MappedDBCReader<SpellCastingRequirements, DBCCastingRequirementsConverter> mappeddbcCastingRequirementsReader;
        [NotPersistent]
        public static MappedDBCReader<SpellCategories, DBCCategoriesConverter> mappeddbcCategoriesReader;
        [NotPersistent]
        public static MappedDBCReader<SpellClassOptions, DBCClassOptionsConverter> mappeddbcClassOptionsReader;
        [NotPersistent]
        public static MappedDBCReader<SpellCooldowns, DBCCooldownsConverter> mappeddbcCooldownsReader;
        [NotPersistent]
        public static MappedDBCReader<SpellEquippedItems, DBCEquippedItemsConverter> mappeddbcEquippedItemsReader;
        [NotPersistent]
        public static MappedDBCReader<SpellInterrupts, DBCSpellInterruptsConverter> mappeddbcInterruptsReader;
        [NotPersistent]
        public static MappedDBCReader<SpellLevels, DBCSpellLevelsConverter> mappeddbcLevelsReader;
        [NotPersistent]
        public static MappedDBCReader<SpellPower, DBCSpellPowerConverter> mappeddbcPowerReader;
        [NotPersistent]
        public static MappedDBCReader<SpellReagents, DBCSpellReagentsConverter> mappeddbcReagentsReader;
        [NotPersistent]
        public static MappedDBCReader<SpellTargetRestrictions, DBCTargetRestrictionsConverter> mappeddbcTargetRestrictionsReader;
        [NotPersistent]
        public static MappedDBCReader<SpellTotems, DBCSpellTotemsConverter> mappeddbcTotemsReader;

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

        [NotPersistent]
        public static MappedDBCReader<SpellEffect, DBCSpellEffectConverter> mappeddbcEffectReader;

		internal static void InitDbcs()
		{
            mappeddbcSpellScalingReader = new MappedDBCReader<SpellScaling, DBCSpellScalingConverter>(RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SPELLSCALING));
            mappeddbcShapeShiftReader = new MappedDBCReader<SpellShapeshift, DBCSpellShapeshiftConverter>(RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SPELLSHAPESHIFT));
            mappeddbcAuraOptionsReader = new MappedDBCReader<SpellAuraOptions, DBCAuraOptionsConverter>(RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SPELLAURAOPTIONS));
            mappeddbcAuraRestrictionsReader = new MappedDBCReader<SpellAuraRestrictions, DBCAuraRestrictionsConverter>(RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SPELLAURARESTRICTIONS));
            mappeddbcCastingRequirementsReader = new MappedDBCReader<SpellCastingRequirements, DBCCastingRequirementsConverter>(RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SPELLCASTINGREQUIREMENTS));
            mappeddbcCategoriesReader = new MappedDBCReader<SpellCategories, DBCCategoriesConverter>(RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SPELLCATEGORIES));
            mappeddbcClassOptionsReader = new MappedDBCReader<SpellClassOptions, DBCClassOptionsConverter>(RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SPELLCLASSOPTIONS));
            mappeddbcCooldownsReader = new MappedDBCReader<SpellCooldowns, DBCCooldownsConverter>(RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SPELLCOOLDOWNS));
            mappeddbcEquippedItemsReader = new MappedDBCReader<SpellEquippedItems, DBCEquippedItemsConverter>(RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SPELLEQUIPPEDITEMS));
            mappeddbcInterruptsReader = new MappedDBCReader<SpellInterrupts, DBCSpellInterruptsConverter>(RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SPELLINTERRUPTS));
            mappeddbcLevelsReader = new MappedDBCReader<SpellLevels, DBCSpellLevelsConverter>(RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SPELLLEVELS));
            mappeddbcPowerReader = new MappedDBCReader<SpellPower, DBCSpellPowerConverter>(RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SPELLPOWER));
            mappeddbcReagentsReader = new MappedDBCReader<SpellReagents, DBCSpellReagentsConverter>(RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SPELLREAGENTS));
            mappeddbcTargetRestrictionsReader = new MappedDBCReader<SpellTargetRestrictions, DBCTargetRestrictionsConverter>(RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SPELLTARGETRESTRICTIONS));
            mappeddbcTotemsReader = new MappedDBCReader<SpellTotems, DBCSpellTotemsConverter>(RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SPELLTOTEMS));
            
			mappeddbcDurationReader = new MappedDBCReader<DurationEntry, DBCDurationConverter>(RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SPELLDURATION));
			mappeddbcRadiusReader = new MappedDBCReader<float, DBCRadiusConverter>(RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SPELLRADIUS));
			mappeddbcCastTimeReader = new MappedDBCReader<uint, DBCCastTimeConverter>(RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SPELLCASTTIMES));
			mappeddbcRangeReader = new MappedDBCReader<SimpleRange, DBCRangeConverter>(RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SPELLRANGE));
			//DBCMechanicReader = new DBCReader<SpellMechanic, DBCMechanicConverter>(RealmServerConfiguration.GetDBCFile(WCellDef.DBC_SPELLMECHANIC));
			mappeddbcMechanicReader = new MappedDBCReader<string, DBCMechanicConverter>(RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SPELLMECHANIC));
			mappeddbcRuneCostReader = new MappedDBCReader<RuneCostEntry, DBCSpellRuneCostConverter>(RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SPELLRUNECOST));

            mappeddbcEffectReader = new MappedDBCReader<SpellEffect, DBCSpellEffectConverter>(RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SPELLEFFECT));
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
				// min range friendly
				range.MaxDist = (uint)GetFloat(rawData, 3);
				// max range friendly
				// flags (uint32)
				// char* ???
				// char* ???

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
									Id = (uint)(id = GetInt32(rawData, 0)),
									Name = GetString(rawData, 1)
								};

				return entry;
			}
		}
		#endregion

		#region SpellRuneCost.dbc
		public class DBCSpellRuneCostConverter : AdvancedDBCRecordConverter<RuneCostEntry>
		{
			public override RuneCostEntry ConvertTo(byte[] rawData, ref int id)
			{
				var entry = new RuneCostEntry
				{
					Id = (uint)(id = GetInt32(rawData, 0)),
					RunicPowerGain = GetInt32(rawData, 4)
				};

				for (var r = 0; r < SpellConstants.StandardRuneTypeCount; r++)
				{
					entry.RequiredRuneAmount += entry.CostPerType[r] = GetInt32(rawData, r+1);
				}

				return entry;
			}
		}
		#endregion

        #region SpellEffect.dbc
        public class DBCSpellEffectConverter : AdvancedDBCRecordConverter<SpellEffect>
        {
            public override SpellEffect ConvertTo(byte[] rawData, ref int id)
			{
                var currentIndex = 0;

                id = (int)GetUInt32(rawData, currentIndex++);
				var effect = new SpellEffect();
                
                effect.EffectType = (SpellEffectType)GetUInt32(rawData, currentIndex++);  // 71

                effect.Amplitude = GetInt32(rawData, currentIndex++);

                effect.AuraType = (AuraType)GetUInt32(rawData, currentIndex++);           // 95

                effect.AuraPeriod = GetInt32(rawData, currentIndex++);

                effect.BasePoints = GetInt32(rawData, currentIndex++);                    // 80

                effect.SpellPowerCoEfficient = GetFloat(rawData, currentIndex++);

                effect.ChainAmplitude = GetFloat(rawData, currentIndex++);     // 101

                effect.ChainTargets = GetInt32(rawData, currentIndex++);  // 104

                effect.DiceSides = GetInt32(rawData, currentIndex++);                    // 80

                effect.ItemId = GetUInt32(rawData, currentIndex++);       // 107

                effect.Mechanic = (SpellMechanic)GetUInt32(rawData, currentIndex++);      // 83

                effect.MiscValue = GetInt32(rawData, currentIndex++);     // 110

                effect.MiscValueB = GetInt32(rawData, currentIndex++);    // 113

                effect.PointsPerComboPoint = GetFloat(rawData, currentIndex++);       // 119

                int radiusIndex = GetInt32(rawData, currentIndex++);                                  // 92
                if (radiusIndex > 0)
                {
                    mappeddbcRadiusReader.Entries.TryGetValue(radiusIndex, out effect.Radius);
                }

                int radiusMaxIndex = GetInt32(rawData, currentIndex++);                                  // 92
                if (radiusMaxIndex > 0)
                {
                    mappeddbcRadiusReader.Entries.TryGetValue(radiusMaxIndex, out effect.RadiusMax);
                }

                effect.RealPointsPerLevel = GetFloat(rawData, currentIndex++);            // 77

                effect.AffectMask[0] = GetUInt32(rawData, currentIndex++);
                effect.AffectMask[1] = GetUInt32(rawData, currentIndex++);
                effect.AffectMask[2] = GetUInt32(rawData, currentIndex++);

                effect.TriggerSpellId = (SpellId)GetUInt32(rawData, currentIndex++);      // 116

                effect.ImplicitTargetA = (ImplicitSpellTargetType)GetUInt32(rawData, currentIndex++);      // 86

                effect.ImplicitTargetB = (ImplicitSpellTargetType)GetUInt32(rawData, currentIndex++);      // 89

				// Fix: This is a default AoE effect, thus doesn't have a fact at destination
				if (effect.ImplicitTargetA == ImplicitSpellTargetType.AllEnemiesAroundCaster &&
					effect.ImplicitTargetB == ImplicitSpellTargetType.AllEnemiesInArea)
				{
					effect.ImplicitTargetB = ImplicitSpellTargetType.None;
				}

                effect.SpellId = (SpellId)GetUInt32(rawData, currentIndex++);

                effect.EffectIndex = GetInt32(rawData, currentIndex);

                SpellEffectsCollection.Add(effect);

				return effect;
			}
        }
        #endregion

        #region SpellAuraOptions.dbc
        public class DBCAuraOptionsConverter : AdvancedDBCRecordConverter<SpellAuraOptions>
        {
            public override SpellAuraOptions ConvertTo(byte[] rawData, ref int id)
            {
                var auraOptions = new SpellAuraOptions();

                id = (int)GetUInt32(rawData, 0);
                auraOptions.MaxStackCount = GetInt32(rawData, 1);
                auraOptions.ProcChance = GetUInt32(rawData, 2);
                auraOptions.ProcCharges = GetInt32(rawData, 3);
                auraOptions.ProcTriggerFlags = (ProcTriggerFlags)GetInt32(rawData, 4);

                return auraOptions;
            }
        }
        #endregion

        #region SpellAuraRestrictions.dbc
        public class DBCAuraRestrictionsConverter : AdvancedDBCRecordConverter<SpellAuraRestrictions>
        {
            public override SpellAuraRestrictions ConvertTo(byte[] rawData, ref int id)
            {
                var auraRestrictions = new SpellAuraRestrictions();

                id = (int)GetUInt32(rawData, 0);
                auraRestrictions.RequiredCasterAuraState = (AuraState)GetUInt32(rawData, 1);
                auraRestrictions.RequiredTargetAuraState = (AuraState)GetUInt32(rawData, 2);
                auraRestrictions.ExcludeCasterAuraState = (AuraState)GetUInt32(rawData, 3);
                auraRestrictions.ExcludeTargetAuraState = (AuraState)GetUInt32(rawData, 4);
                auraRestrictions.RequiredCasterAuraId = (SpellId)GetUInt32(rawData, 5);
                auraRestrictions.RequiredTargetAuraId = (SpellId)GetUInt32(rawData, 6);
                auraRestrictions.ExcludeCasterAuraId = (SpellId)GetUInt32(rawData, 7);
                auraRestrictions.ExcludeTargetAuraId = (SpellId)GetUInt32(rawData, 8);

                return auraRestrictions;
            }
        }
        #endregion

        #region SpellCastingRequirements.dbc
        public class DBCCastingRequirementsConverter : AdvancedDBCRecordConverter<SpellCastingRequirements>
        {
            public override SpellCastingRequirements ConvertTo(byte[] rawData, ref int id)
            {
                var castingRequirements = new SpellCastingRequirements();

                id = (int)GetUInt32(rawData, 0);
                castingRequirements.FacingFlags = (SpellFacingFlags)GetUInt32(rawData, 1);
                castingRequirements.MinFactionId = GetUInt32(rawData, 2);
                castingRequirements.MinReputation = GetUInt32(rawData, 3);
                castingRequirements.AreaGroupId = GetUInt32(rawData, 4);
                castingRequirements.RequiredAuraVision = GetUInt32(rawData, 5);
                castingRequirements.RequiredSpellFocus = (SpellFocus)GetUInt32(rawData, 6);

                return castingRequirements;
            }
        }
        #endregion

        #region SpellCategories.dbc
        public class DBCCategoriesConverter : AdvancedDBCRecordConverter<SpellCategories>
        {
            public override SpellCategories ConvertTo(byte[] rawData, ref int id)
            {
                var spellCategories = new SpellCategories();

                id = (int)GetUInt32(rawData, 0);
                spellCategories.Category = GetUInt32(rawData, 1);
                spellCategories.DefenseType = (SpellDefenseType)GetUInt32(rawData, 2);
                spellCategories.DispelType = (DispelType)GetUInt32(rawData, 3);
                spellCategories.Mechanic = (SpellMechanic)GetUInt32(rawData, 4);
                spellCategories.PreventionType = (SpellPreventionType)GetUInt32(rawData, 5);
                spellCategories.StartRecoveryCategory = GetInt32(rawData, 6);

                return spellCategories;
            }
        }
        #endregion

        #region SpellClassOptions.dbc
        public class DBCClassOptionsConverter : AdvancedDBCRecordConverter<SpellClassOptions>
        {
            public override SpellClassOptions ConvertTo(byte[] rawData, ref int id)
            {
                var spellClassOptions = new SpellClassOptions();

                var currentIndex = 0;
                id = (int)GetUInt32(rawData, currentIndex++);

                spellClassOptions.ModalNextSpell = GetUInt32(rawData, currentIndex++);
                for (var i = 0; i < SpellConstants.SpellClassMaskSize; i++)
                    spellClassOptions.SpellClassMask[i] = GetUInt32(rawData, currentIndex++);

                spellClassOptions.SpellClassSet = (SpellClassSet)GetUInt32(rawData, currentIndex);

                return spellClassOptions;
            }
        }
        #endregion

        #region SpellCooldowns.dbc
        public class DBCCooldownsConverter : AdvancedDBCRecordConverter<SpellCooldowns>
        {
            public override SpellCooldowns ConvertTo(byte[] rawData, ref int id)
            {
                var spellCooldowns = new SpellCooldowns();

                id = (int)GetUInt32(rawData, 0);
                spellCooldowns.CategoryCooldownTime = GetInt32(rawData, 1);
                spellCooldowns.CooldownTime = GetInt32(rawData, 2);
                spellCooldowns.StartRecoveryTime = GetInt32(rawData, 3);

                return spellCooldowns;
            }
        }
        #endregion

        #region SpellEquippedItems.dbc
        public class DBCEquippedItemsConverter : AdvancedDBCRecordConverter<SpellEquippedItems>
        {
            public override SpellEquippedItems ConvertTo(byte[] rawData, ref int id)
            {
                var spellEquippedItems = new SpellEquippedItems();

                id = (int)GetUInt32(rawData, 0);
                spellEquippedItems.RequiredItemClass = (ItemClass)GetInt32(rawData, 1);
                spellEquippedItems.RequiredItemInventorySlotMask = (InventorySlotTypeMask)GetInt32(rawData, 2);
                spellEquippedItems.RequiredItemSubClassMask = (ItemSubClassMask)GetInt32(rawData, 3);

                return spellEquippedItems;
            }
        }
        #endregion

        #region SpellInterrupts.dbc
        public class DBCSpellInterruptsConverter : AdvancedDBCRecordConverter<SpellInterrupts>
        {
            public override SpellInterrupts ConvertTo(byte[] rawData, ref int id)
            {
                var spellInterrupts = new SpellInterrupts();

                id = (int)GetUInt32(rawData, 0);
                spellInterrupts.AuraInterruptFlags = (AuraInterruptFlags)GetUInt64(rawData, 1);
                spellInterrupts.ChannelInterruptFlags = (ChannelInterruptFlags)GetUInt64(rawData, 3);
                spellInterrupts.InterruptFlags = (InterruptFlags)GetInt32(rawData, 5);

                return spellInterrupts;
            }
        }
        #endregion

        #region SpellLevels.dbc
        public class DBCSpellLevelsConverter : AdvancedDBCRecordConverter<SpellLevels>
        {
            public override SpellLevels ConvertTo(byte[] rawData, ref int id)
            {
                var spellLevels = new SpellLevels();

                id = (int)GetUInt32(rawData, 0);
                spellLevels.MaxLevel = GetInt32(rawData, 1);
                spellLevels.BaseLevel = GetInt32(rawData, 2);
                spellLevels.Level = GetInt32(rawData, 3);

                return spellLevels;
            }
        }
        #endregion

        #region SpellPower.dbc
        public class DBCSpellPowerConverter : AdvancedDBCRecordConverter<SpellPower>
        {
            public override SpellPower ConvertTo(byte[] rawData, ref int id)
            {
                var spellPower = new SpellPower();

                id = (int)GetUInt32(rawData, 0);
                spellPower.PowerCost = GetInt32(rawData, 1);
                spellPower.PowerCostPerlevel = GetInt32(rawData, 2);
                spellPower.PowerCostPercentage = GetInt32(rawData, 3);
                spellPower.PowerPerSecond = GetInt32(rawData, 4);
                spellPower.PowerDisplayId = GetInt32(rawData, 5);
                spellPower.unk400 = GetInt32(rawData, 6);

                return spellPower;
            }
        }
        #endregion

        #region SpellReagents.dbc
        public class DBCSpellReagentsConverter : AdvancedDBCRecordConverter<SpellReagents>
        {
            public override SpellReagents ConvertTo(byte[] rawData, ref int id)
            {
                var spellReagents = new SpellReagents();
                var reagents = new List<ItemStackDescription>();
                var currentIndex = 0;
                id = (int)GetUInt32(rawData, currentIndex++);

                for (var i = 0; i < 8; i++)
                {
                    spellReagents.ReagentIds[i] = (ItemId)GetUInt32(rawData, currentIndex++);
                }

                for (var i = 0; i < 8; i++)
                {
                    var count = GetInt32(rawData, currentIndex++);
                    spellReagents.ReagentCounts[i] = count;
                    if (count > 0 && spellReagents.ReagentIds[i] > 0)
                    {
                        reagents.Add(new ItemStackDescription(spellReagents.ReagentIds[i], count));
                    }
                }

                spellReagents.Reagents = reagents.ToArray();

                return spellReagents;
            }
        }
        #endregion

        #region SpellScaling.dbc
        public class DBCSpellScalingConverter : AdvancedDBCRecordConverter<SpellScaling>
        {
            public override SpellScaling ConvertTo(byte[] rawData, ref int id)
            {
                var spellScaling = new SpellScaling();

                var currentIndex = 0;
                id = (int)GetUInt32(rawData, currentIndex++);
                spellScaling.CastTimeMin = GetInt32(rawData, currentIndex++);
                spellScaling.CastTimeMax = GetInt32(rawData, currentIndex++);
                spellScaling.CastTimeDiv = GetInt32(rawData, currentIndex++);
                spellScaling.Class = GetInt32(rawData, currentIndex++);

                for (var i = 0; i < spellScaling.SpellScalingEffects.Length; i++)
                {
                    spellScaling.SpellScalingEffects[i] = new SpellScaling.SpellScalingEffectData { Coefficient = GetFloat(rawData, currentIndex++) };
                }
                for (var i = 0; i < spellScaling.SpellScalingEffects.Length; i++)
                {
                    spellScaling.SpellScalingEffects[i].Delta = GetFloat(rawData, currentIndex++);
                }
                for (var i = 0; i < spellScaling.SpellScalingEffects.Length; i++)
                {
                    spellScaling.SpellScalingEffects[i].ComboPointsCoefficient = GetFloat(rawData, currentIndex++);
                }

                spellScaling.Scaling = GetFloat(rawData, currentIndex++);
                spellScaling.ScalingLevelThreshold = GetFloat(rawData, currentIndex);

                return spellScaling;
            }
        }
        #endregion

        #region SpellShapeshift.dbc
        public class DBCSpellShapeshiftConverter : AdvancedDBCRecordConverter<SpellShapeshift>
        {
            public override SpellShapeshift ConvertTo(byte[] rawData, ref int id)
            {
                var spellShapeshift = new SpellShapeshift();

                id = (int)GetUInt32(rawData, 0);
                spellShapeshift.RequiredShapeshiftMask = (ShapeshiftMask)GetUInt64(rawData, 1);
                spellShapeshift.ExcludeShapeshiftMask = (ShapeshiftMask)GetUInt64(rawData, 3);
                spellShapeshift.StanceBarOrder = GetUInt32(rawData, 5);

                return spellShapeshift;
            }
        }
        #endregion

        #region SpellTargetRestrictions.dbc
        public class DBCTargetRestrictionsConverter : AdvancedDBCRecordConverter<SpellTargetRestrictions>
        {
            public override SpellTargetRestrictions ConvertTo(byte[] rawData, ref int id)
            {
                var spellTargetRestrictions = new SpellTargetRestrictions();

                id = (int)GetUInt32(rawData, 0);
                spellTargetRestrictions.MaxTargets = GetInt32(rawData, 1);
                spellTargetRestrictions.MaxTargetLevel = GetInt32(rawData, 2);
                spellTargetRestrictions.CreatureMask = (CreatureMask)GetUInt32(rawData, 3);
                spellTargetRestrictions.TargetFlags = (SpellTargetFlags)GetUInt32(rawData, 4);

                return spellTargetRestrictions;
            }
        }
        #endregion

        #region SpellTotems.dbc
        public class DBCSpellTotemsConverter : AdvancedDBCRecordConverter<SpellTotems>
        {
            public override SpellTotems ConvertTo(byte[] rawData, ref int id)
            {
                var spellCooldowns = new SpellTotems();

                id = (int)GetUInt32(rawData, 0);
                spellCooldowns.RequiredToolCategories[0] = (ToolCategory)GetUInt32(rawData, 1);
                spellCooldowns.RequiredToolCategories[1] = (ToolCategory)GetUInt32(rawData, 2);
                spellCooldowns.RequiredToolIds[0] = GetUInt32(rawData, 3);
                spellCooldowns.RequiredToolIds[1] = GetUInt32(rawData, 4);

                return spellCooldowns;
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

				var spell = new Spell
				{
					Id = GetUInt32(rawData, currentIndex++),
					SpellId = (SpellId)GetInt32(rawData, 0)
				};

				try
				{
					spell.Attributes = (SpellAttributes)GetUInt32(rawData, currentIndex++);                // 4
					spell.AttributesEx = (SpellAttributesEx)GetUInt32(rawData, currentIndex++);            // 5
					spell.AttributesExB = (SpellAttributesExB)GetUInt32(rawData, currentIndex++);          // 6
					spell.AttributesExC = (SpellAttributesExC)GetUInt32(rawData, currentIndex++);          // 7
                    spell.AttributesExD = (SpellAttributesExD)GetUInt32(rawData, currentIndex++);          // 8
                    spell.AttributesExE = (SpellAttributesExE)GetUInt32(rawData, currentIndex++);          // 9
                    spell.AttributesExF = (SpellAttributesExF)GetUInt32(rawData, currentIndex++);          // 10
                    spell.AttributesExG = (SpellAttributesExG)GetUInt32(rawData, currentIndex++);          // 10
                    spell.AttributesExH = (SpellAttributesExH)GetUInt32(rawData, currentIndex++);          // 10
                    spell.Unk_400_1 = GetUInt32(rawData, currentIndex++);                                  // 12

					int castTimeIndex = GetInt32(rawData, currentIndex++);                                  // 28
					if (castTimeIndex > 0)
					{
						if (!mappeddbcCastTimeReader.Entries.TryGetValue(castTimeIndex, out spell.CastDelay))
						{
							ContentMgr.OnInvalidClientData("DBC Spell \"{0}\" referred to invalid CastTime-Entry: {1}", spell.Name, castTimeIndex);
						}
					}

					var durationIndex = GetInt32(rawData, currentIndex++);                                          // 40
					if (durationIndex > 0)
					{
						if (!mappeddbcDurationReader.Entries.TryGetValue(durationIndex, out spell.Durations))
						{
							ContentMgr.OnInvalidClientData("DBC Spell \"{0}\" referred to invalid Duration-Entry: {1}", spell.Name, durationIndex);
						}
					}

					spell.PowerType = (PowerType)GetUInt32(rawData, currentIndex++);        // 41

					var rangeIndex = GetInt32(rawData, currentIndex++);                     // 46
					if (rangeIndex > 0)
					{
						if (!mappeddbcRangeReader.Entries.TryGetValue(rangeIndex, out spell.Range))
						{
							ContentMgr.OnInvalidClientData("DBC Spell \"{0}\" referred to invalid Range-Entry: {1}", spell.Name, rangeIndex);
						}
					}

					spell.ProjectileSpeed = GetFloat(rawData, currentIndex++);              // 47

					spell.Visual = GetUInt32(rawData, currentIndex++);              // 128
					spell.Visual2 = GetUInt32(rawData, currentIndex++);             // 129
					spell.SpellbookIconId = GetUInt32(rawData, currentIndex++);     // 130
					spell.BuffIconId = GetUInt32(rawData, currentIndex++);          // 131

					spell.Name = GetString(rawData, ref currentIndex);              // 133


                    spell.RankDesc = GetString(rawData, ref currentIndex);          // 124
					spell.Description = GetString(rawData, ref currentIndex);       // 125
					spell.BuffDescription = GetString(rawData, ref currentIndex);   // 126

					spell.SchoolMask = (DamageSchoolMask)GetUInt32(rawData, currentIndex++);  

					var runeCostId = GetInt32(rawData, currentIndex++);
					if (runeCostId != 0)
					{
						mappeddbcRuneCostReader.Entries.TryGetValue(runeCostId, out spell.RuneCostEntry);
					}
					spell.MissileId = GetUInt32(rawData, currentIndex++);       

					spell.spellDescriptionVariablesID = GetUInt32(rawData, currentIndex++);

                    spell.SpellDifficultyId = GetInt32(rawData, currentIndex++);

                    spell.ExtraCoeffiecient = GetFloat(rawData, currentIndex++);

                    spell.SpellScalingId = GetInt32(rawData, currentIndex++); // SpellScaling.dbc
                    if (spell.SpellScalingId != 0)
                    {
                        mappeddbcSpellScalingReader.Entries.TryGetValue(spell.SpellScalingId, out spell.SpellScaling);
                    }

                    spell.SpellAuraOptionsId = GetInt32(rawData, currentIndex++); // SpellAuraOptions.dbc
                    if (spell.SpellAuraOptionsId != 0)
                    {
                        mappeddbcAuraOptionsReader.Entries.TryGetValue(spell.SpellAuraOptionsId, out spell.SpellAuraOptions);
                    }
                    if(spell.SpellAuraOptions == null)
                    {
                        spell.SpellAuraOptions = new SpellAuraOptions();
                    }

                    spell.SpellAuraRestrictionsId = GetInt32(rawData, currentIndex++); // SpellAuraRestrictions.dbc
                    if (spell.SpellAuraRestrictionsId != 0)
                    {
                        mappeddbcAuraRestrictionsReader.Entries.TryGetValue(spell.SpellAuraRestrictionsId, out spell.SpellAuraRestrictions);
                    }

                    spell.SpellCastingRequirementsId = GetInt32(rawData, currentIndex++); // SpellCastingRequirements.dbc
                    if (spell.SpellCastingRequirementsId != 0)
                    {
                        mappeddbcCastingRequirementsReader.Entries.TryGetValue(spell.SpellCastingRequirementsId, out spell.SpellCastingRequirements);
                    }

                    spell.SpellCategoriesId = GetInt32(rawData, currentIndex++); // SpellCategories.dbc
                    if (spell.SpellCategoriesId != 0)
                    {
                        mappeddbcCategoriesReader.Entries.TryGetValue(spell.SpellCategoriesId, out spell.SpellCategories);
                    }
                    if(spell.SpellCategories == null)
                    {
                        spell.SpellCategories = new SpellCategories();
                    }

                    spell.SpellClassOptionsId = GetInt32(rawData, currentIndex++); // SpellClassOptions.dbc
                    if (spell.SpellClassOptionsId != 0)
                    {
                        mappeddbcClassOptionsReader.Entries.TryGetValue(spell.SpellClassOptionsId, out spell.SpellClassOptions);
                    }
                    if(spell.SpellClassOptions == null)
                    {
                        spell.SpellClassOptions = new SpellClassOptions();
                    }

                    spell.SpellCooldownsId = GetInt32(rawData, currentIndex++); // SpellCooldowns.dbc
                    if (spell.SpellCooldownsId != 0)
                    {
                        mappeddbcCooldownsReader.Entries.TryGetValue(spell.SpellCooldownsId, out spell.SpellCooldowns);
                    }

                    spell.UnknownIndex = GetInt32(rawData, currentIndex++);

                    spell.SpellEquippedItemsId = GetInt32(rawData, currentIndex++); // SpellEquippedItems.dbc
                    if (spell.SpellEquippedItemsId != 0)
                    {
                        mappeddbcEquippedItemsReader.Entries.TryGetValue(spell.SpellEquippedItemsId, out spell.SpellEquippedItems);
                    }

                    spell.SpellInterruptsId = GetInt32(rawData, currentIndex++); // SpellInterrupts.dbc
                    if (spell.SpellInterruptsId != 0)
                    {
                        mappeddbcInterruptsReader.Entries.TryGetValue(spell.SpellInterruptsId, out spell.SpellInterrupts);
                    }

                    spell.SpellLevelsId = GetInt32(rawData, currentIndex++); // SpellLevels.dbc
                    if (spell.SpellLevelsId != 0)
                    {
                        mappeddbcLevelsReader.Entries.TryGetValue(spell.SpellLevelsId, out spell.SpellLevels);
                    }

                    spell.SpellPowerId = GetInt32(rawData, currentIndex++); // SpellPower.dbc
                    if (spell.SpellPowerId != 0)
                    {
                        mappeddbcPowerReader.Entries.TryGetValue(spell.SpellPowerId, out spell.SpellPower);
                    }

                    spell.SpellReagentsId = GetInt32(rawData, currentIndex++); // SpellReagents.dbc
                    if (spell.SpellReagentsId != 0)
                    {
                        mappeddbcReagentsReader.Entries.TryGetValue(spell.SpellReagentsId, out spell.SpellReagents);
                    }

                    spell.ShapeShiftId = GetInt32(rawData, currentIndex++); // SpellShapeshift.dbc
                    if (spell.ShapeShiftId != 0)
                    {
                        mappeddbcShapeShiftReader.Entries.TryGetValue(spell.ShapeShiftId, out spell.SpellShapeshift);
                    }

                    spell.SpellTargetRestrictionsId = GetInt32(rawData, currentIndex++); // SpellTargetRestrictions.dbc
                    if (spell.SpellTargetRestrictionsId != 0)
                    {
                        mappeddbcTargetRestrictionsReader.Entries.TryGetValue(spell.SpellTargetRestrictionsId, out spell.SpellTargetRestrictions);
                    }

                    spell.SpellTotemsId = GetInt32(rawData, currentIndex++); // SpellTotems.dbc
                    if (spell.SpellTotemsId != 0)
                    {
                        mappeddbcTotemsReader.Entries.TryGetValue(spell.SpellTotemsId, out spell.SpellTotems);
                    }
                    spell.UnknownIndex2 = GetInt32(rawData, currentIndex++);

                    
                    

                    var effects = new List<SpellEffect>(3);

					for (int i = 0; i < 3; i++)
					{
					    var effect = SpellEffectsCollection.Get(spell.SpellId, (uint)i);
					    
						if (effect != null)
						{
						    effect.Spell = spell;
						    if (effect.EffectType != SpellEffectType.None ||
						        effect.BasePoints > 0 || effect.AuraType != 0 ||
						        effect.TriggerSpellId != 0)
						    {

						        effects.Add(effect);
						    }
						}
					}
					spell.Effects = effects.ToArray();

				}
				catch (Exception e)
				{
					throw new Exception(string.Format("Unable to parse Spell from DBC file. Index: " + currentIndex), e);
				}
				#endregion

				SpellHandler.AddSpell(spell);
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
					var reagent = new ItemStackDescription { ItemId = id, Amount = count };
					list.Add(reagent);
				}
			}
		}

		#endregion

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