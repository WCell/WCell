/*************************************************************************
 *
 *   file		: QuestHandler.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-04-08 11:02:58 +0200 (�t, 08 IV 2008) $
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
using System.Collections.Generic;
using WCell.Util.Logging;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Quests;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Network;
using WCell.RealmServer.Quests;

namespace WCell.RealmServer.Handlers
{
	/// <summary>
	/// Sequence in Quest packets upon completion:
	/// 
	/// CMSG_QUESTGIVER_COMPLETE_QUEST
	/// SMSG_QUESTGIVER_OFFER_REWARD
	/// CMSG_QUESTGIVER_REQUEST_REWARD
	/// SMSG_QUESTGIVER_QUEST_COMPLETE
	/// SMSG_QUESTGIVER_QUEST_DETAILS
	/// CMSG_QUESTGIVER_CHOOSE_REWARD
	/// CMSG_QUESTGIVER_STATUS_MULTIPLE_QUERY
	/// 
	/// or:
	/// CMSG_QUESTGIVER_COMPLETE_QUEST
	/// SMSG_QUESTGIVER_QUEST_COMPLETE
	/// CMSG_QUESTGIVER_CHOOSE_REWARD
	/// CMSG_QUESTGIVER_STATUS_MULTIPLE_QUERY
	/// 
	/// </summary>
	public static class QuestHandler
	{
		#region FINISHED
		/// <summary>
		/// Handles the quest confirm accept.
		/// </summary>
		/// <param name="client">The client.</param>
		/// <param name="packet">The packet.</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_QUEST_CONFIRM_ACCEPT)]
		public static void HandleQuestConfirmAccept(IRealmClient client, RealmPacketIn packet)
		{
			SendQuestConfirmAccept(client);
		}

		public static void SendQuestConfirmAccept(IRealmClient client)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_QUEST_CONFIRM_ACCEPT))
			{
				packet.Write(0);

				client.Send(packet);
			}
		}

        /// <summary>
		/// Handles the quest position of interest query.
		/// </summary>
		/// <param name="client">The client.</param>
		/// <param name="packet">The packet.</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_QUEST_POI_QUERY)]
        public static void HandleQuestPOIQuery(IRealmClient client, RealmPacketIn packet)
        {
            uint count = packet.ReadUInt32();
            var questIds = new List<uint>();
            for(var i = 0; i < count; i++)
                questIds.Add(packet.ReadUInt32());
            SendQuestPOIResponse(client, count, questIds);
        }

        public static void SendQuestPOIResponse(IRealmClient client, uint count, IEnumerable<uint> questIds)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_QUEST_POI_QUERY_RESPONSE))
            {
                packet.Write(count);
                foreach (var questId in questIds)
                {
                    List<QuestPOI> poiList;
                    QuestMgr.POIs.TryGetValue(questId, out poiList);
                    if (poiList != null)
                    {
                        packet.Write(questId);                  // quest ID
                        packet.Write((uint)poiList.Count);      // POI count

                        foreach (var poi in poiList)
                        {
                            packet.Write(poi.PoiId);            // POI index
                            packet.Write(poi.ObjectiveIndex);   // objective index
                            packet.Write((uint) poi.MapID);     // mapid
                            packet.Write((uint) poi.ZoneId);    // world map area id
                            packet.Write(poi.FloorId);          // floor id
                            packet.Write(poi.Unk3);             // unknown
                            packet.Write(poi.Unk4);             // unknown
                            packet.Write((uint)poi.Points.Count); // POI points count

                            foreach (var questPOIPoints in poi.Points)
                            {
                                packet.Write(questPOIPoints.X); // POI point x
                                packet.Write(questPOIPoints.Y); // POI point y
                            }
                        }
                    }
                    else
                    {
                        packet.Write(questId); // quest ID
                        packet.Write(0u); // POI count
                    }
                }

                client.Send(packet);
            }
        }

		/// <summary>
		/// Handles the quest giver cancel.
		/// </summary>
		/// <param name="client">The client.</param>
		/// <param name="packet">The packet.</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_QUESTGIVER_CANCEL)]
		public static void HandleQuestGiverCancel(IRealmClient client, RealmPacketIn packet)
		{
			GossipHandler.SendConversationComplete(client.ActiveCharacter);
		}
		#endregion

		/// <summary>
		/// Handles the quest query.
		/// </summary>
		/// <param name="client">The client.</param>
		/// <param name="packet">The packet.</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_QUEST_QUERY)]
		public static void HandleQuestQuery(IRealmClient client, RealmPacketIn packet)
		{
			uint questid = packet.ReadUInt32();
			QuestTemplate qt = QuestMgr.GetTemplate(questid);
			if (qt != null)
			{
				SendQuestQueryResponse(qt, client.ActiveCharacter);
			}
		}

		/// <summary>
		/// Handles the quest giver hello.
		/// </summary>
		/// <param name="client">The client.</param>
		/// <param name="packet">The packet.</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_QUESTGIVER_HELLO)]
		public static void HandleQuestGiverHello(IRealmClient client, RealmPacketIn packet)
		{
			var guid = packet.ReadEntityId();
			var chr = client.ActiveCharacter;
			var qHolder = chr.QuestLog.GetQuestGiver(guid);
			if (qHolder != null)
			{
				qHolder.StartQuestDialog(chr);
			}
		}

		/// <summary>
		/// Handles the quest giver request reward.
		/// </summary>
		/// <param name="client">The client.</param>
		/// <param name="packet">The packet.</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_QUESTGIVER_REQUEST_REWARD)]
		public static void HandleQuestGiverRequestReward(IRealmClient client, RealmPacketIn packet)
		{
			var guid = packet.ReadEntityId();
			var chr = client.ActiveCharacter;
			var qHolder = chr.QuestLog.GetQuestGiver(guid);
			var questid = packet.ReadUInt32();
			var qt = QuestMgr.GetTemplate(questid);
			if ((qHolder != null) && (qt != null))
			{
				//SendQuestGiverOfferReward(qHolder, qt, client.ActiveCharacter);
			}
		}

		/// <summary>
		/// Handles the quest giver status query.
		/// </summary>
		/// <param name="client">The client.</param>
		/// <param name="packet">The packet.</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_QUESTGIVER_STATUS_QUERY)]
		public static void HandleQuestGiverStatusQuery(IRealmClient client, RealmPacketIn packet)
		{
			var guid = packet.ReadEntityId();
			var chr = client.ActiveCharacter;
			var qHolder = chr.QuestLog.GetQuestGiver(guid);

			if (qHolder != null)
			{
				var status = qHolder.QuestHolderInfo.GetHighestQuestGiverStatus(chr);
				SendQuestGiverStatus(qHolder, status, chr);
			}
		}

		/// <summary>
		/// Handles the quest giver accept quest.
		/// </summary>
		/// <param name="client">The client.</param>
		/// <param name="packet">The packet.</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_QUESTGIVER_ACCEPT_QUEST)]
		public static void HandleQuestGiverAcceptQuest(IRealmClient client, RealmPacketIn packet)
		{
			var guid = packet.ReadEntityId();
			var chr = client.ActiveCharacter;
			var qGiver = chr.QuestLog.GetQuestGiver(guid);
			var questid = packet.ReadUInt32();
			var qt = QuestMgr.GetTemplate(questid);

			//if it's item then check if has associated
			//character - has the quest and it is shareable
			//gameobject - has the quest, has all the requirements

			if (qt != null && qGiver != null)
			{
				if (qGiver.QuestHolderInfo.QuestStarts.Contains(qt))
				{
					chr.QuestLog.TryAddQuest(qt, qGiver);
				}
			}
		}

		/*
		 * HandleQueryQuestsCompleted( WorldPacket & recv_data )
+{
+    uint32 count = 0;
+
+    WorldPacket data(SMSG_QUERY_QUESTS_COMPLETED_RESPONSE, 4+4*count);
+    data << uint32(count);
+
+    for(QuestStatusMap::const_iterator itr = _player->getQuestStatusMap().begin(); itr != _player->getQuestStatusMap().end(); ++itr)
+    {
+        if(itr->second.m_rewarded)
+        {
+            data << uint32(itr->first);
+            count++;
+        }
+    }
+    data.put<uint32>(0, count);
+    SendPacket(&data);
+}*/
		[ClientPacketHandler(RealmServerOpCode.SMSG_QUERY_QUESTS_COMPLETED)]
		public static void HandleQuestCompletedQuery(IRealmClient client, RealmPacketIn packet)
		{
			SendQuestCompletedQueryResponse(client.ActiveCharacter);
		}

		#region Packet sending
		public static void SendQuestCompletedQueryResponse(Character chr)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_QUERY_QUESTS_COMPLETED_RESPONSE, 4))
			{
				packet.Write(chr.QuestLog.FinishedQuests.Count);
				foreach (var entry in chr.QuestLog.FinishedQuests)
				{
					packet.Write(entry);
				}

				chr.Send(packet);
			}
		}

		/// <summary>
		/// Sends the quest invalid.
		/// </summary>
		/// <param name="chr">The character.</param>
		/// <param name="reason">The reason.</param>
		public static void SendQuestInvalid(Character chr, QuestInvalidReason reason)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_QUESTGIVER_QUEST_INVALID, 4))
			{
				packet.Write((int)reason);

				chr.Send(packet);
			}
		}

		/// <summary>
		/// Sends the quest update complete.
		/// </summary>
		/// <param name="chr">The character.</param>
		public static void SendQuestUpdateComplete(Character chr, uint questId)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_QUESTUPDATE_COMPLETE))
			{
				packet.Write(questId);

				chr.Send(packet);
			}
		}

		/// <summary>
		/// Sends the quest update failed.
		/// </summary>
		/// <param name="qst">The quest.</param>
		/// <param name="chr">The client.</param>
		public static void SendQuestUpdateFailed(Character chr, Quest qst)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_QUESTUPDATE_FAILED))
			{
				packet.Write(qst.Template.Id);

				chr.Send(packet);
			}
		}


		/// <summary>
		/// Sends the quest update failed timer.
		/// </summary>
		/// <param name="qst">The QST.</param>
		/// <param name="chr">The character</param>
		public static void SendQuestUpdateFailedTimer(Character chr, Quest qst)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_QUESTUPDATE_FAILEDTIMER))
			{
				packet.Write(qst.Template.Id);

				chr.Send(packet);
			}
		}

		/// <summary>
		/// Sends the quest update add kill, this should actually cover both GameObject interaction 
		/// together with killing the objectBase.
		/// </summary>
		/// <param name="quest">The QST.</param>
		/// <param name="chr">The client.</param>
		/// <param name="obj">The unit.</param>
		public static void SendUpdateInteractionCount(Quest quest,
			ObjectBase obj, QuestInteractionTemplate interaction, uint currentCount, Character chr)
		{
			using (var pckt = new RealmPacketOut(RealmServerOpCode.SMSG_QUESTUPDATE_ADD_KILL))
			{
				pckt.Write(quest.Template.Id);
				pckt.Write(interaction.RawId);								// ID of interaction
				pckt.Write(currentCount);									// current count
				pckt.Write(interaction.Amount);								// total count
				pckt.Write(obj != null ? obj.EntityId : EntityId.Zero);		// guid of object
				chr.Client.Send(pckt);
			}
		}

		/// <summary>
		/// Sends the quest update add item.
		/// </summary>
		/// <param name="chr">The client.</param>
		public static void SendUpdateItems(ItemId item, int diff, Character chr)
		{
			using (var pckt = new RealmPacketOut(RealmServerOpCode.SMSG_QUESTUPDATE_ADD_ITEM, 0))
			{
				//pckt.Write((uint)item);
				//pckt.Write(diff);

				chr.Client.Send(pckt);
			}
		}

		/// <summary>
		/// Sends the quest query response.
		/// </summary>
		/// <param name="qt">The quest id.</param>
		/// <param name="chr">The client.</param>
		public static void SendQuestQueryResponse(QuestTemplate qt, Character chr)
		{
			var locale = chr.Client.Info.Locale;
			using (var pckt = new RealmPacketOut(RealmServerOpCode.SMSG_QUEST_QUERY_RESPONSE))
			{
				pckt.Write(qt.Id);
				pckt.Write((uint)qt.IsActive);
				pckt.Write(qt.Level);
				pckt.Write(qt.MinLevel);			// since 3.3
				pckt.Write(qt.Category);			// questsort
				pckt.Write((uint)qt.QuestType);
				pckt.Write(qt.SuggestedPlayers);
				pckt.Write((uint)qt.ObjectiveMinReputation.ReputationIndex);
				pckt.Write(qt.ObjectiveMinReputation.Value);
				pckt.Write((uint)qt.ObjectiveMaxReputation.ReputationIndex);
				pckt.Write(qt.ObjectiveMaxReputation.Value);				//  (#10)

				pckt.Write(qt.FollowupQuestId);
				pckt.Write(qt.CalcRewardXp(chr));										// since 3.3

				if (qt.Flags.HasFlag(QuestFlags.HiddenRewards))
				{
					pckt.Write(0);
				}
				else
				{
					pckt.Write(qt.RewMoney);
				}
				pckt.Write(qt.MoneyAtMaxLevel);
				pckt.Write((uint)qt.CastSpell);
				pckt.Write((uint)qt.RewSpell);

				pckt.Write(qt.RewHonorAddition);
				pckt.WriteFloat(qt.RewHonorMultiplier);										// since 3.3

				pckt.Write((uint)qt.SrcItemId);
				pckt.Write((uint)qt.Flags);
				pckt.Write((uint)qt.RewardTitleId);
				pckt.Write(qt.PlayersSlain);
				pckt.Write(qt.RewardTalents);// NEW 3.0.2 RewardTalentCount (#21)
				pckt.Write(0);										// since 3.3: bonus arena points
				pckt.Write(0);										// since 3.3

				int i;
				if (qt.Flags.HasFlag(QuestFlags.HiddenRewards))
				{
					for (i = 0; i < QuestConstants.MaxRewardItems; ++i)
					{
						pckt.WriteUInt(0u);
						pckt.WriteUInt(0u);
					}
					for (i = 0; i < QuestConstants.MaxRewardChoiceItems; ++i)
					{
						pckt.WriteUInt(0u);
						pckt.WriteUInt(0u);
					}
				}
				else
				{
					for (i = 0; i < QuestConstants.MaxRewardItems; i++)
					{
						if (i < qt.RewardItems.Length)
						{
							pckt.Write((uint)qt.RewardItems[i].ItemId);
							pckt.Write(qt.RewardItems[i].Amount);
						}
						else
						{
							pckt.WriteUInt(0u);
							pckt.WriteUInt(0u);
						}
					}

					for (i = 0; i < QuestConstants.MaxRewardChoiceItems; i++)
					{
						if (i < qt.RewardChoiceItems.Length)
						{
							pckt.Write((uint)qt.RewardChoiceItems[i].ItemId);
							pckt.Write(qt.RewardChoiceItems[i].Amount);
						}
						else
						{
							pckt.WriteUInt(0u);
							pckt.WriteUInt(0u);
						}
					}
				}
				// #### since 3.3
				for (i = 0; i < QuestConstants.MaxReputations; i++)
					pckt.Write((uint)qt.RewardReputations[i].Faction);
				for (i = 0; i < QuestConstants.MaxReputations; i++)
					pckt.Write(qt.RewardReputations[i].ValueId);
				for (i = 0; i < QuestConstants.MaxReputations; i++)
					pckt.Write(qt.RewardReputations[i].Value);

				//     ######

				pckt.Write((uint)qt.MapId);
				pckt.Write(qt.PointX);
				pckt.Write(qt.PointY);
				pckt.Write(qt.PointOpt);

				pckt.WriteCString(qt.Titles.Localize(locale));
				pckt.WriteCString(qt.Instructions.Localize(locale));
				pckt.WriteCString(qt.Details.Localize(locale));
				pckt.WriteCString(qt.EndTexts.Localize(locale));
				pckt.WriteCString(qt.CompletedTexts.Localize(locale));												// since 3.3

				for (i = 0; i < QuestConstants.MaxObjectInteractions; i++)
				{
					pckt.Write(qt.ObjectOrSpellInteractions[i].RawId);		// Mob or GO entry ID [i]
					pckt.Write(qt.ObjectOrSpellInteractions[i].Amount);	// amount [i],
					pckt.Write(0);
					pckt.Write(0);									// since 3.3
				}

				for (i = 0; i < QuestConstants.MaxCollectableItems; i++)
				{
					if (i < qt.CollectableItems.Length)
					{
						pckt.Write((uint)qt.CollectableItems[i].ItemId);
						pckt.Write(qt.CollectableItems[i].Amount);
					}
					else
					{
						pckt.WriteUInt(0u);
						pckt.WriteUInt(0u);
					}
				}

				for (i = 0; i < QuestConstants.MaxObjectiveTexts; i++)
				{
					var set = qt.ObjectiveTexts[(int)locale];
					if (set != null)
					{
						pckt.Write(set.Texts[i]);
					}
					else
					{
						pckt.Write("");
					}
				}
				chr.Client.Send(pckt);
			}
		}

		public static void SendQuestLogFull(Character chr)
		{
			using (var pckt = new RealmPacketOut(RealmServerOpCode.SMSG_QUESTLOG_FULL))
			{
				chr.Send(pckt);
			}
		}

		/// <summary>
		/// Sends the quest giver quest detail.
		/// </summary>
		/// <param name="questGiver">The qg.</param>
		/// <param name="qt">The quest id.</param>
		/// <param name="chr">The client.</param>
		/// <param name="acceptable">if set to <c>true</c> [acceptable].</param>
		public static void SendDetails(IEntity questGiver, QuestTemplate qt, Character chr, bool acceptable)
		{
			var locale = chr.Locale;
			using (var pckt = new RealmPacketOut(RealmServerOpCode.SMSG_QUESTGIVER_QUEST_DETAILS))
			{
				pckt.Write(questGiver != null ? questGiver.EntityId : EntityId.Zero);

				pckt.Write(EntityId.Zero);						// unknown, wotlk, quest sharing?

				pckt.Write(qt.Id);

				pckt.WriteCString(qt.Titles.Localize(locale));
				pckt.WriteCString(qt.Details.Localize(locale));
				pckt.WriteCString(qt.Instructions.Localize(locale));


				pckt.Write((byte)(acceptable ? 1 : 0));			// doesn't work
				pckt.WriteUInt((uint)qt.Flags);
				pckt.WriteUInt(qt.SuggestedPlayers);
				pckt.Write((byte)0); // probably some pvp flag
				if (qt.Flags.HasFlag(QuestFlags.HiddenRewards))
				{
					pckt.WriteUInt(0u);		// choice items length
					pckt.WriteUInt(0u);		// reward items length
					pckt.WriteUInt(0u);		// money
					pckt.WriteUInt(0u);		// xp
				}
				else
				{
					pckt.Write(qt.RewardChoiceItems.Length);
					for (uint i = 0; i < qt.RewardChoiceItems.Length; i++)
					{
						pckt.Write((uint)qt.RewardChoiceItems[i].ItemId);
						pckt.Write(qt.RewardChoiceItems[i].Amount);
						var template = qt.RewardChoiceItems[i].Template;
						if (template != null)
						{
							pckt.Write(template.DisplayId);
						}
						else
						{
							pckt.Write(0);
						}
					}

					pckt.Write(qt.RewardItems.Length);
					for (uint i = 0; i < qt.RewardItems.Length; i++)
					{
						pckt.Write((uint)qt.RewardItems[i].ItemId);
						pckt.Write(qt.RewardItems[i].Amount);

						var template = qt.RewardItems[i].Template;
						if (template != null)
						{
							pckt.Write(template.DisplayId);
						}
						else
						{
							pckt.Write(0);
						}
					}

					if (chr.Level >= RealmServerConfiguration.MaxCharacterLevel)
					{
						pckt.Write(qt.MoneyAtMaxLevel);
					}
					else
					{
						pckt.Write(qt.RewMoney);
					}

					pckt.Write(qt.CalcRewardXp(chr));						// since 3.3
				}

				pckt.Write(qt.RewHonorAddition);
				pckt.Write(qt.RewHonorMultiplier);						// since 3.3
				pckt.Write((uint)qt.RewSpell);
				pckt.Write((uint)qt.CastSpell);
				pckt.Write((uint)qt.RewardTitleId);		// since 2.4.0
				pckt.Write(qt.RewardTalents);

				// #### since 3.3
				pckt.Write(0);						// bonus arena points
				pckt.Write(0);
				for (uint i = 0; i < QuestConstants.MaxReputations; ++i)
				{
					pckt.Write((uint) qt.RewardReputations[i].Faction);
				}
				for (uint i = 0; i < QuestConstants.MaxReputations; ++i)
				{
					pckt.Write(qt.RewardReputations[i].ValueId);
				}
				for (uint i = 0; i < QuestConstants.MaxReputations; ++i)
				{
					pckt.Write(qt.RewardReputations[i].Value);
				}

				pckt.Write(QuestConstants.MaxEmotes);
				for (var i = 0; i < QuestConstants.MaxEmotes; i++)
				{
					var emote = qt.QuestDetailedEmotes[i];
					pckt.Write((int)emote.Type);
					pckt.Write(emote.Delay);
				}
				chr.Client.Send(pckt);
			}
		}

		/// <summary>
		/// Offers the reward of the given Quest to the given Character.
		/// When replying the Quest will be complete.
		/// </summary>
		/// <param name="questGiver">The qg.</param>
		/// <param name="qt">The quest id.</param>
		/// <param name="chr">The client.</param>
		public static void SendQuestGiverOfferReward(IEntity questGiver, QuestTemplate qt, Character chr)
		{
			//if (questGiver.QuestHolderInfo != null)

			//var list = qg.QuestHolderInfo.QuestEnds;
			//if (list != null && list.Contains(qt))

			var locale = chr.Locale;
			using (var pckt = new RealmPacketOut(RealmServerOpCode.SMSG_QUESTGIVER_OFFER_REWARD))
			{
				pckt.Write(questGiver.EntityId);
				pckt.WriteUInt(qt.Id);
				pckt.WriteCString(qt.Titles.Localize(locale));
				pckt.WriteCString(qt.OfferRewardTexts.Localize(locale));
				pckt.WriteByte((byte)(qt.FollowupQuestId > 0 ? 1 : 0));

				pckt.WriteUInt((uint)qt.Flags);
				pckt.WriteUInt(qt.SuggestedPlayers); // Suggested Group Num

				pckt.Write(qt.OfferRewardEmotes.Length);
				for (uint i = 0; i < qt.OfferRewardEmotes.Length; i++)
				{
					pckt.Write(qt.OfferRewardEmotes[i].Delay);
					pckt.Write((uint)qt.OfferRewardEmotes[i].Type);
				}

				pckt.Write(qt.RewardChoiceItems.Length);
				for (var i = 0; i < qt.RewardChoiceItems.Length; i++)
				{
					pckt.Write((uint)qt.RewardChoiceItems[i].ItemId);
					pckt.Write(qt.RewardChoiceItems[i].Amount);
					var template = qt.RewardChoiceItems[i].Template;
					if (template != null)
					{
						pckt.Write(template.DisplayId);
					}
					else
					{
						pckt.Write(0);
					}
				}

				pckt.Write(qt.RewardItems.Length);
				for (var i = 0; i < qt.RewardItems.Length; i++)
				{
					pckt.Write((uint)qt.RewardItems[i].ItemId);
					pckt.Write(qt.RewardItems[i].Amount);
					var template = qt.RewardItems[i].Template;
					if (template != null)
					{
						pckt.WriteUInt(template.DisplayId);
					}
					else
					{
						pckt.Write(0);
					}
				}

				if (chr.Level >= RealmServerConfiguration.MaxCharacterLevel)
				{
					pckt.Write(qt.MoneyAtMaxLevel);
				}
				else
				{
					pckt.Write(qt.RewMoney);
				}

				pckt.Write(qt.CalcRewardXp(chr));
				pckt.Write(qt.CalcRewardHonor(chr)); // honor points
				pckt.Write(qt.RewHonorMultiplier); // since 3.3

				pckt.Write((uint)0x08); // unused by client
				pckt.Write((uint)qt.RewSpell);
				pckt.Write((uint)qt.CastSpell);
				pckt.Write((uint)qt.RewardTitleId);
				pckt.Write(qt.RewardTalents); // reward talents
				pckt.Write(0); // since 3.3
				pckt.Write(0); // since 3.3

				// #### since 3.3
				for (uint i = 0; i < QuestConstants.MaxReputations; i++)
					pckt.Write((uint)qt.RewardReputations[i].Faction);
				for (uint i = 0; i < QuestConstants.MaxReputations; i++)
					pckt.Write(qt.RewardReputations[i].ValueId);
				for (uint i = 0; i < QuestConstants.MaxReputations; i++)
					pckt.Write(qt.RewardReputations[i].Value);

				//     ######

				chr.Client.Send(pckt);
			}
		}

		/// <summary>
		/// Sends SMSG_QUESTGIVER_REQUEST_ITEMS
		/// </summary>
		/// <param name="qg">The qg.</param>
		/// <param name="qt">The qt.</param>
		/// <param name="chr">The client.</param>
		public static void SendRequestItems(IQuestHolder qg, QuestTemplate qt, Character chr, bool closeOnCancel)
		{
			if (qg.QuestHolderInfo != null)
			{
				//var list = qg.QuestHolderInfo.QuestEnds;
				//if (list != null && list.Contains(qt))
				{
					var locale = chr.Locale;
					using (var pckt = new RealmPacketOut(RealmServerOpCode.SMSG_QUESTGIVER_REQUEST_ITEMS))
					{
						pckt.Write(qg.EntityId);
						pckt.Write(qt.Id);
						pckt.Write(qt.Titles.Localize(locale));
						pckt.Write(qt.ProgressTexts.Localize(locale));
						pckt.Write((uint)0); // emote delay
						pckt.Write((uint)qt.RequestEmoteType); // Emote type
						if (closeOnCancel)
						{
							pckt.Write((uint)1);
						}
						else
						{
							pckt.Write((uint)0);
						}
						pckt.Write((uint)qt.Flags);
						pckt.Write((uint)qt.SuggestedPlayers);
						if (qt.RewMoney < 0)
						{
							pckt.Write((uint)-qt.RewMoney);
						}
						else
						{
							pckt.Write((uint)0);
						}
						pckt.Write(qt.CollectableItems.Length);
						for (uint i = 0; i < qt.CollectableItems.Length; i++)
						{
							pckt.Write((uint)qt.CollectableItems[i].ItemId);
							pckt.Write(qt.CollectableItems[i].Amount);
							var template = qt.CollectableItems[i].Template;
							if (template != null)
							{
								pckt.Write(template.DisplayId);
							}
							else
							{
								pckt.Write((uint)0);
							}
						}
						pckt.Write((uint)2); // unknown, probably IsActive
						pckt.Write(4);			// if can advance: 4
						pckt.Write((uint)8); //always eight
						pckt.Write((uint)10); // always ten

						chr.Client.Send(pckt);
					}
				}
			}
		}

		/// <summary>
		/// Sends packet, which informs client about IQuestHolder's status.
		/// </summary>
		/// <param name="qg">The qg.</param>
		/// <param name="status">The status.</param>
		/// <param name="chr">The client.</param>
		public static void SendQuestGiverStatus(IQuestHolder qg, QuestStatus status, Character chr)
		{
			qg.OnQuestGiverStatusQuery(chr);
			using (var pckt = new RealmPacketOut(RealmServerOpCode.SMSG_QUESTGIVER_STATUS))
			{
				pckt.Write(qg.EntityId);
				pckt.Write((byte)status);

				chr.Client.Send(pckt);
			}
		}

		/// <summary>
		/// Sends the quest giver quest complete.
		/// </summary>
		/// <param name="qt">The quest id.</param>
		/// <param name="chr">The client.</param>
		public static void SendComplete(QuestTemplate qt, Character chr)
		{
			using (var pckt = new RealmPacketOut(RealmServerOpCode.SMSG_QUESTGIVER_QUEST_COMPLETE))
			{
				pckt.Write(qt.Id);


				if (chr.Level >= RealmServerConfiguration.MaxCharacterLevel)
				{
					pckt.Write(0u);
					pckt.Write(qt.MoneyAtMaxLevel);
				}
				else
				{
					pckt.Write(qt.CalcRewardXp(chr));
					pckt.Write(qt.RewMoney);
				}
				pckt.Write(qt.CalcRewardHonor(chr));
				pckt.Write(qt.RewardTalents);
				pckt.Write(0);							// since 3.3: Arena reward

				chr.Client.Send(pckt);
				/*if (chr.Level >= LevelMgr.MaxCharacterLevel)
				{
					pckt.Write(qt.MoneyAtMaxLevel);
				}
				else
				{
					pckt.Write(qt.RewMoney);
				}
				pckt.Write((uint)0); // unknown value

				pckt.Write(qt.RewardItems.Length);

				foreach (var reward in qt.RewardItems)
				{
					pckt.Write(reward.ItemId);
					pckt.Write(reward.Amount);
				}*/
			}
		}

		public static void SendQuestPushResult(Character receiver, QuestPushResponse qpr, Character giver)
		{
			using (var pckt = new RealmPacketOut(RealmServerOpCode.MSG_QUEST_PUSH_RESULT))
			{
				pckt.Write(receiver.EntityId);
				pckt.Write((byte)qpr);
				giver.Send(pckt);
			}
		}

		/// <summary>
		/// Sends the quest giver quest list.
		/// </summary>
		/// <param name="qHolder">The quest giver.</param>
		/// <param name="list">The list.</param>
		/// <param name="chr">The character.</param>
		public static void SendQuestList(IQuestHolder qHolder, List<QuestTemplate> list, Character chr)
		{
			using (var pkt = new RealmPacketOut(new PacketId(RealmServerOpCode.SMSG_QUESTGIVER_QUEST_LIST)))
			{
				pkt.Write(qHolder.EntityId);
				if (qHolder.QuestHolderInfo != null)
				{
					pkt.Write("Stay a while and listen..."); // TODO need to change to dynamic text, character-dependant
					pkt.Write((uint)0); // player emote
					pkt.Write((uint)1); // npc emote

					var amount = Math.Min(QuestConstants.MaxQuestsPerQuestGiver, list.Count);
					pkt.Write((byte)amount);

					foreach (var qt in list)
					{
						pkt.Write(qt.Id);
						var quest = chr.QuestLog.GetActiveQuest(qt.Id);
						if (quest != null)
						{
							if (quest.CompleteStatus == QuestCompleteStatus.Completed)
							{
								//status = (uint)qt.GetEndStatus(qHolder.QuestHolderInfo, chr);
								pkt.Write(4);
							}
							else
							{
								pkt.Write((uint)QuestStatus.NotCompleted);
							}
						}
						else
						{
							var status = (uint)qt.GetAvailability(chr);
							pkt.Write(status);
						}
						pkt.WriteUInt(qt.Level);
						pkt.WriteUInt((uint)qt.Flags);
						pkt.Write((byte)0); // 3.3.3 question/exclamation mark
						pkt.WriteCString(qt.DefaultTitle);
					}
					chr.Client.Send(pkt);
				}
			}
		}

		#endregion

		/// <summary>
		/// Handles CMSG_QUESTGIVER_CHOOSE_REWARD.
		/// </summary>
		/// <param name="client">The client.</param>
		/// <param name="packet">The packet.</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_QUESTGIVER_CHOOSE_REWARD)]
		public static void HandleQuestGiverChooseReward(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var guid = packet.ReadEntityId();
			var questid = packet.ReadUInt32();

			var quest = chr.QuestLog.GetQuestById(questid);
			if (quest != null)
			{
				var rewardSlot = packet.ReadUInt32();

				var qHolder = chr.QuestLog.GetQuestGiver(guid);
				if (qHolder != null && quest.CompleteStatus == QuestCompleteStatus.Completed)
				{
					quest.TryFinish(qHolder, rewardSlot);
				}
			}
		}

		/// <summary>
		/// Handles the quest giver complete quest.
		/// </summary>
		/// <param name="client">The client.</param>
		/// <param name="packet">The packet.</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_QUESTGIVER_COMPLETE_QUEST)]
		public static void HandleQuestGiverCompleteQuest(IRealmClient client, RealmPacketIn packet)
		{
			var guid = packet.ReadEntityId();
			var chr = client.ActiveCharacter;
			var qHolder = chr.QuestLog.GetQuestGiver(guid);
			if (qHolder != null)
			{
				var questid = packet.ReadUInt32();
				var quest = chr.QuestLog.GetQuestById(questid);
				if (quest != null && qHolder.QuestHolderInfo.QuestEnds.Contains(quest.Template))
				{
					if (quest.CompleteStatus != QuestCompleteStatus.Completed)
					{
						SendRequestItems(qHolder, quest.Template, chr, false);
					}
					else
					{
						quest.OfferQuestReward(qHolder);
					}
				}
			}
		}

		/// <summary>
		/// Handles the quest giver query quest.
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_QUESTGIVER_QUERY_QUEST)]
		public static void HandleQuestGiverQueryQuest(IRealmClient client, RealmPacketIn packet)
		{
			var guid = packet.ReadEntityId();
			var chr = client.ActiveCharacter;
			var questid = packet.ReadUInt32();

			var qHolder = chr.QuestLog.GetQuestGiver(guid);
			var qt = QuestMgr.GetTemplate(questid);
			if (qHolder != null && qt != null)
			{
				//var qs = qt.GetStartStatus(qHolder.QuestHolderInfo, client.ActiveCharacter);
				if (!chr.QuestLog.HasActiveQuest(questid))
				{
					var autoAccept = qt.Flags.HasFlag(QuestFlags.AutoAccept);
                    SendDetails(qHolder, qt, chr, !autoAccept);
                    if (autoAccept)
                    {
                        chr.QuestLog.TryAddQuest(qt, qHolder);
                    }
				}
				else
				{
					SendRequestItems(qHolder, qt, chr, false);
				}
			}
		}

		/// <summary>
		/// Handles the quest log remove quest.
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_QUESTLOG_REMOVE_QUEST)]
		public static void HandleQuestLogRemoveQuest(IRealmClient client, RealmPacketIn packet)
		{
			var slot = packet.ReadByte();

			var quest = client.ActiveCharacter.QuestLog.GetQuestBySlot(slot);
			if (quest != null)
			{
				quest.Cancel(false);
			}
		}

		/// <summary>
		/// Handles the questgiver status multiple query.
		/// </summary>
		/// <param name="client">The client.</param>
		/// <param name="packet">The packet.</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_QUESTGIVER_STATUS_MULTIPLE_QUERY)]
		public static void HandleQuestgiverStatusMultipleQuery(IRealmClient client, RealmPacketIn packet)
		{
			client.ActiveCharacter.FindAndSendAllNearbyQuestGiverStatuses();
		}


		#region UNFINISHED

		[ClientPacketHandler(RealmServerOpCode.CMSG_PUSHQUESTTOPARTY)]
		public static void HandlePushQuestToParty(IRealmClient client, RealmPacketIn packet)
		{
			var questid = packet.ReadUInt32();

			var qt = QuestMgr.GetTemplate(questid);

			if (qt != null && qt.Sharable)
			{

				var qst = client.ActiveCharacter.QuestLog.GetActiveQuest(qt.Id);
				if (qst != null)
				{
					var group = client.ActiveCharacter.Group;
					if (group != null)
					{
						group.SyncRoot.EnterReadLock();
						try
						{
							foreach (var member in group)
							{
								var chr = member.Character;

								if (chr == null) continue;

								if (chr.QuestLog.ActiveQuestCount >= QuestLog.MaxQuestCount)
								{
									SendQuestPushResult(chr, QuestPushResponse.QuestlogFull, client.ActiveCharacter);
									continue;
								}
								if (chr.QuestLog.GetActiveQuest(qt.Id) != null)
								{
									SendQuestPushResult(chr, QuestPushResponse.AlreadyHave, client.ActiveCharacter);
									continue;
								}
								if (chr.QuestLog.FinishedQuests.Contains(qt.Id) && !qt.Repeatable)
								{
									SendQuestPushResult(chr, QuestPushResponse.AlreadyFinished,
														client.ActiveCharacter);
									continue;
								}
								if ((qt.CheckBasicRequirements(chr) != QuestInvalidReason.Ok) || !chr.IsAlive)
								{
									SendQuestPushResult(chr, QuestPushResponse.CannotTake, client.ActiveCharacter);
									continue;
								}
								// not sure about range
								if (!chr.IsInRadius(client.ActiveCharacter, 30f))
								{
									SendQuestPushResult(chr, QuestPushResponse.TooFar, client.ActiveCharacter);
									continue;
								}
								/*
									if (chr.CanInteractWith(client.ActiveCharacter){
									// bank, trading, combat, talking to a quest npc allready, accepting a quest from another person sharing a quest
										QuestMgr.SendQuestPushResult(chr, QuestPushResponse.Busy, client.ActiveCharacter);
										continue;
									}
									*/
								SendQuestPushResult(chr, QuestPushResponse.Sharing, client.ActiveCharacter);
								SendDetails(client.ActiveCharacter, qt, chr, true);
							}
						}
						finally
						{
							group.SyncRoot.ExitReadLock();
						}
					}

				}
			}
		}

		[ClientPacketHandler(RealmServerOpCode.MSG_QUEST_PUSH_RESULT)]
		public static void HandleQuestPushResult(IRealmClient client, RealmPacketIn packet)
		{
			var guid = packet.ReadEntityId(); // who to send packet to
			var result = packet.ReadByte(); // status
			var sharer = client.ActiveCharacter.Map.GetObject(guid) as Character;

			if (sharer != null && client.ActiveCharacter.Group != null)
			{
				if (client.ActiveCharacter.Group == sharer.Group)
				{
					SendQuestPushResult(client.ActiveCharacter, (QuestPushResponse)result, sharer);
				}
			}
		}

		#endregion

		#region Unknown Structure of Packets, which are probably not used
		/// <summary>
		/// Handles the quest log swap quest.
		/// </summary>
		/// <param name="client">The client.</param>
		/// <param name="packet">The packet.</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_QUESTLOG_SWAP_QUEST)]
		public static void HandleQuestLogSwapQuest(IRealmClient client, RealmPacketIn packet)
		{
		}

		/// <summary>
		/// Handles the quest giver query autolaunch.
		/// </summary>
		/// <param name="client">The client.</param>
		/// <param name="packet">The packet.</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_QUESTGIVER_QUEST_AUTOLAUNCH)]
		public static void HandleQuestGiverQueryAutoLaunch(IRealmClient client, RealmPacketIn packet)
		{
			//is it used? 
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_FLAG_QUEST)]
		public static void HandleFlagQuest(IRealmClient client, RealmPacketIn packet)
		{
			//is it used?
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_FLAG_QUEST_FINISH)]
		public static void HandleFlagQuestFinish(IRealmClient client, RealmPacketIn packet)
		{
			//cheat opcode to flag quest finished in questlog
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_CLEAR_QUEST)]
		public static void HandleClearQuest(IRealmClient client, RealmPacketIn packet)
		{
			//is it used?
		}

		public static void SendQuestForceRemoved(IRealmClient client, QuestTemplate quest)
		{
			using (var pkt = new RealmPacketOut(RealmServerOpCode.SMSG_QUEST_FORCE_REMOVE, 4))
			{
				pkt.Write((uint)quest.Id);
				client.Send(pkt);
			}
		}
		#endregion

		/// <summary>
		/// Finds and sends all surrounding QuestGiver's current Quest-Status to the given Character
		/// </summary>
		/// <param name="chr">The <see cref="Character"/>.</param>
		public static void FindAndSendAllNearbyQuestGiverStatuses(this Character chr)
		{
			using (var pkt = new RealmPacketOut(new PacketId(RealmServerOpCode.SMSG_QUESTGIVER_STATUS_MULTIPLE)))
			{
				var objs = chr.KnownObjects;
				var count = 0;
				pkt.Position += sizeof(int);		// leave space for count
				if (objs != null)
				{
					foreach (var obj in objs)
					{
						if (obj is IQuestHolder && !(obj is Character))
						{
							var questgiver = (IQuestHolder)obj;
							questgiver.OnQuestGiverStatusQuery(chr);

							if (questgiver.QuestHolderInfo != null)
							{
								pkt.Write(questgiver.EntityId);
								pkt.Write((byte)questgiver.QuestHolderInfo.GetHighestQuestGiverStatus(chr));
								count++;
							}
						}
					}
				}
				pkt.Position = pkt.HeaderSize;
				pkt.Write(count);
				chr.Client.Send(pkt);
			}
		}
	}
}