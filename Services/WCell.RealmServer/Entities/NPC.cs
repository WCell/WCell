/*************************************************************************
 *
 *   file		: NPC.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-02-20 06:16:32 +0100 (lø, 20 feb 2010) $
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
using System.Linq;
using WCell.Constants;
using WCell.Constants.Items;
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
using WCell.RealmServer.NPCs.Spawns;
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
	public partial class NPC : Unit, IQuestHolder
	{
		public static float BossSpellCritChance = 5f;

		public static NPC Create(NPCId id)
		{
			return NPCMgr.GetEntry(id).Create();
		}

		protected internal NPCSpawnPoint m_spawnPoint;
		protected NPCEntry m_entry;
		protected TimerEntry m_decayTimer;
		private string m_name;
		protected IPetRecord m_PetRecord;
		protected PetTalentCollection m_petTalents;
		protected ThreatCollection m_threatCollection;
		protected AIGroup m_group;

		internal protected NPC()
		{
			m_threatCollection = new ThreatCollection();

			// auras
			m_auras = new NPCAuraCollection(this);

			m_spells = NPCSpellCollection.Obtain(this);
		}

		#region Creation & Init
		protected internal virtual void SetupNPC(NPCEntry entry, NPCSpawnPoint spawnPoint)
		{
			NPCSpawnEntry spawnEntry;
			if (spawnPoint != null)
			{
				// Spawn-specific information
				spawnEntry = spawnPoint.SpawnEntry;
				m_spawnPoint = spawnPoint;
				Phase = spawnEntry.Phase;
				m_orientation = spawnEntry.Orientation;

				if (spawnEntry.DisplayIdOverride != 0)
				{
					DisplayId = spawnEntry.DisplayIdOverride;
				}
			}
			else
			{
				Phase = 1;
				spawnEntry = entry.FirstSpawnEntry;
			}

			GenerateId(entry.Id);

			SetEntry(entry);
		}

		public void SetEntry(NPCId entryId)
		{
			var entry = NPCMgr.GetEntry(entryId);
			SetEntry(entry);
		}

		public void SetEntry(NPCEntry entry)
		{
			Entry = entry;

			if (m_spawnPoint == null || m_spawnPoint.SpawnEntry.DisplayIdOverride == 0)
			{
				if (entry.ModelInfos.Length > 0)
				{
					Model = entry.ModelInfos.GetRandom();
				}
			}
			NativeDisplayId = DisplayId;

			if (m_brain != null)
			{
				// overriding already existing entry
			}
			else
			{
				// new
				m_Movement = new Movement(this);
				m_brain = m_entry.BrainCreator(this);
				m_brain.IsRunning = true;
			}

			// misc stuff
			Name = m_entry.DefaultName;
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

			MainWeapon = m_entry.CreateMainHandWeapon();
			RangedWeapon = m_entry.CreateRangedWeapon();
			OffHandWeapon = entry.CreateOffhandWeapon();

			// Set model after Scale
			Model = m_entry.GetRandomModel();

			GossipMenu = entry.DefaultGossip; // set gossip menu

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
			SetInt32(UnitFields.MAXHEALTH, health);
			SetInt32(UnitFields.BASE_HEALTH, health);

			if (m_entry.IsDead || m_spawnPoint == null || !m_spawnPoint.SpawnEntry.IsDead)
			{
				SetInt32(UnitFields.HEALTH, health);
			}
			else if (m_entry.Regenerates)
			{
				Regenerates = true;
				HealthRegenPerTickNoCombat = Math.Max((int)m_entry.MaxHealth / 10, 1);
			}

			var mana = entry.GetRandomMana();
			SetInt32(UnitFields.MAXPOWER1, mana);
			SetInt32(UnitFields.BASE_MANA, mana);
			SetInt32(UnitFields.POWER1, mana);

			HoverHeight = entry.HoverHeight;

			PowerCostMultiplier = 1f;

			if (PowerType == PowerType.Mana)
			{
				ManaRegenPerTickInterruptedPct = 20;
			}

			UpdateUnitState();

			if (m_entry.InhabitType.HasFlag(InhabitType.Air))
			{
				Flying++;
			}

			AddStandardEquipment();
			if (m_entry.AddonData != null)
			{
				// first add general addon data
				AddAddonData(m_entry.AddonData);
			}
			if (m_spawnPoint != null && m_spawnPoint.SpawnEntry.AddonData != null)
			{
				// then override with per-spawn addon data
				AddAddonData(m_spawnPoint.SpawnEntry.AddonData);
			}

			if (m_mainWeapon != GenericWeapon.Peace)
			{
				IncMeleePermissionCounter();
			}

			if (IsImmovable)
			{
				InitImmovable();
			}
			Level = entry.GetRandomLevel();

			AddMessage(UpdateSpellRanks);
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
								  UnitFlags.SelectableNotAttackable_3 | UnitFlags.NotAttackable))
				{
					Invulnerable++;
				}
				if (flags.HasAnyFlag(UnitFlags.NotSelectable))
				{
					IsEvading = true;
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
				if (flags.HasFlag(UnitFlags.Passive))
				{
					HasPermissionToMove = false;
				}
			}
		}

		private void InitImmovable()
		{
			m_Movement.MayMove = false;

			// if immovables have a single AreaAura, better cast it
			if (HasSpells && Spells.Count == 1)
			{
				var spell = Spells.First();
				if (spell.IsAreaAura)
				{
					AddMessage(() => SpellCast.Start(spell, true));
				}
			}
		}

		private void AddStandardEquipment()
		{
			NPCEquipmentEntry equipment;

			if (m_spawnPoint != null && m_spawnPoint.SpawnEntry != null && m_spawnPoint.SpawnEntry.Equipment != null)
			{
				equipment = m_spawnPoint.SpawnEntry.Equipment;
			}
			else
			{
				equipment = m_entry.Equipment;
			}

			if (equipment != null)
			{
				SetEquipment(equipment);
			}
		}

		private void AddAddonData(NPCAddonData data)
		{
			SetUInt32(UnitFields.BYTES_0, data.Bytes);
			SetUInt32(UnitFields.BYTES_2, data.Bytes2);

			EmoteState = data.EmoteState;

			if (data.MountModelId != 0)
			{
				Mount(data.MountModelId);
			}
		}
		#endregion

		#region Properties
		public NPCEntry Entry
		{
			get
			{
				return m_entry;
			}
			private set
			{
				m_entry = value;
				EntryId = value.Id;
			}
		}

		public override ObjectTemplate Template
		{
			get { return Entry; }
		}

		public override string Name
		{
			get { return m_name; }
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

		/// <summary>
		/// Uncontrolled NPCs that are not summoned can evade
		/// </summary>
		public bool CanEvade
		{
			get
			{
				return m_Map.CanNPCsEvade &&
					m_spawnPoint != null &&
					(m_master == this || m_master == null);
			}
		}

		public bool IsImmovable
		{
			get { return m_entry.Type == CreatureType.Totem || m_entry.Type == CreatureType.None; }
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
			get { return m_entry.Faction; }
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
			get { return (NPCSpellCollection)m_spells; }
		}

		public override SpellCollection Spells
		{
			get { return m_spells; }
		}

		public override NPCSpawnPoint SpawnPoint
		{
			get { return m_spawnPoint; }
		}

		public NPCSpawnEntry SpawnEntry
		{
			get { return m_spawnPoint != null ? m_spawnPoint.SpawnEntry : null; }
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
					RemainingDecayDelayMillis = m_entry.DefaultDecayDelayMillis;
				}
			}
		}

		public override uint LootMoney
		{
			get { return m_entry.MoneyDrop; }
		}

		/// <summary>
		/// Remaining time until the NPC decay (or 0 if already decayed or decaying did not start yet).
		/// Deactivates the timer if set to a value smaller or equal to 0
		/// </summary>
		/// <remarks>Requires map-context</remarks>
		public int RemainingDecayDelayMillis
		{
			get
			{
				return m_decayTimer != null ? m_decayTimer.RemainingInitialDelayMillis : 0;
			}
			set
			{
				if (value <= 0)
				{
					StopDecayTimer();
				}
				else
				{
					if (m_decayTimer == null)
					{
						m_decayTimer = new TimerEntry(DecayNow);
					}
					m_decayTimer.Start(value);
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

		public override int StaminaWithoutHealthContribution
		{
			get
			{
				if (IsHunterPet)
				{
					// hunter pets gain no health from their base stamina
					return GetBaseStatValue(StatType.Stamina);
				}
				// TODO: NPC stats
				return 0;
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
		#endregion

		#region NPC Items
		/// <summary>
		/// Sets this NPC's equipment to the given entry
		/// </summary>
		public void SetEquipment(NPCEquipmentEntry equipment)
		{
			for (var i = 0; i < equipment.ItemIds.Length; i++)
			{
				var itemId = equipment.ItemIds[i];
				if (itemId != 0)
				{
					var item = ItemMgr.GetTemplate(itemId);
					if (item != null)
					{
						SheathType = item.SheathType;		// TODO: How to use the sheath type of all items?
					}
					SetUInt32(UnitFields.VIRTUAL_ITEM_SLOT_ID + i, (uint)itemId);
				}
			}
		}

		public void SetMainWeaponVisual(ItemId item)
		{
			SetUInt32(UnitFields.VIRTUAL_ITEM_SLOT_ID, (uint)item);
		}

		public void SetOffhandWeaponVisual(ItemId item)
		{
			SetUInt32(UnitFields.VIRTUAL_ITEM_SLOT_ID_2, (uint)item);
		}

		public void SetRangedWeaponVisual(ItemId item)
		{
			SetUInt32(UnitFields.VIRTUAL_ITEM_SLOT_ID_3, (uint)item);
		}

		/// <summary>
		/// NPCs only have their default items which may always be used, so no invalidation
		/// takes place.
		/// </summary>
		protected override IWeapon GetOrInvalidateItem(InventorySlotType slot)
		{
			switch (slot)
			{
				case InventorySlotType.WeaponMainHand:
					return m_entry.CreateMainHandWeapon();
				case InventorySlotType.WeaponRanged:
					return m_entry.CreateRangedWeapon();
				case InventorySlotType.WeaponOffHand:
					return m_entry.CreateOffhandWeapon();
			}
			return null;
		}
		#endregion

		public override float GetCritChance(DamageSchool school)
		{
			var value = m_entry.Rank > CreatureRank.Normal ? BossSpellCritChance : 0;
			if (m_CritMods != null)
			{
				value += m_CritMods[(int)school];
			}
			return value;
		}

		public override NPC SpawnMinion(NPCEntry entry, ref Vector3 position, int durationMillis)
		{
			var minion = base.SpawnMinion(entry, ref position, durationMillis);
			if (Group == null)
			{
				Group = new AIGroup(this);
			}
			Group.Add(minion);
			return minion;
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

			// hand out experience
			bool heroic;
			if (m_Map != null)
			{
				m_Map.OnNPCDied(this);
				heroic = m_Map.IsHeroic;
			}
			else
			{
				heroic = false;
			}

			var looter = m_FirstAttacker;

			if (!HasPlayerMaster)	// not player-owned NPC
			{
				var playerLooter = looter != null ? looter.PlayerOwner : null;
				if (playerLooter != null &&
					LootMgr.GetOrCreateLoot(this, playerLooter, LootEntryType.NPCCorpse, heroic) != null)
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
			}

			// notify events
			m_entry.NotifyDied(this);

			UnitFlags |= UnitFlags.SelectableNotAttackable;

			// send off the tamer
			if (m_currentTamer != null)
			{
				PetHandler.SendTameFailure(m_currentTamer, TameFailReason.TargetDead);
				CurrentTamer.SpellCast.Cancel(SpellFailedReason.Ok);
			}

			// reset spawn timer
			if (m_spawnPoint != null)
			{
				m_spawnPoint.SignalSpawnlingDied(this);
			}

			if (m_master != null)
			{
				// notify master
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

			if (m_spawnPoint != null)
			{
				// add back to pool's AIGroup
				m_spawnPoint.Pool.SpawnedObjects.Add(this);
			}

			m_brain.IsRunning = true;
			m_brain.EnterDefaultState();
			m_brain.OnActivate();

			m_entry.NotifyActivated(this);
		}
		#endregion

		/// <summary>
		/// Checks whether this NPC is of the given type
		/// </summary>
		public bool CheckCreatureType(CreatureMask mask)
		{
			return mask.HasFlag((CreatureMask)(1 << ((int)Entry.Type - 1)));
		}

		#region Movement / Transport
		protected internal override void OnEnterMap()
		{
			base.OnEnterMap();

			// add to set of spawned objects of SpawnPoint
			if (m_spawnPoint != null)
			{
				m_spawnPoint.SignalSpawnlingActivated(this);
			}

			if (m_auras.Count == 0)
			{
				// add auras
				if (m_entry.AddonData != null)
				{
					foreach (var aura in m_entry.AddonData.Auras)
					{
						m_auras.CreateSelf(aura, true);
					}
				}

				if (m_spawnPoint != null && m_spawnPoint.SpawnEntry.AddonData != null)
				{
					foreach (var aura in m_spawnPoint.SpawnEntry.AddonData.Auras)
					{
						m_auras.CreateSelf(aura, true);
					}
				}
			}

			// initialize type-specific things
			foreach (var handler in m_entry.InstanceTypeHandlers)
			{
				if (handler != null)
				{
					handler(this);
				}
			}

			// trigger events
			if (m_brain != null)
			{
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
					m_master.OnMinionEnteredMap(this);
				}
				else
				{
					// master already gone
					Delete();
					return;
				}
			}
		}

		protected internal override void OnLeavingMap()
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
					m_master.OnMinionLeftMap(this);
				}
				else
				{
					// master already gone
					Delete();
				}
			}
		}
		#endregion

		#region Decay & Dispose
		/// <summary>
		/// Marks this NPC lootable (after NPC died)
		/// </summary>
		private void EnterLootableState()
		{
			FirstAttacker = null;
			RemainingDecayDelayMillis = m_entry.DefaultDecayDelayMillis * 2;
			MarkUpdate(UnitFields.DYNAMIC_FLAGS);
		}

		/// <summary>
		/// Marks this NPC non-lootable (after NPC was looted)
		/// </summary>
		private void EnterFinalState()
		{
			FirstAttacker = null;
			RemainingDecayDelayMillis = m_entry.DefaultDecayDelayMillis;
			if (m_entry.GetSkinningLoot() != null)
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

		void DecayNow(int delay)
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

		protected internal override void DeleteNow()
		{
			// make sure IsDeleted = true
			m_Deleted = true;
			m_entry.NotifyDeleted(this);

			if (m_spawnPoint != null && m_spawnPoint.ActiveSpawnling == this)
			{
				// remove if NPC was for some reason still considered active in pool
				m_spawnPoint.SignalSpawnlingDied(this);
			}
			m_auras.ClearWithoutCleanup();
			base.DeleteNow();
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
			get
			{
				var range = base.MaxAttackRange;
				if (m_spells != null && NPCSpells.MaxCombatSpellRange > range)
				{
					range = NPCSpells.MaxCombatSpellRange;
				}
				return range;
			}
		}

		protected override void OnEnterCombat()
		{
			base.OnEnterCombat();

			if (m_target != null)
			{
				// add Target into threat collection
				m_threatCollection.AddNewIfNotExisted(m_target);
			}

		}

		protected override void OnLeaveCombat()
		{
			base.OnLeaveCombat();
		}

		public override float AggroBaseRange
		{
			get
			{
				return m_entry.AggroBaseRange;
			}
		}
		#endregion

		/// <summary>
		/// Also sends a message to the Character, if not valid
		/// </summary>
		internal bool CheckVendorInteraction(Character chr)
		{
			if (chr.Map != m_Map ||
				!IsInRadiusSq(chr, NPCMgr.DefaultInteractionDistanceSq) ||
				!chr.CanSee(this))
			{
				NPCHandler.SendNPCError(chr, this, VendorInventoryError.TooFarAway);
				return false;
			}

			if (!IsAlive)
			{
				NPCHandler.SendNPCError(chr, this, VendorInventoryError.VendorDead);
				return false;
			}

			if (chr.IsAlive == IsSpiritHealer)
			{
				NPCHandler.SendNPCError(chr, this, VendorInventoryError.YouDead);
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

		#region Talk
		public override void Say(float radius, string message)
		{
			ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterSay, SpokenLanguage, message, radius);
		}

		public override void Say(float radius, string[] localizedMsgs)
		{
			ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterSay, SpokenLanguage, localizedMsgs, radius);
		}


		public override void Yell(float radius, string message)
		{
			ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterYell, SpokenLanguage, message, radius);
		}

		public override void Yell(float radius, string[] localizedMsgs)
		{
			ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterYell, SpokenLanguage, localizedMsgs, radius);
		}


		public override void Emote(float radius, string message)
		{
			ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterEmote, SpokenLanguage, message, radius);
		}

		public override void Emote(float radius, string[] localizedMsgs)
		{
			ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterEmote, SpokenLanguage, localizedMsgs, radius);
		}

		/// <summary>
		/// Yells to everyone within the map to hear
		/// </summary>
		/// <param name="message"></param>
		public void YellToMap(string[] messages)
		{
			Yell(-1, messages);
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
			return CheckVendorInteraction(chr);
		}

		public void OnQuestGiverStatusQuery(Character chr)
		{
			// do nothing
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
				//SetEntityId(UnitFields.CREATEDBY, EntityId.Zero);
				SetEntityId(UnitFields.SUMMONEDBY, EntityId.Zero);
				SetEntityId(UnitFields.CHARMEDBY, EntityId.Zero);
				Master = null;
			}
			if (m_PetRecord != null)
			{
				DeletePetRecord();
			}
		}
		#endregion

		#region Loot
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

		public override void OnFinishedLooting()
		{
			EnterFinalState();
		}
		#endregion

		public override int GetBasePowerRegen()
		{
			if (IsPlayerOwned)
			{
				return RegenerationFormulas.GetPowerRegen(this);
			}
			else
			{
				// TODO: NPC power regen
				return BasePower / 50;
			}
		}

		public override void Update(int dt)
		{
			if (m_decayTimer != null)
			{
				m_decayTimer.Update(dt);
			}

			if (m_target != null && CanMove)
			{
				// always face the target
				SetOrientationTowards(m_target);
			}

			base.Update(dt);
		}

		public override void Dispose(bool disposing)
		{
			if (m_Map != null)
			{
				m_currentTamer = null;
				m_Map.UnregisterUpdatableLater(m_decayTimer);
				base.Dispose(disposing);
			}
		}

		public override string ToString()
		{
			return Name + " (ID: " + EntryId + ", #" + EntityId.Low + ")";
		}

	}
}