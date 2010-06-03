/*************************************************************************
 *
 *   file		: NPC.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-02-20 06:16:32 +0100 (l√∏, 20 feb 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1257 $
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
using WCell.Constants.Looting;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Constants.Pets;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Core.Timers;
using WCell.RealmServer.AI;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.Network;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Auctioneer;
using WCell.RealmServer.NPCs.Pets;
using WCell.RealmServer.NPCs.Trainers;
using WCell.RealmServer.NPCs.Vendors;
using WCell.RealmServer.Quests;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Talents;
using WCell.RealmServer.Taxi;
using WCell.Util;
using WCell.RealmServer.AI.Groups;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Entities
{
	/// <summary>
	/// Represents a non-player character.
	/// </summary>
	public partial class NPC : Unit, IQuestHolder, IHasTalents
	{
		public static float BossSpellCritChance = 5f;

		public static NPC Create(NPCId id)
		{
			return NPCMgr.GetEntry(id).Create();
		}

		protected internal SpawnPoint m_spawnPoint;
		protected NPCEntry m_entry;
		protected TimerEntry m_decayTimer;
		private string m_name;
		protected IPetRecord m_PetRecord;
		protected TalentCollection m_petTalents;
		protected ThreatCollection m_threatCollection;
		protected AIGroup m_group;

		internal protected NPC()
		{
			m_threatCollection = new ThreatCollection();
		}

		protected internal virtual void SetupNPC(NPCEntry entry, SpawnPoint spawnPoint)
		{
			// auras
			m_auras = new NPCAuraCollection(this);

			var mainWeapon = entry.CreateMainHandWeapon();

			SpawnEntry spawnEntry;
			if (spawnPoint != null)
			{
				// Spawn-specific information
				spawnEntry = spawnPoint.SpawnEntry;
				m_spawnPoint = spawnPoint;
				Phase = spawnEntry.PhaseMask;
				m_orientation = spawnEntry.Orientation;
				if (spawnEntry.MountId != 0)
				{
					Mount(spawnEntry.MountId);
				}

				if (spawnEntry.DisplayIdOverride != 0)
				{
					DisplayId = spawnEntry.DisplayIdOverride;
				}

				SetUInt32(UnitFields.BYTES_0, spawnEntry.Bytes);
				SetUInt32(UnitFields.BYTES_2, spawnEntry.Bytes2);
			}
			else
			{
				Phase = 1;
				spawnEntry = entry.FirstSpawnEntry;
			}

			EmoteState = (spawnEntry != null && spawnEntry.EmoteState != 0) ? spawnEntry.EmoteState : entry.EmoteState;
			Entry = entry;
			NativeDisplayId = DisplayId;

			// misc stuff
			Faction = entry.Faction;
			NPCFlags = entry.NPCFlags;
			UnitFlags = entry.UnitFlags;
			DynamicFlags = entry.DynamicFlags;
			Class = entry.ClassId;
			Race = entry.RaceId;
			YieldsXpOrHonor = entry.GeneratesXp;
			SheathType = SheathType.Melee;

			// speeds
			m_runSpeed = entry.RunSpeed;
			m_swimSpeed = entry.RunSpeed;
			m_swimBackSpeed = entry.RunSpeed;
			m_walkSpeed = entry.WalkSpeed;
			m_walkBackSpeed = entry.WalkSpeed;
			m_flightSpeed = entry.FlySpeed;
			m_flightBackSpeed = entry.FlySpeed;

			Array.Copy(entry.Resistances, m_baseResistances, m_baseResistances.Length);

			MainWeapon = mainWeapon;

			// Set Level/Scale *after* MainWeapon is set:
			var level = entry.GetRandomLevel();
			Level = level;

			// Set model after Scale
			Model = m_entry.GetRandomModel();

			m_gossipMenu = entry.DefaultGossip; // set gossip menu

			// TODO: Init stats
			//for (int i = 0; i < 5; i++)
			//{
			//    m_baseStats[i] = statVal;
			//}
			PowerType = PowerType.Mana;
			SetBaseStat(StatType.Strength, 1, false);
			SetBaseStat(StatType.Agility, 1, false);
			SetBaseStat(StatType.Intellect, 1, false);
			SetBaseStat(StatType.Stamina, 1, false);
			SetBaseStat(StatType.Spirit, 1, false);

			// health + power
			var health = entry.GetRandomHealth();
			SetUInt32(UnitFields.MAXHEALTH, health);
			SetUInt32(UnitFields.BASE_HEALTH, health);

			if (spawnEntry == null || !spawnEntry.IsDead)
			{
				SetUInt32(UnitFields.HEALTH, health);
			}

			var mana = entry.GetRandomMana();
			SetInt32(UnitFields.MAXPOWER1, mana);
			SetInt32(UnitFields.BASE_MANA, mana);
			SetInt32(UnitFields.POWER1, mana);

			OffHandWeapon = entry.CreateOffhandWeapon();
			RangedWeapon = entry.CreateRangedWeapon();

			HoverHeight = entry.HoverHeight;

			m_Movement = new Movement(this);

			PowerCostMultiplier = 1f;

			if (PowerType == PowerType.Mana)
			{
				ManaRegenPerTickInterruptedPct = 20;
			}

			HealthRegenPerTickNoCombat = BaseHealth / 5;

			UpdateUnitState();

			if (m_entry.InhabitType.HasFlag(InhabitType.Air))
			{
				Flying++;
			}

			if (m_entry.Spells != null)
			{
				EnsureSpells();
			}

			if (m_entry.Type == NPCType.Totem || m_entry.Type == NPCType.None)
			{
				m_Movement.MayMove = false;
			}

			AddEquipment();

			CanMelee = mainWeapon != GenericWeapon.Peace;

			// TODO: Don't create talents if this is not a pet
			m_petTalents = new TalentCollection(this);

			m_brain = m_entry.BrainCreator(this);
			m_brain.IsRunning = true;
		}

		/// <summary>
		/// Update Unit-fields, according to given flags
		/// </summary>
		private void UpdateUnitState()
		{
			var flags = UnitFlags;
			if (flags != UnitFlags.None)
			{
				if (
					flags.HasAnyFlag(UnitFlags.SelectableNotAttackable | UnitFlags.SelectableNotAttackable_2 |
								  UnitFlags.SelectableNotAttackable_3))
				{
					Invulnerable++;
				}
				if (flags.HasFlag(UnitFlags.Combat))
				{
					IsInCombat = true;
				}
				if (flags.HasFlag(UnitFlags.Confused))
				{
					IncMechanicCount(SpellMechanic.Disoriented);
				}
				if (flags.HasFlag(UnitFlags.Disarmed))
				{
					IncMechanicCount(SpellMechanic.Disarmed);
				}
				if (flags.HasFlag(UnitFlags.Stunned))
				{
					Stunned++;
				}
				if (flags.HasFlag(UnitFlags.Silenced))
				{
					IncMechanicCount(SpellMechanic.Silenced);
				}
			}
		}

		/// <summary>
		/// Uncontrolled NPCs that are not summoned can evade
		/// </summary>
		public bool CanEvade
		{
			get
			{
				return m_region.CanNPCsEvade &&
					m_spawnPoint != null &&
					(m_master == this || m_master == null);
			}
		}

		private void AddEquipment()
		{
			NPCEquipmentEntry equipment;

			if (m_spawnPoint != null && m_spawnPoint.SpawnEntry != null)
			{
				equipment = m_spawnPoint.SpawnEntry.Equipment;
			}
			else
			{
				equipment = m_entry.Equipment;
			}

			if (equipment != null)
			{
				for (var i = 0; i < equipment.ItemIds.Length; i++)
				{
					var item = equipment.ItemIds[i];
					SetUInt32(UnitFields.VIRTUAL_ITEM_SLOT_ID + i, (uint)item);
				}
			}
		}

		#region Properties
		public NPCEntry Entry
		{
			get
			{
				return m_entry;
			}
			private set
			{
				if (m_entry != null)
				{
					throw new InvalidOperationException("Trying to change Entry of NPC: " + this);
				}

				m_entry = value;
				GenerateId(value.Id);
				if (m_entry.Regenerates)
				{
					InitializeRegeneration();
					HealthRegenPerTickNoCombat = Math.Max((int)m_entry.MaxHealth / 10, 1);
				}
				m_name = m_entry.DefaultName;
				EntryId = value.Id;
			}
		}

		public override string Name
		{
			get
			{
				return m_name;
			}
			set
			{
				m_name = value;
				PetNameTimestamp = Utility.GetEpochTime();
			}
		}

		internal void SetName(string name, uint timeStamp)
		{
			m_name = name;
			PetNameTimestamp = timeStamp;
		}

		public override UpdatePriority UpdatePriority
		{
			get
			{
				//return m_brain.UpdatePriority; // NPCs always have a Brain
				if (IsAreaActive)
				{
					return UpdatePriority.HighPriority;
				}
				return UpdatePriority.Inactive;
			}
		}

		public override Faction DefaultFaction
		{
			get
			{
				return m_entry.Faction;
			}
		}

		public ThreatCollection ThreatCollection
		{
			get { return m_threatCollection; }
		}

		/// <summary>
		/// Gets a random Unit from those who generated Threat
		/// </summary>
		public Unit GetRandomAttacker()
		{
			var rand = Utility.Random(m_threatCollection.AggressorPairs.Count);
			var i = 0;

			foreach (var target in m_threatCollection.AggressorPairs)
			{
				if (!CanBeAggroedBy(target.Key))
				{
					--rand;
				}

				if (i++ >= rand)
				{
					return target.Key;
				}
			}
			return null;
		}

		/// <summary>
		/// The AIGroup this NPC currently belongs to.
		/// </summary>
		public AIGroup Group
		{
			get { return m_group; }
			set
			{
				if (m_group != value)
				{
					m_brain.OnGroupChange(value);
					m_group = value;
					m_threatCollection.Group = value;
				}
			}
		}

		public NPCSpellCollection NPCSpells
		{
			get
			{
				if (m_spells == null)
				{
					m_spells = new NPCSpellCollection(this);
				}
				return (NPCSpellCollection)m_spells;
			}
		}

		public override SpellCollection Spells
		{
			get { return NPCSpells; }	// ensure that spell collection is created
		}

		public override SpawnPoint SpawnPoint
		{
			get { return m_spawnPoint; }
		}

		public override LinkedList<WaypointEntry> Waypoints
		{
			get
			{
				return m_spawnPoint != null ? m_spawnPoint.SpawnEntry.Waypoints : null;
			}
		}

		public override ObjectTypeCustom CustomType
		{
			get
			{
				return ObjectTypeCustom.Object | ObjectTypeCustom.Unit | ObjectTypeCustom.NPC;
			}
		}

		/// <summary>
		/// An NPC is decaying after it died and was looted empty.
		/// Setting this to true, starts or resets the decay-timer.
		/// Once the NPC decayed, it will be deleted.
		/// </summary>
		public bool IsDecaying
		{
			get
			{
				return m_decayTimer != null;
			}
			set
			{
				if (!value && IsDecaying)
				{
					StopDecayTimer();
				}
				else if (value && !IsDecaying)
				{
					RemainingDecayDelay = m_entry.DefaultDecayDelay;
				}
			}
		}

		public override uint LootMoney
		{
			get { return m_entry.MoneyDrop; }
		}

		/// <summary>
		/// Remaining time until the NPC decay (or 0.0f if already decaying or if decaying was not initialized yet).
		/// Deactivates the timer if set to a value smaller or equal to 0
		/// </summary>
		/// <remarks>Requires region-context</remarks>
		public float RemainingDecayDelay
		{
			get
			{
				return m_decayTimer != null ? m_decayTimer.RemainingInitialDelay : 0;
			}
			set
			{
				if (value <= 0)
				{
					StopDecayTimer();
				}
				else
				{
					m_decayTimer = new TimerEntry(DecayNow);
					m_decayTimer.Start(value, 0);
				}
			}
		}

		public IRealmClient Client
		{
			get
			{
				if (m_master != null && m_master is Character)
				{
					return ((Character)m_master).Client;
				}
				return null;
			}
		}

		#endregion

		#region NPC-specific Fields

		public override uint DisplayId
		{
			get { return base.DisplayId; }
			set
			{
				base.DisplayId = value;
				if (IsActivePet && m_master is Character)
				{
					// Update Group Update flags
					((Character)m_master).GroupUpdateFlags |= GroupUpdateFlags.PetDisplayId;
				}
			}
		}

		public override int Health
		{
			get { return base.Health; }
			set
			{
				base.Health = value;
				//Update Group Update flags
				if (m_PetRecord != null && m_master is Character)
					((Character)m_master).GroupUpdateFlags |= GroupUpdateFlags.PetHealth;
			}
		}

		public override int MaxHealth
		{
			get
			{
				return base.MaxHealth;
			}
			internal set
			{
				base.MaxHealth = value;

				//Update Group Update flags
				if (m_PetRecord != null && m_master is Character)
					((Character)m_master).GroupUpdateFlags |= GroupUpdateFlags.PetMaxHealth;
			}
		}

		public override int Power
		{
			get
			{
				return base.Power;
			}
			set
			{
				base.Power = value;
				//Update Group Update flags
				if (m_PetRecord != null && m_master is Character)
					((Character)m_master).GroupUpdateFlags |= GroupUpdateFlags.PetPower;
			}
		}

		public override PowerType PowerType
		{
			get
			{
				return base.PowerType;
			}
			set
			{
				base.PowerType = value;
				//Update Group Update flags
				if (m_PetRecord != null && m_master is Character)
				{
					// Since we're updating the power type, we also must trigger power and maxpower
					((Character)m_master).GroupUpdateFlags |= GroupUpdateFlags.PetPowerType | GroupUpdateFlags.PetPower | GroupUpdateFlags.PetMaxPower;
				}
			}
		}

		public override int MaxPower
		{
			get
			{
				return base.MaxPower;
			}
			internal set
			{
				base.MaxPower = value;
				//Update Group Update flags
				if (m_PetRecord != null && m_master is Character)
					((Character)m_master).GroupUpdateFlags |= GroupUpdateFlags.PetMaxPower;
			}
		}


		public override int Stamina
		{
			get
			{
				if (HasMaster)
				{
					var bonus = Math.Max(((Master.Stamina * PetMgr.StaminaScaler) / 100), 0);
					return (base.Stamina + bonus);
				}
				return base.Stamina;
			}
		}

		public override int Armor
		{
			get
			{
				if (HasMaster)
				{
					var bonus = Math.Max(((Master.Armor * PetMgr.ArmorScaler) / 100), 0);
					return (base.Armor + bonus);
				}
				return base.Armor;
			}
			internal set
			{
				if (HasMaster)
				{
					var bonus = Math.Max(((Master.Armor * PetMgr.ArmorScaler) / 100), 0);
					base.Armor = Math.Max(value - bonus, 0);
				}
				base.Armor = value;
			}
		}

		public override int MeleeAttackPower
		{
			get
			{
				if (HasMaster)
				{
					var bonus = Math.Max(((Master.MeleeAttackPower * PetMgr.MeleeAttackPowerScaler) / 100), 0);
					return (base.MeleeAttackPower + bonus);
				}
				return base.MeleeAttackPower;
			}
			internal set
			{
				if (HasMaster)
				{
					var bonus = Math.Max(((Master.MeleeAttackPower * PetMgr.MeleeAttackPowerScaler) / 100), 0);
					base.Armor = Math.Max(value - bonus, 0);
				}
				base.MeleeAttackPower = value;
			}
		}

		//
		// Todo: Pets get about 7.5% of the hunterís ranged attack power added to their spell damage.
		//

		public override int FireResist
		{
			get
			{
				if (HasMaster)
				{
					var bonus = Math.Max(((Master.FireResist * PetMgr.ResistanceScaler) / 100), 0);
					return (base.FireResist + bonus);
				}
				return base.FireResist;
			}
			internal set
			{
				if (HasMaster)
				{
					var bonus = Math.Max(((Master.FireResist * PetMgr.ResistanceScaler) / 100), 0);
					base.FireResist = Math.Max(value - bonus, 0);
				}
				base.FireResist = value;
			}
		}

		public override int NatureResist
		{
			get
			{
				if (HasMaster)
				{
					var bonus = Math.Max(((Master.NatureResist * PetMgr.ResistanceScaler) / 100), 0);
					return (base.NatureResist + bonus);
				}
				return base.NatureResist;
			}
			internal set
			{
				if (HasMaster)
				{
					var bonus = Math.Max(((Master.NatureResist * PetMgr.ResistanceScaler) / 100), 0);
					base.NatureResist = Math.Max(value - bonus, 0);
				}
				base.NatureResist = value;
			}
		}

		public override int FrostResist
		{
			get
			{
				if (HasMaster)
				{
					var bonus = Math.Max(((Master.FrostResist * PetMgr.ResistanceScaler) / 100), 0);
					return (base.FrostResist + bonus);
				}
				return base.FrostResist;
			}
			internal set
			{
				if (HasMaster)
				{
					var bonus = Math.Max(((Master.FrostResist * PetMgr.ResistanceScaler) / 100), 0);
					base.FrostResist = Math.Max(value - bonus, 0);
				}
				base.FrostResist = value;
			}
		}

		public override int ArcaneResist
		{
			get
			{
				if (HasMaster)
				{
					var bonus = Math.Max(((Master.ArcaneResist * PetMgr.ResistanceScaler) / 100), 0);
					return (base.ArcaneResist + bonus);
				}
				return base.ArcaneResist;
			}
			internal set
			{
				if (HasMaster)
				{
					var bonus = Math.Max(((Master.ArcaneResist * PetMgr.ResistanceScaler) / 100), 0);
					base.ArcaneResist = Math.Max(value - bonus, 0);
				}
				base.ArcaneResist = value;
			}
		}

		public override int ShadowResist
		{
			get
			{
				if (HasMaster)
				{
					var bonus = (Master.ShadowResist * PetMgr.ResistanceScaler) / 100;
					return (base.ShadowResist + bonus);
				}
				return base.ShadowResist;
			}
			internal set
			{
				if (HasMaster)
				{
					var bonus = ((Master.ShadowResist * PetMgr.ResistanceScaler) / 100);
					base.ShadowResist = Math.Max(value - bonus, 0);
				}
				base.ShadowResist = value;
			}
		}
		#endregion

		public override float GetSpellCritChance(DamageSchool school)
		{
			var value = m_entry.Rank > CreatureRank.Normal ? BossSpellCritChance : 0;
			if (m_spellCritMods != null)
			{
				value += m_spellCritMods[(int)school];
			}
			return value;
		}

		public override NPC SpawnMinion(NPCEntry entry, ref Vector3 position, int durationMillis)
		{
			var minion = base.SpawnMinion(entry, ref position, durationMillis);
			minion.Group = Group;
			return minion;
		}

		public override uint GetLootId(LootEntryType lootType)
		{
			switch (lootType)
			{
				case LootEntryType.NPCCorpse:
					return m_entry.LootId;
				case LootEntryType.Skinning:
					return m_entry.SkinLootId;
				case LootEntryType.PickPocketing:
					return m_entry.PickPocketLootId;
			}
			return 0;
		}

		public override bool UseGroupLoot
		{
			get { return true; }
		}

		#region Death
		protected override bool OnBeforeDeath()
		{
			if (!m_entry.NotifyBeforeDeath(this))
			{
				// NPC didn't die
				if (Health == 0)
				{
					Health = 1;
				}
				return false;
			}
			return true;
		}

		protected override void OnDeath()
		{
			m_brain.IsRunning = false;

			var looter = m_FirstAttacker;
			UnitFlags |= UnitFlags.SelectableNotAttackable;

			if (looter is Character && LootMgr.GetOrCreateLoot(this, (Character)looter, LootEntryType.NPCCorpse, m_region.IsHeroic) != null)
			{
				// NPCs don't have Corpse objects -> Spawning NPC Corpses will cause client to crash
				//RemainingDecayDelay = m_entry.DefaultDecayDelay * 10;
				EnterLootableState();
			}
			else
			{
				// no loot
				EnterFinalState();
			}

			if (looter is Character && YieldsXpOrHonor)
			{
				if (m_region.XpCalculator != null)
				{
					// TODO: Consider reductions if someone else killed the mob
					var chr = (Character)looter;
					var baseXp = m_region.XpCalculator(looter.Level, this);
					XpGenerator.CombatXpDistributer(chr, this, baseXp);

					if (chr.Group != null)
					{
						chr.Group.DistributeGroupQuestKills(chr, this);
					}
					else
					{
						chr.QuestLog.OnNPCInteraction(this);
					}
				}
			}

			if (m_currentTamer != null)
			{
				PetHandler.SendTameFailure(m_currentTamer, TameFailReason.TargetDead);
				CurrentTamer.SpellCast.Cancel(SpellFailedReason.Ok);
			}

			// reset spawn point if mob died
			if (m_spawnPoint != null)
			{
				m_spawnPoint.SignalSpawnlingDied(this);
			}

			m_entry.NotifyDied(this);

			if (m_master != null)
			{
				if (m_master.IsInWorld)
				{
					m_master.OnMinionDied(this);
				}
				else
				{
					Delete();
				}
			}
		}

		protected internal override void OnResurrect()
		{
			base.OnResurrect();
			StopDecayTimer();

			UnitFlags &= ~UnitFlags.SelectableNotAttackable;
			MarkUpdate(UnitFields.DYNAMIC_FLAGS);

			m_brain.EnterDefaultState();
			m_brain.OnActivate();

			m_entry.NotifyActivated(this);
		}
		#endregion

		/// <summary>
		/// Checks whether this NPC is of the given type
		/// </summary>
		public bool CheckCreatureType(TargetCreatureMask mask)
		{
			var type = Entry.Type;
		    return mask.HasFlag((TargetCreatureMask) (1 << ((int) type - 1)));
		}

		internal void SetScale()
		{
			var level = Level;
			if (HasMaster && m_entry.Family != null)
			{
				if (level >= m_entry.Family.MaxScaleLevel)
				{
					ScaleX = m_entry.Family.MaxScale * m_entry.Scale;
				}
				else
				{
					ScaleX = (m_entry.Family.MinScale + ((m_entry.Family.MaxScaleLevel - level) * m_entry.Family.ScaleStep)) *
						m_entry.Scale;
				}
			}
			else
			{
				ScaleX = m_entry.Scale;
			}
		}

		#region Movement / Transport
		protected internal override void OnEnterRegion()
		{
			base.OnEnterRegion();

			// default auras
			if (m_auras.Count == 0 && m_spawnPoint != null)
			{
				foreach (var aura in m_entry.Auras)
				{
					m_auras.AddSelf(aura, true);
				}

				foreach (var aura in m_spawnPoint.SpawnEntry.Auras)
				{
					m_auras.AddSelf(aura, true);
				}
			}

			foreach (var handler in m_entry.InstanceTypeHandlers)
			{
				if (handler != null)
				{
					handler(this);
				}
			}

			if (m_brain != null)
			{
				//spawn.Brain.MovementAI.SetSpawnPoint(pos);
				m_brain.OnActivate();
			}
			m_entry.NotifyActivated(this);

			if (m_spawnPoint != null)
			{
				m_spawnPoint.SpawnEntry.NotifySpawned(this);
			}

			if (m_master != null)
			{
				if (m_master.IsInWorld)
				{
					m_master.OnMinionEnteredRegion(this);
				}
				else
				{
					// master already gone
					Delete();
				}
			}
		}

		protected internal override void OnLeavingRegion()
		{
			if (IsAlive)
			{
				// remove from SpawnPoint
				if (m_spawnPoint != null)
				{
					m_spawnPoint.SignalSpawnlingDied(this);
				}
			}

			if (m_master != null)
			{
				if (m_master.IsInWorld)
				{
					m_master.OnMinionLeftRegion(this);
				}
				else
				{
					// master already gone
					Delete();
				}
			}
		}
		#endregion

		public override void OnFinishedLooting()
		{
			EnterFinalState();
		}

		public bool CanInteractWith(Character chr)
		{
			if (chr.Region != m_region ||
				!IsInRadiusSq(chr, NPCMgr.DefaultInteractionDistanceSq) ||
				!chr.CanSee(this))
			{
				NPCHandler.SendNPCError(chr, this, VendorInventoryError.TooFarAway);
				return false;
			}

			if (chr.IsAlive == IsSpiritHealer)
			{
				NPCHandler.SendNPCError(chr, this, VendorInventoryError.YouDead);
				return false;
			}

			if (chr.IsOnTaxi || m_isInCombat || IsOnTaxi)
			{
				return false;
			}

			if (!chr.CanInteract || !CanInteract)
			{
				return false;
			}

			var reputation = chr.Reputations.GetOrCreate(Faction.ReputationIndex);
			if (reputation != null && !reputation.CanInteract)
			{
				NPCHandler.SendNPCError(chr, this, VendorInventoryError.BadRep);
				return false;
			}

			return true;
		}

		#region Decay & Dispose
		/// <summary>
		/// Marks this NPC lootable (usually when dead)
		/// </summary>
		private void EnterLootableState()
		{
			FirstAttacker = null;
			RemainingDecayDelay = m_entry.DefaultDecayDelay * 2;
			MarkUpdate(UnitFields.DYNAMIC_FLAGS);
		}

		/// <summary>
		/// Marks this NPC lootable (usually when dead)
		/// </summary>
		private void EnterFinalState()
		{
			FirstAttacker = null;
			RemainingDecayDelay = m_entry.DefaultDecayDelay;
			if (m_entry.SkinningLoot != null)
			{
				UnitFlags |= UnitFlags.Skinnable;
			}

			if (m_loot != null)
			{
				m_loot.ForceDispose();
				m_loot = null;
			}

			MarkUpdate(UnitFields.DYNAMIC_FLAGS);
		}

		void DecayNow(float delay)
		{
			//if (IsAlive && m_master != null && !IsSummoned)
			//{
			//    AbandonMaster();
			//}
			//else

			if (m_loot != null)
			{
				m_loot.ForceDispose();
				m_loot = null;
			}
			Delete();
		}

		void StopDecayTimer()
		{
			if (m_decayTimer != null)
			{
				m_decayTimer.Stop();
				m_decayTimer = null;
			}
		}
		#endregion

		#region Combat
		public override float MaxAttackRange
		{
			get { return Math.Max(base.MaxAttackRange, NPCSpells.MaxCombatSpellRange); }
		}

		protected override void OnEnterCombat()
		{
			base.OnEnterCombat();

			// target is aggro'ed and will also enter combat
			if (m_target != null && !m_target.IsInCombat)
			{
				// client won't accept enforced Target but will select it automatically
				// m_target.Target = this

				m_target.IsInCombat = true;
			}

		}

		protected override void OnLeaveCombat()
		{
			base.OnLeaveCombat();

			InitializeRegeneration();
		}

		/// <summary>
		/// 
		/// </summary>
		protected internal override void OnDamageAction(IDamageAction action)
		{
			if (m_FirstAttacker == null && action.Attacker != null)
			{
				FirstAttacker = action.Attacker;
			}

			if (!action.Victim.IsAlive && YieldsXpOrHonor && action.Attacker is Character && action.Attacker.YieldsXpOrHonor)
			{
				action.Attacker.Proc(ProcTriggerFlags.GainExperience, this, action, true);
			}

			base.OnDamageAction(action);
		}
		#endregion

		#region Talk
		public override void Say(string message)
		{
			ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterSay, SpokenLanguage, message);
		}

		public void Say(string message, float radius)
		{
			ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterSay, SpokenLanguage, message, radius);
		}

		public void Say(string[] localizedMsgs)
		{
			ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterSay, SpokenLanguage, localizedMsgs);
		}

		public void Say(string[] localizedMsgs, float radius)
		{
			ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterSay, SpokenLanguage, localizedMsgs, radius);
		}

		public override void Yell(string message)
		{
			ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterYell, SpokenLanguage, message);
		}

		public void Yell(string message, float radius)
		{
			ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterYell, SpokenLanguage, message, radius);
		}

		public void Yell(string[] localizedMsgs)
		{
			ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterYell, SpokenLanguage, localizedMsgs);
		}

		public void Yell(string[] localizedMsgs, float radius)
		{
			ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterYell, SpokenLanguage, localizedMsgs, radius);
		}

		public override void Emote(string message)
		{
			ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterEmote, SpokenLanguage, message);
		}

		public void Emote(string message, float radius)
		{
			ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterEmote, SpokenLanguage, message, radius);
		}

		public void Emote(string[] localizedMsgs)
		{
			ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterEmote, SpokenLanguage, localizedMsgs);
		}

		public void Emote(string[] localizedMsgs, float radius)
		{
			ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterEmote, SpokenLanguage, localizedMsgs, radius);
		}

		/// <summary>
		/// Yells to everyone within the region to hear
		/// </summary>
		/// <param name="message"></param>
		public virtual void YellToRegion(string message)
		{
			ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterYell, ChatLanguage.Universal, message, -1);
		}
		#endregion

		#region Taxi Vendor

		/// <summary>
		/// Whether this unit is a TaxiVendor
		/// </summary>
		public bool IsTaxiVendor
		{
			get { return NPCFlags.HasFlag(NPCFlags.FlightMaster); }
		}

		/// <summary>
		/// The TaxiNode this TaxiVendor is associated with
		/// </summary>
		private PathNode vendorTaxiNode;

		/// <summary>
		/// The TaxiNode this TaxiVendor is associated with
		/// </summary>
		public PathNode VendorTaxiNode
		{
			get
			{
				if (!IsTaxiVendor) return null;

				if (vendorTaxiNode == null)
				{
					vendorTaxiNode = TaxiMgr.GetNearestTaxiNode(Position);
				}
				return vendorTaxiNode;
			}
			internal set { vendorTaxiNode = value; }
		}

		#endregion

		#region Banker

		/// <summary>
		/// Whether this is a Banker
		/// </summary>
		public bool IsBanker
		{
			get { return NPCFlags.HasFlag(NPCFlags.Banker); }
		}

		#endregion

		#region InnKeeper
		/// <summary>
		/// Whether this is an InnKeeper
		/// </summary>
		public bool IsInnKeeper
		{
			get { return NPCFlags.HasFlag(NPCFlags.InnKeeper); }
		}


		/// <summary>
		/// The location to which a Character can bind to when talking to this NPC 
		/// or null if this is not an InnKeeper.
		/// </summary>
		public NamedWorldZoneLocation BindPoint
		{
			get;
			internal set;
		}
		#endregion

		#region Vendor

		/// <summary>
		/// Whether this is a Vendor
		/// </summary>
		public bool IsVendor
		{
			get
			{
				return m_entry.IsVendor;
			}
		}

		/// <summary>
		/// A list of Items this Vendor has for sale. 
		/// Returns <c>VendorItemEntry.EmptyList</c> if this is not a Vendor
		/// </summary>
		public List<VendorItemEntry> ItemsForSale
		{
			get
			{
				if (VendorEntry == null)
				{
					return VendorItemEntry.EmptyList;
				}
				return VendorEntry.ItemsForSale;
			}
		}

		private VendorEntry m_VendorEntry;

		/// <summary>
		/// The Vendor-specific details for this NPC
		/// </summary>
		public VendorEntry VendorEntry
		{
			get { return m_VendorEntry; }
			set
			{
				m_VendorEntry = value;
			}
		}

		/// <summary>
		/// Whether this is a Stable Master
		/// </summary>
		public bool IsStableMaster
		{
			get { return NPCFlags.HasFlag(NPCFlags.StableMaster); }
		}

		#endregion

		#region Petitioner

		/// <summary>
		/// Whether this NPC can issue Charters
		/// </summary>
		public bool IsPetitioner
		{
			get { return NPCFlags.HasFlag(NPCFlags.Petitioner); }
		}

		/// <summary>
		/// Whether this is a Tabard Vendor
		/// </summary>
		public bool IsTabardVendor
		{
			get { return NPCFlags.HasFlag(NPCFlags.TabardDesigner); }
		}

		/// <summary>
		/// Whether this NPC can issue Guild Charters
		/// </summary>
		public bool IsGuildPetitioner
		{
			get { return NPCFlags.HasAnyFlag(NPCFlags.TabardDesigner | NPCFlags.Petitioner); }
		}

		/// <summary>
		/// Whether this NPC can issue Arena Charters
		/// </summary>
		public bool IsArenaPetitioner
		{
			get { return NPCFlags.HasFlag(NPCFlags.Petitioner); }
		}

		#endregion

		#region QuestGiver

		/// <summary>
		/// Whether this NPC starts a quest (or multiple quests)
		/// </summary>
		public bool IsQuestGiver
		{
			get
			{
				return NPCFlags.HasFlag(NPCFlags.QuestGiver);
			}
			internal set
			{
				if (value)
				{
					NPCFlags |= NPCFlags.QuestGiver;
				}
				else
				{

					NPCFlags &= ~NPCFlags.QuestGiver;
				}
			}
		}

		/// <summary>
		/// All available Quest information, in case that this is a QuestGiver
		/// </summary>
		public QuestHolderInfo QuestHolderInfo
		{
			get
			{
				return m_entry.QuestHolderInfo;
			}
			internal set
			{
				m_entry.QuestHolderInfo = value;
			}
		}

		public bool CanGiveQuestTo(Character chr)
		{
			return CanInteractWith(chr);
		}
		#endregion

		#region Trainer
		/// <summary>
		/// Whether this is a Trainer.
		/// </summary>
		public bool IsTrainer
		{
			get
			{
				return TrainerEntry != null;
			}
		}

		/// <summary>
		/// The Trainer-specific details for this NPC.
		/// </summary>
		public TrainerEntry TrainerEntry;

		/// <summary>
		/// Whether this NPC can train the character in their specialty.
		/// </summary>
		/// <returns>True if able to train.</returns>
		public bool CanTrain(Character character)
		{
			return IsTrainer && TrainerEntry.CanTrain(character);
		}

		#endregion

		#region Auctioneer
		/// <summary>
		/// Whether this is an Auctioneer.
		/// </summary>
		public bool IsAuctioneer
		{
			get
			{
				return AuctioneerEntry != null;
			}
		}

		/// <summary>
		/// The Aucioneer-specific details of this NPC.
		/// </summary>
		public AuctioneerEntry AuctioneerEntry;
		#endregion

		#region NPC Controlling
		private Character m_currentTamer;

		/// <summary>
		/// The Character that currently tries to tame this Creature (or null if not being tamed)
		/// </summary>
		public Character CurrentTamer
		{
			get { return m_currentTamer; }
			set
			{
				if (value != m_currentTamer)
				{
					m_currentTamer = value;
				}
			}
		}

		/// <summary>
		/// We were controlled and reject the Controller.
		/// Does nothing if not controlled.
		/// </summary>
		public void RejectMaster()
		{
			if (m_master != null)
			{
				SetEntityId(UnitFields.CREATEDBY, EntityId.Zero);
				if (Summoner != null)
				{
					Summoner = null;
				}
				if (Charmer != null)
				{
					Charmer = null;
				}
			}
			if (m_PetRecord != null)
			{
				DeletePetRecord();
			}
		}
		#endregion

		public override void Update(float dt)
		{
			if (m_decayTimer != null)
			{
				m_decayTimer.Update(dt);
			}

			if (m_target != null)
			{
				// always face the target
				SetOrientationTowards(m_target);
			}

			base.Update(dt);
		}

		/// <summary>
		/// Deletes this NPC and spawns a new instance of it.
		/// </summary>
		public void Reset()
		{
			Delete();
			if (m_spawnPoint != null)
			{
				ContextHandler.ExecuteInContext(m_spawnPoint.SpawnOne);
			}
			else
			{
				m_entry.Create(Region, Position);
			}
		}

		protected internal override void DeleteNow()
		{
			base.DeleteNow();
		}

		public override void Dispose(bool disposing)
		{
			if (m_region != null)
			{
				m_currentTamer = null;
				m_region.UnregisterUpdatableLater(m_decayTimer);
				base.Dispose(disposing);
			}
		}

		public override string ToString()
		{
			return Name + " (ID: " + EntryId + ", #" + EntityId.Low + ")";
		}

	}
}
