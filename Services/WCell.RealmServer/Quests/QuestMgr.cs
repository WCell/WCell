/*************************************************************************
 *
 *   file		: QuestMgr.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-04-08 11:02:58 +0200 (út, 08 IV 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 244 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using NLog;
using WCell.Constants;
using WCell.Constants.Quests;
using WCell.Core;
using WCell.Core.DBC;
using WCell.Core.Initialization;
using WCell.Core.Network;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Items;
using WCell.RealmServer.NPCs;
using WCell.Util;
using WCell.Util.Variables;
using System.Collections.Generic;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Global;
using WCell.RealmServer.Spells;
using WCell.Constants.Spells;

namespace WCell.RealmServer.Quests
{
	/// <summary>
	/// Implementation of quest manager which is handlint most of the quest actions at server.
	/// TODO: Faction-restrictions
	/// </summary>
	[GlobalMgr]
	public static class QuestMgr
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		#region Global Variables
		/// <summary>
		/// Amount of levels above the Character level at which a player becomes obsolete to the player
		/// </summary>
		public static int LevelObsoleteOffset = 7;


		/// <summary>
		/// The Xp Reward Level that is used to determine the Xp reward upon completion.
		/// </summary>
		public static readonly QuestXPInfo[] QuestXpInfos = new QuestXPInfo[101];

		/// <summary>
		/// Amount of levels above the Character level at which a player is not allowed to do the Quest
		/// </summary>
		public static int LevelRequirementOffset = 1;

		//private static float sharingRadius = 30f;

		/*public static float SharingRadius
		{
			get { return sharingRadius; }
			set
			{
				sharingRadius = value;
				SharingRadiusSq = sharingRadius * sharingRadius;
			}
		}*/

		//[NotVariable]
		//public static float SharingRadiusSq = sharingRadius * sharingRadius;

		// private static int m_npcQuestGiverCount, m_npcQuestFinisherCount, m_goQuestGiverCount, m_goQuestFinisherCount;

		internal static uint _questCount;

		internal static int _questFinisherCount, _questStarterCount;

		public static uint QuestCount
		{
			get { return _questCount; }
		}

		public static int QuestFinisherCount
		{
			get { return _questFinisherCount; }
		}

		public static int QuestStarterCount
		{
			get { return _questStarterCount; }
		}

		private static bool loaded;

		public static bool Loaded
		{
			get { return loaded; }
			private set
			{
				if (loaded = value)
				{
					RealmServer.InitMgr.SignalGlobalMgrReady(typeof(QuestMgr));
				}
			}
		}

		[NotVariable]
		public static QuestTemplate[] Templates = new QuestTemplate[30000];

		public static QuestTemplate GetTemplate(uint id)
		{
			return id < Templates.Length ? Templates[id] : null;
		}
		#endregion

		#region Quest initialization and loading
		[Initialization(InitializationPass.Fifth, "Initialize Quests")]
		public static void Initialize()
		{
#if !DEV
			LoadAll();
#endif
		}

		/// <summary>
		/// Loads the quest templates.
		/// </summary>
		/// <returns></returns>
		public static bool LoadAll()
		{
			if (!Loaded)
			{
				new DBCReader<QuestXpConverter>(RealmServerConfiguration.GetDBCFile("QuestXP.dbc"));

				Templates = new QuestTemplate[30000];

				ContentHandler.Load<QuestTemplate>();
				CreateGraph();
				Loaded = true;

				EnsureCharacterQuestsLoaded();
				AddSpellCastObjectives();

				log.Debug("{0} Quests loaded.", _questCount);
			}
			return true;
		}

		private static void AddSpellCastObjectives()
		{
			foreach (var spell in SpellHandler.QuestCompletors)
			{
				var questId = (uint)spell.GetEffect(SpellEffectType.QuestComplete).MiscValue;
				var quest = GetTemplate(questId);
				if (quest != null)
				{
					quest.AddSpellCastObjective(spell.SpellId);
				}
			}
		}

		private static void CreateGraph()
		{
			var groups = new Dictionary<int, List<uint>>();
			foreach (var quest in Templates)
			{
				if (quest != null)
				{
					if (quest.ExclusiveGroup != 0)
					{
						groups.GetOrCreate(quest.ExclusiveGroup).Add(quest.Id);
					}
					else if (quest.NextQuestId != 0)
					{
						var nextQuest = GetTemplate((uint)Math.Abs(quest.NextQuestId));
						if (nextQuest == null)
						{
							ContentHandler.OnInvalidDBData("NextQuestId {0} is invalid in: {1}", quest.NextQuestId, quest);
						}
						else
						{
							if (quest.NextQuestId > 0)
							{
								nextQuest.ReqAllFinishedQuests.Add(quest.Id);
							}
							else
							{
								nextQuest.ReqAllActiveQuests.Add(quest.Id);
							}
						}
					}
					if (quest.PreviousQuestId != 0)
					{
						//var prevQuest = GetTemplate((uint)Math.Abs(quest.PreviousQuestId));
						//if (prevQuest == null)
						//{
						//    ContentHandler.OnInvalidDBData("PreviousQuestId {0} is invalid in: {1}", quest.PreviousQuestId, quest);
						//}
						//else
						if (quest.PreviousQuestId > 0)
						{
							quest.ReqAllFinishedQuests.Add((uint)quest.PreviousQuestId);
						}
						else
						{
							quest.ReqAllActiveQuests.Add((uint)-quest.PreviousQuestId);
						}
					}
				}
			}

			foreach (var group in groups)
			{
				foreach (var qid in group.Value)
				{
					var quest = GetTemplate(qid);
					foreach (var qid2 in group.Value)
					{
						if (qid2 != qid)
						{
							if (group.Key > 0)
							{
								quest.ReqUndoneQuests.Add(qid2);
							}
						}
					}

					if (quest.NextQuestId != 0)
					{
						var nextQuest = GetTemplate((uint)Math.Abs(quest.NextQuestId));
						if (nextQuest == null)
						{
							ContentHandler.OnInvalidDBData("NextQuestId {0} is invalid in: {1}", quest.NextQuestId, quest);
						}
						else
						{
							if (group.Key > 0)
							{
								nextQuest.ReqAllFinishedQuests.Add(quest.Id);
							}
							else
							{
								nextQuest.ReqAnyFinishedQuests.Add(quest.Id);
							}
						}
					}
				}
			}
		}

		private static void EnsureCharacterQuestsLoaded()
		{
			var chrs = World.GetAllCharacters();
			for (var i = 0; i < chrs.Count; i++)
			{
				var chr = chrs[i];
				chr.AddMessage(() =>
								{
									if (chr.IsInWorld)
									{
										chr.InitQuests();
									}
								});
			}
		}

		public static bool UnloadAll()
		{
			if (Loaded)
			{
				Loaded = false;
				Templates = new QuestTemplate[30000];
				//GC.Collect();
				return true;
			}
			return false;
		}

		public static bool Reload()
		{
			return UnloadAll() && LoadAll();
		}

		[Initialization]
		[DependentInitialization(typeof(NPCMgr))]
		[DependentInitialization(typeof(QuestMgr))]
		public static void EnsureNPCRelationsLoaded()
		{
			ContentHandler.Load<NPCQuestGiverRelation>();
		}

		/// <summary>
		/// Loads the quest giver relations.
		/// </summary>
		/// <returns></returns>
		[Initialization]
		[DependentInitialization(typeof(GOMgr))]
		[DependentInitialization(typeof(QuestMgr))]
		public static void EnsureGOQuestRelationsLoaded()
		{
			ContentHandler.Load<GOQuestGiverRelation>();
		}

		/// <summary>
		/// Loads the quest giver relations.
		/// </summary>
		/// <returns></returns>
		[Initialization]
		[DependentInitialization(typeof(ItemMgr))]
		[DependentInitialization(typeof(QuestMgr))]
		public static void SetItemQuestRelations()
		{
			foreach (var item in ItemMgr.Templates)
			{
				if (item != null && item.QuestId != 0)
				{
					var quest = GetTemplate(item.QuestId);
					if (quest != null)
					{
						item.QuestHolderInfo = new QuestHolderInfo();
						item.QuestHolderInfo.QuestStarts.Add(quest);
					}
				}
			}
		}

		#endregion

		#region Other
		public static void AddQuest(QuestTemplate template)
		{
			if (Templates.Get(template.Id) == null)
			{
				_questCount++;
			}
			ArrayUtil.Set(ref Templates, template.Id, template);
		}

		public static void StartQuestDialog(this IQuestHolder qHolder, Character chr)
		{
			chr.OnInteract(qHolder as WorldObject);

			var list = qHolder.QuestHolderInfo.GetAvailableQuests(chr);

			if (list.Count > 0)
			{
				if (list.Count == 1 && !chr.QuestLog.HasActiveQuest(list[0].Id))
				{
					// start a single quest if there is only one and the user did not start it yet
					QuestHandler.SendDetails(qHolder, list[0], chr, true);
				}
				else
				{
					QuestHandler.SendQuestList(qHolder, list, chr);
				}
			}
		}
		#endregion
	}

	public static class QuestUtil
	{
		public static bool CanFinish(this QuestStatus status)
		{
			return status == QuestStatus.Completable || status == QuestStatus.CompletableNoMinimap ||
				   status == QuestStatus.RepeateableCompletable;
		}

		public static bool IsAvailable(this QuestStatus status)
		{
			return status != QuestStatus.NotAvailable && status != QuestStatus.TooHighLevel;
		}
	}
}
