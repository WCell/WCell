/*************************************************************************
 *
 *   file		: Owner.Fields.cs
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
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Constants.Quests;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.Core.Timers;
using WCell.RealmServer.AreaTriggers;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Database;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.GameObjects.Handlers;
using WCell.RealmServer.Global;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Guilds;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Help.Tickets;
using WCell.RealmServer.Instances;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.Items;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Mail;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.Network;
using WCell.RealmServer.Privileges;
using WCell.RealmServer.RacesClasses;
using WCell.RealmServer.Skills;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Talents;
using WCell.RealmServer.Taxi;
using WCell.Core;
using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.NPCs.Vehicles;
using WCell.RealmServer.Trade;
using WCell.Util.Graphics;
using WCell.RealmServer.Achievement;

namespace WCell.RealmServer.Entities
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Character
	{
		#region Fields

		protected string m_name;
		protected internal CharacterRecord m_record;

		/// <summary>
		/// All objects that are currently visible by this Character.
		/// Don't manipulate this collection.
		/// </summary>
		/// <remarks>Requires region context.</remarks>
		internal HashSet<WorldObject> KnownObjects = WorldObjectSetPool.Obtain();

		/// <summary>
		/// All objects that are currently in BroadcastRadius of this Character.
		/// Don't manipulate this collection.
		/// </summary>
		/// <remarks>Requires region context.</remarks>
		public readonly ICollection<WorldObject> NearbyObjects = new List<WorldObject>();

		protected TimerEntry m_logoutTimer;
		protected IRealmClient m_client;
		//protected int m_inRegear;

		public MoveControl MoveControl;

		private BattlegroundInfo m_bgInfo;
		protected InstanceCollection m_InstanceCollection;

		protected GroupMember m_groupMember;
		protected GroupUpdateFlags m_groupUpdateFlags = GroupUpdateFlags.None;

		protected GuildMember m_guildMember;

		/// <summary>
		/// All skills of this Character
		/// </summary>
		protected SkillCollection m_skills;

		/// <summary>
		/// All talents of this Character
		/// </summary>
		protected TalentCollection m_talents;

	    protected AchievementCollection m_achievements;

		protected PlayerInventory m_inventory;

		protected ReputationCollection m_reputations;

		protected MailAccount m_mailAccount;

		protected Archetype m_archetype;

		/// <summary>
		/// The current corpse of this Character or null
		/// </summary>
		protected Corpse m_corpse;

		/// <summary>
		/// Auto releases Corpse after expiring
		/// </summary>
		protected TimerEntry m_corpseReleaseTimer;

		/// <summary>
		/// All languages known to this Character
		/// </summary>
		protected internal readonly HashSet<ChatLanguage> KnownLanguages = new HashSet<ChatLanguage>();

		/// <summary>
		/// The time when this Character started falling
		/// </summary>
		protected int m_fallStart;
		/// <summary>
		/// The Z coordinate of the character when this character character falling
		/// </summary>
		protected float m_fallStartHeight;

		protected DateTime m_swimStart;
		protected float m_swimSurfaceHeight;

		protected DateTime m_lastPlayTimeUpdate;

		/// <summary>
		/// The position at which this Character was last (used for speedhack check)
		/// </summary>
		public Vector3 LastPosition;

		/// <summary>
		/// A bit-mask of the indexes of the TaxiNodes known to this Character in the global TaxiNodeArray
		/// </summary>
		protected TaxiNodeMask m_taxiNodeMask;

		protected IWorldZoneLocation m_bindLocation;

		protected bool m_isLoggingOut;

		protected DateTime m_lastRestUpdate;
		protected AreaTrigger m_restTrigger;

		private List<ChatChannel> m_chatChannels;

		/// <summary>
		/// Set to the ritual, this Character is currently participating in (if any)
		/// </summary>
		protected internal SummoningRitualHandler m_currentRitual;

		protected internal SummonRequest m_summonRequest;

		private LooterEntry m_looterEntry;
		private ExtraInfo m_ExtraInfo;

		protected TradeWindow m_tradeWindow;
		#endregion

		/// <summary>
		/// Contains certain info that is almost only used by staff and should usually not be available to normal players.
		/// <remarks>Guaranteed to be non-null</remarks>
		/// </summary>
		public ExtraInfo ExtraInfo
		{
			get
			{
				if (m_ExtraInfo == null)
				{
					m_ExtraInfo = new ExtraInfo(this);
				}
				return m_ExtraInfo;
			}
		}

		public override int GetUnmodifiedBaseStatValue(StatType stat)
		{
			return ClassBaseStats.Stats[(int)stat];
		}

		public override bool IsPlayer
		{
			get { return true; }
		}

		public override bool MayTeleport
		{
			get { return Role.IsStaff || (!IsKicked && CanMove && IsPlayerControlled); }
		}

		public override WorldObject Mover
		{
			get { return MoveControl.Mover; }
		}

		#region BYTES

		public byte[] PlayerBytes
		{
			get { return GetByteArray(PlayerFields.BYTES); }
			set { SetByteArray(PlayerFields.BYTES, value); }
		}

		public byte Skin
		{
			get { return GetByte(PlayerFields.BYTES, 0); }
			set { SetByte(PlayerFields.BYTES, 0, value); }
		}

		public byte Facial
		{
			get { return GetByte(PlayerFields.BYTES, 1); }
			set { SetByte(PlayerFields.BYTES, 1, value); }
		}

		public byte HairStyle
		{
			get { return GetByte(PlayerFields.BYTES, 2); }
			set { SetByte(PlayerFields.BYTES, 2, value); }
		}

		public byte HairColor
		{
			get { return GetByte(PlayerFields.BYTES, 3); }
			set { SetByte(PlayerFields.BYTES, 3, value); }
		}

		#endregion

		#region BYTES_2

		public byte[] PlayerBytes2
		{
			get { return GetByteArray(PlayerFields.BYTES_2); }
			set { SetByteArray(PlayerFields.BYTES_2, value); }
		}

		public byte FacialHair
		{
			get { return GetByte(PlayerFields.BYTES_2, 0); }
			set { SetByte(PlayerFields.BYTES_2, 0, value); }
		}

		/// <summary>
		/// 0x10 for SpellSteal
		/// </summary>
		public byte PlayerBytes2_2
		{
			get { return GetByte(PlayerFields.BYTES_2, 1); }
			set { SetByte(PlayerFields.BYTES_2, 1, value); }
		}

		/// <summary>
		/// Use player.Inventory.BankBags.Inc/DecBagSlots() to change the amount of cont slots in use
		/// </summary>
		public byte BankBagSlots
		{
			get { return GetByte(PlayerFields.BYTES_2, 2); }
			internal set { SetByte(PlayerFields.BYTES_2, 2, value); }
		}

		/// <summary>
		/// 0x01 -> Rested State
		/// 0x02 -> Normal State
		/// </summary>
		public RestState RestState
		{
			get { return (RestState)GetByte(PlayerFields.BYTES_2, 3); }
			set { SetByte(PlayerFields.BYTES_2, 3, (byte)value); }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool IsResting
		{
			get { return m_restTrigger != null; }
		}

		/// <summary>
		/// The AreaTrigger that triggered the current Rest-state (or null if not resting).
		/// Will automatically be set when the Character enters a Rest-Type AreaTrigger
		/// and will be unset once the Character is too far away from that trigger.
		/// </summary>
		public AreaTrigger RestTrigger
		{
			get { return m_restTrigger; }
			set
			{
				if (m_restTrigger != value)
				{
					if (value == null)
					{
						// leaving rest state
						UpdateRest();
						m_record.RestTriggerId = 0;
						RestState = RestState.Normal;
					}
					else
					{
						// start resting
						m_lastRestUpdate = DateTime.Now;
						m_record.RestTriggerId = (int)value.Id;
						RestState = RestState.Resting;
					}
					m_restTrigger = value;
				}
			}
		}

		#endregion

		#region BYTES_3

		public byte[] PlayerBytes3
		{
			get { return GetByteArray(PlayerFields.BYTES_3); }
			set { SetByteArray(PlayerFields.BYTES_3, value); }
		}

		public override GenderType Gender
		{
			get { return (GenderType)GetByte(PlayerFields.BYTES_3, 0); }
			set
			{
				SetByte(PlayerFields.BYTES_3, 0, (byte)value);
				base.Gender = value;
			}
		}

		/// <summary>
		/// Totally smashed
		/// 60 Drunk
		/// 30 Tipsy
		/// </summary>
		public byte DrunkState
		{
			get { return GetByte(PlayerFields.BYTES_3, 1); }
			set
			{
				if (value > 100)
					value = 100;
				SetByte(PlayerFields.BYTES_3, 1, value);
			}
		}

		public byte PlayerBytes3_3
		{
			get { return GetByte(PlayerFields.BYTES_3, 2); }
			set { SetByte(PlayerFields.BYTES_3, 2, value); }
		}

		public byte PvPRank
		{
			get { return GetByte(PlayerFields.BYTES_3, 3); }
			set { SetByte(PlayerFields.BYTES_3, 3, value); }
		}

		#endregion

		#region PLAYER_FIELD_BYTES

		/// <summary>
		/// BYTES
		/// </summary>
		public byte[] Bytes
		{
			get { return GetByteArray(PlayerFields.PLAYER_FIELD_BYTES); }
			set { SetByteArray(PlayerFields.PLAYER_FIELD_BYTES, value); }
		}

		/// <summary>
		/// 
		/// </summary>
		public CorpseReleaseFlags CorpseReleaseFlags
		{
			get { return (CorpseReleaseFlags)GetByte(PlayerFields.PLAYER_FIELD_BYTES, 0); }
			set { SetByte(PlayerFields.PLAYER_FIELD_BYTES, 0, (byte)value); }
		}

		public byte Bytes_2
		{
			get { return GetByte(PlayerFields.PLAYER_FIELD_BYTES, 1); }
			set { SetByte(PlayerFields.PLAYER_FIELD_BYTES, 1, value); }
		}

		public byte ActionBarMask
		{
			get { return GetByte(PlayerFields.PLAYER_FIELD_BYTES, 2); }
			set { SetByte(PlayerFields.PLAYER_FIELD_BYTES, 2, value); }
		}

		public byte Bytes_4
		{
			get { return GetByte(PlayerFields.PLAYER_FIELD_BYTES, 3); }
			set { SetByte(PlayerFields.PLAYER_FIELD_BYTES, 3, value); }
		}

		#endregion

		#region PLAYER_FIELD_BYTES2

		public byte[] Bytes2
		{
			get { return GetByteArray(PlayerFields.PLAYER_FIELD_BYTES2); }
			set { SetByteArray(PlayerFields.PLAYER_FIELD_BYTES2, value); }
		}

		public byte Bytes2_1
		{
			get { return GetByte(PlayerFields.PLAYER_FIELD_BYTES2, 0); }
			set { SetByte(PlayerFields.PLAYER_FIELD_BYTES2, 0, value); }
		}

		/// <summary>
		/// Set to 0x40 for mage invis
		/// </summary>
		public byte Bytes2_2
		{
			get { return GetByte(PlayerFields.PLAYER_FIELD_BYTES2, 1); }
			set { SetByte(PlayerFields.PLAYER_FIELD_BYTES2, 1, value); }
		}

		public byte Bytes2_3
		{
			get { return GetByte(PlayerFields.PLAYER_FIELD_BYTES2, 2); }
			set { SetByte(PlayerFields.PLAYER_FIELD_BYTES2, 2, value); }
		}

		public byte Bytes2_4
		{
			get { return GetByte(PlayerFields.PLAYER_FIELD_BYTES2, 3); }
			set { SetByte(PlayerFields.PLAYER_FIELD_BYTES2, 3, value); }
		}

		#endregion

		#region Misc

		public PlayerFlags PlayerFlags
		{
			get { return (PlayerFlags)GetUInt32(PlayerFields.FLAGS); }
			set { SetUInt32(PlayerFields.FLAGS, (uint)value); }
		}

		public int Experience
		{
			get { return GetInt32(PlayerFields.XP); }
			set { SetInt32(PlayerFields.XP, value); }
		}

		public int NextLevelXP
		{
			get { return GetInt32(PlayerFields.NEXT_LEVEL_XP); }
			set { SetInt32(PlayerFields.NEXT_LEVEL_XP, value); }
		}

		/// <summary>
		/// The amount of experience to be gained extra due to resting
		/// </summary>
		public int RestXp
		{
			get { return GetInt32(PlayerFields.REST_STATE_EXPERIENCE); }
			set { SetInt32(PlayerFields.REST_STATE_EXPERIENCE, value); }
		}

		/// <summary>
		/// Total amount of money in Copper Coins.
		/// Always make sure that the amount added or subtracted won't lead to a resulting amount greater than uint.Max or less than 0 or
		/// use Set, Add and SubtractMoney methods to safely modify the amount of money.
		/// </summary>
		/// <remarks>1 silver = 100 copper; 1 gold = 10,000 copper</remarks>
		public uint Money
		{
			get { return GetUInt32(PlayerFields.COINAGE); }
			set { SetUInt32(PlayerFields.COINAGE, value); }
		}

		/// <summary>
		/// The amount of gold coins of this Character. 
		/// </summary>
		public uint Gold
		{
			get { return GetUInt32(PlayerFields.COINAGE) / 10000; }
			set
			{
				var remainder = GetUInt32(PlayerFields.COINAGE) % 10000;
				SetUInt32(PlayerFields.COINAGE, (10000 * value) + remainder);
			}
		}

		/// <summary>
		/// The amount of silver coins of this Character. 
		/// 100 silver is automatically changed into 1 piece of gold.
		/// </summary>
		/// <example>30 silver + 120 silver = 1 gold, 50 silver</example>
		public uint Silver
		{
			get { return (GetUInt32(PlayerFields.COINAGE) % 10000) / 100; }
			set
			{
				var oldVal = GetUInt32(PlayerFields.COINAGE);
				var silver = (oldVal % 10000) / 100;
				SetUInt32(PlayerFields.COINAGE, oldVal + ((value - silver) * 100));
			}
		}

		public void SetMoney(uint amount)
		{
			SetUInt32(PlayerFields.COINAGE, amount);
		}

		/// <summary>
		/// Adds the given amount of money
		/// </summary>
		public void AddMoney(uint amount)
		{
			SetUInt32(PlayerFields.COINAGE, Money + amount);
		}

		/// <summary>
		/// Subtracts the given amount of Money. Returns false if its more than this Character has.
		/// </summary>
		public bool SubtractMoney(uint amount)
		{
			var money = Money;
			if (amount > money)
			{
				return false;
			}
			SetUInt32(PlayerFields.COINAGE, money - amount);
			return true;
		}

		/// <summary>
		/// Set to <value>-1</value> to disable the watched faction
		/// </summary>
		public int WatchedFaction
		{
			get { return GetInt32(PlayerFields.WATCHED_FACTION_INDEX); }
			set { SetInt32(PlayerFields.WATCHED_FACTION_INDEX, value); }
		}

		public uint ChosenTitle
		{
			get { return GetUInt32(PlayerFields.CHOSEN_TITLE); }
			set { SetUInt32(PlayerFields.CHOSEN_TITLE, value); }
		}

		public CharTitlesMask KnownTitleMask
		{
			get { return (CharTitlesMask)GetUInt64(PlayerFields._FIELD_KNOWN_TITLES); }
			set { SetUInt64(PlayerFields._FIELD_KNOWN_TITLES, (ulong)value); }
		}

		public ulong KnownTitleMask2
		{
			get { return GetUInt64(PlayerFields._FIELD_KNOWN_TITLES1); }
			set { SetUInt64(PlayerFields._FIELD_KNOWN_TITLES1, value); }
		}

		public ulong KnownTitleMask3
		{
			get { return GetUInt64(PlayerFields._FIELD_KNOWN_TITLES2); }
			set { SetUInt64(PlayerFields._FIELD_KNOWN_TITLES2, value); }
		}

		public uint KillsTotal
		{
			get { return GetUInt32(PlayerFields.KILLS); }
			set { SetUInt32(PlayerFields.KILLS, value); }
		}

		public ushort KillsToday
		{
			get { return GetUInt16Low(PlayerFields.KILLS); }
			set { SetUInt16Low(PlayerFields.KILLS, value); }
		}

		public ushort KillsYesterday
		{
			get { return GetUInt16High(PlayerFields.KILLS); }
			set { SetUInt16High(PlayerFields.KILLS, value); }
		}

		public uint HonorToday
		{
			get { return GetUInt32(PlayerFields.TODAY_CONTRIBUTION); }
			set { SetUInt32(PlayerFields.TODAY_CONTRIBUTION, value); }
		}

		public uint HonorYesterday
		{
			get { return GetUInt32(PlayerFields.YESTERDAY_CONTRIBUTION); }
			set { SetUInt32(PlayerFields.YESTERDAY_CONTRIBUTION, value); }
		}

		public uint LifetimeHonorableKills
		{
			get { return GetUInt32(PlayerFields.LIFETIME_HONORBALE_KILLS); }
			set { SetUInt32(PlayerFields.LIFETIME_HONORBALE_KILLS, value); }
		}

		public uint HonorPoints
		{
			get { return GetUInt32(PlayerFields.HONOR_CURRENCY); }
			set { SetUInt32(PlayerFields.HONOR_CURRENCY, value); }
		}

		public uint ArenaPoints
		{
			get { return GetUInt32(PlayerFields.ARENA_CURRENCY); }
			set { SetUInt32(PlayerFields.ARENA_CURRENCY, value); }
		}

		public uint GuildId
		{
			get { return GetUInt32(PlayerFields.GUILDID); }
			internal set { SetUInt32(PlayerFields.GUILDID, value); }
		}

		public uint GuildRank
		{
			get { return GetUInt32(PlayerFields.GUILDRANK); }
			internal set { SetUInt32(PlayerFields.GUILDRANK, value); }
		}

		/// <summary>
		/// The 3 classmasks of spells to not use require reagents for
		/// </summary>
		public uint[] NoReagentCost
		{
			get
			{
				return new[] { GetUInt32(PlayerFields.NO_REAGENT_COST_1), GetUInt32(PlayerFields.NO_REAGENT_COST_1_2), GetUInt32(PlayerFields.NO_REAGENT_COST_1_3) };
			}
			internal set
			{
				SetUInt32(PlayerFields.NO_REAGENT_COST_1, value[0]);
				SetUInt32(PlayerFields.NO_REAGENT_COST_1_2, value[1]);
				SetUInt32(PlayerFields.NO_REAGENT_COST_1_3, value[2]);
			}
		}

		#endregion

		public override Faction DefaultFaction
		{
			get { return FactionMgr.Get(Race); }
		}

        public int ReputationGainModifierPercent { get; set; }

        public int KillExperienceGainModifierPercent { get; set; }

        public int QuestExperienceGainModifierPercent { get; set; }

		#region CombatRatings

		/// <summary>
		/// Gets the total modifier of the corresponding CombatRating (in %) 
		/// </summary>
		public int GetCombatRating(CombatRating rating)
		{
			return GetInt32(PlayerFields.COMBAT_RATING_1 - 1 + (int)rating);
		}

		public void SetCombatRating(CombatRating rating, int value)
		{
			SetInt32(PlayerFields.COMBAT_RATING_1 - 1 + (int)rating, value);
			UpdateChancesByCombatRating(rating);
		}

		/// <summary>
		/// Modifies the given CombatRating modifier by the given delta
		/// </summary>
		public void ModCombatRating(CombatRating rating, int delta)
		{
			var val = GetInt32(PlayerFields.COMBAT_RATING_1 - 1 + (int)rating);
			SetInt32(PlayerFields.COMBAT_RATING_1 - 1 + (int)rating, val);
			UpdateChancesByCombatRating(rating);
		}


		public void ModCombatRating(uint[] ratings, int delta)
		{
			for (var i = 0; i < ratings.Length; i++)
			{
				var rating = ratings[i];
				ModCombatRating((CombatRating)rating, delta);
			}
		}

		#endregion

		#region Tracking of Resources & Creatures

		public CreatureMask CreatureTracking
		{
			get { return (CreatureMask)GetUInt32(PlayerFields.TRACK_CREATURES); }
			internal set { SetUInt32(PlayerFields.TRACK_CREATURES, (uint)value); }
		}

		public LockMask ResourceTracking
		{
			get { return (LockMask)GetUInt32(PlayerFields.TRACK_RESOURCES); }
			internal set { SetUInt32(PlayerFields.TRACK_RESOURCES, (uint)value); }
		}

		#endregion

		#region Misc Combat effecting Fields

		public float BlockChance
		{
			get { return GetFloat(PlayerFields.BLOCK_PERCENTAGE); }
			internal set { SetFloat(PlayerFields.BLOCK_PERCENTAGE, value); }
		}

		/// <summary>
		/// Amount of damage reduced when an attack is blocked
		/// </summary>
		public uint BlockValue
		{
			get { return GetUInt32(PlayerFields.SHIELD_BLOCK); }
			set { SetUInt32(PlayerFields.SHIELD_BLOCK, value); }
		}

		/// <summary>
		/// Value in %
		/// </summary>
		public float DodgeChance
		{
			get { return GetFloat(PlayerFields.DODGE_PERCENTAGE); }
			set { SetFloat(PlayerFields.DODGE_PERCENTAGE, value); }
		}

		public override float ParryChance
		{
			get { return GetFloat(PlayerFields.PARRY_PERCENTAGE); }
			internal set { SetFloat(PlayerFields.PARRY_PERCENTAGE, value); }
		}

		public uint Expertise
		{
			get { return GetUInt32(PlayerFields.EXPERTISE); }
			set { SetUInt32(PlayerFields.EXPERTISE, value); }
		}

		public float CritChanceMeleePct
		{
			get { return GetFloat(PlayerFields.CRIT_PERCENTAGE); }
			internal set { SetFloat(PlayerFields.CRIT_PERCENTAGE, value); }
		}

		public float CritChanceRangedPct
		{
			get { return GetFloat(PlayerFields.RANGED_CRIT_PERCENTAGE); }
			internal set { SetFloat(PlayerFields.RANGED_CRIT_PERCENTAGE, value); }
		}

		public float CritChanceOffHandPct
		{
			get { return GetFloat(PlayerFields.OFFHAND_CRIT_PERCENTAGE); }
			internal set { SetFloat(PlayerFields.OFFHAND_CRIT_PERCENTAGE, value); }
		}

		///// <summary>
		///// Reduces/increases the target chance to dodge the attack
		///// </summary>
		//public int TargetDodgeChanceMod
		//{
		//    get;
		//    set;
		//}

		private int m_OffhandDmgPctMod;

		/// <summary>
		/// Percent added to offhand damage
		/// </summary>
		public int OffhandDmgPctMod
		{
			get { return m_OffhandDmgPctMod; }
			set
			{
				m_OffhandDmgPctMod = value;
				this.UpdateOffHandDamage();
			}
		}

		/// <summary>
		/// Character's hit chance in %
		/// </summary>
		public float HitChance
		{
			get;
			set;
		}

		public float RangedHitChance
		{
			get; set;
		}

		public override uint Defense
		{
			get; internal set;
		}
		#endregion

		#region Quests

		public void ResetQuest(int slot)
		{
			var i = slot * 5;
			SetUInt32((PlayerFields.QUEST_LOG_1_1 + i), 0);
			SetUInt32((PlayerFields.QUEST_LOG_1_2 + i), 0);
			SetUInt32((PlayerFields.QUEST_LOG_1_3 + i), 0);
			SetUInt32((PlayerFields.QUEST_LOG_1_3_2 + i), 0);
			SetUInt32((PlayerFields.QUEST_LOG_1_4 + i), 0);
		}

		/// <summary>
		/// Gets the quest field.
		/// </summary>
		/// <param name="slot">The slot.</param>
		public uint GetQuestId(int slot)
		{
			return GetUInt32(PlayerFields.QUEST_LOG_1_1 + (slot * 5));
		}

		/// <summary>
		/// Sets the quest field, where fields are indexed from 0.
		/// </summary>
		/// <param name="slot">The slot.</param>
		/// <param name="questid">The questid.</param>
		public void SetQuestId(int slot, uint questid)
		{
			SetUInt32((PlayerFields.QUEST_LOG_1_1 + (slot * 5)), questid);
		}

		/// <summary>
		/// Gets the state of the quest.
		/// </summary>
		/// <param name="slot">The slot.</param>
		/// <returns></returns>
		public QuestCompleteStatus GetQuestState(int slot)
		{
			return (QuestCompleteStatus)GetUInt32(PlayerFields.QUEST_LOG_1_2 + (slot * 5));
		}

		/// <summary>
		/// Sets the state of the quest.
		/// </summary>
		/// <param name="slot">The slot.</param>
		/// <param name="completeStatus">The status.</param>
		public void SetQuestState(int slot, QuestCompleteStatus completeStatus)
		{
			SetUInt32((PlayerFields.QUEST_LOG_1_2 + (slot * 5)), (uint)completeStatus);
		}

		/// <summary>
		/// Sets the quest count at the given index for the given Quest to the given value.
		/// </summary>
		/// <param name="slot">The slot.</param>
		/// <param name="interactionIndex">The count slot.</param>
		/// <param name="value">The value.</param>
		internal void SetQuestCount(int slot, uint interactionIndex, ushort value)
		{
			var field = PlayerFields.QUEST_LOG_1_3 + ((int)interactionIndex >> 1);
			SetUInt16Low((field + (slot * 5)), value);
		}

		/// <summary>
		/// Gets the quest time.
		/// </summary>
		/// <param name="slot">The slot.</param>
		/// <returns></returns>
		internal uint GetQuestTimeLeft(byte slot)
		{
			return GetUInt32((PlayerFields.QUEST_LOG_1_4 + (slot * 5)));
		}

		/// <summary>
		/// Sets the quest time.
		/// </summary>
		/// <param name="slot">The slot.</param>
		internal void SetQuestTimeLeft(byte slot, uint timeleft)
		{
			SetUInt32((PlayerFields.QUEST_LOG_1_4 + (slot * 5)), timeleft);
		}

		/*
		uint16 FindQuestSlot( uint32 quest_id ) const;
		uint32 GetQuestSlotQuestId(uint16 slot) const { return GetUInt32Value(PLAYER_QUEST_LOG_1_1 + slot*MAX_QUEST_OFFSET + QUEST_ID_OFFSET); }
		uint32 GetQuestSlotState(uint16 slot)   const { return GetUInt32Value(PLAYER_QUEST_LOG_1_1 + slot*MAX_QUEST_OFFSET + QUEST_STATE_OFFSET); }
		uint32 GetQuestSlotCounters(uint16 slot)const { return GetUInt32Value(PLAYER_QUEST_LOG_1_1 + slot*MAX_QUEST_OFFSET + QUEST_COUNTS_OFFSET); }
		uint8 GetQuestSlotCounter(uint16 slot,uint8 counter) const { return GetByteValue(PLAYER_QUEST_LOG_1_1 + slot*MAX_QUEST_OFFSET + QUEST_COUNTS_OFFSET,counter); }
		uint32 GetQuestSlotTime(uint16 slot)    const { return GetUInt32Value(PLAYER_QUEST_LOG_1_1 + slot*MAX_QUEST_OFFSET + QUEST_TIME_OFFSET); }
		void SetQuestSlot(uint16 slot,uint32 quest_id, uint32 timer = 0)
		{
			SetUInt32Value(PLAYER_QUEST_LOG_1_1 + slot*MAX_QUEST_OFFSET + QUEST_ID_OFFSET,quest_id);
			SetUInt32Value(PLAYER_QUEST_LOG_1_1 + slot*MAX_QUEST_OFFSET + QUEST_STATE_OFFSET,0);
			SetUInt32Value(PLAYER_QUEST_LOG_1_1 + slot*MAX_QUEST_OFFSET + QUEST_COUNTS_OFFSET,0);
			SetUInt32Value(PLAYER_QUEST_LOG_1_1 + slot*MAX_QUEST_OFFSET + QUEST_TIME_OFFSET,timer);
		}
		void SetQuestSlotCounter(uint16 slot,uint8 counter,uint8 count) { SetByteValue(PLAYER_QUEST_LOG_1_1 + slot*MAX_QUEST_OFFSET + QUEST_COUNTS_OFFSET,counter,count); }
		void SetQuestSlotState(uint16 slot,uint32 state) { SetFlag(PLAYER_QUEST_LOG_1_1 + slot*MAX_QUEST_OFFSET + QUEST_STATE_OFFSET,state); }
		void RemoveQuestSlotState(uint16 slot,uint32 state) { RemoveFlag(PLAYER_QUEST_LOG_1_1 + slot*MAX_QUEST_OFFSET + QUEST_STATE_OFFSET,state); }
		void SetQuestSlotTimer(uint16 slot,uint32 timer) { SetUInt32Value(PLAYER_QUEST_LOG_1_1 + slot*MAX_QUEST_OFFSET + QUEST_TIME_OFFSET,timer); }
		}*/

		#region Daily quests

		/// <summary>
		/// This array stores completed daily quests
		/// </summary>
		/// <returns></returns>
		//TODO change return type to Quest
		public uint[] DailyQuests
		{
			get
			{
				var dailyquestids = new uint[25];
				for (var i = 0; i < 25; i++)
				{
					dailyquestids[i] = GetUInt32((PlayerFields.DAILY_QUESTS_1 + i));
				}
				return dailyquestids;
			}
		}

		/// <summary>
		/// Gets the quest field.
		/// </summary>
		/// <param name="slot">The slot.</param>
		public uint GetDailyQuest(byte slot)
		{
			//TODO Do we need to check if slot is > 25?
			return GetUInt32((PlayerFields.DAILY_QUESTS_1 + slot));
		}

		/// <summary>
		/// Sets the quest field, where fields are indexed from 0.
		/// </summary>
		/// <param name="slot">The slot.</param>
		/// <param name="questid">The questid.</param>
		public void SetDailyQuest(byte slot, uint questid)
		{
			//TODO Do we need to check if slot is > 25?
			SetUInt32((PlayerFields.DAILY_QUESTS_1 + slot), questid);
		}

		public void ResetDailyQuests()
		{
			for (int i = 0; i < 25; i++)
			{
				SetUInt32((PlayerFields.DAILY_QUESTS_1 + i), 0);
			}
		}

		#endregion

		#endregion

		#region Damage
		/// <summary>
		/// Modifies the damage for the given school by the given delta.
		/// </summary>
		protected internal override void AddDamageDoneModSilently(DamageSchool school, int delta)
		{
			PlayerFields field;
			if (delta == 0)
			{
				return;
			}
			if (delta > 0)
			{
				field = PlayerFields.MOD_DAMAGE_DONE_POS;
			}
			else
			{
				field = PlayerFields.MOD_DAMAGE_DONE_NEG;
				delta = -delta;
			}
			SetInt32(field + (int)school, GetInt32(field + (int)school) + delta);
		}

		/// <summary>
		/// Modifies the damage for the given school by the given delta.
		/// </summary>
		protected internal override void RemoveDamageDoneModSilently(DamageSchool school, int delta)
		{
			PlayerFields field;
			if (delta == 0)
			{
				return;
			}
			if (delta > 0)
			{
				field = PlayerFields.MOD_DAMAGE_DONE_POS;
			}
			else
			{
				field = PlayerFields.MOD_DAMAGE_DONE_NEG;
				delta = -delta;
			}
			SetUInt32(field + (int)school, GetUInt32(field + (int)school) - (uint)delta);
		}

		protected internal override void ModDamageDoneFactorSilently(DamageSchool school, float delta)
		{
			if (delta == 0)
			{
				return;
			}
			var field = PlayerFields.MOD_DAMAGE_DONE_PCT + (int)school;
			SetFloat(field, GetFloat(field) + delta);
		}

		public override float GetDamageDoneFactor(DamageSchool school)
		{
			return GetFloat(PlayerFields.MOD_DAMAGE_DONE_PCT + (int)school);
		}

		public override int GetDamageDoneMod(DamageSchool school)
		{
			return GetInt32(PlayerFields.MOD_DAMAGE_DONE_POS + (int)school) -
					GetInt32(PlayerFields.MOD_DAMAGE_DONE_NEG + (int)school);
		}
		#endregion

		#region Healing Done

		/// <summary>
		/// Increased healing done *by* this Character
		/// </summary>
		public int HealingDoneMod
		{
			get { return GetInt32(PlayerFields.MOD_HEALING_DONE_POS); }
			set { SetInt32(PlayerFields.MOD_HEALING_DONE_POS, value); }
		}

		/// <summary>
		/// Increased healing % done *by* this Character
		/// </summary>
		public float HealingDoneModPct
		{
			get { return GetFloat(PlayerFields.MOD_HEALING_DONE_PCT); }
			set { SetFloat(PlayerFields.MOD_HEALING_DONE_PCT, value); }
		}

		/// <summary>
		/// Increased healing done *to* this Character
		/// </summary>
		public float HealingTakenModPct
		{
			get { return GetFloat(PlayerFields.MOD_HEALING_PCT); }
			set { SetFloat(PlayerFields.MOD_HEALING_PCT, value); }
		}
		#endregion

		/// <summary>
		/// Returns the SpellCritChance for the given DamageType (0-100)
		/// </summary>
		public override float GetCritChance(DamageSchool school)
		{
			return GetFloat(PlayerFields.SPELL_CRIT_PERCENTAGE1 + (int)school);
		}

		/// <summary>
		/// Sets the SpellCritChance for the given DamageType
		/// </summary>
		internal void SetCritChance(DamageSchool school, float val)
		{
			SetFloat(PlayerFields.SPELL_CRIT_PERCENTAGE1 + (int)school, val);
		}

		public EntityId FarSight
		{
			get { return GetEntityId(PlayerFields.FARSIGHT); }
			set { SetEntityId(PlayerFields.FARSIGHT, value); }
		}

		/// <summary>
		/// Make sure that the given slot is actually an EquipmentSlot
		/// </summary>
		internal void SetVisibleItem(InventorySlot slot, Item item)
		{
			var offset = PlayerFields.VISIBLE_ITEM_1_ENTRYID + ((int)slot * ItemConstants.PlayerFieldVisibleItemSize);
			if (item != null)
			{
				SetUInt32(offset, item.Template.Id);
			}
			else
			{
				SetUInt32(offset, 0);
			}
		}

		#region Action Buttons
		/// <summary>
		/// Sets an ActionButton with the given information.
		/// </summary>
		public void BindActionButton(uint btnIndex, uint action, byte type, bool update = true)
		{
			CurrentSpecProfile.IsDirty = true;
			var actions = CurrentSpecProfile.ActionButtons;
			btnIndex = btnIndex * 4;
			if (action == 0)
			{
				// unset it
				Array.Copy(ActionButton.EmptyButton, 0, actions, btnIndex, ActionButton.Size);
			}
			else
			{
				actions[btnIndex] = (byte)(action & 0x0000FF);
				actions[btnIndex + 1] = (byte)((action & 0x00FF00) >> 8);
				actions[btnIndex + 2] = (byte)((action & 0xFF000) >> 16);
				actions[btnIndex + 3] = type;
			}

			if (update)
			{
				CharacterHandler.SendActionButtons(this);
			}
		}

		/// <summary>
		/// Sets the given button to the given spell and resends it to the client
		/// </summary>
		public void BindSpellToActionButton(uint btnIndex, SpellId spell, bool update = true)
		{
			BindActionButton(btnIndex, (uint) spell, 0);
			if (update)
			{
				CharacterHandler.SendActionButtons(this);
			}
		}

		/// <summary>
		/// Sets the given action button
		/// </summary>
		public void BindActionButton(ActionButton btn, bool update = true)
		{
			btn.Set(CurrentSpecProfile.ActionButtons);
			CurrentSpecProfile.IsDirty = true;
			if (update)
			{
				CharacterHandler.SendActionButtons(this);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public byte[] ActionButtons
		{
			get { return CurrentSpecProfile.ActionButtons; }
		}
		#endregion

		#region Custom Properties

		public override ObjectTypeCustom CustomType
		{
			get { return ObjectTypeCustom.Object | ObjectTypeCustom.Unit | ObjectTypeCustom.Player; }
		}
		#endregion

		public CharacterRecord Record
		{
			get { return m_record; }
		}

		/// <summary>
		/// The active ticket of this Character or null if there is none
		/// </summary>
		public Ticket Ticket { get; internal set; }

		#region Base Unit Fields Overrides
		public override int Health
		{
			get { return base.Health; }
			set
			{
				base.Health = value;
				//Update Group Update flags
				GroupUpdateFlags |= GroupUpdateFlags.Health;
			}
		}

		public override int MaxHealth
		{
			get { return base.MaxHealth; }
			internal set
			{
				base.MaxHealth = value;
				//Update Group Update flags
				GroupUpdateFlags |= GroupUpdateFlags.MaxHealth;
			}
		}

		public int BaseHealthCapacity
		{
			get { return m_archetype.Class.GetLevelSetting(Level).Health; }
		}

		public override int Power
		{
			get { return base.Power; }
			set
			{
				base.Power = value;
				//Update Group Update flags
				GroupUpdateFlags |= GroupUpdateFlags.Power;
			}
		}

		public override PowerType PowerType
		{
			get { return base.PowerType; }
			set
			{
				base.PowerType = value;
				//Update Group Update flags
				GroupUpdateFlags |= GroupUpdateFlags.PowerType | GroupUpdateFlags.Power | GroupUpdateFlags.MaxPower;
			}
		}

		public int BaseManaPoolCapacity
		{
			get { return m_archetype.Class.GetLevelSetting(Level).Mana; }
		}

		public override int MaxPower
		{
			get { return base.MaxPower; }
			internal set
			{
				base.MaxPower = value;
				//Update Group Update flags
				GroupUpdateFlags |= GroupUpdateFlags.MaxPower;
			}
		}

		public override int Level
		{
			get { return base.Level; }
			set
			{
				base.Level = value;
				//Update Group Update flags
				GroupUpdateFlags |= GroupUpdateFlags.Level;
			}
		}

		public override int MaxLevel
		{
			get { return GetInt32(PlayerFields.MAX_LEVEL); }
			internal set { SetInt32(PlayerFields.MAX_LEVEL, value); }
		}

		public override Zone Zone
		{
			get { return base.Zone; }
			internal set
			{
				if (m_zone != value)
				{
					if (value != null)
					{
						if (m_region != null && value.ParentZoneId == 0)
						{
							value.EnterZone(this, m_zone);
						}
					}
					base.Zone = value;

					//Update Group Update flags
					GroupUpdateFlags |= GroupUpdateFlags.ZoneId;
				}
			}
		}

		public bool IsZoneExplored(ZoneId id)
		{
			var zone = World.GetZoneInfo(id);
			return zone != null && IsZoneExplored(zone);
		}

		public bool IsZoneExplored(ZoneTemplate zone)
		{
			return IsZoneExplored(zone.ExplorationBit);
		}

		public bool IsZoneExplored(int explorationBit)
		{
			var index = explorationBit >> 3;
			if (index >= UpdateFieldMgr.ExplorationZoneFieldSize)
			{
				return false;
			}
			//return (GetUInt32((int)PlayerFields.EXPLORED_ZONES_1 + (int)index) & (1 << ((int)explorationBit % 32))) != 0;
			return (m_record.ExploredZones[index] & (1 << (explorationBit % 8))) != 0;
		}

		public void SetZoneExplored(ZoneId id, bool explored)
		{
			var zone = World.GetZoneInfo(id);
			if (zone != null)
			{
				SetZoneExplored(zone, explored);
			}
		}

		public void SetZoneExplored(ZoneTemplate zone, bool gainXp)
		{
			var index = zone.ExplorationBit >> 5;
			if (index >= UpdateFieldMgr.ExplorationZoneFieldSize * 4)
			{
				return;
			}

			//var intVal = GetUInt32((int)PlayerFields.EXPLORED_ZONES_1 + (int)index);
			var byteVal = m_record.ExploredZones[index];
			var bit = (zone.ExplorationBit - 1) % 8;
			if ((byteVal & (1 << bit)) == 0)
			{
				if (gainXp)
				{
					var xp = XpGenerator.GetExplorationXp(zone, this);
					if (xp > 0)
					{
						if (Level >= RealmServerConfiguration.MaxCharacterLevel)
						{
							CharacterHandler.SendExplorationExperience(this, zone.Id, 0);
						}
						else
						{
							GainXp(xp, false);
							CharacterHandler.SendExplorationExperience(this, zone.Id, xp);
						}
					}
				}

				var value = (byte)(byteVal | (1 << bit));
				SetByte((int)PlayerFields.EXPLORED_ZONES_1 + (zone.ExplorationBit >> 5), index % 4, value);
				m_record.ExploredZones[index] = value;
			}

			foreach (var child in zone.ChildZones)
			{
				SetZoneExplored(child, gainXp);
			}
		}

		public override Vector3 Position
		{
			get { return base.Position; }
			internal set
			{
				base.Position = value;
				//Update Group Update flags
				GroupUpdateFlags |= GroupUpdateFlags.Position;
			}
		}

		public override uint Phase
		{
			get { return m_Phase; }
			set
			{
				m_Phase = value;
				CharacterHandler.SendPhaseShift(this, value);
			}
		}

		#endregion

		#region Misc Properties

		public override bool IsInWorld
		{
			get { return m_initialized; }
		}

		/// <summary>
		/// The type of this object (player, corpse, item, etc)
		/// </summary>
		public override ObjectTypeId ObjectTypeId
		{
			get { return ObjectTypeId.Player; }
		}

		/// <summary>
		/// The client currently playing the character.
		/// </summary>
		public IRealmClient Client
		{
			get { return m_client; }
			protected set { m_client = value; }
		}

		/// <summary>
		/// The status of the character.
		/// </summary>
		public CharacterStatus Status
		{
			get
			{
				CharacterStatus status = CharacterStatus.OFFLINE;

				if (IsAFK)
					status |= CharacterStatus.AFK;

				if (IsDND)
					status |= CharacterStatus.DND;

				if (IsInWorld)
					status |= CharacterStatus.ONLINE;

				return status;
			}
		}

		/// <summary>
		/// The GroupMember object of this Character (if it he/she is in any group)
		/// </summary>
		public GroupMember GroupMember
		{
			get { return m_groupMember; }
			internal set { m_groupMember = value; }
		}

		/// <summary>
		/// The GuildMember object of this Character (if it he/she is in a guild)
		/// </summary>
		public GuildMember GuildMember
		{
			get { return m_guildMember; }
			set
			{
				m_guildMember = value;
				if (m_guildMember != null)
				{
					GuildId = (uint)m_guildMember.Guild.Id;
					GuildRank = (uint)m_guildMember.RankId;
				}
				else
				{
					GuildId = 0;
					GuildRank = 0;
				}
			}
		}

		/// <summary>
		/// Characters get disposed after Logout sequence completed and
		/// cannot (and must not) be used anymore.
		/// </summary>
		public bool IsDisposed
		{
			get { return m_auras == null; }
		}

		/// <summary>
		/// The Group of this Character (if it he/she is in any group)
		/// </summary>
		public Group Group
		{
			get
			{
				if (m_groupMember != null)
				{
					var subGroup = m_groupMember.SubGroup;
					return subGroup != null ? subGroup.Group : null;
				}
				return null;
			}
		}

		/// <summary>
		/// The subgroup in which the character is (if any)
		/// </summary>
		public SubGroup SubGroup
		{
			get
			{
				if (m_groupMember != null)
				{
					return m_groupMember.SubGroup;
				}
				return null;
			}
		}

		public GroupUpdateFlags GroupUpdateFlags
		{
			get { return m_groupUpdateFlags; }
			set { m_groupUpdateFlags = value; }
		}

		/// <summary>
		/// The guild in which the character is (if any)
		/// </summary>
		public Guild Guild
		{
			get
			{
				if (m_guildMember != null)
				{
					return m_guildMember.Guild;
				}
				return null;
			}
		}

		/// <summary>
		/// The account this character belongs to.
		/// </summary>
		public RealmAccount Account { get; protected internal set; }

		public RoleGroup Role
		{
			get
			{
				var acc = Account;
				return acc != null ? acc.Role : PrivilegeMgr.Instance.LowestRole;
			}
		}

		public override ClientLocale Locale
		{
			get { return m_client.Info.Locale; }
			set { m_client.Info.Locale = value; }
		}

		/// <summary>
		/// The name of this character.
		/// </summary>
		public override string Name
		{
			get { return m_name; }
			set
			{
				throw new NotImplementedException("Dynamic renaming of Characters is not yet implemented.");
				//m_name = value;
			}
		}

		public Corpse Corpse
		{
			get { return m_corpse; }
			internal set
			{
				if (value == null && m_corpse != null)
				{
					m_corpse.StartDecay();
					m_record.CorpseX = null;
				}
				m_corpse = value;
			}
		}

		/// <summary>
		/// The <see cref="Archetype">Archetype</see> of this Character
		/// </summary>
		public Archetype Archetype
		{
			get { return m_archetype; }
			set
			{
				m_archetype = value;
				Race = value.Race.Id;
				Class = value.Class.Id;
			}
		}

		///<summary>
		///</summary>
		public byte Outfit { get; set; }

		/// <summary>
		/// The channels the character is currently joined to.
		/// </summary>
		public List<ChatChannel> ChatChannels
		{
			get { return m_chatChannels; }
			set { m_chatChannels = value; }
		}

		/// <summary>
		/// Whether this Character is currently trading with someone
		/// </summary>
		public bool IsTrading
		{
			get { return m_tradeWindow != null; }
		}

		/// <summary>
		/// Current trading progress of the character
		/// Null if none
		/// </summary>
		public TradeWindow TradeWindow
		{
			get { return m_tradeWindow; }
			set { m_tradeWindow = value; }
		}

		/// <summary>
		/// Last login time of this character.
		/// </summary>
		public DateTime LastLogin
		{
			get { return m_record.LastLogin.Value; }
			set { m_record.LastLogin = value; }
		}

		/// <summary>
		/// Last logout time of this character.
		/// </summary>
		public DateTime? LastLogout
		{
			get { return m_record.LastLogout; }
			set { m_record.LastLogout = value; }
		}

		public bool IsFirstLogin
		{
			get { return m_record.LastLogout == null; }
		}

		public TutorialFlags TutorialFlags { get; set; }

		/// <summary>
		/// Total play time of this Character in seconds
		/// </summary>
		public uint TotalPlayTime
		{
			get { return (uint)m_record.TotalPlayTime; }
			set { m_record.TotalPlayTime = (int)value; }
		}

		/// <summary>
		/// How long is this Character already on this level in seconds
		/// </summary>
		public uint LevelPlayTime
		{
			get { return (uint)m_record.LevelPlayTime; }
			set { m_record.LevelPlayTime = (int)value; }
		}

		/// <summary>
		/// Whether or not this character has the GM-tag set.
		/// </summary>
		public bool ShowAsGameMaster
		{
			get { return PlayerFlags.HasFlag(PlayerFlags.GM); }
			set
			{
				if (value)
				{
					PlayerFlags |= PlayerFlags.GM;
				}
				else
				{
					PlayerFlags &= ~PlayerFlags.GM;
				}
			}
		}

		/// <summary>
		/// Gets/Sets the godmode
		/// </summary>
		public bool GodMode
		{
			get { return m_record.GodMode; }
			set
			{
				m_record.GodMode = value;
				var cast = m_spellCast;
				if (cast != null)
				{
					cast.GodMode = value;
				}

				if (value)
				{
					Health = MaxHealth;
					Power = MaxPower;
					//NoReagentCost = new[] {0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF};

					// clear cooldowns
					m_spells.ClearCooldowns();
					ShowAsGameMaster = true;

					// make invulnerable
					IncMechanicCount(SpellMechanic.Invulnerable);
				}
				else
				{
					//NoReagentCost = new[] {0u, 0u, 0u};
					DecMechanicCount(SpellMechanic.Invulnerable);
					ShowAsGameMaster = false;
				}
			}
		}

		protected override void InitSpellCast()
		{
			base.InitSpellCast();
			m_spellCast.GodMode = GodMode;
		}

		/// <summary>
		/// Whether the PvP Flag is set.
		/// </summary>
		public bool IsPvPFlagSet
		{
			get { return PlayerFlags.HasFlag(PlayerFlags.PVP); }
			set
			{
				if (value)
				{
					PlayerFlags |= PlayerFlags.PVP;
					return;
				}
				PlayerFlags &= ~PlayerFlags.PVP;
			}
		}

		/// <summary>
		/// Whether the PvP Flag reset timer is active.
		/// </summary>
		public bool IsPvPTimerActive
		{
			get { return PlayerFlags.HasFlag(PlayerFlags.PVPTimerActive); }
			set
			{
				if (value)
				{
					PlayerFlags |= PlayerFlags.PVPTimerActive;
					return;
				}
				PlayerFlags &= ~PlayerFlags.PVPTimerActive;
			}
		}

		#region AFK & DND etc
		/// <summary>
		/// Whether or not this character is AFK.
		/// </summary>
		public bool IsAFK
		{
			get { return PlayerFlags.HasFlag(PlayerFlags.AFK); }
			set
			{
				if (value)
				{
					PlayerFlags |= PlayerFlags.AFK;
				}
				else
				{
					PlayerFlags &= ~PlayerFlags.AFK;
				}
				GroupUpdateFlags |= GroupUpdateFlags.Status;
			}
		}

		/// <summary>
		/// The custom AFK reason when player is AFK.
		/// </summary>
		public string AFKReason { get; set; }

		/// <summary>
		/// Whether or not this character is DND.
		/// </summary>
		public bool IsDND
		{
			get { return PlayerFlags.HasFlag(PlayerFlags.DND); }
			set
			{
				if (value)
				{
					PlayerFlags |= PlayerFlags.DND;
				}
				else
				{
					PlayerFlags &= ~PlayerFlags.DND;
				}
				GroupUpdateFlags |= GroupUpdateFlags.Status;
			}
		}

		/// <summary>
		/// The custom DND reason when player is DND.
		/// </summary>
		public string DNDReason { get; set; }

		/// <summary>
		/// Gets the chat tag for the character.
		/// </summary>
		public override ChatTag ChatTag
		{
			get
			{
				if (ShowAsGameMaster)
				{
					return ChatTag.GM;
				}
				if (IsAFK)
				{
					return ChatTag.AFK;
				}
				if (IsDND)
				{
					return ChatTag.DND;
				}

				return ChatTag.None;
			}
		}
		#endregion

		#region Interfaces & Collections
		/// <summary>
		/// Collection of reputations with all factions known to this Character
		/// </summary>
		public ReputationCollection Reputations
		{
			get { return m_reputations; }
		}

		/// <summary>
		/// Collection of all this Character's skills
		/// </summary>
		public SkillCollection Skills
		{
			get { return m_skills; }
		}

		/// <summary>
		/// Collection of all this Character's Talents
		/// </summary>
		public TalentCollection Talents
		{
			get { return m_talents; }
		}

        /// <summary>
        /// Collection of all this Character's Achievements
        /// </summary>
	    public AchievementCollection Achievements
	    {
            get { return m_achievements; }
	    }

		/// <summary>
		/// All spells known to this chr
		/// </summary>
		public PlayerAuraCollection PlayerAuras
		{
			get { return (PlayerAuraCollection)m_auras; }
		}

		/// <summary>
		/// All spells known to this chr
		/// </summary>
		public PlayerSpellCollection PlayerSpells
		{
			get { return (PlayerSpellCollection)m_spells; }
		}

		/// <summary>
		/// Mask of the activated Flight Paths
		/// </summary>
		public TaxiNodeMask TaxiNodes
		{
			get { return m_taxiNodeMask; }
		}

		/// <summary>
		/// The Tavern-location of where the Player bound to
		/// </summary>
		public IWorldZoneLocation BindLocation
		{
			get
			{
				CheckBindLocation();
				return m_bindLocation;
			}
			internal set { m_bindLocation = value; }
		}

		/// <summary>
		/// The Inventory of this Character contains all Items and Item-related things
		/// </summary>
		public PlayerInventory Inventory
		{
			get { return m_inventory; }
		}

		/// <summary>
		/// Returns the same as Inventory but with another type (for IContainer interface)
		/// </summary>
		public BaseInventory BaseInventory
		{
			get { return m_inventory; }
		}


		/// <summary>
		/// The Character's MailAccount
		/// </summary>
		public MailAccount MailAccount
		{
			get { return m_mailAccount; }
			set
			{
				if (m_mailAccount != value)
				{
					m_mailAccount = value;
				}
			}
		}
		#endregion

		/// <summary>
		/// Unused talent-points for this Character
		/// </summary>
		public int FreeTalentPoints
		{
			get { return (int)GetUInt32(PlayerFields.CHARACTER_POINTS1); }
			set
			{
				if (value < 0)
					value = 0;

				//m_record.FreeTalentPoints = value;
				SetUInt32(PlayerFields.CHARACTER_POINTS1, (uint)value);
				TalentHandler.SendTalentGroupList(m_talents);
			}
		}

		/// <summary>
		/// Doesn't send a packet to the client
		/// </summary>
		public void UpdateFreeTalentPointsSilently(int delta)
		{
			SetUInt32(PlayerFields.CHARACTER_POINTS1, (uint)(FreeTalentPoints + delta));
		}

		/// <summary>
		/// Forced logout must not be cancelled
		/// </summary>
		public bool IsKicked
		{
			get { return m_isLoggingOut && !IsPlayerLogout; }
		}

		/// <summary>
		/// The current GossipConversation that this Character is having
		/// </summary>
		public GossipConversation GossipConversation { get; set; }

		public void StartGossip(GossipMenu menu)
		{
			GossipConversation = new GossipConversation(menu, this, this, menu.KeepOpen);
			GossipConversation.DisplayCurrentMenu();
		}

		/// <summary>
		/// Returns whether this Character is invited into a Group already
		/// </summary>
		/// <returns></returns>
		public bool IsInvitedToGroup
		{
			get { return RelationMgr.Instance.HasPassiveRelations(EntityId.Low, CharacterRelationType.GroupInvite); }
		}

		/// <summary>
		/// Returns whether this Character is invited into a Guild already
		/// </summary>
		/// <returns></returns>
		public bool IsInvitedToGuild
		{
			get { return RelationMgr.Instance.HasPassiveRelations(EntityId.Low, CharacterRelationType.GuildInvite); }
		}

		#endregion
	}
}