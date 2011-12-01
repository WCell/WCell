/*************************************************************************
 *
 *   file		: QuestTemplate.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-02-06 19:13:00 +0100 (st, 06 II 2008) $
 *   last author	: $LastChangedBy: Nivelo $
 *   revision		: $Rev: 112 $
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
using WCell.Constants.Achievements;
using WCell.Constants.Factions;
using WCell.Constants.GameObjects;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Constants.Quests;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.RealmServer.AreaTriggers;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Lang;
using WCell.RealmServer.NPCs;
using WCell.Util;
using WCell.Util.Data;

namespace WCell.RealmServer.Quests
{
	/// <summary>
	/// Quest Templates represent all information that is associated with a possible ingame Quest.
	/// TODO: AreaTrigger relations
	/// </summary>
	[DataHolder]
	public partial class QuestTemplate : IDataHolder
	{
		private const uint MaxId = 200000;

		/// <summary>
		/// The list of all entities that can start this Quest
		/// </summary>
		public readonly List<IQuestHolderEntry> Starters = new List<IQuestHolderEntry>(3);

		/// <summary>
		/// The List of all entities that can finish this Quest
		/// </summary>
		public readonly List<IQuestHolderEntry> Finishers = new List<IQuestHolderEntry>(3);

		#region QuestRoot

		/// <summary>
		/// Unique identifier of quest.
		/// </summary>
		public uint Id;

		/// <summary>
		/// Determines whether quest is active or not. 
		/// </summary>
		public QuestTemplateStatus IsActive = QuestTemplateStatus.Active;

		/// <summary>
		/// Level of given quest, for which the quest is optimized. (If FFFFFFFF, then it's level independent or special)
		/// </summary>
		public uint Level, MinLevel;

		/// <summary>
		/// 
		/// </summary>
		public int Category;

		/// <summary>
		/// Restricted to this Zone
		/// </summary>
		[NotPersistent]
		public ZoneTemplate ZoneTemplate;

		/// <summary>
		/// Id for QuestSort.dbc
		/// </summary>
		//[NotPersistent]
		//public ZoneId ZoneId;

		/// <summary>
		/// QuestType, for more detailed description of type look at <seealso cref="Constants.Quests.QuestType"/>
		/// </summary>
		public QuestType QuestType;

		/// <summary>
		/// Number of players quest is optimized for.
		/// </summary>
		public uint SuggestedPlayers;

		/// <summary>
		/// 
		/// </summary>
		public FactionReputationEntry ObjectiveMinReputation;

		/// <summary>
		/// Player cannot have more than this in reputation
		/// </summary>
		public FactionReputationEntry ObjectiveMaxReputation;

		/// <summary>
		/// Gets or sets the reward money in copper, if it's negative,
		/// money will be required for quest completition and deducted
		/// from player's money after completition.
		/// 1     = 1 copper
		/// 10    = 10 copper
		/// 100   = 1 silver
		/// 1000  = 10 silver
		/// 10000 = 1 gold 
		/// </summary>
		public int RewMoney;

		/// <summary>
		/// Money gained instead of RewMoney at level 70.
		/// </summary>
		public uint MoneyAtMaxLevel;

		/// <summary>
		/// Given spell id, which is added to character's spell book when finishing the quest.
		/// </summary>
		public SpellId RewSpell;

		/// <summary>
		/// Cast spell id of spell which is casted on character when finishing the quest.
		/// </summary>
		public SpellId CastSpell;

		/// <summary>
		/// 
		/// </summary>
		[NotPersistent]
		public uint BonusHonor;

		/// <summary>
		/// An Quest-starting Item
		/// </summary>
		public ItemId SrcItemId;

		/// <summary>
		/// QuestFlags, for more detailed description of flags look at <see cref="QuestFlags"/>
		/// </summary>
		public QuestFlags Flags;

		/// <summary>
		/// 
		/// </summary>
		public TitleId RewardTitleId;

		/* 
		public uint PlayerKillCount;
		*/

		public uint RewardTalents;

		/// <summary>
		/// The id of the loot to be sent via mail to the finisher after completion
		/// </summary>
		public uint RewardMailTemplateId;

		/// <summary>
		/// The delay after which the Reward Mail should be sent
		/// </summary>
		public uint RewardMailDelaySeconds;

		/// <summary>
		/// Array of items containing item ID, index and quantity of items.
		/// </summary>
		[Persistent(QuestConstants.MaxRewardItems)]
		public ItemStackDescription[] RewardItems;

		/// <summary>
		/// Array of items containing item ID, index and quantity of items.
		/// </summary>
		[Persistent(QuestConstants.MaxRewardChoiceItems)]
		public ItemStackDescription[] RewardChoiceItems;

		/// <summary>
		/// Map Id of point showing something.
		/// </summary>
		public MapId MapId;

		/// <summary>
		/// X-coordinate of point showing something.
		/// </summary>
		public float PointX;

		/// <summary>
		/// Y-coordinate of point showing something.
		/// </summary>
		public float PointY;

		/// <summary>
		/// Options of point showing something.
		/// </summary>
		public uint PointOpt;

		[Persistent((int)ClientLocale.End)]
		public string[] Titles;

		/// <summary>
		/// Title (name) of the quest to be shown in <see cref="QuestLog"/> in the server's default language.
		/// </summary>
		public string DefaultTitle
		{
			get { return Titles != null ? Titles.LocalizeWithDefaultLocale() : "[unknown Quest]"; }
		}

		/// <summary>
		/// Text sumarizing the objectives of quest.
		/// </summary>
		[Persistent((int)ClientLocale.End)]
		public string[] Instructions;

		/// <summary>
		/// Objective of the quest to be shown in <see cref="QuestLog"/> in the server's default language.
		/// </summary>
		[NotPersistent]
		public string DefaultObjective
		{
			get { return Instructions.LocalizeWithDefaultLocale(); }
		}

		/// <summary>
		/// Detailed quest descriptions shown in <see cref="QuestLog"/>
		/// </summary>
		[Persistent((int)ClientLocale.End)]
		public string[] Details;

		[NotPersistent]
		public string DefaultDetailText
		{
			get { return Details.LocalizeWithDefaultLocale(); }
		}

		/// <summary>
		/// Text which is QuestGiver going to say upon quest finishing.
		/// </summary>
		[Persistent((int)ClientLocale.End)]
		public string[] EndTexts;

		[NotPersistent]
		public string DefaultEndText
		{
			get { return EndTexts.LocalizeWithDefaultLocale(); }
		}

		///<summary>
		/// Text which is displayed in quest objectives window once all objectives are completed 
		/// </summary>
		[Persistent((int)ClientLocale.End)]
		public string[] CompletedTexts;

		[NotPersistent]
		public string DefaultCompletedText
		{
			get { return CompletedTexts.LocalizeWithDefaultLocale(); }
		}

		/// <summary>
		/// Array of interactions containing ID, index and quantity.
		/// </summary>
		[Persistent(QuestConstants.MaxObjectInteractions)]
		public QuestInteractionTemplate[] ObjectOrSpellInteractions = new QuestInteractionTemplate[QuestConstants.MaxObjectInteractions];

        /// <summary>
        /// Array of source items interactions containing ID and quantity.
        /// </summary>
        [Persistent(QuestConstants.MaxObjectInteractions)]
        public ItemStackDescription[] CollectableSourceItems = new ItemStackDescription[QuestConstants.MaxObjectInteractions];

		[NotPersistent]
		public QuestInteractionTemplate[] GOInteractions;

		public bool HasGOEvent
		{
			get { return GOInteractions != null || GOInteraction != null; }
		}

		[NotPersistent]
		public QuestInteractionTemplate[] NPCInteractions;

		[NotPersistent]
		public QuestInteractionTemplate[] SpellInteractions;

		public bool HasObjectOrSpellInteractions
		{
			get; 
			private set;
		}

		public bool RequiresSpellCasts
		{
			get;
			private set;
		}

		public bool HasNPCInteractionEvent
		{
			get
			{
				return NPCInteracted != null ||
				(NPCInteractions != null && NPCInteractions.Length > 0);
			}
		}

		/// <summary>
		/// Array of items you need to collect.
		/// If the items are quest-only,
		/// they will be deleted upon canceling quest.
		/// </summary>
		[Persistent(QuestConstants.MaxObjectInteractions)]
		public ItemStackDescription[] CollectableItems;

		/// <summary>
		/// Array of quest objectives text, every value is a short note that is shown
		/// once all objectives of the corresponding slot have been fullfilled.
		/// </summary>
		[Persistent((int)ClientLocale.End)]
		public QuestObjectiveSet[] ObjectiveTexts = new QuestObjectiveSet[(int)ClientLocale.End];

        [NotPersistent]
	    public List<uint> EventIds = new List<uint>();

		#endregion

		#region QuestSettings

		/// <summary>
		/// Special Quest flags, unknown purpose.
		/// </summary>
		public QuestSpecialFlags SpecialFlags;

		/// <summary>
		/// Time limit for timed Quest. It's not taken into account
		/// if there's no Timed flag set in QuestFlags
		/// </summary>
		public uint TimeLimit;

		/// <summary>
		/// Text which will be shown when the objectives are done. In the
		/// offering rewards window.
		/// </summary>
		[Persistent((int)ClientLocale.End)]
		public string[] OfferRewardTexts;

		[NotPersistent]
		public string DefaultOfferRewardText
		{
			get { return OfferRewardTexts.LocalizeWithDefaultLocale(); }
		}

		/// <summary>
		/// Text which will be shown when the objectives aren't done yet. In the 
		/// window where you have to have items.
		/// </summary>
		[Persistent((int)ClientLocale.End)]
		public string[] ProgressTexts;

		[NotPersistent]
		public string DefaultProgressText
		{
			get { return ProgressTexts.LocalizeWithDefaultLocale(); }
		}

		/// <summary>
		/// Value indicating whether this <see cref="QuestTemplate"/> is repeatable.
		/// </summary>
		public bool Repeatable;

		/// <summary>
		/// Value indicating whether this <see cref="QuestTemplate"/> is available only for clients
		/// with expansion.
		/// probably obsolete, there is QuestFlag for this
		/// </summary>
		public ClientId RequiredClientId;

		/// <summary>
		/// Array of Items to be given upon accepting the quest. These items will be destroyed when the Quest is solved or canceled.
		/// </summary>
		[NotPersistent]
		public List<ItemStackDescription> ProvidedItems = new List<ItemStackDescription>(1);

		#endregion

		#region QuestRequirements

		/// <summary>
		/// Required minimal level to be able to see this quest.
		/// </summary>
		public uint RequiredLevel;

		/// <summary>
		/// Required race mask to check availability to player.
		/// </summary>
		public RaceMask RequiredRaces;

		/// <summary>
		/// Required class mask to check availability to player.
		/// </summary>
		public ClassMask RequiredClass;

		public int ReqSkillOrClass;

		public SkillId RequiredSkill;

		/// <summary>
		/// Tradeskill level which is required to accept this quest.
		/// </summary>
		public uint RequiredSkillValue;

		/// <summary>
		/// Represents the Reward XP column id.
		/// </summary>
		public int RewXPId;
		#endregion

		#region Graph
		// Represents the Quest graph
		public int PreviousQuestId, NextQuestId, ExclusiveGroup;
		public uint FollowupQuestId;

		/// <summary>
		/// 
        /// </summary>
