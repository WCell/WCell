using NLog;
using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Misc;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Network;
using WCell.Util;
using WCell.Util.Threading;

namespace WCell.RealmServer.Handlers
{
	public static class QueryHandler
	{
		private static Logger s_log = LogManager.GetCurrentClassLogger();

		#region CMSG_QUERY_TIME
		/// <summary>
		/// Handles an incoming time query.
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param> 
		[ClientPacketHandler(RealmServerOpCode.CMSG_QUERY_TIME)]
		public static void QueryTimeRequest(IRealmClient client, RealmPacketIn packet)
		{
			SendQueryTimeReply(client);
		}

		/// <summary>
		/// Send a "time query" reply to the client.
		/// </summary>
		/// <param name="client">the client to send to</param>
		public static void SendQueryTimeReply(IRealmClient client)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_QUERY_TIME_RESPONSE, 4))
			{
				packet.Write(Utility.GetEpochTime());
				//packet.Write(Utility.GetSystemTime());

				client.Send(packet);
			}
		}
		#endregion

		#region CMSG_NAME_QUERY
		/// <summary>
		/// Handles an incoming name query.
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param> 
		[ClientPacketHandler(RealmServerOpCode.CMSG_NAME_QUERY)]
		public static void NameQueryRequest(IRealmClient client, RealmPacketIn packet)
		{
			var id = packet.ReadEntityId();

			ILivingEntity entity = client.ActiveCharacter;

			if (entity.EntityId.Low != id.Low)
			{
				entity = World.GetNamedEntity(id.Low) as ILivingEntity;
			}

			if (entity != null)
			{
				SendNameQueryReply(client, entity);
			}
			else
			{
				RealmServer.IOQueue.AddMessage(new Message(() =>
				{
					var record = CharacterRecord.LoadRecordByEntityId(id.Low);
					if (record == null)
					{
						s_log.Warn("{0} queried name of non-existing Character: " + id, client);
					}
					else
					{
						SendNameQueryReply(client, record);
					}
				}));
			}
		}

		/// <summary>
		/// Sends a "name query" reply to the client.
		/// </summary>
		/// <param name="client">the client to send to</param>
		/// <param name="entity">the character information to be sent</param>
		public static void SendNameQueryReply(IPacketReceiver client, ILivingEntity entity)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_NAME_QUERY_RESPONSE))
			{
				entity.EntityId.WritePacked(packet);
				packet.Write((byte)0);         // new in 3.1.0 - this is a type, ranging from 0-3
				packet.WriteCString(entity.Name);
				packet.Write((byte)0); // cross realm bg name? (256 bytes max)
				packet.Write((byte)entity.Race);
				packet.Write((byte)entity.Gender);
				packet.Write((byte)entity.Class);

				packet.Write((byte)0); // hasDeclinedNames

				//if (hasDeclinedNames)
				//{
				//    for (int i=0;i<4;i++)
				//    {
				//        packet.WriteCString("");
				//    }
				//}


				client.Send(packet);
			}
		}
		#endregion

		#region CMSG_CREATURE_QUERY
		/// <summary>
		/// Handles an incoming creature name query.
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param> 
		[ClientPacketHandler(RealmServerOpCode.CMSG_CREATURE_QUERY)]
		public static void HandleCreatureQueryRequest(IRealmClient client, RealmPacketIn packet)
		{
			uint creatureEntryId = packet.ReadUInt32();
			//var creatureEntityId = packet.ReadEntityId(); //actually we dont really care about the guid since its all static data

			if (creatureEntryId != 0)
			{
				var entry = NPCMgr.GetEntry(creatureEntryId);

				if (entry != null)
				{
					SendCreatureQueryResponse(client, entry);
				}
			}
		}

		private static void SendCreatureQueryResponse(IRealmClient client, NPCEntry entry)
		{
			var name = entry.Names.Localize(client);
			var title = entry.Titles.Localize(client);
			using (
				var pkt = new RealmPacketOut(RealmServerOpCode.SMSG_CREATURE_QUERY_RESPONSE,
											 48 + name.Length + title.Length))
			{
				pkt.WriteUInt(entry.Id);
				pkt.WriteCString(name);
				pkt.Write((byte)0); // Name2
				pkt.Write((byte)0); // Name3
				pkt.Write((byte)0); // Name4
				pkt.WriteCString(title);
				pkt.WriteCString(entry.InfoString);
				pkt.Write((uint)entry.EntryFlags);
				pkt.Write((uint)entry.Type);
				pkt.Write((uint)entry.FamilyId);
				pkt.Write((uint)entry.Rank);
				pkt.Write(0); // UInt1
				pkt.Write(entry.SpellGroupId);
				var i = 0;
				for (; i < entry.DisplayIds.Length; i++)
				{
					pkt.Write(entry.DisplayIds[i]);
				}
				for (; i < 4; i++)
				{
					pkt.Write(0);
				}

				pkt.Write(0);						// hp mod?
				pkt.Write(0);						// mana mod?
				pkt.Write(entry.IsLeader);

				for (i = 0; i < 4; i++)
				{
					pkt.Write(entry.QuestIds[i]);
				}
				pkt.Write(0); // id from CreatureMovement.dbc

				client.Send(pkt);
			}
		}
		#endregion

		#region CMSG_NPC_TEXT_QUERY
		/// <summary>
		/// Handles client's npc text query
		/// </summary>
		/// <param name="client">realm client</param>
		/// <param name="packet">packet incoming</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_NPC_TEXT_QUERY)]
		public static void HandleNPCTextQuery(IRealmClient client, RealmPacketIn packet)
		{
			var textId = packet.ReadUInt32();
			var entityId = packet.ReadEntityId();

			//var obj = client.ActiveCharacter.Map.GetObject(entityId) as IGossipEntry;

			//if (obj != null)
			//{
			//    SendNPCTextUpdate(client.ActiveCharacter, obj);
			//}
			//else

			var text = GossipMgr.GetEntry(textId);
			if (text != null)
			{
				SendNPCTextUpdate(client.ActiveCharacter, text);
			}
		}

		/// <summary>
		/// Sends a npc text update to the character
		/// </summary>
		/// <param name="character">recieving character</param>
		/// <param name="text">class holding all info about text</param>
		public static void SendNPCTextUpdate(Character character, IGossipEntry text)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_NPC_TEXT_UPDATE))
			{
				packet.Write(text.GossipId);

				var i = 0;
				for (; i < text.GossipTexts.Length; i++)
				{
					var entry = text.GossipTexts[i];
					packet.WriteFloat(entry.Probability);

					var maleText = entry.GetTextMale(character.GossipConversation);
					string femaleText;
					if (text.IsDynamic)
					{
						// generated dynamically anyway
						femaleText = maleText;
					}
					else
					{
						femaleText = entry.GetTextFemale(character.GossipConversation);
					}
					packet.WriteCString(maleText);
					packet.WriteCString(femaleText);


					packet.Write((uint)entry.Language);

					for (int emoteIndex = 0; emoteIndex < 3; emoteIndex++)
					{
						// TODO: Emotes
						//packet.Write((uint)entry.Emotes[emoteIndex]);
						//packet.Write(entry.EmoteDelays[emoteIndex]);
						packet.Write(0L);
					}
				}

				for (; i < 8; i++)
				{
					packet.WriteFloat(0);
					packet.WriteByte(0);
					packet.WriteByte(0);
					packet.Fill(0, 4 * 7);
				}

				character.Client.Send(packet);
			}
		}

		/// <summary>
		/// Sends a simple npc text update to the character
		/// </summary>
		/// <param name="character">recieving character</param>
		/// <param name="id">id of text to update</param>
		/// <param name="title">gossip window's title</param>
		/// <param name="text">gossip window's text</param>
		public static void SendNPCTextUpdateSimple(Character character, uint id, string title, string text)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_NPC_TEXT_UPDATE))
			{
				packet.Write(id);

				packet.WriteFloat(1);
				packet.WriteCString(title);
				packet.WriteCString(text);
				packet.Fill(0, 4 * 7);

				for (var i = 1; i < 8; i++)
				{
					packet.WriteFloat(0);
					packet.WriteByte(0);
					packet.WriteByte(0);
					packet.Fill(0, 4 * 7);
				}

				character.Client.Send(packet);
			}
		}
		#endregion

		#region CMSG_PAGE_TEXT_QUERY
		[ClientPacketHandler(RealmServerOpCode.CMSG_PAGE_TEXT_QUERY)]
		public static void HandlePageTextQuery(IRealmClient client, RealmPacketIn packet)
		{
			var pageId = packet.ReadUInt32();

			SendPageText(client.ActiveCharacter, pageId);
		}

		public static void SendPageText(Character chr, uint pageId)
		{
			var entry = PageTextEntry.GetEntry(pageId);
			if (entry != null)
			{
				do
				{
					SendPageText(chr, entry);
					entry = entry.NextPageEntry;
				} while (entry != null);
			}
			else
			{
				using (var outPack = new RealmPacketOut(RealmServerOpCode.SMSG_PAGE_TEXT_QUERY_RESPONSE, 100))
				{
					outPack.Write("-page is missing-");
					outPack.Write(0);
					chr.Send(outPack);
				}
			}
		}

		public static void SendPageText(Character chr, PageTextEntry entry)
		{
			var locale = chr.Locale;
			while (entry != null)
			{
				using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PAGE_TEXT_QUERY_RESPONSE, 100))
				{
					packet.Write(entry.PageId);
					packet.Write(entry.Texts.Localize(locale));
					packet.Write(entry.NextPageId);
					chr.Send(packet);
				}
				entry = entry.NextPageEntry;
			}
		}
		#endregion
	}
}