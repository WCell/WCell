/*************************************************************************
 *
 *   file		: Character.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-02-20 06:16:32 +0100 (l�, 20 feb 2010) $
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
using NLog;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Commands;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.Global;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Help.Tickets;
using WCell.RealmServer.Instances;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.Items;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Quests;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Talents;
using WCell.RealmServer.Taxi;
using WCell.Util;
using WCell.Util.Commands;
using WCell.RealmServer.Battlegrounds;
using WCell.Util.Graphics;
using WCell.RealmServer.Spells.Auras;
using WCell.Core.Timers;
using WCell.RealmServer.RacesClasses;

namespace WCell.RealmServer.Entities
{
	///<summary>
	/// Represents a unit controlled by a player in the game world
	///</summary>
	public partial class Character : Unit, IUser, IContainer, ITicketHandler, IInstanceHolderSet, IHasTalents, ICharacterSet
	{
		private static new Logger log = LogManager.GetCurrentClassLogger();

		public static new readonly List<Character> EmptyArray = new List<Character>();

		/// <summary>
		/// The delay until a normal player may logout in seconds.
		/// </summary>
		public static float DefaultLogoutDelay = 20.0f;

		/// <summary>
		/// Speed increase when dead and in Ghost form
		/// </summary>
		public static float DeathSpeedIncrease = 0.25f;

		/// <summary>
		/// whether to check for speedhackers
		/// </summary>
		public static bool SpeedHackCheck;

		/// <summary>
		/// The factor that is applied to the maximum distance before detecting someone as a SpeedHacker
		/// </summary>
		public static float SpeedHackToleranceFactor = 1.5f;

		/// <summary>
		/// Clears all trade-related fields for the character.
		/// </summary>
		public void ClearTrade()
		{
		}

		public void UpdatePlayedTime()
		{
			var now = DateTime.Now;
			var timeDiff = now - m_lastPlayTimeUpdate;

			LevelPlayTime += (uint)timeDiff.TotalSeconds;
			TotalPlayTime += (uint)timeDiff.TotalSeconds;
			m_lastPlayTimeUpdate = now;
		}

		#region Properties
		/// <summary>
		/// Check to see if character is in an instance
		/// </summary>
		public bool IsInInstance
		{
			get
			{
				return m_region != null && m_region.IsInstance;
			}
		}

		/// <summary>
		/// Check to see if character is in a group
		/// </summary>
		public bool IsInGroup
		{
			get { return m_groupMember != null; }
		}

		/// <summary>
		/// Check to see if character is in a Guild
		/// </summary>
		public bool IsInGuild
		{
			get { return m_guildMember != null; }
		}

		/// <summary>
		/// Check to see if character is in a group
		/// </summary>
		public bool IsInRaid
		{
			get
			{
				return Group is RaidGroup;
			}
		}

		/// <summary>
		/// Check to see if character is in the same instance as group members
		/// </summary>
		public bool IsInGroupInstance
		{
			get
			{
				var group = Group;
				if (group != null)
				{
					return group.GetActiveInstance(m_region.RegionTemplate) != null;
				}
				return false;
			}
		}

		/// <summary>
		/// Personal Dungeon Difficulty, might differ from current Difficulty
		/// </summary>
		public DungeonDifficulty DungeonDifficulty
		{
			get { return m_record.DungeonDifficulty; }
			set
			{
				m_record.DungeonDifficulty = value;
				if (m_groupMember == null)
				{
					InstanceHandler.SendDungeonDifficulty(this);
				}
			}
		}

		public RaidDifficulty RaidDifficulty
		{
			get { return m_record.RaidDifficulty; }
			set
			{
				m_record.RaidDifficulty = value;
				if (m_groupMember == null)
				{
					InstanceHandler.SendRaidDifficulty(this);
				}
			}
		}
		#endregion

		public uint GetInstanceDifficulty(bool isRaid)
		{
			return m_groupMember == null ? (isRaid ? (uint)m_record.RaidDifficulty : (uint)m_record.DungeonDifficulty) : m_groupMember.Group.DungeonDifficulty;
		}

		#region Death/Resurrect
		public override bool IsAlive
		{
			get { return !(m_auras.GhostAura != null || Health == 0); }
		}

		/// <summary>
		/// whether the Corpse is reclaimable 
		/// (Character must be ghost and the reclaim delay must have passed)
		/// </summary>
		public bool IsCorpseReclaimable
		{
			get
			{
				return IsGhost && DateTime.Now > m_record.LastResTime.AddMilliseconds(Corpse.MinReclaimDelay);
			}
		}

		/// <summary>
		/// Character can reclaim if Corpse is reclaimable and Character is close to Corpse,
		/// or if there is no Corpse, Character must be somewhere near a SpiritHealer
		/// </summary>
		public bool CanReclaimCorpse
		{
			get
			{
				return IsCorpseReclaimable && ((m_corpse != null && IsInRadiusSq(m_corpse, Corpse.ReclaimRadiusSq)) ||
					// No Corpse but close to Spirithealer
					(IsGhost && (m_corpse == null &&
					KnownObjects.Contains(obj => obj is Unit && ((Unit)obj).IsSpiritHealer))));
			}
		}

		/// <summary>
		/// Last time this Character died
		/// </summary>
		public DateTime LastDeathTime
		{
			get { return m_record.LastDeathTime; }
			set { m_record.LastDeathTime = value; }
		}

		/// <summary>
		/// Last time this Character was resurrected
		/// </summary>
		public DateTime LastResTime
		{
			get { return m_record.LastResTime; }
			set { m_record.LastResTime = value; }
		}


		protected override bool OnBeforeDeath()
		{
			if (Health == 0)
			{
				// make sure, we have Health
				Health = 1;
			}

			if (!m_region.RegionTemplate.NotifyPlayerBeforeDeath(this))
			{
				return false;
			}

			if (IsDueling)
			{
				// cancel duel before IsAlive is false etc.
				Duel.OnDeath(this);
				return false;
			}
			return true;
		}

		protected override void OnDeath()
		{
			m_record.LastDeathTime = DateTime.Now;
			MarkDead();

			// start release timer
			m_corpseReleaseTimer = new TimerEntry(dt => ReleaseCorpse());
			m_corpseReleaseTimer.Start(Corpse.AutoReleaseDelay, 0);
		}

		internal protected override void OnResurrect()
		{
			base.OnResurrect();
			CorpseReleaseFlags &= ~CorpseReleaseFlags.ShowCorpseAutoReleaseTimer;

			if (m_corpse != null)
			{
				Corpse = null;
			}
			m_record.LastResTime = DateTime.Now;

			CharacterHandler.SendCorpseReclaimDelay(m_client, Corpse.MinReclaimDelay);

			if (m_region != null)
			{
				m_region.RegionTemplate.NotifyPlayerResurrected(this);
			}
		}

		/// <summary>
		/// Resurrects, applies ResurrectionSickness and damages Items, if applicable
		/// </summary>
		public void ResurrectWithConsequences()
		{
			Resurrect();

			if (Level > 10)
			{
				// Apply resurrection sickness and durability loss (see http://www.wowwiki.com/Death)
				SpellCast.TriggerSelf(SpellId.ResurrectionSickness);
				m_inventory.Iterate(item =>
				{
					if (item.MaxDurability > 0)
					{
						item.Durability = Math.Max(0, item.Durability -
							(((item.Durability * PlayerInventory.SHResDurabilityLossPct) + 50) / 100));
					}
					return true;
				});
			}
		}

		/// <summary>
		/// Marks this Character dead (just died, Corpse not released)
		/// </summary>
		void MarkDead()
		{
			CorpseReleaseFlags |= CorpseReleaseFlags.ShowCorpseAutoReleaseTimer;
			IncMechanicCount(SpellMechanic.Rooted);

			var healer = m_region.GetNearestSpiritHealer(ref m_position);
			if (healer != null)
			{
				CharacterHandler.SendHealerPosition(m_client, healer);
			}
		}

		/// <summary>
		/// Characters become Ghosts after they released the Corpse
		/// </summary>
		void BecomeGhost()
		{
			SpellCast.Start(SpellHandler.Get(SpellId.Ghost), true, this);
		}

		/// <summary>
		/// 
		/// </summary>
		protected internal override void OnDamageAction(IDamageAction action)
		{
			base.OnDamageAction(action);

			var pvp = action.Attacker.IsPvPing;
			var chr = action.Attacker.CharacterMaster;

			var killingBlow = !IsAlive;

			if (action.Attacker != null &&
				m_activePet != null &&
				m_activePet.CanBeAggroedBy(action.Attacker))
			{
				m_activePet.ThreatCollection.AddNewIfNotExisted(action.Attacker);
			}

			if (pvp && chr.IsInBattleground)
			{
				// Add BG stats
				var attackerStats = chr.Battlegrounds.Stats;
				var victimStats = Battlegrounds.Stats;
				attackerStats.TotalDamage += action.ActualDamage;
				if (killingBlow)
				{
					attackerStats.KillingBlows++;
				}
				if (victimStats != null)
				{
					victimStats.Deaths++;
				}
			}

			if (killingBlow)
			{
				if (!pvp)
				{
					// durability loss
					m_inventory.ApplyDurabilityLoss(PlayerInventory.DeathDurabilityLossPct);
				}

				m_region.RegionTemplate.NotifyPlayerDied(action);
			}
		}

		/// <summary>
		/// Finds the item for the given slot. Unequips it if it may not currently be used.
		/// Returns the item to be equipped or null, if invalid.
		/// </summary>
		protected override IWeapon GetOrInvalidateItem(InventorySlotType type)
		{
			var slot = (int)ItemMgr.EquipmentSlotsByInvSlot[(int)type][0];
			var item = m_inventory[slot];
			if (item == null)
			{
				return null;
			}

			InventoryError err = InventoryError.OK;
			m_inventory.Equipment.CheckAdd(slot, 1, item, ref err);
			if (err == InventoryError.OK)
			{
				return item;
			}
			else
			{
				item.Unequip();
				return null;
			}
		}

		protected override void OnHeal(Unit healer, SpellEffect effect, int value)
		{
			base.OnHeal(healer, effect, value);
			if (healer is Character)
			{
				var chr = (Character)healer;
				if (chr.IsInBattleground)
				{
					chr.Battlegrounds.Stats.TotalHealing += value;
				}
			}
		}

		/// <summary>
		/// Spawns the corpse and teleports the dead Character to the nearest SpiritHealer
		/// </summary>
		internal void ReleaseCorpse()
		{
			if (IsAlive)
			{
				return;
			}

			DecMechanicCount(SpellMechanic.Rooted);
			//ClearSelfKnowledge();
			BecomeGhost();

			Corpse = SpawnCorpse(false, false);
			m_record.CorpseX = m_corpse.Position.X;
			m_record.CorpseY = m_corpse.Position.Y;
			m_record.CorpseZ = m_corpse.Position.Z;
			m_record.CorpseO = m_corpse.Orientation;
			m_record.CorpseRegion = m_region.Id;		// we are spawning the corpse in the same region
			m_corpseReleaseTimer.Stop();

			// we need health to walk again
			SetUInt32(UnitFields.HEALTH, 1);

			m_region.OnSpawnedCorpse(this);
		}

		/// <summary>
		/// Spawns and returns a new Corpse at the Character's current location
		/// </summary>
		/// <param name="bones"></param>
		/// <param name="lootable"></param>
		/// <returns></returns>
		public Corpse SpawnCorpse(bool bones, bool lootable)
		{
			return SpawnCorpse(bones, lootable, m_region, m_position, m_orientation);
		}

		/// <summary>
		/// Spawns and returns a new Corpse at the given location
		/// </summary>
		/// <param name="bones"></param>
		/// <param name="lootable"></param>
		/// <returns></returns>
		public Corpse SpawnCorpse(bool bones, bool lootable, Region region, Vector3 pos, float o)
		{
			var corpse = new Corpse(this, pos, o, DisplayId, Facial, Skin,
				HairStyle, HairColor, FacialHair, GuildId, Gender, Race,
				bones ? CorpseFlags.Bones : CorpseFlags.None, lootable ? CorpseDynamicFlags.PlayerLootable : CorpseDynamicFlags.None);

			for (var i = EquipmentSlot.Head; i <= EquipmentSlot.Tabard; i++)
			{
				var item = m_inventory[(int)i];
				if (item != null)
				{
					corpse.SetItem(i, item.Template);
				}
			}

			corpse.Position = pos;
			region.AddObjectLater(corpse);
			return corpse;
		}

		/// <summary>
		/// Tries to teleport to the next SpiritHealer, if there is any.
		/// 
		/// TODO: Graveyards
		/// </summary>
		public void TeleportToNearestGraveyard()
		{
			TeleportToNearestGraveyard(true);
		}

		/// <summary>
		/// Tries to teleport to the next SpiritHealer, if there is any.
		/// 
		/// TODO: Graveyards
		/// </summary>
		public void TeleportToNearestGraveyard(bool allowSameRegion)
		{
			if (allowSameRegion)
			{
				var healer = m_region.GetNearestSpiritHealer(ref m_position);
				if (healer != null)
				{
					TeleportTo(healer);
					return;
				}
			}

			if (m_region.RegionTemplate.RepopRegion != null)
			{
				TeleportTo(m_region.RegionTemplate.RepopRegion, m_region.RegionTemplate.RepopPosition);
			}
			else
			{
				TeleportToBindLocation();
			}
		}
		#endregion

		#region Experience & Levels
		public LevelStatInfo ClassBaseStats
		{
			get { return m_archetype.GetLevelStats((uint)Level); }
		}

		public void UpdateRest()
		{
			if (m_restTrigger != null)
			{
				var now = DateTime.Now;
				RestXp += RestGenerator.GetRestXp(now - m_lastRestUpdate, this);

				m_lastRestUpdate = now;
			}
		}

		public void GainCombatXp(int experience, INamed killed, bool gainRest)
		{
			if (Level >= RealmServerConfiguration.MaxCharacterLevel)
			{
				return;
			}

			// Generate the message to send
			var message = string.Format("{0} dies, you gain {1} experience.", killed.Name, experience);

			XP += experience + (experience*KillExperienceGainModifierPercent/100);
			if (gainRest && RestXp > 0)
			{
				var bonus = Math.Min(RestXp, experience);
				message += string.Format(" (+{0} exp Rested bonus)", bonus);
				XP += bonus;
				RestXp -= bonus;
			}

			// Send the message
			ChatMgr.SendCombatLogExperienceMessage(this, message);
			TryLevelUp();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="experience"></param>
		/// <param name="gainRest">If true, subtracts the given amount of experience from RestXp and adds it ontop of the given xp</param>
		public void GainXp(int experience)
		{
			GainXp(experience, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="experience"></param>
		/// <param name="gainRest">If true, subtracts the given amount of experience from RestXp and adds it ontop of the given xp</param>
		public void GainXp(int experience, bool gainRest)
		{
            XP += experience + (experience * KillExperienceGainModifierPercent / 100);
			if (gainRest && RestXp > 0)
			{
				var bonus = Math.Min(RestXp, experience);
				XP += bonus;
				RestXp -= bonus;
			}

			TryLevelUp();
		}

		internal bool TryLevelUp()
		{
			var nextLevelXp = NextLevelXP;
			var level = Level;
			var leveled = false;
			var xp = XP;

			while (xp >= nextLevelXp && level < RealmServerConfiguration.MaxCharacterLevel)
			{
				XP = xp -= nextLevelXp;
				Level = ++level;
				NextLevelXP = nextLevelXp = XpGenerator.GetXpForlevel(level + 1);

				if (level >= 10)
				{
					FreeTalentPoints++;
				}

				var evt = LeveledUp;
				if (evt != null)
				{
					evt(this);
				}
				leveled = true;
			}

			if (leveled)
			{
				ModStatsForLevel(level);
				m_auras.ReapplyAllAuras();
				SaveLater();
				return true;
			}
			return false;
		}

		public void ModStatsForLevel(int level)
		{
			var lvlClassSetting = m_archetype.Class.GetLevelSetting(level);
			var lvlStats = m_archetype.GetLevelStats((uint)level);

			var oldPower = BasePower;
			var oldHealth = BaseHealth;
			var oldStrength = Strength;
			var oldAgility = Agility;
			var oldStamina = Stamina;
			var oldIntellect = Intellect;
			var oldSpirit = Spirit;

			SetBaseStat(StatType.Strength, lvlStats.Strength);
			SetBaseStat(StatType.Agility, lvlStats.Agility);
			SetBaseStat(StatType.Stamina, lvlStats.Stamina);
			SetBaseStat(StatType.Intellect, lvlStats.Intellect);
			SetBaseStat(StatType.Spirit, lvlStats.Spirit);

			if (PowerType == PowerType.Mana)
			{
				BasePower = lvlClassSetting.Mana;
			}

			BaseHealth = lvlClassSetting.Health;

			SetInt32(UnitFields.HEALTH, MaxHealth);

			UpdatePlayedTime();
			LevelPlayTime = 0;

			// includes boni through stat-changes
			var healthGain = (BaseHealth - oldHealth);
			var powerGain = (BasePower - oldPower);
			var strGain = lvlStats.Strength - oldStrength;
			var agiGain = lvlStats.Agility - oldAgility;
			var staGain = lvlStats.Stamina - oldStamina;
			var intGain = lvlStats.Intellect - oldIntellect;
			var spiGain = lvlStats.Spirit - oldSpirit;

			CharacterHandler.SendLevelUpInfo(Client, level, healthGain, powerGain, strGain, agiGain, staGain,
											 intGain, spiGain);

			//skills
			Skills.UpdateSkillsForLevel(level);
		}
		#endregion

		#region Chat

		/// <summary>
		/// Adds the given language
		/// </summary>
		public void AddLanguage(ChatLanguage lang)
		{
			var desc = LanguageHandler.GetLanguageDescByType(lang);
			AddLanguage(desc);
		}

		public void AddLanguage(LanguageDescription desc)
		{

			if (!Spells.Contains((uint)desc.SpellId))
			{
				Spells.AddSpell(desc.SpellId);
			}

			if (!m_skills.Contains(desc.SkillId))
			{
				m_skills.Add(desc.SkillId, 300, 300, true);
			}
		}

		/// <summary>
		/// Returns whether the given language can be understood by this Character
		/// </summary>
		public bool CanSpeak(ChatLanguage language)
		{
			return KnownLanguages.Contains(language);
		}

		public void SendMessage(string message)
		{
			ChatMgr.SendSystemMessage(this, message);
			ChatMgr.ChatNotify(null, message, ChatLanguage.Universal, ChatMsgType.System, this);
		}

		public void SendMessage(IChatter sender, string message)
		{
			ChatMgr.SendWhisper(sender, this, message);
		}

		/// <summary>
		/// Sends a message to the client.
		/// </summary>
		public void SendSystemMessage(RealmLangKey key, params object[] args)
		{
			ChatMgr.SendSystemMessage(this, RealmLocalizer.Instance.Translate(Locale, key, args));
		}

		/// <summary>
		/// Sends a message to the client.
		/// </summary>
		public void SendSystemMessage(string msg, params object[] args)
		{
			ChatMgr.SendSystemMessage(this, string.Format(msg, args));
		}

		public void Notify(RealmLangKey key, params object[] args)
		{
			Notify(RealmLocalizer.Instance.Translate(Locale, key, args));
		}

		/// <summary>
		/// Flashes a notification in the middle of the screen
		/// </summary>
		public void Notify(string msg, params object[] args)
		{
			MiscHandler.SendNotification(this, string.Format(msg, args));
		}

		public void SayGroup(string msg, params object[] args)
		{
			SayGroup(string.Format(msg, args));
		}

		public void SayGroup(string msg)
		{
			this.SayGroup(SpokenLanguage, msg);
		}

		public override void Say(string msg)
		{
			this.SayYellEmote(ChatMsgType.Say, SpokenLanguage, msg);
		}

		public override void Yell(string msg)
		{
			this.SayYellEmote(ChatMsgType.Yell, SpokenLanguage, msg);
		}

		public override void Emote(string msg)
		{
			this.SayYellEmote(ChatMsgType.Emote, SpokenLanguage, msg);
		}
		#endregion

		public void Send(RealmPacketOut packet)
		{
			m_client.Send(packet);
		}

		public void Send(byte[] packet)
		{
			m_client.Send(packet);
		}

		/// <summary>
		/// Change target and/or amount of combo points
		/// </summary>
		public override bool ModComboState(Unit target, int amount)
		{
			if (base.ModComboState(target, amount))
			{
				CombatHandler.SendComboPoints(this);
				return true;
			}

			return false;
		}

		#region Interaction with NPCs & GameObjects
		/// <summary>
		/// Called whenever this Character interacts with any WorldObject
		/// </summary>
		/// <param name="obj"></param>
		public void OnInteract(WorldObject obj)
		{
			//Dismount();
			StandState = StandState.Stand;

			if (obj is NPC)
			{
				var npc = (NPC)obj;
				Reputations.OnTalkWith(npc);
				npc.Entry.NotifyInteracting(npc, this);
			}

		}

		/// <summary>
		/// Opens this character's bankbox
		/// </summary>
		public void OpenBank(WorldObject banker)
		{
			OnInteract(banker);
			m_inventory.CurrentBanker = banker;
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SHOW_BANK, 8))
			{
				packet.Write(banker.EntityId);
				m_client.Send(packet);
			}
		}

		/// <summary>
		/// Tries to bind this Character to the given NPC.
		/// </summary>
		/// <returns>whether the given NPC is an actual InnKeeper and this char could be bound to that Inn.</returns>
		public bool TryBindTo(NPC innKeeper)
		{
			OnInteract(innKeeper);
			if (innKeeper.BindPoint != NamedWorldZoneLocation.Zero && innKeeper.CanInteractWith(this))
			{
				BindTo(innKeeper);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Binds this Character to that Location and will teleport him/her whenever the Hearthston is used.
		/// Adds a new HearthStone if the Character doesn't have one.
		/// Make sure that the given NPC is an actual InnKeeper and has a valid BindPoint (else use <c>TryBindTo</c> instead).
		/// </summary>
		public void BindTo(NPC binder)
		{
			m_inventory.EnsureHearthStone();
			BindTo(binder, binder.BindPoint);
		}

		public void BindTo(WorldObject binder, IWorldZoneLocation location)
		{
			m_bindLocation = location;
			// NPCHandler.SendBindConfirm(this, innKeeper, innKeeper.BindPoint.Zone);
			CharacterHandler.SendBindUpdate(this, location);
			NPCHandler.SendPlayerBound(this, binder, location.ZoneId);
		}
		#endregion

		#region Movement Handling
		/// <summary>
		/// Is called whenever the Character moves up or down in water or while flying.
		/// </summary>
		internal protected void MovePitch(float moveAngle)
		{
		}

		/// <summary>
		/// Is called whenever the Character falls
		/// </summary>
		internal protected void OnFalling()
		{
			if (m_fallStart == 0)
			{
				m_fallStart = Environment.TickCount;
				m_fallStartHeight = m_position.Z;
			}


			if (IsFlying || !IsAlive || GodMode)
			{
				return;
			}
			// TODO Immunity against environmental damage

		}

		public bool IsSwimming
		{
			get { return MovementFlags.HasFlag(MovementFlags.Swimming); }
		}

		public bool IsUnderwater
		{
			get { return m_position.Z < m_swimSurfaceHeight - 0.5f; }
		}

		internal protected void OnSwim()
		{
			// TODO: Lookup liquid type and verify heights
			if (!IsSwimming)
			{
				m_swimStart = DateTime.Now;
			}
			else
			{

			}
		}

		internal protected void OnStopSwimming()
		{
			m_swimSurfaceHeight = -2048;
		}

		/// <summary>
		/// Is called whenever the Character is moved while on Taxi, Ship, elevator etc
		/// </summary>
		internal protected void MoveTransport(ref Vector4 transportLocation)
		{
			SendSystemMessage("You have been identified as cheater: Faking transport movement!");
		}

		/// <summary>
		/// Is called whenever a Character moves
		/// </summary>
		public override void OnMove()
		{
			base.OnMove();

			if (m_standState != StandState.Stand)
			{
				StandState = StandState.Stand;
			}

			if (m_currentRitual != null)
			{
				m_currentRitual.Remove(this);
			}

			// TODO: Change speedhack detection
			// TODO: Check whether the character is really in Taxi
			var now = Environment.TickCount;
			if (m_fallStart > 0 && now - m_fallStart > 3000 && m_position.Z == LastPosition.Z)
			{
				if (IsAlive && Flying == 0 && Hovering == 0 && FeatherFalling == 0 && !IsImmune(DamageSchool.Physical))
				{
					var fallDamage = FallDamageGenerator.GetFallDmg(this, m_fallStartHeight - m_position.Z);

					//if (fallDamage > 0)
					//    DoEnvironmentalDamage(EnviromentalDamageType.Fall, fallDamage);

					m_fallStart = 0;
					m_fallStartHeight = 0;
				}
			}

			if (SpeedHackCheck)
			{
				var msg = "You have been identified as a SpeedHacker. - Byebye!";

				// simple SpeedHack protection
				int latency = Client.Latency;
				int delay = now - m_lastMoveTime + Math.Max(1000, latency);

				float speed = Flying > 0 ? FlightSpeed : RunSpeed;
				float maxDistance = (speed / 1000f) * delay * SpeedHackToleranceFactor;
				if (!IsInRadius(ref LastPosition, maxDistance))
				{
					// most certainly a speed hacker
					log.Warn("WARNING: Possible speedhacker [{0}] moved {1} yards in {2} milliseconds (Latency: {3}, Tollerance: {4})",
							 this, GetDistance(ref LastPosition), delay, latency, SpeedHackToleranceFactor);
				}

				Kick(msg);
			}

			LastPosition = MoveControl.Mover.Position;
		}

		public void SetMover(WorldObject mover, bool canControl)
		{
			MoveControl.Mover = mover;
			MoveControl.CanControl = canControl;

			if (mover == null)
			{
				CharacterHandler.SendControlUpdate(this, this, canControl);
			}
			else
			{
				CharacterHandler.SendControlUpdate(this, mover, canControl);
			}
		}

		public void ResetMover()
		{
			MoveControl.Mover = this;
			MoveControl.CanControl = true;
		}

		/// <summary>
		/// Is called whenever a new object appears within vision range of this Character
		/// </summary>
		public void OnEncountered(WorldObject obj)
		{
			obj.AreaCharCount++;
			KnownObjects.Add(obj);
			SendUnknownState(obj);
		}

		/// <summary>
		/// Sends yet unknown information about a new object,
		/// such as Aura packets
		/// </summary>
		/// <param name="obj"></param>
		private void SendUnknownState(WorldObject obj)
		{
			if (obj is Unit)
			{
				var unit = (Unit)obj;

				if (unit.Auras.VisibleAuraCount > 0)
				{
					AuraHandler.SendAllAuras(this, unit);
				}
			}
		}

		/// <summary>
		/// Is called whenever an object leaves this Character's sight
		/// </summary>
		public void OnOutOfRange(WorldObject obj)
		{
			obj.AreaCharCount--;
			if (obj is Character && m_observers != null)
			{
				if (m_observers.Remove((Character)obj))
				{
					// Character was observing: Now destroy items for him
					for (var i = (InventorySlot)0; i < InventorySlot.Bag1; i++)
					{
						var item = m_inventory[i];
						if (item != null)
						{
							item.SendDestroyToPlayer((Character)obj);
						}
					}
				}
			}

			if (obj == DuelOpponent && !Duel.IsActive)
			{
				// opponent vanished before Duel started: Cancel duel
				Duel.Dispose();
			}

			if (obj == m_target)
			{
				// unset current Target
				ClearTarget();
			}

			if (obj == m_activePet)
			{
				ActivePet = null;
			}

			if (GossipConversation != null && obj == GossipConversation.Speaker && GossipConversation.Character == this)
			{
				// stop conversation with a vanished object
				GossipConversation.Dispose();
			}

			if (!(obj is Transport))
			{
				KnownObjects.Remove(obj);

				// send the destroy packet
				obj.SendDestroyToPlayer(this);
			}
		}

		/// <summary>
		/// Is called whenever this Character was added to a new region
		/// </summary>
		internal protected override void OnEnterRegion()
		{
			base.OnEnterRegion();

			// when removed from region, make sure the Character forgets everything and gets everything re-sent
			ClearSelfKnowledge();

			m_lastMoveTime = Environment.TickCount;
			LastPosition = m_position;

			AddPostUpdateMessage(() =>
			{
				// Add Honorless Target buff
				if (m_zone != null && m_zone.Template.IsPvP)
				{
					SpellCast.TriggerSelf(SpellId.HonorlessTarget);
				}
			});

			if (IsPetActive)
			{
				// actually spawn pet
				IsPetActive = true;
			}
		}

		protected internal override void OnLeavingRegion()
		{
			if (m_activePet != null && m_activePet.IsInWorld)
			{
				m_activePet.Region.RemoveObject(m_activePet);
			}

			if (m_minions != null)
			{
				foreach (var minion in m_minions)
				{
					minion.Delete();
				}
			}

			base.OnLeavingRegion();
		}
		#endregion

		#region Quests

		/// <summary>
		/// QuestLog of the character
		/// </summary>
		private QuestLog m_questLog;

		/// <summary>
		/// Gets the quest log.
		/// </summary>
		/// <value>The quest log.</value>
		public QuestLog QuestLog
		{
			get { return m_questLog; }
		}

		public bool CanGiveQuestTo(Character chr)
		{
			return chr.IsAlliedWith(this); // since 3.0 you can share quests within any range
		}
		#endregion

		#region Combat
		/// <summary>
		/// The <see cref="Duel"/> this Character is currently engaged in (or null if not dueling)
		/// </summary>
		public Duel Duel
		{
			get;
			internal set;
		}

		/// <summary>
		/// The opponent that this Character is currently dueling with (or null if not dueling)
		/// </summary>
		public Character DuelOpponent
		{
			get;
			internal set;
		}

		/// <summary>
		/// whether this Character is currently dueling with someone else
		/// </summary>
		public bool IsDueling
		{
			get { return Duel != null && Duel.IsActive; }
		}

		public override bool IsFriendlyWith(IFactionMember opponent)
		{
			if (IsAlliedWith(opponent))
			{
				return true;
			}

			var rep = m_reputations[opponent.Faction.ReputationIndex];
			return rep != null && rep.Standing >= Standing.Friendly;
		}

		public override bool IsHostileWith(IFactionMember opponent)
		{
			if (opponent == this || (opponent is Unit && ((Unit)opponent).Master == this))
			{
				return false;
			}

			if (opponent is Character)
			{
				var chr = (Character)opponent;
				return CanPvP(chr);
			}

			return m_reputations.IsHostile(opponent.Faction);
		}

		public override bool MayAttack(IFactionMember opponent)
		{
			if (opponent == this || (opponent is Unit && ((Unit)opponent).Master == this))
			{
				return false;
			}

			if (opponent is Character)
			{
				var chr = (Character)opponent;
				return CanPvP(chr);
			}

			return m_reputations.CanAttack(opponent.Faction);
		}

		public bool CanPvP(Character chr)
		{
			var state = PvPState;
			if (chr.PvPState < state)
			{
				state = chr.PvPState;
			}
			if (state == PvPState.FFAPVP)
			{
				return true;
			}

			return
				(state == PvPState.PVP && chr.Faction.IsAlliance != m_faction.IsAlliance) ||					// default case
				(IsInBattleground && chr.IsInBattleground && chr.Battlegrounds.Team != Battlegrounds.Team) ||	// battlegrounds
				(DuelOpponent == chr && Duel.IsActive);															// duels
		}

		/// <summary>
		/// One can only cast beneficial spells on people that we are allied with
		/// </summary>
		/// <param name="opponent"></param>
		/// <returns></returns>
		public override bool IsAlliedWith(IFactionMember opponent)
		{
			if (opponent == this ||
				(opponent is Unit && ((Unit)opponent).Master == this))
			{
				return true;
			}

			if (!(opponent is Character) && opponent is WorldObject)
			{
				opponent = ((WorldObject)opponent).Master;
			}

			if (opponent is Character)
			{
				if (IsInBattleground)
				{
					return Battlegrounds.Team == ((Character)opponent).Battlegrounds.Team;
				}

				var group = Group;
				if (group != null && ((Character)opponent).Group == group)
				{
					// cannot ally with duelists
					return DuelOpponent == null && ((Character)opponent).DuelOpponent == null;
				}
			}

			return false;
		}

		public override bool IsInSameDivision(IFactionMember opponent)
		{
			if (opponent == this ||
				(opponent is Unit && ((Unit)opponent).Master == this))
			{
				return true;
			}

			if (!(opponent is Character) && opponent is WorldObject)
			{
				opponent = ((WorldObject)opponent).Master;
			}

			if (opponent is Character)
			{
				if (IsInBattleground)
				{
					return Battlegrounds.Team == ((Character)opponent).Battlegrounds.Team;
				}

				var group = SubGroup;
				if (group != null && ((Character)opponent).SubGroup == group)
				{
					// cannot ally with duelists
					return DuelOpponent == null && ((Character)opponent).DuelOpponent == null;
				}
			}
			return false;
		}

		protected override void OnEnterCombat()
		{
			CancelLooting();
			CancelLogout();
		}

		protected override bool CheckCombatState()
		{
			if (!m_isFighting)
			{
				if (NPCAttackerCount == 0 &&
					(m_activePet == null || m_activePet.NPCAttackerCount == 0) &&
					(Environment.TickCount - m_lastCombatTime) >= CombatDeactivationDelay &&
					!m_auras.HasHarmfulAura())
				{
					// leave combat if we didn't fight for a while and have no debuffs on us
					IsInCombat = false;
				}
				return false;
			}
			return base.CheckCombatState();
		}

		/// <summary>
		/// Adds all damage boni and mali
		/// </summary>
		public void AddDamageModsToAction(DamageAction action)
		{
			if (!action.IsDot)
			{
				// does not add to dot
				action.Damage = GetTotalDamageDoneMod(action.UsedSchool, action.Damage, action.Spell);
			}
			else if (action.SpellEffect != null)
			{
				// periodic damage mod
				action.Damage = PlayerSpells.GetModifiedInt(SpellModifierType.PeriodicEffectValue, action.Spell, action.Damage);
			}
		}

		public override int AddHealingModsToAction(int healValue, SpellEffect effect, DamageSchool school)
		{
			healValue += (int)((healValue * HealingDoneModPct) / 100f);
			healValue += HealingDoneMod;
			if (effect != null)
			{
				healValue = PlayerSpells.GetModifiedInt(SpellModifierType.SpellPower, effect.Spell, healValue);
			}

			return healValue;
		}

		public override int GetGeneratedThreat(int dmg, DamageSchool school, SpellEffect effect)
		{
			var threat = base.GetGeneratedThreat(dmg, school, effect);
			if (effect != null)
			{
				threat = PlayerSpells.GetModifiedInt(SpellModifierType.Threat, effect.Spell, threat);
			}
			return threat;
		}

		public override float CalcCritDamage(float dmg, Unit victim, SpellEffect effect)
		{
			dmg = base.CalcCritDamage(dmg, victim, effect);
			if (effect != null)
			{
				return PlayerSpells.GetModifiedFloat(SpellModifierType.CritDamage, effect.Spell, dmg);
			}
			return dmg;
		}

		public override float CalcCritChanceBase(Unit victim, SpellEffect effect, IWeapon weapon)
		{
			float chance;

			if (weapon.IsRanged)
			{
				chance = CritChanceRangedPct;
			}
			else
			{
				chance = CritChanceMeleePct;
			}

			if (effect != null)
			{
				chance = PlayerSpells.GetModifiedFloat(SpellModifierType.CritChance, effect.Spell, chance);
			}

			return chance;
		}
		#endregion

		#region Looting
		/// <summary>
		/// Whether this Character will automatically pass on loot rolls.
		/// </summary>
		public bool PassOnLoot
		{
			get;
			set;
		}

		/// <summary>
		/// The LooterEntry represents this Character's current loot status
		/// </summary>
		public LooterEntry LooterEntry
		{
			get
			{
				if (m_looterEntry == null)
				{
					m_looterEntry = new LooterEntry(this);
				}
				return m_looterEntry;
			}
		}

		/// <summary>
		/// whether this Character is currently looting something
		/// </summary>
		public bool IsLooting
		{
			get
			{
				return m_looterEntry != null && m_looterEntry.Loot != null;
			}
		}

		/// <summary>
		/// Cancels looting (if this Character is currently looting something)
		/// </summary>
		public void CancelLooting()
		{
			if (m_looterEntry != null)
			{
				m_looterEntry.Release();
			}
		}
		#endregion

		public BaseRelation GetRelationTo(Character chr, CharacterRelationType type)
		{
			return RelationMgr.Instance.GetRelation(EntityId.Low, chr.EntityId.Low, type);
		}

		/// <summary>
		/// Returns whether this Character ignores the Character with the given low EntityId.
		/// </summary>
		/// <returns></returns>
		public bool IsIgnoring(IUser user)
		{
			return RelationMgr.Instance.HasRelation(EntityId.Low, user.EntityId.Low, CharacterRelationType.Ignored);
		}

		/// <summary>
		/// Indicates whether the two Characters are in the same <see cref="Group"/>
		/// </summary>
		/// <param name="chr"></param>
		/// <returns></returns>
		public bool IsAlliedWith(Character chr)
		{
			return m_groupMember != null && chr.m_groupMember != null && m_groupMember.Group == chr.m_groupMember.Group;
		}

		/// <summary>
		/// Binds Character to start position if none other is set
		/// </summary>
		void CheckBindLocation()
		{
			if (!m_bindLocation.IsValid())
			{
				BindTo(this, m_archetype.StartLocation);
			}
		}

		public void TeleportToBindLocation()
		{
			TeleportTo(BindLocation);
		}

		public bool CanFly
		{
			get
			{
				return (m_region.CanFly && (m_zone == null || m_zone.Flags.HasFlag(ZoneFlags.CanFly))) || Role.IsStaff;
			}
		}

		#region Mounts

		public override void Mount(uint displayId)
		{
			if (m_activePet != null)
			{
				// remove active pet
				m_activePet.RemoveFromRegion();
			}

			base.Mount(displayId);
		}

		protected internal override void DoDismount()
		{
			if (IsPetActive)
			{
				// put pet into world
				PlaceOnTop(ActivePet);
			}
			base.DoDismount();
		}
		#endregion

		#region Summoning
		public SummonRequest SummonRequest
		{
			get
			{
				return m_summonRequest;
			}
		}

		/// <summary>
		/// May be executed from outside of this Character's region's context
		/// </summary>
		public void StartSummon(ISummoner summoner)
		{
			StartSummon(summoner, SummonRequest.DefaultTimeout);
		}

		/// <summary>
		/// May be executed from outside of this Character's region's context
		/// </summary>
		/// <param name="summoner"></param>
		/// <param name="timeoutSeconds"></param>
		public void StartSummon(ISummoner summoner, int timeoutSeconds)
		{
			m_summonRequest = new SummonRequest
			{
				ExpiryTime = DateTime.Now.AddSeconds(timeoutSeconds),
				TargetPos = summoner.Position,
				TargetZone = summoner.Zone,
				TargetRegion = summoner.Region
			};

			// make sure the Region was set or else the summoner was disposed before the Request completed
			if (m_summonRequest.TargetRegion != null)
			{
				var client = m_client;
				if (client != null)
				{
					CharacterHandler.SendSummonRequest(client, summoner,
						summoner.Zone != null ? summoner.ZoneTemplate.Id : ZoneId.None,
						timeoutSeconds * 1000);
				}
			}
			else
			{
				//log.Warn("Tried to teleport {0} to a Summoner without a Region: {1}", this, summoner);
			}
		}


		/// <summary>
		/// Cancels a current summon request
		/// </summary>
		public void CancelSummon(bool notify)
		{
			if (m_summonRequest != null)
			{
				if (m_summonRequest.Portal != null && m_summonRequest.Portal.IsInWorld)
				{
					m_summonRequest.Portal.Delete();
				}
				if (notify)
				{
					CharacterHandler.SendCancelSummonRequest(this);
				}
				m_summonRequest = null;
			}
		}
		#endregion

		#region AI
		public override LinkedList<WaypointEntry> Waypoints
		{
			get { return null; }
		}

		public override SpawnPoint SpawnPoint
		{
			get { return null; }
		}
		#endregion

		public void ActivateAllTaxiNodes()
		{
			for (var i = 0; i < TaxiMgr.PathNodesById.Length; i++)
			{
				var node = TaxiMgr.PathNodesById[i];
				if (node != null)
				{
					TaxiNodes.Activate(node);
					SendSystemMessage("Activated Node: " + node);
				}
			}
		}

		public override void SetZone(Zone newZone)
		{
			base.SetZone(newZone);
			if (newZone != null)
			{
				m_region.CallDelayed(CharacterHandler.ZoneUpdateDelay, () =>
				{
					if (IsInWorld && Zone == newZone)
					{
						SetZoneExplored(m_zone.Template, true);
					}
				});
			}
		}

		public override void CancelAllActions()
		{
			base.CancelAllActions();
			if (m_target != null)
			{
				ClearTarget();
			}
			if (TradeInfo != null)
			{
				TradeInfo.Cancel();
			}
		}

		public void ClearTarget()
		{
			CharacterHandler.SendClearTarget(this, m_target);
			Target = null;
		}

		#region Spells
		public override int GetPowerCost(DamageSchool school, Spell spell, int cost)
		{
			cost = base.GetPowerCost(school, spell, cost);
			cost = PlayerSpells.GetModifiedInt(SpellModifierType.PowerCost, spell, cost);
			return cost;
		}
		#endregion

		#region IHasTalents
		public SpecProfile SpecProfile
		{
			get { return Record.SpecProfile; }
			set { Record.SpecProfile = value; }
		}

		public int GetTalentResetPrice()
		{
			if (GodMode)
			{
				return 0;
			}

			var tiers = TalentMgr.TalentResetPriceTiers;
			var lastPriceTier = Record.TalentResetPriceTier;
			var lastResetTime = Record.LastTalentResetTime;
			if (lastResetTime == null)
			{
				return tiers[0];
			}

			var timeLapse = DateTime.Now - lastResetTime.Value;
			if (lastPriceTier < 4) return tiers[lastPriceTier];

			var numDiscounts = timeLapse.Days / 30;
			var newPriceTier = lastPriceTier - numDiscounts;
			if (newPriceTier < 3)
			{
				return tiers[3];
			}

			if (newPriceTier > (tiers.Length - 1))
			{
				return tiers[tiers.Length - 1];
			}

			return tiers[newPriceTier];
		}

		public void ResetFreeTalentPoints()
		{
			// One talent point for level 10 and above
			FreeTalentPoints = (Level - 9);
		}

		public void ResetTalents()
		{
			var price = Talents.ResetAllPrice;
			if (Money < price) return;

			ResetFreeTalentPoints();
			Record.LastTalentResetTime = DateTime.Now;
			Record.TalentResetPriceTier++;
			Money -= price;

			SpecProfile.ResetActiveTalentGroup();
		}

		#endregion

		#region Implementation of IInstanceHolder

		public Character InstanceLeader
		{
			get { return this; }
		}

		public InstanceCollection InstanceLeaderCollection
		{
			get { return Instances; }
		}

		public bool HasInstanceCollection
		{
			get { return m_InstanceCollection != null; }
		}

		/// <summary>
		/// Auto-created if not already existing
		/// </summary>
		public InstanceCollection Instances
		{
			get
			{
				if (m_InstanceCollection == null)
				{
					m_InstanceCollection = new InstanceCollection(this);
				}
				return m_InstanceCollection;
			}
			set { m_InstanceCollection = value; }
		}

		public void ForeachInstanceHolder(Action<InstanceCollection> callback)
		{
			callback(Instances);
		}

		public BaseInstance GetActiveInstance(RegionTemplate regionTemplate)
		{
			var region = m_region;
			if (region != null && region.Id == region.Id)
			{
				return region as BaseInstance;
			}
			var instances = m_InstanceCollection;
			return instances != null ? instances.GetActiveInstance(regionTemplate) : null;
		}
		#endregion

		#region Battlegrounds
		/// <summary>
		/// Whether this Character is in a Battleground at the moment
		/// </summary>
		public bool IsInBattleground
		{
			get { return m_bgInfo != null && m_bgInfo.Team != null; }
		}

		/// <summary>
		/// Represents all <see cref="Battleground"/>-related information of this Character
		/// </summary>
		public BattlegroundInfo Battlegrounds
		{
			get
			{
				if (m_bgInfo == null)
				{
					m_bgInfo = new BattlegroundInfo(this);
				}
				return m_bgInfo;
			}
		}
		#endregion

		#region Honor
		/// <summary>
		/// Is called when the Character kills an Honorable target.
		/// </summary>
		/// <param name="victim">The Honorable character killed.</param>
		internal void OnHonorableKill(IDamageAction action)
		{
			var victim = (Character)action.Victim;
			var ptsForKill = CalcHonorForKill(victim);
			if (ptsForKill == 0) return;

			if (IsInBattleground)
			{
				var team = m_bgInfo.Team;
				var victimStats = victim.Battlegrounds.Stats;

				if (team == victim.Battlegrounds.Team ||
					(victimStats == null || victimStats.Deaths > BattlegroundMgr.MaxHonorableDeaths))
				{
					// don't count kills of town team, visitors or permanent losers
					return;
				}

				var stats = m_bgInfo.Stats;
				++stats.HonorableKills;
				team.DistributeSharedHonor(this, victim, ptsForKill);
			}
			else if (Group != null)
			{
				if (Faction.Group == victim.Faction.Group) return;
				Group.DistributeGroupHonor(this, victim, ptsForKill);
			}
			else
			{
				GiveHonorPoints(ptsForKill);
				KillsToday++;
				LifetimeHonorableKills++;
				HonorHandler.SendPVPCredit(this, ptsForKill * 10, victim);
			}

			if (m_zone != null)
			{
				m_zone.Template.OnHonorableKill(this, victim);
			}
		}

		private uint CalcHonorForKill(Character victim)
		{
			if (victim == this) return 0;
			if (!victim.YieldsXpOrHonor) return 0;

			var kLvl = Level;
			var vLvl = victim.Level;
			var maxLvlDiff = BattlegroundMgr.MaxLvlDiff;
			var maxHonor = (BattlegroundMgr.MaxHonor - 1);
			if (maxHonor < 0)
			{
				maxHonor = 0;
			}

			var lvlDiff = kLvl - vLvl;
			lvlDiff += maxLvlDiff;
			if (lvlDiff < 0) return 0;

			var slope = (maxHonor / (2.0f * maxLvlDiff));
			return (uint)Math.Round((slope * lvlDiff) + 1);
		}

		public void GiveHonorPoints(uint points)
		{
			HonorPoints += points;
			HonorToday += points;
		}

		public uint MaxPersonalArenaRating
		{
			get { return 0; }
		}
		#endregion

		#region PvP Flag

		public DateTime PvPEndTime = DateTime.Now;

		public void TogglePvPFlag()
		{
			SetPvPFlag(!IsPvPFlagSet);
		}

		public void SetPvPFlag(bool state)
		{
			IsPvPFlagSet = state;
			IsPvPTimerActive = !state;

			// If the PvP Flag is up and the character is not pvp-ing
			// or the pvp timer is set, override the pvp state to on
			if (state)
			{
				if (PvPState.HasFlag(PvPState.PVP) || PvPEndTime > DateTime.Now)
				{
					UpdatePvPState(true, true);
				}
				return;
			}

			// The flag is down and the character is pvp-ing in a non-hostile area
			// Set the timer to turn things off
			if (Zone != null)
			{
				if (!Zone.Template.IsHostileTo(this) && PvPState.HasFlag(PvPState.PVP))
				{
					SetPvPResetTimer();
				}
			}
		}

		public void UpdatePvPState(bool state, bool overridden)
		{
			if (!state || overridden)
			{
				SetPvPState(state);
				ClearPvPResetTimer();
				return;
			}

			// Flag is up. Check for running reset timer.
			// If running, reset it.
			if (PvPEndTime > DateTime.Now)
			{
				SetPvPResetTimer();
				return;
			}

			// Flag is up. No running reset timer.
			SetPvPState(true);
		}

		private void SetPvPResetTimer()
		{
			PvPEndTime = DateTime.Now.AddSeconds(300);
		}

		private void ClearPvPResetTimer()
		{
			PvPEndTime = DateTime.Now;
		}

		private void SetPvPState(bool state)
		{
			// TODO: minions?

			if (ActivePet != null)
			{
				if (state)
				{
					PvPState |= PvPState.PVP;
					ActivePet.PvPState |= PvPState.PVP;
				}
				else
				{
					PvPState &= ~PvPState.PVP;
					ActivePet.PvPState &= ~PvPState.PVP;
				}
				return;
			}

			if (state)
			{
				PvPState |= PvPState.PVP;
				return;
			}

			PvPState &= ~PvPState.PVP;
		}
		#endregion

		#region Barbershops
		/// <summary>
		/// Calculates the price of a purchase in a berber shop.
		/// </summary>
		/// <param name="newstyle"></param>
		/// <param name="newcolor"></param>
		/// <param name="newfacial"></param>
		/// <returns>The total price.</returns>
		public uint CalcBarberShopCost(byte newStyle, byte newColor, byte newFacial)
		{
			var level = Level;
			var style = HairStyle;
			var color = HairColor;
			var facial = FacialHair;

			// Happens if character chooses same style as before.
			if ((style == newStyle) && (color == newColor) && (facial == newFacial))
				return 0;

			var price = GameTables.BarberShopCosts[level - 1];

			// Should not happen?
			if (price == 0)
				return 0xFFFFFFFF;

			float cost = 0;

			if (style != newStyle)
			{
				cost += price;
			}
			else if (color != newColor)
			{
				cost += price * 0.5f;
			}

			if (facial != newFacial)
			{
				cost += price * 0.75f;
			}

			// We send in uint.
			return (uint)cost;
		}

		#endregion

		#region IUser
		public BaseCommand<RealmServerCmdArgs> SelectedCommand
		{
			get
			{
				var info = m_ExtraInfo;
				if (info != null)
				{
					return info.m_selectedCommand;
				}
				return null;
			}
			set
			{
				var info = m_ExtraInfo;
				if (info != null)
				{
					info.m_selectedCommand = value;
				}
			}
		}
		#endregion

		#region ITicketHandler
		/// <summary>
		/// The ticket that is currently being handled by this <see cref="ITicketHandler"/>
		/// </summary>
		public Ticket HandlingTicket
		{
			get
			{
				var info = m_ExtraInfo;
				if (info != null)
				{
					return info.m_handlingTicket;
				}
				return null;
			}
			set
			{
				var info = ExtraInfo;
				if (info != null)
				{
					info.m_handlingTicket = value;
				}
			}
		}

		public bool MayHandle(Ticket ticket)
		{
			var handler = ticket.m_handler;
			return handler == null || handler.Role <= Role;
		}
		#endregion

		#region ICharacterSet
		public int Count
		{
			get { return 1; }
		}

		public void ForeachCharacter(Action<Character> callback)
		{
			callback(this);
		}

		public Character[] GetCharacters()
		{
			return new[] { this };
		}
		#endregion

		public override string ToString()
		{
			return Name + " (ID: " + EntityId + ", Account: " + Account + ")";
		}
	}
}