#pragma warning disable 0675
        public bool ShouldBeConnectedInGraph
		{
			get { return (PreviousQuestId | NextQuestId | ExclusiveGroup | FollowupQuestId) != 0; }
		}
#pragma warning restore

		/// <summary>
		/// Quests that may must all be active in order to get this Quest
		/// </summary>
		[NotPersistent]
		public readonly List<uint> ReqAllActiveQuests = new List<uint>(2);

		/// <summary>
		/// Quests that must all be finished in order to get this Quest
		/// </summary>
		[NotPersistent]
		public readonly List<uint> ReqAllFinishedQuests = new List<uint>(2);

		/// <summary>
		/// Quests of which at least one must be active to get this Quest
		/// </summary>
		[NotPersistent]
		public readonly List<uint> ReqAnyActiveQuests = new List<uint>(2);

		/// <summary>
		/// Quests of which at least one must be finished to get this Quest
		/// </summary>
		[NotPersistent]
		public readonly List<uint> ReqAnyFinishedQuests = new List<uint>(2);

		/// <summary>
		/// Quests of which none may have been accepted or completed
		/// </summary>
		[NotPersistent]
		public readonly List<uint> ReqUndoneQuests = new List<uint>(2);

		#endregion

		#region QuestObjectives

		/// <summary>
		/// Triggers or areas which are objective to be explored as requirements.
		/// </summary>
		[NotPersistent]
		public uint[] AreaTriggerObjectives = new uint[0];

		/// <summary>
		/// Number of players to kill
		/// </summary>
		public uint PlayersSlain;
		#endregion

		#region QuestRewards
		/// <summary>
		/// Array of <see href="ReputationReward">ReputationRewards</see>
		/// </summary>
		[Persistent(QuestConstants.MaxReputations)]
		public ReputationReward[] RewardReputations = new ReputationReward[5];

		public uint RewHonorAddition;

		/// <summary>
		/// Multiplier of reward honor
		/// </summary>
		public float RewHonorMultiplier;
		#endregion

		#region QuestEmotes
		public uint OfferRewardEmoteDelay;
		public EmoteType OfferRewardEmoteType;

		public uint RequestItemsEmoteDelay;
		public EmoteType RequestItemsEmoteType;

		public EmoteType RequestEmoteType;

		[Persistent(QuestConstants.MaxEmotes)]
		public EmoteTemplate[] QuestDetailedEmotes = new EmoteTemplate[4];

		[Persistent(QuestConstants.MaxEmotes)]
		public EmoteTemplate[] OfferRewardEmotes = new EmoteTemplate[4];

		#endregion

		/// <summary>
		/// Value indicating whether this <see cref="QuestTemplate"/> is shareable.
		/// </summary>
		public bool Sharable
		{
			get { return Flags.HasFlag(QuestFlags.Sharable); }
		}

		/// <summary>
		/// Value indicating whether this <see cref="QuestTemplate"/> is daily.
		/// </summary>
		public bool IsDaily
		{
			get { return Flags.HasFlag(QuestFlags.Daily); }
		}

		#region Modify Templates
		/// <summary>
		/// To finish this Quest the Character has to interact with the given
		/// kind of GO the given amount of times. This is a unique interaction.
		/// To Add more than one GO for a particular objective add the first with
		/// this method then use <seealso cref="AddLinkedGOInteraction"/>
		/// </summary>
		/// <param name="goId">The entry id of the GO that must be interacted with</param>
		/// <param name="amount">The number of times this GO must be interacted with</param>
		/// <returns>The index of this template in the <see cref="GOInteractions"/> array</returns>
		public int AddGOInteraction(GOEntryId goId, int amount, SpellId requiredSpell = SpellId.None)
		{
			int goIndex;
			if (GOInteractions == null)
			{
				goIndex = 0;
				GOInteractions = new QuestInteractionTemplate[1];
			}
			else
			{
				goIndex = GOInteractions.Length;
				Array.Resize(ref GOInteractions, goIndex + 1);
			}

			var index = ObjectOrSpellInteractions.GetFreeIndex();

			var templ = new QuestInteractionTemplate
			{
				Index = index,
				Amount = amount,
				RequiredSpellId = requiredSpell,
				ObjectType = ObjectTypeId.GameObject
			};
			templ.TemplateId[0] = (uint) goId;

			ArrayUtil.Set(ref ObjectOrSpellInteractions, index, templ);
			GOInteractions[goIndex] = templ;

			return goIndex;
		}

		/// <summary>
		/// Adds an alternative GO to the interaction template that may also
		/// be interacted with for this quest objective <seealso cref="AddGOInteraction"/>
		/// </summary>
		/// <param name="index">The index into the <see cref="GOInteractions"/>
		/// array where the first GO was added with <seealso cref="AddGOInteraction"/></param>
		/// <param name="goEntry">The entry id of the alternative GO that can be interacted with</param>
		public void AddLinkedGOInteraction(uint index, GOEntryId goEntry)
		{
			var templ = GOInteractions.Get(index);
			if (templ.TemplateId.Contains((uint)goEntry))
				return;
			Array.Resize(ref templ.TemplateId, templ.TemplateId.Length + 1);
			ArrayUtil.Add(ref templ.TemplateId, (uint)goEntry);
		}

		/// <summary>
		/// Adds alternative GOs to the interaction template that may also
		/// be interacted with for this quest objective. <seealso cref="AddGOInteraction"/>
		/// </summary>
		/// <param name="index">The index into the <see cref="GOInteractions"/> array
		/// where the first GO was added with <seealso cref="AddGOInteraction"/></param>
		/// <param name="goids">The entry ids of the alternative GOs that can be interacted with</param>
		public void AddLinkedGOInteractions(uint index, IEnumerable<GOEntryId> goids)
		{
			var templ = NPCInteractions.Get(index);
			Array.Resize(ref templ.TemplateId, templ.TemplateId.Length + goids.Count());
			foreach (var npcId in goids.Where(npcId => !templ.TemplateId.Contains((uint)npcId)))
			{
				ArrayUtil.Add(ref templ.TemplateId, (uint)npcId);
			}
		}

		/// <summary>
		/// Adds alternative GOs to the interaction template that may also
		/// be interacted with for this quest objective. <seealso cref="AddGOInteraction"/>
		/// </summary>
		/// <param name="index">The index into the <see cref="GOInteractions"/> array
		/// where the first GO was added with <seealso cref="AddGOInteraction"/></param>
		/// <param name="goids">The entry ids of the alternative GOs that can be interacted with</param>
		public void AddLinkedGOInteractions(uint index, params GOEntryId[] goids)
		{
			var templ = NPCInteractions.Get(index);
			Array.Resize(ref templ.TemplateId, templ.TemplateId.Length + goids.Count());
			foreach (var npcId in goids.Where(npcId => !templ.TemplateId.Contains((uint)npcId)))
			{
				ArrayUtil.Add(ref templ.TemplateId, (uint)npcId);
			}
		}

		/// <summary>
		/// Returns the first QuestInteractionTemplate that requires the given NPC to be interacted with
		/// </summary>
		public QuestInteractionTemplate GetInteractionTemplateFor(GOEntryId goEntryId)
		{
			return GOInteractions.FirstOrDefault(interaction => interaction.TemplateId.Contains((uint)goEntryId));
		}

		public void AddProvidedItem(ItemId id, int amount = 1)
		{
			ProvidedItems.Add(new ItemStackDescription(id, amount));
		}

		public void AddAreaTriggerObjective(uint id)
		{
			ArrayUtil.AddOnlyOne(ref AreaTriggerObjectives, id);
		}

		/// <summary>
		/// To finish this Quest the Character has to interact with the given
		/// kind of NPC the given amount of times. This is a unique interaction.
		/// To Add more than one NPC for a particular objective add the first with
		/// this method then use <seealso cref="AddLinkedNPCInteraction"/>
		/// </summary>
		/// <param name="npcid">The entry id of the NPC that must be interacted with</param>
		/// <param name="amount">The number of times this NPC must be interacted with</param>
		/// <returns>The index of this template in the <see cref="NPCInteractions"/> array</returns>
		public int AddNPCInteraction(NPCId npcid, int amount, SpellId requiredSpell = SpellId.None)
		{
			int npcIndex;
			if (NPCInteractions == null)
			{
				npcIndex = 0;
				NPCInteractions = new QuestInteractionTemplate[1];
			}
			else
			{
				npcIndex = NPCInteractions.Length;
				Array.Resize(ref NPCInteractions, npcIndex + 1);
			}

			var index = ObjectOrSpellInteractions.GetFreeIndex();

			var templ = new QuestInteractionTemplate
			{
				Index = index,
				Amount = amount,
				RequiredSpellId = requiredSpell,
				ObjectType = ObjectTypeId.GameObject
			};
			templ.TemplateId[0] = (uint) npcid;

			ArrayUtil.Set(ref ObjectOrSpellInteractions, index, templ);
			NPCInteractions[npcIndex] = templ;

			return npcIndex;
		}

		/// <summary>
		/// Adds an alternative NPC to the interaction template that may also
		/// be interacted with for this quest objective. <seealso cref="AddNPCInteraction"/>
		/// </summary>
		/// <param name="index">The index into the <see cref="NPCInteractions"/> array
		/// where the first NPC was added with <seealso cref="AddNPCInteraction"/></param>
		/// <param name="npcid">The entry id of the alternative NPC that can be interacted with</param>
		public void AddLinkedNPCInteraction(uint index, NPCId npcid)
		{
			var templ = NPCInteractions.Get(index);
			if (templ.TemplateId.Contains((uint)npcid))
				return;
			Array.Resize(ref templ.TemplateId, templ.TemplateId.Length + 1);
			ArrayUtil.Add(ref templ.TemplateId, (uint)npcid);
		}

		/// <summary>
		/// Adds alternative NPCs to the interaction template that may also
		/// be interacted with for this quest objective. <seealso cref="AddNPCInteraction"/>
		/// </summary>
		/// <param name="index">The index into the <see cref="NPCInteractions"/> array
		/// where the first NPC was added with <seealso cref="AddNPCInteraction"/></param>
		/// <param name="npcids">The entry ids of the alternative NPCs that can be interacted with</param>
		public void AddLinkedNPCInteractions(uint index, IEnumerable<NPCId> npcids)
		{
			var templ = NPCInteractions.Get(index);
			Array.Resize(ref templ.TemplateId, templ.TemplateId.Length + npcids.Count());
			foreach (var npcId in npcids.Where(npcId => !templ.TemplateId.Contains((uint)npcId)))
			{
				ArrayUtil.Add(ref templ.TemplateId, (uint)npcId);
			}
		}

		/// <summary>
		/// Adds alternative NPCs to the interaction template that may also
		/// be interacted with for this quest objective. <seealso cref="AddNPCInteraction"/>
		/// </summary>
		/// <param name="index">The index into the <see cref="NPCInteractions"/> array
		/// where the first NPC was added with <seealso cref="AddNPCInteraction"/></param>
		/// <param name="npcids">The entry ids of the alternative NPCs that can be interacted with</param>
		public void AddLinkedNPCInteractions(uint index, params NPCId[] npcids)
		{
			var templ = NPCInteractions.Get(index);
			Array.Resize(ref templ.TemplateId, templ.TemplateId.Length + npcids.Count());
			foreach (var npcId in npcids.Where(npcId => !templ.TemplateId.Contains((uint)npcId)))
			{
				ArrayUtil.Add(ref templ.TemplateId, (uint)npcId);
			}
		}

		/// <summary>
		/// Returns the first QuestInteractionTemplate that requires the given NPC to be interacted with
		/// </summary>
		public QuestInteractionTemplate GetInteractionTemplateFor(NPCId npcId)
		{
			return NPCInteractions.FirstOrDefault(interaction => interaction.TemplateId.Contains((uint) npcId));
		}

		#endregion

		#region Requirements
		/// <summary>
		/// Checks whether the given Character may do this Quest
		/// </summary>
		public QuestInvalidReason CheckBasicRequirements(Character chr)
		{
		    if (RequiredRaces != 0 && !RequiredRaces.HasAnyFlag(chr.RaceMask))
			{
				return QuestInvalidReason.WrongRace;
			}
			if (RequiredClass != 0 && !RequiredClass.HasAnyFlag(chr.ClassMask))
			{
				return QuestInvalidReason.WrongClass;
			}

			if (RequiredSkill != SkillId.None && RequiredSkill > 0 &&
				!chr.Skills.CheckSkill(RequiredSkill, (int)RequiredSkillValue))
			{
				return QuestInvalidReason.NoRequirements;
			}

			var err = CheckRequiredActiveQuests(chr.QuestLog);
			if (err != QuestInvalidReason.Ok)
			{
				return err;
			}

			err = CheckRequiredFinishedQuests(chr.QuestLog);
			if (err != QuestInvalidReason.Ok)
			{
				return err;
			}

			if (IsDaily && (chr.QuestLog.CurrentDailyCount >= QuestLog.MaxDailyQuestCount))
			{
				return QuestInvalidReason.TooManyDailys;
			}

			if (chr.Account.ClientId < RequiredClientId)
			{
				return QuestInvalidReason.NoExpansionAccount;
			}

			if (chr.QuestLog.TimedQuestSlot != null)
			{
				return QuestInvalidReason.AlreadyOnTimedQuest;
			}
			if (chr.Level < RequiredLevel)
			{
				return QuestInvalidReason.LowLevel;
			}
			if (RewMoney < 0 && chr.Money < -RewMoney)
			{
				return QuestInvalidReason.NotEnoughMoney;
			}
			//TimeOut = 27 how the heck to work with this one?
            if (EventIds.Count != 0)
            {
                bool ok = EventIds.Where(WorldEventMgr.IsEventActive).Any();
                if (!ok)
                    return QuestInvalidReason.NoRequirements;
            }
			return QuestInvalidReason.Ok;
		}

		/// <summary>
		/// Check quest-relation requirements of active quests
		/// </summary>
		private QuestInvalidReason CheckRequiredActiveQuests(QuestLog log)
		{
			for (int i = 0; i < ReqAllActiveQuests.Count; i++)
			{
				// all of these quests must be active
				var preqId = ReqAllActiveQuests[i];
				if (!log.HasActiveQuest(preqId))
				{
					return QuestInvalidReason.NoRequirements;
				}
			}

			if (ReqAnyActiveQuests.Count > 0)
			{
				var found = false;
				for (int i = 0; i < ReqAnyActiveQuests.Count; i++)
				{
					// any of these quests must be active
					var preqId = ReqAnyActiveQuests[i];
					if (log.HasActiveQuest(preqId))
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					return QuestInvalidReason.NoRequirements;
				}
			}
			return QuestInvalidReason.Ok;
		}

		/// <summary>
		/// Check quest-relation requirements of quests that need to be finished for this one to start
		/// </summary>
		private QuestInvalidReason CheckRequiredFinishedQuests(QuestLog log)
		{
			for (int i = 0; i < ReqAllFinishedQuests.Count; i++)
			{
				var preqId = ReqAllFinishedQuests[i];
				if (!log.FinishedQuests.Contains(preqId))
				{
					return QuestInvalidReason.NoRequirements;
				}
			}

			if (ReqAnyActiveQuests.Count > 0)
			{
				var found = false;
				for (int i = 0; i < ReqAnyFinishedQuests.Count; i++)
				{
					var preqId = ReqAnyFinishedQuests[i];
					if (log.FinishedQuests.Contains(preqId))
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					return QuestInvalidReason.NoRequirements;
				}
			}

			if (ReqUndoneQuests.Count > 0)
			{
				for (var i = 0; i < ReqUndoneQuests.Count; i++)
				{
					var preqId = ReqUndoneQuests[i];
					if (log.FinishedQuests.Contains(preqId) || log.HasActiveQuest(preqId))
					{
						return QuestInvalidReason.NoRequirements;
					}
				}
			}
			return QuestInvalidReason.Ok;
		}

		/// <summary>
		/// Determines whether is quest obsolete for given character.
		/// </summary>
		/// <param name="chr">The character.</param>
		/// <returns>
		/// 	<c>true</c> if [is quest obsolete] [the specified qt]; otherwise, <c>false</c>.
		/// </returns>
		public bool IsObsolete(Character chr)
		{
			return chr.Level >= RequiredLevel + QuestMgr.LevelObsoleteOffset;
		}

		/// <summary>
		/// Determines whether [is quest too high level] [the specified qt].
		/// </summary>
		/// <param name="chr">The CHR.</param>
		/// <returns>
		/// 	<c>true</c> if [is quest too high level] [the specified qt]; otherwise, <c>false</c>.
		/// </returns>
		public bool IsTooHighLevel(Character chr)
		{
			return chr.Level + QuestMgr.LevelRequirementOffset < RequiredLevel;
		}
		#endregion

		#region Status
		/// <summary>
		/// Checks the requirements and returns the QuestStatus for ending a Quest.
		/// </summary>
		public QuestStatus GetStartStatus(QuestHolderInfo qh, Character chr)
		{
			var quest = chr.QuestLog.GetActiveQuest(Id);
			if (quest != null)
			{
				return QuestStatus.NotAvailable;
			}

			if (!Repeatable && chr.QuestLog.FinishedQuests.Contains(Id))
			{
				return QuestStatus.NotAvailable;
			}

			var status = CheckBasicRequirements(chr);
			if (status == QuestInvalidReason.LowLevel)
			{
				return QuestStatus.TooHighLevel;
			}

			if (status != QuestInvalidReason.Ok)
			{
				return QuestStatus.NotAvailable;
			}

			if (Repeatable)
			{
				return QuestStatus.Repeatable;
			}

			return IsObsolete(chr) ? QuestStatus.Obsolete : QuestStatus.Available;
		}

		/// <summary>
		/// </summary>
		public QuestStatus GetAvailability(Character chr)
		{
			var status = CheckBasicRequirements(chr);
			if (status == QuestInvalidReason.LowLevel)
			{
				return QuestStatus.TooHighLevel;
			}

			if (IsObsolete(chr))
			{
				return Repeatable ? QuestStatus.Repeatable : QuestStatus.Obsolete;
			}

			return Repeatable ? QuestStatus.Repeatable : QuestStatus.Available;
		}

		/// <summary>
		/// Checks the requirements and returns the QuestStatus for ending a Quest.
		/// </summary>
		/// <param name="chr">The client.</param>
		/// <returns></returns>
		public QuestStatus GetEndStatus(Character chr)
		{
			var quest = chr.QuestLog.GetActiveQuest(Id);
			if (quest == null)
			{
				return QuestStatus.NotAvailable;
			}

			return quest.Status;
		}

		#endregion

		#region QuestScripts

		// nothing yet

		#endregion

		#region Starters and Finishers
		/// <summary>
		/// Returns the GOEntry with the given id or null
		/// </summary>
		public GOEntry GetStarter(GOEntryId id)
		{
			for (var i = 0; i < Starters.Count; i++)
			{
				var starter = Starters[i];
				if (starter is GOEntry && starter.Id == (uint)id)
				{
					return (GOEntry)starter;
				}
			}
			return null;
		}

		/// <summary>
		/// Returns the NPCEntry with the given id or null
		/// </summary>
		public NPCEntry GetStarter(NPCId id)
		{
			for (var i = 0; i < Starters.Count; i++)
			{
				var starter = Starters[i];
				if (starter is NPCEntry && starter.Id == (uint)id)
				{
					return (NPCEntry)starter;
				}
			}
			return null;
		}

		/// <summary>
		/// Returns the ItemTemplate with the given id or null
		/// </summary>
		public ItemTemplate GetStarter(ItemId id)
		{
			for (var i = 0; i < Starters.Count; i++)
			{
				var starter = Starters[i];
				if (starter is ItemTemplate && starter.Id == (uint)id)
				{
					return (ItemTemplate)starter;
				}
			}
			return null;
		}

		/// <summary>
		/// Returns the Starter of the given Type which has the given Id
		/// </summary>
		public T GetStarter<T>(uint id)
			where T : IQuestHolderEntry
		{
			for (var i = 0; i < Starters.Count; i++)
			{
				var starter = Starters[i];
				if (starter is T && starter.Id == id)
				{
					return (T)starter;
				}
			}
			return default(T);
		}

		/// <summary>
		/// Returns the GOEntry with the given id or null
		/// </summary>
		public GOEntry GetFinisher(GOEntryId id)
		{
			for (var i = 0; i < Finishers.Count; i++)
			{
				var finisher = Finishers[i];
				if (finisher is GOEntry && finisher.Id == (uint)id)
				{
					return (GOEntry)finisher;
				}
			}
			return null;
		}

		/// <summary>
		/// Returns the NPCEntry with the given id or null
		/// </summary>
		public NPCEntry GetFinisher(NPCId id)
		{
			for (var i = 0; i < Finishers.Count; i++)
			{
				var finisher = Finishers[i];
				if (finisher is NPCEntry && finisher.Id == (uint)id)
				{
					return (NPCEntry)finisher;
				}
			}
			return null;
		}

		/// <summary>
		/// Returns the Finisher of the given Type which has the given Id
		/// </summary>
		public T GetFinisher<T>(uint id)
			where T : IQuestHolderEntry
		{
			for (var i = 0; i < Finishers.Count; i++)
			{
				var finisher = Finishers[i];
				if (finisher is T && finisher.Id == id)
				{
					return (T)finisher;
				}
			}
			return default(T);
		}
		#endregion

		#region Interactions
		/// <summary>
		/// Tries to give all Initial Items (or none at all).
		/// </summary>
		/// <remarks>If not all Initial Items could be given, the Quest cannot be started.</remarks>
		/// <param name="receiver"></param>
		/// <returns>Whether initial Items were given.</returns>
		public bool GiveInitialItems(Character receiver)
		{
			if (ProvidedItems.Count > 0)
			{
				var err = receiver.Inventory.TryAddAll(ProvidedItems.ToArray());
				if (err != InventoryError.OK)
				{
					ItemHandler.SendInventoryError(receiver.Client, null, null, err);
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Tries to give all Rewards (or none at all).
		/// </summary>
		/// <remarks>If not all Rewards could be given, the Quest remains completable.</remarks>
		/// <param name="receiver"></param>
		/// <param name="qHolder"></param>
		/// <param name="rewardSlot">The slot of choosable items</param>
		/// <returns>Whether Rewards were given.</returns>
		public bool TryGiveRewards(Character receiver, IQuestHolder qHolder, uint rewardSlot)
		{
			if (RewMoney < 0 && receiver.Money - RewMoney < 0)
			{
				QuestHandler.SendRequestItems(qHolder, this, receiver, true);
				return false;
			}

			return GiveRewards(receiver, rewardSlot);
		}

		public bool GiveRewards(Character receiver, uint rewardSlot)
		{
			ItemStackDescription[] items;
			var rewardItemCount = RewardItems.Length;
			if (rewardSlot < RewardChoiceItems.Length)
			{
				items = new ItemStackDescription[rewardItemCount + 1];
				Array.Copy(RewardItems, items, rewardItemCount);
				items[rewardItemCount] = RewardChoiceItems[rewardSlot];
			}
			else
			{
				items = RewardItems;
			}

			var err = receiver.Inventory.TryAddAll(items);
			if (err != InventoryError.OK)
			{
				ItemHandler.SendInventoryError(receiver.Client, null, null, err);
				return false;
			}

			if (!Repeatable)
			{
				if (receiver.Level >= RealmServerConfiguration.MaxCharacterLevel)
				{
					receiver.Money += MoneyAtMaxLevel;
                    receiver.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.MoneyFromQuestReward, MoneyAtMaxLevel);
				}
				else
				{
					receiver.GainXp(CalcRewardXp(receiver), false);
				}
			}

			if (RewMoney > 0)
			{
				receiver.Money += (uint)RewMoney;
                receiver.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.MoneyFromQuestReward, (uint)RewMoney);
			}

			for (var i = 0; i < QuestConstants.MaxReputations; i++)
			{
				if (RewardReputations[i].Faction != 0)
				{
					var value = CalcRewRep(RewardReputations[i].ValueId, RewardReputations[i].Value);
					receiver.Reputations.GainReputation(RewardReputations[i].Faction, value);
				}
			}
			if (RewardTitleId != TitleId.None)
			{
				receiver.SetTitle(RewardTitleId, false);
			}

			if(CastSpell != SpellId.None)
			{
				receiver.SpellCast.TriggerSelf(CastSpell);
			}
			return true;
		}

		public int CalcRewRep(int valueId, int value)
		{
			if (value != 0)
				return value * 100;

			var index = (valueId > 0) ? 0 : 1;
			return QuestMgr.QuestRewRepInfos[index].RewRep[valueId - 1];
		}

		public int CalcRewardHonor(Character character)
		{
			int fullhonor = 0;
			if (RewHonorAddition > 0 || RewHonorMultiplier > 0.0f)
			{
				var info = QuestMgr.QuestHonorInfos.Get(Level);
				if (info != null)
				{
					fullhonor = (int)(info.RewHonor * RewHonorMultiplier * 0.1000000014901161);
					fullhonor += (int)RewHonorAddition;
				}
			}
			return fullhonor;
		}

		public int CalcRewardXp(Character character)
		{
			var info = QuestMgr.QuestXpInfos.Get(Level);
			int fullxp;
			if (info != null)
			{
				fullxp = info.RewXP.Get((uint)RewXPId - 1u);
			}
			else
			{
				// TODO: What to do with quests with funky levels
				fullxp = (int)(MinLevel * 100);
			}
			fullxp = (fullxp * character.QuestExperienceGainModifierPercent / 100);

			int playerLevel = character.Level;

			if (playerLevel <= Level + 5)
			{
				return fullxp;
			}
			if (playerLevel == Level + 6)
			{
				return (fullxp * 8) / 10;
			}
			if (playerLevel == Level + 7)
			{
				return (fullxp * 6) / 10;
			}
			if (playerLevel == Level + 8)
			{
				return (fullxp * 4) / 10;		// 0.4f
			}
			if (playerLevel == Level + 9)
			{
				return fullxp / 5;
			}
			return fullxp / 10;
		}

		#endregion

		#region Dump
		public void Dump(IndentTextWriter writer)
		{
			//writer.WriteLine(this);
			writer.WriteLineNotDefault(QuestType, "Type: " + QuestType);
			writer.WriteLineNotDefault(Flags, "Flags: " + Flags);
			writer.WriteLineNotDefault(RequiredLevel, "RequiredLevel: " + RequiredLevel);
			writer.WriteLineNotDefault(RequiredRaces, "Races: " + RequiredRaces);
			writer.WriteLineNotDefault(RequiredClass, "Class: " + RequiredClass);
			writer.WriteLineNotDefault(ProvidedItems.Count, "ProvidedItems: " + ProvidedItems.ToString(", "));
			writer.WriteLineNotDefault(Starters.Count, "Starters: " + Starters.ToString(", "));
			writer.WriteLineNotDefault(Finishers.Count, "Finishers: " + Finishers.ToString(", "));

			var interactions = ObjectOrSpellInteractions.Where(action => action != null && action.TemplateId[0] != 0).ToList();
			writer.WriteLineNotDefault(interactions.Count(), "Interactions: " + interactions.ToString(", "));

			if (CollectableItems != null && CollectableItems.Length > 0)
			{
				writer.WriteLine("Collectables: " + CollectableItems.ToString(", "));
			}

			writer.WriteLineNotDefault(AreaTriggerObjectives.Length,
				"Req AreaTriggers: " + AreaTriggerObjectives.TransformArray(id => AreaTriggerMgr.GetTrigger(id)).ToString(", "));

			if (Instructions != null)
			{
				var ins = Instructions.Where(obj => !string.IsNullOrEmpty(obj));
				writer.WriteLineNotDefault(ins.Count(), "Instructions: " + ins.ToString(" / ") + "");
			}

			if (ShouldBeConnectedInGraph)
			{
				writer.WriteLine();
				writer.WriteLine("PreviousQuestId: {0}, NextQuestId: {1}, ExclusiveGroup: {2}, FollowupQuestId: {3} ", 
					PreviousQuestId, NextQuestId, ExclusiveGroup, FollowupQuestId);
				writer.WriteLineNotDefault(ReqAllActiveQuests.Count, "ReqAllActiveQuests: " + MakeQuestString(ReqAllActiveQuests));
				writer.WriteLineNotDefault(ReqAllFinishedQuests.Count, "ReqAllFinishedQuests: " + MakeQuestString(ReqAllFinishedQuests));
				writer.WriteLineNotDefault(ReqAnyActiveQuests.Count, "ReqAnyActiveQuests: " + MakeQuestString(ReqAnyActiveQuests));
				writer.WriteLineNotDefault(ReqAnyFinishedQuests.Count, "ReqAnyFinishedQuests: " + MakeQuestString(ReqAnyFinishedQuests));
				writer.WriteLineNotDefault(ReqUndoneQuests.Count, "ReqUndoneQuests: " + MakeQuestString(ReqUndoneQuests));
			}
		}

		string MakeQuestString(IEnumerable<uint> questIds)
		{
			return Utility.GetStringRepresentation(questIds.Select(QuestMgr.GetTemplate));
		}

		#endregion

		public override string ToString()
		{
			return DefaultTitle + " (Id: " + Id + ")";
		}

		#region Events
		internal void NotifyStarted(Quest quest)
		{
			var evt = QuestStarted;
			if (evt != null)
			{
				evt(quest);
			}
		}

		internal void NotifyFinished(Quest quest)
		{
			var evt = QuestFinished;
			if (evt != null)
			{
				evt(quest);
			}
		}

		internal void NotifyCancelled(Quest quest, bool failed)
		{
			var evt = QuestCancelled;
			if (evt != null)
			{
				evt(quest, failed);
			}
		}

		internal void NotifyNPCInteracted(Quest quest, NPC npc)
		{
			var evt = NPCInteracted;
			if (evt != null)
			{
				evt(quest, npc);
			}
		}

		internal void NotifyGOUsed(Quest quest, GameObject go)
		{
			var evt = GOInteraction;
			if (evt != null)
			{
				evt(quest, go);
			}
		}
		#endregion

		public static IEnumerable<QuestTemplate> GetAllDataHolders()
		{
			return QuestMgr.Templates;
		}

		#region Deserialization
		public void FinalizeDataHolder()
		{
            //if(Condition > 0) else if(Condition) < 0, so that values
            //are not overwritten as SFDB uses this column.
            //but UDB doesnt which would mean that,
            //if(Condition > 0) else would make RequiredClass always 0
			if (ReqSkillOrClass > 0)
			{
				// skill
				RequiredSkill = (SkillId)ReqSkillOrClass;
			}
			else if(ReqSkillOrClass < 0)
			{
				// class
				RequiredClass = (ClassMask)(-ReqSkillOrClass);
			}

			if (Category < 0)
			{
				// QuestSort
				var clss = ((QuestSort)(-Category)).GetClassId();
				if (clss != ClassId.End)
				{
					RequiredClass = clss.ToMask();
				}
			}
			else if (Category > 0)
			{
				ZoneTemplate = World.GetZoneInfo((ZoneId)Category);
			}

			List<QuestInteractionTemplate> goInteractions = null;
			List<QuestInteractionTemplate> npcInteractions = null;
			for (uint i = 0; i < ObjectOrSpellInteractions.Length; i++)
			{
				var interaction = ObjectOrSpellInteractions[i];
				if (interaction == null || !interaction.IsValid) continue;
				HasObjectOrSpellInteractions = true;

				if (interaction.RequiredSpellId != 0)
				{
					// SpellCast objective
					RequiresSpellCasts = true;
					if (SpellInteractions == null)
					{
						SpellInteractions = new[] { interaction };
					}
					else
					{
						ArrayUtil.AddOnlyOne(ref SpellInteractions, interaction);
					}
				}
				else
				{
					if (interaction.ObjectType == ObjectTypeId.GameObject)
					{
						// GO objective
						(goInteractions = goInteractions.NotNull()).Add(interaction);
					}
					else
					{
						// NPC interactive
						(npcInteractions = npcInteractions.NotNull()).Add(interaction);
					}
				}
				interaction.Index = i;
			}

			//if (AreaTriggerObjectiveIds == null)
			//{
			//    AreaTriggerObjectiveIds = new uint[0];
			//}
			//else
			//{
			//    ArrayUtil.PruneVals(ref AreaTriggerObjectiveIds);
			//    for (var i = 0; i < AreaTriggerObjectiveIds.Length; i++)
			//    {
			//        var triggerId = AreaTriggerObjectiveIds[i];
			//        var trigger = AreaTriggerMgr.GetTrigger(triggerId);
			//        if (trigger == null)
			//        {
			//            ContentHandler.OnInvalidDBData("Invalid AreaTrigger {0} in Quest: " + this, triggerId);
			//        }
			//        else
			//        {
			//            trigger.TriggerQuest = this;
			//        }
			//    }
			//}

			// make sure that provided items are not required
			var colItems = new List<ItemStackDescription>(4);
			for (var i = 0; i < CollectableItems.Length; i++)
			{
				var item = CollectableItems[i];
				if (item.ItemId == 0 || ProvidedItems.Find(stack => stack.ItemId == item.ItemId).ItemId != 0)
				{
					continue;
				}
				colItems.Add(item);
			}
			CollectableItems = colItems.ToArray();

            for (var i = 0; i < CollectableSourceItems.Length; i++)
            {
                if (CollectableSourceItems[i].ItemId != ItemId.None && CollectableSourceItems[i].Amount == 0)
                {
                    CollectableSourceItems[i].Amount = 1;
                }
            }

			if (goInteractions != null)
			{
				GOInteractions = goInteractions.ToArray();
			}
			if (npcInteractions != null)
			{
				NPCInteractions = npcInteractions.ToArray();
			}

            foreach (var worldEventQuest in WorldEventMgr.WorldEventQuests.Where(worldEventQuest => worldEventQuest.QuestId == Id))
            {
                EventIds.Add(worldEventQuest.EventId);
            }

			ArrayUtil.PruneVals(ref AreaTriggerObjectives);
			ArrayUtil.PruneVals(ref RewardChoiceItems);
			ArrayUtil.PruneVals(ref RewardItems);

			QuestMgr.AddQuest(this);
		}
		#endregion
	}

	/// <summary>
	/// Consists of a Type of objects, an id of the object's Template and
	/// amount of objects to be searched for in order to complete a <see cref="Quest"/>.
	/// </summary>
	public class QuestInteractionTemplate
	{
		[NotPersistent]
		public uint[] TemplateId = new uint[1];

		/// <summary>
		/// Either <see cref="ObjectTypeId.Unit"/> or <see cref="ObjectTypeId.GameObject"/>
		/// </summary>
		[NotPersistent]
		public ObjectTypeId ObjectType = ObjectTypeId.None;

		public int Amount;

		/// <summary>
		/// Spell to be casted.
		/// If not set, the objective is to kill or use the target.
		/// </summary>
		public SpellId RequiredSpellId;

		[NotPersistent]
		public uint Index;

		/// <summary>
		/// The RawId is used in certain Packets.
		/// It encodes TemplateId and Type
		/// The setter should only ever be used
		/// when loading info from the database!
		/// </summary>
		public uint RawId
		{
			get
			{
				return ObjectType == ObjectTypeId.GameObject ?
					//(uint)-(int)(TemplateId | QuestConstants.GOIndicator) : TemplateId;
					(uint.MaxValue - TemplateId[0] + 1) : TemplateId[0];
			}
			set
			{
				if (value > QuestConstants.GOIndicator)
				{
					// GO
					TemplateId[0] = uint.MaxValue - value + 1;
					ObjectType = ObjectTypeId.GameObject;
				}
				else if (value != 0)
				{
					// NPC
					TemplateId[0] = value;
					ObjectType = ObjectTypeId.Unit;
				}
			}
		}

		public bool IsValid
		{
			get { return TemplateId[0] != 0 || RequiredSpellId != 0; }
		}

		public override string ToString()
		{
			var templates = TemplateId.Where(templ => templ != 0);
			return (Amount != 1 ? Amount + "x " : "") + ObjectType + " " + ObjectType.ToString(TemplateId, ", ") + (RequiredSpellId != 0 ? (" - Spell: " + RequiredSpellId) : "");
		}
	}

	[DataHolder]
	public class QuestPOI : IDataHolder
	{
		public uint QuestId;
		public uint PoiId;
		public int ObjectiveIndex;
		public MapId MapID;
		public ZoneId ZoneId;
		public uint FloorId;
		public uint Unk3;
		public uint Unk4;

		[NotPersistent]
		public List<QuestPOIPoints> Points = new List<QuestPOIPoints>();

		public void FinalizeDataHolder()
		{
			if (QuestMgr.POIs.ContainsKey(QuestId))
				QuestMgr.POIs[QuestId].Add(this);
			else
			{
				var list = new List<QuestPOI> { this };
				QuestMgr.POIs.Add(QuestId, list);
			}
		}
	}

	[DataHolder]
	public class QuestPOIPoints : IDataHolder
	{
		public uint QuestId;
		public uint PoiId;
		public float X;
		public float Y;

		public void FinalizeDataHolder()
		{
			List<QuestPOI> list;
			if (QuestMgr.POIs.TryGetValue(QuestId, out list))
			{
				foreach (var questpoi in list.Where(questpoi => questpoi.PoiId == PoiId))
				{
					questpoi.Points.Add(this);
				}
			}
		}
	}

	public struct EmoteTemplate
	{
		public uint Count;
		public uint Delay;
		public EmoteType Type;
	}

	public struct ReputationReward
	{
		//public FactionReputationIndex Faction;
		public FactionId Faction;
		public int ValueId;
		public int Value;
	}
}