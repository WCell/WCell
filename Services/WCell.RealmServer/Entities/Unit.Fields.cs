/*************************************************************************
 *
 *   file		: Unit.Fields.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-02-17 05:08:19 +0100 (on, 17 feb 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1256 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Constants.Pets;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.NPCs.Vehicles;
using WCell.RealmServer.RacesClasses;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Talents;
using WCell.Util;
using WCell.RealmServer.NPCs;
using WCell.Constants.Items;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Entities
{
	public partial class Unit
	{
		protected internal int[] m_baseStats = new int[5];
		protected int[] m_baseResistances = new int[DamageSchoolCount];
		protected UnitModelInfo m_model;

		internal readonly int[] IntMods = new int[UnitUpdates.FlatIntModCount + 1];
		internal readonly float[] FloatMods = new float[UnitUpdates.MultiplierModCount + 1];
		//internal readonly int[] BaseMods = new int[UnitUpdates.BaseModCount];
		//internal readonly float[] FlatModsFloat = new float[UnitUpdates.FlatFloatModCount];

		protected Unit m_target;
		protected Unit m_charm;
		protected WorldObject m_channeled;

		protected Transport m_transport;
		protected Vector3 m_transportPosition;
		protected float m_transportOrientation;
		protected uint m_transportTime;

		#region Objects
		public Unit Charm
		{
			get
			{
				return m_charm;
			}
			set
			{
				m_charm = value;
				if (value != null)
				{
					SetEntityId(UnitFields.CHARM, value.EntityId);
				}
				else
				{
					SetEntityId(UnitFields.CHARM, EntityId.Zero);
				}
			}
		}

		public Unit Charmer
		{
			get { return m_master; }
			set
			{
				SetEntityId(UnitFields.CHARMEDBY, value != null ? value.EntityId : EntityId.Zero);
				Master = value;
			}
		}

		public bool IsCharmed
		{
			get { return m_master != null; }
		}

		public Unit Summoner
		{
			get { return m_master; }
			set
			{
				SetEntityId(UnitFields.SUMMONEDBY, value != null ? value.EntityId : EntityId.Zero);
				Master = value;
			}
		}

		public EntityId Creator
		{
			get { return GetEntityId(UnitFields.CREATEDBY); }
			set
			{
				SetEntityId(UnitFields.CREATEDBY, value);
			}
		}

		public EntityId Summon
		{
			get { return GetEntityId(UnitFields.SUMMON); }
			set
			{
				SetEntityId(UnitFields.SUMMON, value);
			}
		}

		/// <summary>
		/// The Unit's currently selected target.
		/// If set to null, also forces this Unit to leave combat mode.
		/// </summary>
		public Unit Target
		{
			get
			{
				if (m_target != null && !m_target.IsInWorld)
				{
					// make sure to only return a Target that is in world
					Target = null;
				}
				return m_target;
			}
			set
			{
				if (m_target != value)
				{
					if (value != null)
					{
						SetEntityId(UnitFields.TARGET, value.EntityId);
						if (this is NPC)
						{
							// turn towards it (since thats the way it's seen anyway)
							Orientation = GetAngleTowards(value);
						}
					}
					else
					{
						SetEntityId(UnitFields.TARGET, EntityId.Zero);
						IsFighting = false;
					}
					m_target = value;
					CancelPendingAbility();
				}
			}
		}

		/// <summary>
		/// As long as this count is up, cannot leave combat
		/// </summary>
		public int NPCAttackerCount
		{
			get;
			internal set;
		}

		public WorldObject ChannelObject
		{
			get { return m_channeled; }
			set
			{
				//if (value != null) {

				SetEntityId(UnitFields.CHANNEL_OBJECT, value != null ? value.EntityId : EntityId.Zero);
				//}
				m_channeled = value;
			}
		}
		#endregion

		#region Transport

		public ITransportInfo TransportInfo
		{
			get
			{
				return m_vehicleSeat != null ? (ITransportInfo)m_vehicleSeat.Vehicle : m_transport;
			}
		}

		/// <summary>
		/// The <see cref="Transport"/> that this Unit is on (if any).
		/// </summary>
		public Transport Transport
		{
			get { return m_transport; }
			internal set { m_transport = value; }
		}

		public Vector3 TransportPosition
		{
			get { return m_transportPosition; }
			internal set { m_transportPosition = value; }
		}

		public float TransportOrientation
		{
			get { return m_transportOrientation; }
			internal set { m_transportOrientation = value; }
		}

		public uint TransportTime
		{
			get { return Utility.GetSystemTime() - m_transportTime; }
			internal set { m_transportTime = value; }
		}

		public byte TransportSeat
		{
			get { return VehicleSeat != null ? VehicleSeat.Index : (byte)0; }
		}

		/// <summary>
		/// Currently occupied VehicleSeat (if riding in vehicle)
		/// </summary>
		public VehicleSeat VehicleSeat
		{
			get { return m_vehicleSeat; }
		}

		public Vehicle Vehicle
		{
			get { return m_vehicleSeat != null ? m_vehicleSeat.Vehicle : null; }
		}

		#endregion

		public virtual int MaxLevel
		{
			get { return int.MaxValue; }
			internal set
			{
				// do nothing
			}
		}

		/// <summary>
		/// The Level of this Unit.
		/// </summary>
		public virtual int Level
		{
			get { return GetInt32(UnitFields.LEVEL); }
			set
			{
				SetInt32(UnitFields.LEVEL, value);
				OnLevelChanged();
			}
		}

		protected virtual void OnLevelChanged()
		{
		}

		public override int CasterLevel
		{
			get { return Level; }
		}

		public override Faction Faction
		{
			get { return m_faction; }
			set
			{
				if (value == null)
				{
					throw new NullReferenceException(string.Format("Faction cannot be set to null (Unit: {0}, Map: {1})", this, m_Map));
				}

				m_faction = value;
				SetUInt32(UnitFields.FACTIONTEMPLATE, value.Template.Id);
			}
		}

		public abstract Faction DefaultFaction
		{
			get;
		}

		public override FactionId FactionId
		{
			get
			{
				return m_faction.Id;
			}
			set
			{
				Faction fac = FactionMgr.Get(value);
				if (fac != null)
				{
					Faction = fac;
				}
				// what to do if faction doesn't exist?
			}
		}

		public FactionGroup FactionGroup
		{
			get { return m_faction.Group; }
		}

		public uint FactionTemplateId
		{
			get
			{
				return m_faction.Template.Id;
			}
		}

		public UnitFlags UnitFlags
		{
			get { return (UnitFlags)GetUInt32(UnitFields.FLAGS); }
			set { SetUInt32(UnitFields.FLAGS, (uint)value); }
		}

		public UnitFlags2 UnitFlags2
		{
			get { return (UnitFlags2)GetUInt32(UnitFields.FLAGS_2); }
			set { SetUInt32(UnitFields.FLAGS_2, (uint)value); }
		}

		public float BoundingRadius
		{
			get { return GetFloat(UnitFields.BOUNDINGRADIUS); }
			set
			{
				SetFloat(UnitFields.BOUNDINGRADIUS, value);
			}
		}

		public UnitModelInfo Model
		{
			get { return m_model; }
			set
			{
				m_model = value;
				SetUInt32(UnitFields.DISPLAYID, m_model.DisplayId);

				BoundingRadius = m_model.BoundingRadius * ScaleX;
				CombatReach = m_model.CombatReach * ScaleX;
			}
		}

		#region Display

		public virtual uint DisplayId
		{
			get { return GetUInt32(UnitFields.DISPLAYID); }
			set
			{
				var model = UnitMgr.GetModelInfo(value);
				if (model == null)
				{
					log.Error("Trying to set DisplayId of {0} to an invalid value: {1}", this, value);
					return;
				}
				Model = model;
			}
		}

		public uint NativeDisplayId
		{
			get { return GetUInt32(UnitFields.NATIVEDISPLAYID); }
			set { SetUInt32(UnitFields.NATIVEDISPLAYID, value); }
		}

		public uint MountDisplayId
		{
			get { return GetUInt32(UnitFields.MOUNTDISPLAYID); }
			set { SetUInt32(UnitFields.MOUNTDISPLAYID, value); }
		}

		public ItemId VirtualItem1
		{
			get { return (ItemId)GetUInt32(UnitFields.VIRTUAL_ITEM_SLOT_ID); }
			set { SetUInt32(UnitFields.VIRTUAL_ITEM_SLOT_ID, (uint)value); }
		}

		public ItemId VirtualItem2
		{
			get { return (ItemId)GetUInt32(UnitFields.VIRTUAL_ITEM_SLOT_ID_2); }
			set { SetUInt32(UnitFields.VIRTUAL_ITEM_SLOT_ID_2, (uint)value); }
		}

		public ItemId VirtualItem3
		{
			get { return (ItemId)GetUInt32(UnitFields.VIRTUAL_ITEM_SLOT_ID_3); }
			set { SetUInt32(UnitFields.VIRTUAL_ITEM_SLOT_ID_3, (uint)value); }
		}
		#endregion


		#region Pet Info

		public uint PetNumber
		{
			get { return GetUInt32(UnitFields.PETNUMBER); }
			set { SetUInt32(UnitFields.PETNUMBER, value); }
		}

		/// <summary>
		/// Changing this makes clients send a pet name query
		/// </summary>
		public uint PetNameTimestamp
		{
			get { return GetUInt32(UnitFields.PET_NAME_TIMESTAMP); }
			set { SetUInt32(UnitFields.PET_NAME_TIMESTAMP, value); }
		}

		public int PetExperience
		{
			get { return GetInt32(UnitFields.PETEXPERIENCE); }
			set { SetInt32(UnitFields.PETEXPERIENCE, value); }
		}

		/// <summary>
		/// 
		/// </summary>
		public int NextPetLevelExperience
		{
			get { return GetInt32(UnitFields.PETNEXTLEVELEXP); }
			set { SetInt32(UnitFields.PETNEXTLEVELEXP, value); }
		}

		#endregion

		public UnitDynamicFlags DynamicFlags
		{
			get { return (UnitDynamicFlags)GetUInt32(UnitFields.DYNAMIC_FLAGS); }
			set { SetUInt32(UnitFields.DYNAMIC_FLAGS, (uint)value); }
		}

		public SpellId ChannelSpell
		{
			get { return (SpellId)GetUInt32(UnitFields.CHANNEL_SPELL); }
			set
			{
				SetUInt32(UnitFields.CHANNEL_SPELL, (uint)value);
			}
		}

		public float CastSpeedFactor
		{
			get { return GetFloat(UnitFields.MOD_CAST_SPEED); }
			set { SetFloat(UnitFields.MOD_CAST_SPEED, value); }
		}

		/// <summary>
		/// Whether this Unit is summoned
		/// </summary>
		public bool IsSummoned
		{
			get
			{
				//return SpawnPoint == null;
				return CreationSpellId != 0;
			}
		}

		/// <summary>
		/// Whether this Unit belongs to someone
		/// </summary>
		public bool IsMinion
		{
			get
			{
				return m_master != this;
			}
		}

		/// <summary>
		/// The spell that created this Unit
		/// </summary>
		public SpellId CreationSpellId
		{
			get { return (SpellId)GetUInt32(UnitFields.CREATED_BY_SPELL); }
			set { SetUInt32(UnitFields.CREATED_BY_SPELL, (uint)value); }
		}

		public NPCFlags NPCFlags
		{
			get { return (NPCFlags)GetUInt32(UnitFields.NPC_FLAGS); }
			set
			{
				SetUInt32(UnitFields.NPC_FLAGS, (uint)value);
				// has an influence on dynamic flags
				MarkUpdate(UnitFields.DYNAMIC_FLAGS);
			}
		}

		public EmoteType EmoteState
		{
			get { return (EmoteType)GetUInt32(UnitFields.NPC_EMOTESTATE); }
			set { SetUInt32(UnitFields.NPC_EMOTESTATE, (uint)value); }
		}

		public float HoverHeight
		{
			get { return GetFloat(UnitFields.HOVERHEIGHT); }
			set { SetFloat(UnitFields.HOVERHEIGHT, value); }
		}

		/// <summary>
		/// Pet's Training Points, deprecated
		/// </summary>
		public uint TrainingPoints
		{
			//get { return GetUInt32(UnitFields.TRAINING_POINTS); }
			//set { SetUInt32(UnitFields.TRAINING_POINTS, value); }
			get;
			set;
		}

		#region Base Stats

		public int Strength
		{
			get { return GetInt32(UnitFields.STAT0); }
		}

		public int Agility
		{
			get { return GetInt32(UnitFields.STAT1); }
		}

		public int Stamina
		{
			get { return GetInt32(UnitFields.STAT2); }
		}

		/// <summary>
		/// The amount of stamina that does not contribute to health.
		/// </summary>
		public virtual int StaminaWithoutHealthContribution
		{
			get { return 20; }
		}

		public int Intellect
		{
			get { return GetInt32(UnitFields.STAT3); }
		}

		public int Spirit
		{
			get { return GetInt32(UnitFields.STAT4); }
		}

		internal int[] BaseStats
		{
			get { return m_baseStats; }
		}

		/// <summary>
		/// Stat value, after modifiers
		/// </summary>
		public int GetTotalStatValue(StatType stat)
		{
			return GetInt32(UnitFields.STAT0 + (int)stat);
		}

		public int GetBaseStatValue(StatType stat)
		{
			return m_baseStats[(int)stat];
		}

		public virtual int GetUnmodifiedBaseStatValue(StatType stat)
		{
			return m_baseStats[(int)stat];
		}

		public void SetBaseStat(StatType stat, int value)
		{
			SetBaseStat(stat, value, true);
		}

		public void SetBaseStat(StatType stat, int value, bool update)
		{
			m_baseStats[(int)stat] = value;
			if (update)
			{
				this.UpdateStat(stat);
			}
		}

		public void ModBaseStat(StatType stat, int delta)
		{
			SetBaseStat(stat, (m_baseStats[(int)stat] + delta));
		}

		public void AddStatMod(StatType stat, int delta, bool passive)
		{
			if (passive)
			{
				ModBaseStat(stat, delta);
			}
			else
			{
				AddStatMod(stat, delta);
			}
		}

		public void AddStatMod(StatType stat, int delta)
		{
			UnitFields field;
			if (delta == 0)
			{
				return;
			}

			if (delta > 0)
			{
				field = UnitFields.POSSTAT0;
			}
			else
			{
				field = UnitFields.NEGSTAT0;
			}
			SetInt32(field + (int)stat, GetInt32(field + (int)stat) + delta);

			this.UpdateStat(stat);
		}

		public void RemoveStatMod(StatType stat, int delta, bool passive)
		{
			if (passive)
			{
				ModBaseStat(stat, -delta);
			}
			else
			{
				RemoveStatMod(stat, delta);
			}
		}

		/// <summary>
		/// Removes the given delta from positive or negative stat buffs correspondingly
		/// </summary>
		public void RemoveStatMod(StatType stat, int delta)
		{
			UnitFields field;
			if (delta == 0)
			{
				return;
			}
			if (delta > 0)
			{
				field = UnitFields.POSSTAT0;
			}
			else
			{
				field = UnitFields.NEGSTAT0;
			}
			SetInt32(field + (int)stat, GetInt32(field + (int)stat) - delta);

			this.UpdateStat(stat);
		}
		#endregion

		#region Base Stat Mods

		public int StrengthBuffPositive
		{
			get { return GetInt32(UnitFields.POSSTAT0); }
			set
			{
				SetInt32(UnitFields.POSSTAT0, value);
				this.UpdateStrength();
			}
		}

		public int AgilityBuffPositive
		{
			get { return GetInt32(UnitFields.POSSTAT1); }
			set
			{
				SetInt32(UnitFields.POSSTAT1, value);
				this.UpdateAgility();
			}
		}

		public int StaminaBuffPositive
		{
			get { return GetInt32(UnitFields.POSSTAT2); }
			set
			{
				SetInt32(UnitFields.POSSTAT2, value);
				this.UpdateStamina();
			}
		}

		public int IntellectBuffPositive
		{
			get { return GetInt32(UnitFields.POSSTAT3); }
			set
			{
				SetInt32(UnitFields.POSSTAT3, value);
				this.UpdateIntellect();
			}
		}

		public int SpiritBuffPositive
		{
			get { return GetInt32(UnitFields.POSSTAT4); }
			set
			{
				SetInt32(UnitFields.POSSTAT4, value);
				this.UpdateSpirit();
			}
		}

		public int StrengthBuffNegative
		{
			get { return GetInt32(UnitFields.NEGSTAT0); }
			set
			{
				SetInt32(UnitFields.NEGSTAT0, value);
				this.UpdateStrength();
			}
		}

		public int AgilityBuffNegative
		{
			get { return GetInt32(UnitFields.NEGSTAT1); }
			set
			{
				SetInt32(UnitFields.NEGSTAT1, value);
				this.UpdateAgility();
			}
		}

		public int StaminaBuffNegative
		{
			get { return GetInt32(UnitFields.NEGSTAT2); }
			set
			{
				SetInt32(UnitFields.NEGSTAT2, value);
				this.UpdateStamina();
			}
		}

		public int IntellectBuffNegative
		{
			get { return GetInt32(UnitFields.NEGSTAT3); }
			set
			{
				SetInt32(UnitFields.NEGSTAT3, value);
				this.UpdateIntellect();
			}
		}

		public int SpiritBuffNegative
		{
			get { return GetInt32(UnitFields.NEGSTAT4); }
			set
			{
				SetInt32(UnitFields.NEGSTAT4, value);
				this.UpdateSpirit();
			}
		}

		#endregion

		#region Resistances
		/// <summary>
		/// Physical resist
		/// </summary>
		public int Armor
		{
			get { return GetInt32(UnitFields.RESISTANCES); }
			internal set { SetInt32(UnitFields.RESISTANCES, value); }
		}

		public int HolyResist
		{
			get { return GetInt32(UnitFields.RESISTANCES_2); }
			internal set { SetInt32(UnitFields.RESISTANCES_2, value); }
		}

		public int FireResist
		{
			get { return GetInt32(UnitFields.RESISTANCES_3); }
			internal set { SetInt32(UnitFields.RESISTANCES_3, value); }
		}

		public int NatureResist
		{
			get { return GetInt32(UnitFields.RESISTANCES_4); }
			internal set { SetInt32(UnitFields.RESISTANCES_4, value); }
		}

		public int FrostResist
		{
			get { return GetInt32(UnitFields.RESISTANCES_5); }
			internal set { SetInt32(UnitFields.RESISTANCES_5, value); }
		}

		public int ShadowResist
		{
			get { return GetInt32(UnitFields.RESISTANCES_6); }
			internal set { SetInt32(UnitFields.RESISTANCES_6, value); }
		}

		public int ArcaneResist
		{
			get { return GetInt32(UnitFields.RESISTANCES_7); }
			internal set { SetInt32(UnitFields.RESISTANCES_7, value); }
		}

		internal int[] BaseResistances
		{
			get { return m_baseResistances; }
		}

		/// <summary>
		/// Returns the total resistance-value of the given school
		/// </summary>
		public int GetResistance(DamageSchool school)
		{
			var value = GetBaseResistance(school);
			value += GetInt32(UnitFields.RESISTANCEBUFFMODSPOSITIVE + (int)school);
			value += GetInt32(UnitFields.RESISTANCEBUFFMODSNEGATIVE + (int)school);
			if (value < 0)
			{
				value = 0;
			}
			return value;
		}

		/// <summary>
		/// Returns the base resistance-value of the given school
		/// </summary>
		public int GetBaseResistance(DamageSchool school)
		{
			return m_baseResistances[(int)school];
		}

		public void SetBaseResistance(DamageSchool school, int value)
		{
			if (value < 0)
			{
				value = 0;
			}
			m_baseResistances[(uint)school] = value;
			OnResistanceChanged(school);
		}

		/// <summary>
		/// Adds the given amount to the base of the given resistance for the given school
		/// </summary>
		public void ModBaseResistance(DamageSchool school, int delta)
		{
			SetBaseResistance(school, m_baseResistances[(int)school] + delta);
		}

		/// <summary>
		/// Adds the given amount to the base of all given resistance-schools
		/// </summary>
		public void ModBaseResistance(uint[] schools, int delta)
		{
			foreach (var flag in schools)
			{
				ModBaseResistance((DamageSchool)flag, delta);
			}
		}

		public void AddResistanceBuff(DamageSchool school, int delta)
		{
			UnitFields field;
			if (delta == 0)
			{
				return;
			}
			if (delta > 0)
			{
				field = UnitFields.RESISTANCEBUFFMODSPOSITIVE;
				//ModBaseResistance(school, delta);
			}
			else
			{
				field = UnitFields.RESISTANCEBUFFMODSNEGATIVE;
				//ModBaseResistance(school, -delta);
			}
			SetInt32(field + (int)school, GetInt32(field + (int)school) + delta);
			OnResistanceChanged(school);
		}

		/// <summary>
		/// Removes the given delta from positive or negative stat buffs correspondingly
		/// </summary>
		public void RemoveResistanceBuff(DamageSchool school, int delta)
		{
			UnitFields field;
			if (delta == 0)
			{
				return;
			}
			if (delta > 0)
			{
				field = UnitFields.RESISTANCEBUFFMODSPOSITIVE;
			}
			else
			{
				field = UnitFields.RESISTANCEBUFFMODSNEGATIVE;
				//ModBaseResistance(school, delta);
			}
			SetInt32(field + (int)school, GetInt32(field + (int)school) - delta);
			OnResistanceChanged(school);
		}

		protected virtual void OnResistanceChanged(DamageSchool school)
		{
			SetInt32(UnitFields.RESISTANCES + (int)school, GetBaseResistance(school) + GetResistanceBuffPositive(school) - GetResistanceBuffNegative(school));
		}

		public int GetResistanceBuffPositive(DamageSchool school)
		{
			return GetInt32(UnitFields.RESISTANCEBUFFMODSPOSITIVE + (int)school);
		}

		public int GetResistanceBuffNegative(DamageSchool school)
		{
			return GetInt32(UnitFields.RESISTANCEBUFFMODSNEGATIVE + (int)school);
		}

		public int ArmorBuffPositive
		{
			get { return GetInt32(UnitFields.RESISTANCEBUFFMODSPOSITIVE); }
		}

		public int HolyResistBuffPositive
		{
			get { return GetInt32(UnitFields.RESISTANCEBUFFMODSPOSITIVE + 1); }
		}

		public int FireResistBuffPositive
		{
			get { return GetInt32(UnitFields.RESISTANCEBUFFMODSPOSITIVE + 2); }
		}

		public int NatureResistBuffPositive
		{
			get { return GetInt32(UnitFields.RESISTANCEBUFFMODSPOSITIVE + 3); }
		}

		public int FrostResistBuffPositive
		{
			get { return GetInt32(UnitFields.RESISTANCEBUFFMODSPOSITIVE + 4); }
		}

		public int ShadowResistBuffPositive
		{
			get { return GetInt32(UnitFields.RESISTANCEBUFFMODSPOSITIVE + 5); }
		}

		public int ArcaneResistBuffPositive
		{
			get { return GetInt32(UnitFields.RESISTANCEBUFFMODSPOSITIVE + 6); }
		}

		public int ArmorBuffNegative
		{
			get { return GetInt32(UnitFields.RESISTANCEBUFFMODSNEGATIVE); }
		}

		public int HolyResistBuffNegative
		{
			get { return GetInt32(UnitFields.RESISTANCEBUFFMODSNEGATIVE + 1); }
		}

		public int FireResistBuffNegative
		{
			get { return GetInt32(UnitFields.RESISTANCEBUFFMODSNEGATIVE + 2); }
		}

		public int NatureResistBuffNegative
		{
			get { return GetInt32(UnitFields.RESISTANCEBUFFMODSNEGATIVE + 3); }
		}

		public int FrostResistBuffNegative
		{
			get { return GetInt32(UnitFields.RESISTANCEBUFFMODSNEGATIVE + 4); }
		}

		public int ShadowResistBuffNegative
		{
			get { return GetInt32(UnitFields.RESISTANCEBUFFMODSNEGATIVE + 5); }
		}

		public int ArcaneResistBuffNegative
		{
			get { return GetInt32(UnitFields.RESISTANCEBUFFMODSNEGATIVE + 6); }
		}
		#endregion

		public AuraStateMask AuraState
		{
			get { return (AuraStateMask)GetUInt32(UnitFields.AURASTATE); }
			set
			{
				SetUInt32(UnitFields.AURASTATE, (uint)value);
				if (m_auras is PlayerAuraCollection && AuraState != value)
				{
					((PlayerAuraCollection)m_auras).OnAuraStateChanged();
				}
			}
		}
		/// <summary>
		/// Helper function for Aurastate related fix and Conflagrate spell.
		/// see UpdateFieldHandler/Warlockfixes
		/// </summary>
		public SpellId GetStrongestImmolate()
		{
			var immolate = this.Auras[SpellLineId.WarlockImmolate];
			var shadowflamerank1 = this.Auras[SpellId.Shadowflame_3];
			var shadowflamerank2 = this.Auras[SpellId.Shadowflame_5];
			if (immolate != null)
			{
				return immolate.Spell.SpellId;
			}
			else if (shadowflamerank2 != null)
			{
				return shadowflamerank2.Spell.SpellId;
			}
			else if (shadowflamerank1 != null)
			{
				return shadowflamerank1.Spell.SpellId;
			}
			return SpellId.None;
		}

		#region UNIT_FIELD_BYTES_0

		public byte[] UnitBytes0
		{
			get { return GetByteArray(UnitFields.BYTES_0); }
			set { SetByteArray(UnitFields.BYTES_0, value); }
		}

		public virtual RaceId Race
		{
			get { return (RaceId)GetByte(UnitFields.BYTES_0, 0); }
			set { SetByte(UnitFields.BYTES_0, 0, (byte)value); }
		}

		public virtual ClassId Class
		{
			get { return (ClassId)GetByte(UnitFields.BYTES_0, 1); }
			set { SetByte(UnitFields.BYTES_0, 1, (byte)value); }
		}

		public BaseClass GetBaseClass()
		{
			return ArchetypeMgr.GetClass(Class);
		}

		/// <summary>
		/// Race of the character.
		/// </summary>
		public RaceMask RaceMask
		{
			get { return (RaceMask)(1 << ((int)Race - 1)); }
		}

		/// <summary>
		/// RaceMask2 of the character.
		/// </summary>
		public RaceMask2 RaceMask2
		{
			get { return (RaceMask2)(1 << ((int)Race)); }
		}

		/// <summary>
		/// Class of the character.
		/// </summary>
		public ClassMask ClassMask
		{
			get { return (ClassMask)(1 << ((int)Class - 1)); }
		}

		/// <summary>
		/// ClassMask2 of the character.
		/// </summary>
		public ClassMask2 ClassMask2
		{
			get { return (ClassMask2)(1 << ((int)Class)); }
		}

		public virtual GenderType Gender
		{
			get { return (GenderType)GetByte(UnitFields.BYTES_0, 2); }
			set { SetByte(UnitFields.BYTES_0, 2, (byte)value); }
		}

		/// <summary>
		/// Make sure the PowerType is valid or it will crash the client
		/// </summary>
		public virtual PowerType PowerType
		{
			get { return (PowerType)GetByte(UnitFields.BYTES_0, 3); }
			set
			{
				SetByte(UnitFields.BYTES_0, 3, (byte)((byte)value % (byte)PowerType.End));
			}
		}

		#endregion

		#region UNIT_FIELD_BYTES_1

		public byte[] UnitBytes1
		{
			get { return GetByteArray(UnitFields.BYTES_1); }
			set { SetByteArray(UnitFields.BYTES_1, value); }
		}

		public virtual StandState StandState
		{
			get { return (StandState)GetByte(UnitFields.BYTES_1, 0); }
			set { SetByte(UnitFields.BYTES_1, 0, (byte)value); }
		}

		public StateFlag StateFlags
		{
			get { return (StateFlag)GetByte(UnitFields.BYTES_1, 2); }
			set { SetByte(UnitFields.BYTES_1, 2, (byte)value); }
		}

		public byte UnitBytes1_3
		{
			get { return GetByte(UnitFields.BYTES_1, 3); }
			set { SetByte(UnitFields.BYTES_1, 3, value); }
		}

		#endregion

		#region UNIT_FIELD_BYTES_2

		public byte[] UnitBytes2
		{
			get { return GetByteArray(UnitFields.BYTES_2); }
			set { SetByteArray(UnitFields.BYTES_2, value); }
		}

		/// <summary>
		/// Set to 0x01 for Spirit Healers, Totems (?)
		/// </summary>
		public SheathType SheathType
		{
			get { return (SheathType)GetByte(UnitFields.BYTES_2, NPCConstants.SheathTypeIndex); }
			set { SetByte(UnitFields.BYTES_2, NPCConstants.SheathTypeIndex, (byte)value); }
		}

		/// <summary>
		/// Flags
		/// 0x1 - In PVP
		/// 0x4 - Free for all PVP
		/// 0x8 - In PVP Sanctuary
		/// </summary>
		public PvPState PvPState
		{
			get { return (PvPState)GetByte(UnitFields.BYTES_2, NPCConstants.PvpStateIndex); }
			set { SetByte(UnitFields.BYTES_2, NPCConstants.PvpStateIndex, (byte)value); }
		}

		/// <summary>
		/// </summary>
		public PetState PetState
		{
			get { return (PetState)GetByte(UnitFields.BYTES_2, 2); }
			set { SetByte(UnitFields.BYTES_2, 2, (byte)value); }
		}

		#endregion

		#region Shapeshifting
		/// <summary>
		/// The entry of the current shapeshift form
		/// </summary>
		public ShapeshiftEntry ShapeshiftEntry
		{
			get { return SpellHandler.ShapeshiftEntries.Get((uint)ShapeshiftForm); }
		}

		public ShapeshiftForm ShapeshiftForm
		{
			get { return (ShapeshiftForm)GetByte(UnitFields.BYTES_2, 3); }
			set
			{
				// TODO: Shapeshifters dont use their weapons
				// TODO: AttackTime is overridden
				// TODO: Horde shapeshifters are missing some models

				var oldForm = ShapeshiftForm;
				if (oldForm != 0)
				{
					var oldEntry = SpellHandler.ShapeshiftEntries.Get((uint)value);
					if (oldEntry != null)
					{
						// remove old shapeshift spells
						if (HasSpells)
						{
							foreach (var spell in oldEntry.DefaultActionBarSpells)
							{
								if (spell != 0)
								{
									Spells.Remove(spell);
								}
							}
						}
					}
				}


				var entry = SpellHandler.ShapeshiftEntries.Get((uint)value);
				if (entry != null)
				{
					var model = FactionGroup == FactionGroup.Horde && entry.ModelIdHorde != 0 ? entry.ModelHorde : entry.ModelAlliance;
					if (model != null)
					{
						Model = model;
					}

					if (IsPlayer)
					{
						foreach (var spell in entry.DefaultActionBarSpells)
						{
							if (spell != 0)
							{
								Spells.AddSpell(spell);
							}
						}
					}
					if (entry.PowerType != PowerType.End)
					{
						PowerType = entry.PowerType;
					}
					else
					{
						SetDefaultPowerType();
					}
				}
				else
				{
					if (oldForm != 0)
					{
						// reset Model
						DisplayId = NativeDisplayId;
					}
					SetDefaultPowerType();
				}

				SetByte(UnitFields.BYTES_2, 3, (byte)value);

				if (m_auras is PlayerAuraCollection)
				{
					((PlayerAuraCollection)m_auras).OnShapeshiftFormChanged();
				}
			}
		}

		/// <summary>
		/// Sets this Unit's default PowerType
		/// </summary>
		public void SetDefaultPowerType()
		{
			var clss = GetBaseClass();
			if (clss != null)
			{
				PowerType = clss.DefaultPowerType;
			}
			else
			{
				PowerType = PowerType.Mana;
			}
		}

		public ShapeshiftMask ShapeshiftMask
		{
			get { return (ShapeshiftMask)(1 << (int)(ShapeshiftForm - 1)); }
		}
		#endregion

		/// <summary>
		/// Resets health, Power and Auras
		/// </summary>
		public void Cleanse()
		{
			foreach (var aura in m_auras)
			{
				if (aura.CasterUnit != this)
				{
					aura.Remove(true);
				}
			}
			Health = MaxHealth;
			Power = BasePower;
		}

		/// <summary>
		/// Whether this is actively controlled by a player. 
		/// Not to be confused with IsOwnedByPlayer.
		/// </summary>
		public override bool IsPlayerControlled
		{
			get
			{
				return UnitFlags.HasAnyFlag(UnitFlags.PlayerControlled);
			}
		}

		/// <summary>
		/// If this is not an Honorless Target
		/// </summary>
		public bool YieldsXpOrHonor
		{
			get;
			set;
		}

		public UnitExtraFlags ExtraFlags
		{
			get;
			set;
		}

		#region Health

		public void Kill()
		{
			Kill(null);
		}

		public void Kill(Unit killer)
		{
			if (killer != null)
			{
				if (FirstAttacker == null)
				{
					FirstAttacker = killer;
				}
				LastKiller = killer;
			}
			Health = 0;
		}

		/// <summary>
		/// This Unit's current Health. 
		/// Health cannot exceed MaxHealth.
		/// If Health reaches 0, the Unit dies.
		/// If Health is 0 and increases, the Unit gets resurrected.
		/// </summary>
		public virtual int Health
		{
			get { return (int)GetUInt32(UnitFields.HEALTH); }
			set
			{
				var oldHealth = Health;
				var maxHealth = MaxHealth;

				if (value >= maxHealth)
				{
					value = maxHealth;
				}
				else if (value < 0)
				{
					value = 0;
				}

				if (value != oldHealth)
				{
					if (value < 1)
					{
						Die(false);
					}
					else
					{
						SetUInt32(UnitFields.HEALTH, (uint)value);
						UpdateHealthAuraState();


						if (oldHealth == 0)
						{
							// Unit didn't repop yet -> Also make sure he/she is unrooted
							DecMechanicCount(SpellMechanic.Rooted);
						}

						if (!IsAlive || oldHealth < 1)
						{
							// we are getting resurrected
							if (m_auras.GhostAura != null)
							{
								m_auras.GhostAura.Remove(false);
							}
							else
							{
								OnResurrect();
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Base maximum health, before modifiers.
		/// </summary>
		public int BaseHealth
		{
			get { return GetInt32(UnitFields.BASE_HEALTH); }
			set
			{
				SetInt32(UnitFields.BASE_HEALTH, value);
				this.UpdateMaxHealth();
			}
		}

		/// <summary>
		/// Total maximum Health of this Unit. 
		/// In order to change this value, set BaseHealth.
		/// </summary>
		public virtual int MaxHealth
		{
			get { return GetInt32(UnitFields.MAXHEALTH); }
			internal set
			{
				SetInt32(UnitFields.MAXHEALTH, value);
			}
		}

		private int m_maxHealthModFlat;

		public int MaxHealthModFlat
		{
			get { return m_maxHealthModFlat; }
			set
			{
				m_maxHealthModFlat = value;
				this.UpdateMaxHealth();
			}
		}

		public float MaxHealthModScalar
		{
			get { return GetFloat(UnitFields.MAXHEALTHMODIFIER); }
			set
			{
				SetFloat(UnitFields.MAXHEALTHMODIFIER, value);
				this.UpdateMaxHealth();
			}
		}

		/// <summary>
		/// Current amount of health in percent
		/// </summary>
		public int HealthPct
		{
			get
			{
				var max = MaxHealth;
				return (100 * Health + (max >> 1)) / max;
			}
			set { Health = ((value * MaxHealth) + 50) / 100; }
		}

		/// <summary>
		/// </summary>
		protected void UpdateHealthAuraState()
		{
			// set health state
			var pct = HealthPct;

			if (pct <= 20)
			{
				AuraState = (AuraState & ~(AuraStateMask.HealthAbove75Pct | AuraStateMask.Health35Percent)) |
					AuraStateMask.Health20Percent;
			}
			else if (pct <= 35)
			{
				AuraState = (AuraState & ~(AuraStateMask.HealthAbove75Pct | AuraStateMask.Health20Percent)) |
					AuraStateMask.Health35Percent;
			}
			else if (pct <= 75)
			{
				AuraState = (AuraState & ~(AuraStateMask.Health35Percent | AuraStateMask.Health20Percent | AuraStateMask.HealthAbove75Pct));
			}
			else
			{
				AuraState = (AuraState & ~(AuraStateMask.Health35Percent | AuraStateMask.Health20Percent)) |
							AuraStateMask.HealthAbove75Pct;
			}
		}
		#endregion

		#region Power

		/// <summary>
		/// The flat PowerCostModifier for your default Power
		/// </summary>
		public int PowerCostModifier
		{
			get { return GetInt32(UnitFields.POWER_COST_MODIFIER + (int)PowerType); }
			internal set { SetInt32(UnitFields.POWER_COST_MODIFIER + (int)PowerType, value); }
		}

		/// <summary>
		/// The PowerCostMultiplier for your default Power
		/// </summary>
		public float PowerCostMultiplier
		{
			get { return GetFloat(UnitFields.POWER_COST_MULTIPLIER + (int)PowerType); }
			internal set { SetFloat(UnitFields.POWER_COST_MULTIPLIER + (int)PowerType, value); }
		}

		/// <summary>
		/// Base maximum power, before modifiers.
		/// </summary>
		public int BasePower
		{
			get { return GetInt32(UnitFields.BASE_MANA); }
			set
			{
				//if (PowerType == PowerType.Mana)
				SetInt32(UnitFields.BASE_MANA, value);

				this.UpdateMaxPower();

				if (PowerType != PowerType.Rage && PowerType != PowerType.Energy)
				{
					Power = MaxPower;
				}
			}
		}

		public void SetBasePowerDontUpdate(int value)
		{
			SetInt32(UnitFields.BASE_MANA, value);

			if ((PowerType != PowerType.Rage) && (PowerType != PowerType.Energy))
			{
				Power = MaxPower;
			}
		}

		/// <summary>
		/// The amount of the Unit's default Power (Mana, Energy, Rage, Happiness etc)
		/// </summary>
		public virtual int Power
		{
			get
			{
				// power is now calculated and regenerated continuously client-side
				// so we also need to interpolate power values server-side
				return (int)(internalPower + 0.5f);
			}
			set
			{
				value = MathUtil.ClampMinMax(value, 0, MaxPower);
				internalPower = value;
				
				SetInt32(UnitFields.POWER1 + (int) PowerType, value);
				MiscHandler.SendPowerUpdate(this, PowerType, value);
			}
		}


		protected float internalPower;
		internal void UpdatePower(int delayMillis)
		{
			internalPower += (PowerRegenPerTickActual * delayMillis) / (float)RegenerationFormulas.RegenTickDelayMillis;	// rounding
			internalPower = MathUtil.ClampMinMax(internalPower, 0, MaxPower);
			//SetInt32(UnitFields.POWER1 + (int)PowerType, (int)(val + 0.5f));
		}

		/// <summary>
		/// The max amount of the Unit's default Power (Mana, Energy, Rage, Happiness etc)
		/// NOTE: This is not related to Homer Simpson nor to any brand of hair blowers
		/// </summary>
		public virtual int MaxPower
		{
			get { return GetInt32(UnitFields.MAXPOWER1 + (int)PowerType); }
			internal set { SetInt32(UnitFields.MAXPOWER1 + (int)PowerType, value); }
		}

		#region Power Types

		//public int PowerMana
		//{
		//    get { return (int)GetUInt32(UnitFields.POWER1); }
		//    set { SetUInt32(UnitFields.POWER1, (uint)value); }
		//}

		//public int PowerRage
		//{
		//    get { return (int)GetUInt32(UnitFields.POWER2); }
		//    set { SetUInt32(UnitFields.POWER2, (uint)value); }
		//}

		//public int PowerFocus
		//{
		//    get { return (int)GetUInt32(UnitFields.POWER3); }
		//    set { SetUInt32(UnitFields.POWER3, (uint)value); }
		//}

		//public int PowerEnergy
		//{
		//    get { return (int)GetUInt32(UnitFields.POWER4); }
		//    set { SetUInt32(UnitFields.POWER4, (uint)value); }
		//}

		//public int PowerHappiness
		//{
		//    get { return (int)GetUInt32(UnitFields.POWER5); }
		//    set { SetUInt32(UnitFields.POWER5, (uint)value); }
		//}

		//public uint MaxPowerMana
		//{
		//    get { return GetUInt32(UnitFields.MAXPOWER1); }
		//    set { SetUInt32(UnitFields.MAXPOWER1, value); }
		//}

		//public uint MaxPowerRage
		//{
		//    get { return GetUInt32(UnitFields.MAXPOWER2); }
		//    set { SetUInt32(UnitFields.MAXPOWER2, value); }
		//}

		//public uint MaxPowerFocus
		//{
		//    get { return GetUInt32(UnitFields.MAXPOWER3); }
		//    set { SetUInt32(UnitFields.MAXPOWER3, value); }
		//}

		//public uint MaxPowerEnergy
		//{
		//    get { return GetUInt32(UnitFields.MAXPOWER4); }
		//    set { SetUInt32(UnitFields.MAXPOWER4, value); }
		//}

		//public uint MaxPowerHappiness
		//{
		//    get { return GetUInt32(UnitFields.MAXPOWER5); }
		//    set { SetUInt32(UnitFields.MAXPOWER5, value); }
		//}

		#endregion
		#endregion

		public virtual float ParryChance
		{
			get { return 5f; }
			internal set { }
		}

		/// <summary>
		/// Amount of additional yards to be allowed to jump without having any damage inflicted.
		/// TODO: Implement correctly (needs client packets)
		/// </summary>
		public int SafeFall
		{
			get;
			internal set;
		}

		public int AoEDamageModifierPct
		{
			get;
			set;
		}

		public virtual uint Defense
		{
			get
			{
				return (uint)(5 * Level);
			}
			internal set
			{ }
		}

		public virtual TalentCollection Talents
		{
			get { return null; }
		}
	}
}