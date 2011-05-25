/*************************************************************************
 *
 *   file		: Spell.Fields.cs
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
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Core.ClientDB;
using WCell.RealmServer.Items;
using WCell.Util;
using WCell.Util.Data;
using WCell.RealmServer.Misc;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spells
{
	public partial class Spell
	{
		[NotPersistent]
		public uint Id;//1
		public SpellId SpellId;//1

		public SpellAttributes Attributes;//7
		public SpellAttributesEx AttributesEx;//8
		public SpellAttributesExB AttributesExB;//9
		public SpellAttributesExC AttributesExC;//10
		public SpellAttributesExD AttributesExD;//11
		public SpellAttributesExE AttributesExE;//12
		public SpellAttributesExF AttributesExF;//13
        public SpellAttributesExG AttributesExG;//13
        public SpellAttributesExH AttributesExH;//13

        //4.0.0 unk
        public uint Unk_400_1;

        public int SpellDifficultyId;
        public int SpellScalingId;
        public SpellScaling SpellScaling;

        /// <summary>
        /// 3.2.2 related to description?
        /// </summary>
        public uint spellDescriptionVariablesID;

		/// <summary>
		/// Cast delay in milliseconds
		/// </summary>
		public uint CastDelay;//22

		/// <summary>
		/// SpellDuration.dbc
		/// </summary>
		public int DurationIndex;
		[NotPersistent]
		public DurationEntry Durations;//34
		public PowerType PowerType;//35

		/// <summary>
		/// SpellRange.dbc
		/// </summary>
		public int RangeIndex;
		/// <summary>
		/// Read from SpellRange.dbc
		/// </summary>
		[CanBeNull]
		[NotPersistent]
		public SimpleRange Range;//40
		/// <summary>
		/// The speed of the projectile in yards per second
		/// </summary>
		public float ProjectileSpeed;//41

		/// <summary>
		/// Does not count void effect handlers
		/// </summary>
		[NotPersistent]
		public int EffectHandlerCount;

		[NotPersistent]
		public SpellEffect[] Effects;//65 - 118

		/// <summary>
		/// SpellVisual.dbc
		/// </summary>
		public uint Visual;//119
		/// <summary>
		/// SpellVisual.dbc
		/// </summary>
		public uint Visual2;//120

		/// <summary>
		/// SpellIcon.dbc
		/// </summary>
		public uint SpellbookIconId;//121
		/// <summary>
		/// SpellIcon.dbc
		/// </summary>
		public uint BuffIconId;//122

		public string Name;// 124 - 140
		private string _rankDesc;

		public string RankDesc// 141 - 157
		{
			get { return _rankDesc; }
			set
			{
				_rankDesc = value;

				if (value.Length > 0)
				{
					var rank = numberRegex.Match(value);
					if (rank.Success)
					{
						int.TryParse(rank.Value, out Rank);
					}
				}
			}
		}

		public int Rank;
		public string Description; // 158 - 174
		public string BuffDescription; // 175 - 191

		public DamageSchoolMask SchoolMask;

		/// <summary>
		/// SpellRuneCost.dbc
		/// </summary>
		public RuneCostEntry RuneCostEntry;

		/// <summary>
		/// SpellMissile.dbc
		/// </summary>
		public uint MissileId;

		/// <summary>
		/// PowerDisplay.dbc
		/// </summary>
		/// <remarks>Added in 3.1.0</remarks>
		public int PowerDisplayId;

		[NotPersistent]
		public DamageSchool[] Schools;

	    public int ShapeShiftId;
        [CanBeNull]
        public SpellShapeshift SpellShapeshift;
	    public float ExtraCoeffiecient;

        public int SpellAuraOptionsId;
        [NotNull]
        public SpellAuraOptions SpellAuraOptions;
        public int SpellAuraRestrictionsId;
        [CanBeNull]
        public SpellAuraRestrictions SpellAuraRestrictions;
        public int SpellCastingRequirementsId;
        [CanBeNull]
        public SpellCastingRequirements SpellCastingRequirements;
        public int SpellCategoriesId;
        [NotNull]
        public SpellCategories SpellCategories;
        public int SpellClassOptionsId;
        [NotNull]
        public SpellClassOptions SpellClassOptions;
        public int SpellCooldownsId;
        [CanBeNull]
        public SpellCooldowns SpellCooldowns;
        public int UnknownIndex;
        public int SpellEquippedItemsId;
        [CanBeNull]
        public SpellEquippedItems SpellEquippedItems;
        public int SpellInterruptsId;
        [CanBeNull]
        public SpellInterrupts SpellInterrupts;
        public int SpellLevelsId;
        [CanBeNull]
        public SpellLevels SpellLevels;
        public int SpellPowerId;
        [CanBeNull]
	    public SpellPower SpellPower;
        public int SpellReagentsId;
        [CanBeNull]
	    public SpellReagents SpellReagents;
        public int SpellTargetRestrictionsId;
        [CanBeNull]
        public SpellTargetRestrictions SpellTargetRestrictions;
        public int SpellTotemsId;
        [CanBeNull]
        public SpellTotems SpellTotems;
	    public int UnknownIndex2;

	}

    public sealed class SpellAuraOptions
    {
        public int MaxStackCount;
        public uint ProcChance; //29
        public int ProcCharges; //30
        public ProcTriggerFlags ProcTriggerFlags;
    }

    public sealed class SpellAuraRestrictions
    {
        public AuraState RequiredCasterAuraState;//18
		public AuraState RequiredTargetAuraState;//19
		public AuraState ExcludeCasterAuraState;
		public AuraState ExcludeTargetAuraState;

        /// <summary>
		/// Can only cast if caster has this Aura
		/// Used for some new BG features (Homing missiles etc)
		/// </summary>
		public SpellId RequiredCasterAuraId;
		/// <summary>
		/// Can only cast if target has this Aura
		/// </summary>
		public SpellId RequiredTargetAuraId;
		/// <summary>
		/// Cannot be cast if caster has this
		/// </summary>
		public SpellId ExcludeCasterAuraId;
		/// <summary>
		/// Cannot be cast on target if he has this
		/// </summary>
		public SpellId ExcludeTargetAuraId;
    }

    public sealed class SpellCastingRequirements
    {
        public SpellFacingFlags FacingFlags;

        public uint MinFactionId;                               // 2        m_minFactionID not used
        public uint MinReputation;                              // 3        m_minReputation not used

        /// <summary>
        /// AreaGroup.dbc
        /// </summary>
        public uint AreaGroupId; // 211

        public uint RequiredAuraVision;                         // 5        m_requiredAuraVision not used

        /// <summary>
        /// SpellFocusObject.dbc
        /// </summary>
        public SpellFocus RequiredSpellFocus; //17
    }

    public sealed class SpellCategories
    {
        /// <summary>
		/// SpellCategory.dbc
		/// </summary>
		public uint Category;
        public SpellDefenseType DefenseType;
        /// <summary>
		/// SpellDispelType.dbc
		/// </summary>
		public DispelType DispelType;
        /// <summary>
		/// SpellMechanic.dbc
		/// </summary>
		public SpellMechanic Mechanic;
        public SpellPreventionType PreventionType;
        public int StartRecoveryCategory;
    }

    public sealed class SpellClassOptions
    {
        public uint ModalNextSpell;
        [Persistent(3)]
		public uint[] SpellClassMask = new uint[SpellConstants.SpellClassMaskSize];

        private SpellClassSet spellClassSet;

        ////////////
        //// Custom
        ////////////
        
        public SpellClassSet SpellClassSet
        {
            get { return spellClassSet; }
            set
            {
                spellClassSet = value;
                ClassId = value.ToClassId();
            }
        }

        public ClassId ClassId;
    }

    public sealed class SpellCooldowns
    {
        public int CategoryCooldownTime;
        public int CooldownTime;
        public int StartRecoveryTime;
    }

    public sealed class SpellEquippedItems
    {
        /// <summary>
		/// ItemClass.dbc
		/// </summary>
		public ItemClass RequiredItemClass;//62

		/// <summary>
		/// Mask of InventorySlots, used for Enchants only
		/// </summary>
		public InventorySlotTypeMask RequiredItemInventorySlotMask;//64

        		/// <summary>
		/// Mask of ItemSubClasses, used for Enchants and Combat Abilities
		/// </summary>
		public ItemSubClassMask RequiredItemSubClassMask;//63
    }

    public sealed class SpellInterrupts
    {
		public AuraInterruptFlags AuraInterruptFlags;
		public ChannelInterruptFlags ChannelInterruptFlags;
        public InterruptFlags InterruptFlags;
    }

    public sealed class SpellLevels
    {
		public int MaxLevel;
		public int BaseLevel;
		public int Level;
    }

    public sealed class SpellPower
    {
        public int PowerCost;
        public int PowerCostPerlevel;
        public int PowerCostPercentage;
        public int PowerPerSecond;
        /// <summary>
        /// PowerDisplay.dbc
        /// </summary>
        public int PowerDisplayId;
        public int unk400;

    }

    public sealed class SpellReagents
    {
        public ItemId[] ReagentIds = new ItemId[8]; //[8];
        public int[] ReagentCounts = new int[8]; //[8];

        //Custom fields
        public ItemStackDescription[] Reagents;
    }

    public sealed class SpellScaling
    {
        public int Id;

        /// <summary>
        /// Cast time minimum in ms
        /// </summary>
        public int CastTimeMin;

        /// <summary>
        /// Cast time maximum in ms
        /// </summary>
        public int CastTimeMax;

        /// <summary>
        /// Divisor used in the equation for cast time scaling
        /// First level with maximum cast time
        /// </summary>
        public int CastTimeDiv;

        /// <summary>
        /// Index in GtSpellScaling.dbc
        /// </summary>
        public int Class;

        public SpellScalingEffectData[] SpellScalingEffects = new SpellScalingEffectData[3];

        public float Scaling;

        /// <summary>
        /// Maximum level for scaling equation
        /// </summary>
        public float ScalingLevelThreshold;

        ///////////////
        //// Custom
        ///////////////

        public class SpellScalingEffectData
        {
            /// <summary>
            /// Average coefficient
            /// </summary>
            public float Coefficient;

            /// <summary>
            /// Variance
            /// </summary>
            public float Delta;

            /// <summary>
            /// Combo points coefficient
            /// </summary>
            public float ComboPointsCoefficient;
        }

        

        /// <summary>
        /// Gets the real cast time for this data at the given level.
        /// </summary>
        /// <param name="level">The level of the player casting a spell with this scaling data.</param>
        /// <returns>The appropriate cast time in milliseconds for the given level.</returns>
        public int GetCastTimeForLevel(int level)
        {
            var castTime = (CastTimeMin + ((CastTimeMax - CastTimeMin) / (CastTimeDiv - 1)) * (level - 1));
            if (castTime > CastTimeMax)
                castTime = CastTimeMax;

            return castTime;
        }
    }

    public sealed class SpellShapeshift
    {
        /// <summary>
		/// SpellShapeshiftForm.dbc
		/// </summary>
		public ShapeshiftMask RequiredShapeshiftMask;
		/// <summary>
		/// SpellShapeshiftForm.dbc
		/// </summary>
		public ShapeshiftMask ExcludeShapeshiftMask;

        public uint StanceBarOrder;
    }

    public sealed class SpellTargetRestrictions
    {
        public int MaxTargets;                      //199 
        public int MaxTargetLevel;
        /// <summary>
		/// CreatureType.dbc
		/// </summary>
		public CreatureMask CreatureMask;
        public SpellTargetFlags TargetFlags;
    }

    public sealed class SpellTotems
    {
        public ToolCategory[] RequiredToolCategories = new ToolCategory[2];
        public uint[] RequiredToolIds = new uint[2];
    }
}
