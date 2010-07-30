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
using WCell.RealmServer.Items;
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

		/// <summary>
		/// SpellCategory.dbc
		/// </summary>
		public uint Category;//3

		/// <summary>
		/// SpellDispelType.dbc
		/// </summary>
		public DispelType DispelType;//5
		/// <summary>
		/// SpellMechanic.dbc
		/// </summary>
		public SpellMechanic Mechanic;//6

		public SpellAttributes Attributes;//7
		public SpellAttributesEx AttributesEx;//8
		public SpellAttributesExB AttributesExB;//9
		public SpellAttributesExC AttributesExC;//10
		public SpellAttributesExD AttributesExD;//11
		public SpellAttributesExE AttributesExE;//12
		public SpellAttributesExF AttributesExF;//13

        // 3.2.2 unk
        public uint Unk_322_1;
        public uint Unk_322_2;
        public uint Unk_322_3;
        public float Unk_322_4_1;
        public float Unk_322_4_2;
        public float Unk_322_4_3;

        /// <summary>
        /// 3.2.2 related to description?
        /// </summary>
        public uint spellDescriptionVariablesID;

		/// <summary>
		/// SpellShapeshiftForm.dbc
		/// </summary>
		public ShapeshiftMask RequiredShapeshiftMask;//13
		/// <summary>
		/// SpellShapeshiftForm.dbc
		/// </summary>
		public ShapeshiftMask ExcludeShapeshiftMask;//14

		public SpellTargetFlags TargetFlags;
		/// <summary>
		/// CreatureType.dbc
		/// </summary>
		public CreatureMask CreatureMask;
		/// <summary>
		/// SpellFocusObject.dbc
		/// </summary>
		public SpellFocus RequiredSpellFocus;//17

		public SpellFacingFlags FacingFlags;

		public AuraState RequiredCasterAuraState;//18
		public AuraState RequiredTargetAuraState;//19
		public AuraState ExcludeCasterAuraState;
		public AuraState ExcludeTargetAuraState;

		/// <summary>
		/// Can only cast if caster has this Aura
		/// Used for some new BG features (Homing missiles etc)
		/// </summary>
		public uint RequiredCasterAuraId;
		/// <summary>
		/// Can only cast if target has this Aura
		/// </summary>
		public uint RequiredTargetAuraId;
		/// <summary>
		/// Cannot be cast if caster has this
		/// </summary>
		public uint ExcludeCasterAuraId;
		/// <summary>
		/// Cannot be cast on target if he has this
		/// </summary>
		public uint ExcludeTargetAuraId;

		/// <summary>
		/// Cast delay in milliseconds
		/// </summary>
		public uint CastDelay;//22

		public int CooldownTime;//23
		public int categoryCooldownTime;//24

		public int CategoryCooldownTime
		{
			get { return categoryCooldownTime; }
		}

		public InterruptFlags InterruptFlags;//25
		public AuraInterruptFlags AuraInterruptFlags;//26
		public ChannelInterruptFlags ChannelInterruptFlags;//27
		public ProcTriggerFlags ProcTriggerFlags;//28
		public uint ProcChance;//29
		public int ProcCharges;//30
		public int MaxLevel;//31
		public int BaseLevel;//32
		public int Level;//33
		/// <summary>
		/// SpellDuration.dbc
		/// </summary>
		public int DurationIndex;
		[NotPersistent]
		public DurationEntry Durations;//34
		public PowerType PowerType;//35
		public int PowerCost;//36
		public int PowerCostPerlevel;//37
		public int PowerPerSecond;//38
		/// <summary>
		/// Unused so far
		/// </summary>
		public int PowerPerSecondPerLevel;//39

		/// <summary>
		/// SpellRange.dbc
		/// </summary>
		public int RangeIndex;
		/// <summary>
		/// Read from SpellRange.dbc
		/// </summary>
		[NotPersistent]
		public SimpleRange Range;//40
		/// <summary>
		/// The speed of the projectile in yards per second
		/// </summary>
		public float ProjectileSpeed;//41
		/// <summary>
		/// Hunter ranged spells have this. It seems always to be 75
		/// </summary>
		public SpellId ModalNextSpell;//42
		public int MaxStackCount;//43
		[Persistent(2)]
		public uint[] RequiredToolIds;//44 - 45
		[Persistent(8)]
		public uint[] ReagentIds;
		[Persistent(8)]
		public uint[] ReagentCounts;
		[NotPersistent]
		public ItemStackDescription[] Reagents; // 46 - 61

		/// <summary>
		/// ItemClass.dbc
		/// </summary>
		public ItemClass RequiredItemClass;//62

		/// <summary>
		/// Mask of ItemSubClasses, used for Enchants and Combat Abilities
		/// </summary>
		public ItemSubClassMask RequiredItemSubClassMask;//63

		/// <summary>
		/// Mask of InventorySlots, used for Enchants only
		/// </summary>
		public InventorySlotTypeMask RequiredItemInventorySlotMask;//64

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

		public uint Priority;//123

		public string Name;// 124 - 140
		private string m_RankDesc;

		public string RankDesc// 141 - 157
		{
			get { return m_RankDesc; }
			set
			{
				m_RankDesc = value;

				if (value.Length > 0)
				{
					var rank = numberRegex.Match(value);
					if (rank.Success)
					{
						byte.TryParse(rank.Value, out Rank);
					}
				}
			}
		}

		public byte Rank;
		public string Description; // 158 - 174
		public string BuffDescription; // 175 - 191

		public int PowerCostPercentage;              //192
		/// <summary>
		/// Always 0?
		/// </summary>
		public int StartRecoveryTime;               //194
		public int StartRecoveryCategory;           //195
		public uint MaxTargetLevel;
		private SpellClassSet spellClassSet;		 //196
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

		[Persistent(3)]
		public uint[] SpellClassMask = new uint[SpellConstants.SpellClassMaskSize];
		public uint MaxTargets;                      //199 
		public SpellDefenseType DefenseType;
		public SpellPreventionType PreventionType;
		public int StanceBarOrder;
		/// <summary>
		/// Used for effect-value damping when using chain targets, eg:
		///		DamageMultipliers: 0.6, 1, 1
		///		"Each jump reduces the effectiveness of the heal by 40%.  Heals $x1 total targets."
		/// </summary>
		[Persistent(3)]
		public float[] DamageMultipliers = new float[3];
		/// <summary>
		/// only one spellid:6994 has this value = 369
		/// </summary>
		public uint MinFactionId;
		/// <summary>
		/// only one spellid:6994 has this value = 4
		/// </summary>
		public uint MinReputation;
		/// <summary>
		/// only one spellid:26869  has this flag = 1 
		/// </summary>
		public uint RequiredAuraVision;
		[Persistent(2)]
		public TotemCategory[] RequiredTotemCategories = new TotemCategory[2];// 209 - 210
		/// <summary>
		/// AreaGroup.dbc
		/// </summary>
		public uint AreaGroupId;// 211
		public DamageSchoolMask SchoolMask;

		/// <summary>
		/// SpellRuneCost.dbc
		/// </summary>
		public uint RuneCostId;
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

	}

}