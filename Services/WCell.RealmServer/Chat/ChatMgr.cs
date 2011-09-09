/*************************************************************************
 *
 *   file		    : ChatMgr.cs
 *   copyright		: (C) The WCell Team
 *   email		    : info@wcell.org
 *   last changed	: $LastChangedDate: 2008-02-18 07:46:37 -0800 (Tue, 19 Feb 2008) $
 *   last author	: $LastChangedBy: anonemous $
 *   revision		: $Rev: 148 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using WCell.Constants;
using WCell.Constants.Chat;
using WCell.Constants.Guilds;
using WCell.Constants.Misc;
using WCell.Core;
using WCell.RealmServer.Commands;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Guilds;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Network;
using WCell.Util;

namespace WCell.RealmServer.Chat
{

	/// <summary>
	/// Manager for sending and receiving chat messages.
	/// </summary>
	public static partial class ChatMgr
	{
		/// <summary>
		/// The start of the lowercase alphabet in the ASCII table.
		/// </summary>
		private const int LowercaseAlphabetStart = ('a');

		/// <summary>
		/// The end of the lowercase alphabet in the ASCII table.
		/// </summary>
		private const int LowercaseAlphabetEnd = ('z');

		/// <summary>
		/// The radius in which people can hear a someone else say something.
		/// </summary>
		public static float ListeningRadius = 50f;

		/// <summary>
		/// The radius in which people can hear a someone else say something.
		/// </summary>
		public static float YellRadius = 300f;

		/// <summary>
		/// Whether normal say/yell/emote is global. (sent to everyone in the world)
		/// </summary>
		public static bool GlobalChat = true;

		/// <summary>
		/// Whether chat messages of opposite factions will be scrambled
		/// </summary>
		public static bool ScrambleChat = true;

		/// <summary>
		/// Array of chat type handlers.
		/// </summary.
		public static readonly ChatParserDelegate[] ChatParsers = new ChatParserDelegate[(int)ChatMsgType.End];

		/// <summary>
		/// Default static constructor.
		/// </summary>
		static ChatMgr()
		{
			ChatParsers[(int)ChatMsgType.Say] = SayYellEmoteParser;
			ChatParsers[(int)ChatMsgType.Yell] = SayYellEmoteParser;
			ChatParsers[(int)ChatMsgType.Emote] = SayYellEmoteParser;
			ChatParsers[(int)ChatMsgType.Party] = GroupParser;
			ChatParsers[(int)ChatMsgType.Raid] = SubGroupParser;
			ChatParsers[(int)ChatMsgType.RaidLeader] = SubGroupParser;
			ChatParsers[(int)ChatMsgType.RaidWarn] = SubGroupParser;
			ChatParsers[(int)ChatMsgType.Guild] = GuildParser;
			ChatParsers[(int)ChatMsgType.Officer] = OfficerParser;
			ChatParsers[(int)ChatMsgType.Whisper] = WhisperParser;
			ChatParsers[(int)ChatMsgType.Channel] = ChannelParser;
			ChatParsers[(int)ChatMsgType.AFK] = AFKParser;
			ChatParsers[(int)ChatMsgType.DND] = AFKParser;
		}

		#region Parsers

		/// <summary>
		/// Parses any incoming say, yell, or emote messages.
		/// </summary>
		/// <param name="type">the type of chat message indicated by the client</param>
		/// <param name="language">the chat language indicated by the client</param>
		/// <param name="packet">the actual chat message packet</param>
		private static void SayYellEmoteParser(Character sender, ChatMsgType type, ChatLanguage language, RealmPacketIn packet)
		{
			var msg = ReadMessage(packet);
			if (msg.Length == 0)
				return;

			SayYellEmote(sender, type, language, msg, type == ChatMsgType.Yell ? YellRadius : ListeningRadius);
		}

		public static void SayYellEmote(this Character sender, ChatMsgType type, ChatLanguage language, string msg, float radius)
		{
			if (RealmCommandHandler.HandleCommand(sender, msg, sender.Target as Character))
				return;

			if (type != ChatMsgType.WhisperInform && msg.Length == 0)
			{
				return;
			}

			if (GlobalChat)
			{
				using (var packetOut = CreateCharChatMessage(type, language, sender.EntityId, sender.EntityId, null, msg, sender.ChatTag))
				{
					foreach (var chr in World.GetAllCharacters())
					{
						chr.Send(packetOut);
					}
				}
			}
			else
			{
				var faction = sender.FactionGroup;
				RealmPacketOut pckt = null, scrambledPckt = null;

				var scrambleDefault = ScrambleChat && sender.Role.ScrambleChat;
				Func<WorldObject, bool> iterator = obj =>
				{
					if ((obj is Character))
					{
						var chr = (Character)obj;
						if (!scrambleDefault || chr.FactionGroup == faction || !chr.Role.ScrambleChat)
						{
							if (pckt == null)
							{
								pckt = CreateCharChatMessage(type, language, sender.EntityId, sender.EntityId, null, msg, sender.ChatTag);
							}
							chr.Send(pckt);
						}
						else
						{
							if (scrambledPckt == null)
							{
								scrambledPckt = CreateCharChatMessage(type, language, sender.EntityId, sender.EntityId, null,
																	  ScrambleMessage(msg), sender.ChatTag);
							}
							chr.Send(scrambledPckt);
						}
					}
					return true;
				};

				if (radius == WorldObject.BroadcastRange)
				{
					sender.NearbyObjects.Iterate(iterator);
				}
				else
				{
					sender.IterateEnvironment(radius, iterator);
				}

				if (pckt != null)
				{
					pckt.Close();
				}
				if (scrambledPckt != null)
				{
					scrambledPckt.Close();
				}
			}
		}

		/// <summary>
		/// Parses any incoming party or raid messages.
		/// </summary>
		/// <param name="sender">The character sending the message</param>
		/// <param name="type">the type of chat message indicated by the client</param>
		/// <param name="language">the chat language indicated by the client</param>
		/// <param name="packet">the actual chat message packet</param>
		private static void GroupParser(Character sender, ChatMsgType type, ChatLanguage language, RealmPacketIn packet)
		{
			var msg = ReadMessage(packet);

			if (msg.Length == 0)
				return;

			SayGroup(sender, language, msg);
		}

		public static void SayGroup(this Character sender, ChatLanguage language, string msg)
		{
			if (RealmCommandHandler.HandleCommand(sender, msg, sender.Target as Character))
				return;

			var group = sender.Group;
			if (group != null)
				using (var packetOut =
					CreateCharChatMessage(ChatMsgType.Party, ChatLanguage.Universal, sender, sender, null, msg, sender.ChatTag))
				{
					group.SendAll(packetOut);
				}
		}

		/// <summary>
		/// Parses any incoming party or raid messages.
		/// </summary>
		/// <param name="sender">The character sending the message</param>
		/// <param name="type">the type of chat message indicated by the client</param>
		/// <param name="language">the chat language indicated by the client</param>
		/// <param name="packet">the actual chat message packet</param>
		private static void SubGroupParser(Character sender, ChatMsgType type, ChatLanguage language, RealmPacketIn packet)
		{
			string msg = ReadMessage(packet);

			if (msg.Length == 0)
				return;

			if (RealmCommandHandler.HandleCommand(sender, msg, sender.Target as Character))
				return;

			var group = sender.SubGroup;
			if (group != null)
			{
				using (var packetOut = CreateCharChatMessage(type, ChatLanguage.Universal, sender, sender, null, msg))
				{
					group.Send(packetOut, null);
				}
			}
		}

		/// <summary>
		/// Parses any incoming guild message.
		/// </summary>
		/// <param name="sender">The character sending the message</param>
		/// <param name="type">the type of chat message indicated by the client</param>
		/// <param name="language">the chat language indicated by the client</param>
		/// <param name="packet">the actual chat message packet</param>
		private static void GuildParser(Character sender, ChatMsgType type, ChatLanguage language, RealmPacketIn packet)
		{
			var msg = ReadMessage(packet);

			if (msg.Length == 0)
				return;

			if (RealmCommandHandler.HandleCommand(sender, msg, sender.Target as Character))
				return;

			var guild = Guild.CheckPrivs(sender, GuildCommandId.MEMBER, GuildPrivileges.GCHATSPEAK);
			if (guild != null)
			{
				SendGuildMessage(sender, guild, msg);
			}
		}

		/// <summary>
		/// Parses any incoming officer message.
		/// </summary>
		/// <param name="sender">The character sending the message</param>
		/// <param name="type">the type of chat message indicated by the client</param>
		/// <param name="language">the chat language indicated by the client</param>
		/// <param name="packet">the actual chat message packet</param>
		private static void OfficerParser(Character sender, ChatMsgType type, ChatLanguage language, RealmPacketIn packet)
		{
			string msg = ReadMessage(packet);

			if (msg.Length == 0)
				return;

			if (RealmCommandHandler.HandleCommand(sender, msg, sender.Target as Character))
				return;

			var guild = Guild.CheckPrivs(sender, GuildCommandId.MEMBER, GuildPrivileges.GCHATSPEAK);
			if (guild != null)
			{
				SendGuildOfficerMessage(sender, guild, msg);
			}
		}

		/// <summary>
		/// Parses any incoming whispers.
		/// </summary>
		/// <param name="type">the type of chat message indicated by the client</param>
		/// <param name="language">the chat language indicated by the client</param>
		/// <param name="packet">the actual chat message packet</param>
		private static void WhisperParser(Character sender, ChatMsgType type, ChatLanguage language, RealmPacketIn packet)
		{
			var recipient = packet.ReadCString();
			var msg = ReadMessage(packet);

			if (msg.Length == 0)
				return;

			if (RealmCommandHandler.HandleCommand(sender, msg, sender.Target as Character))
				return;

			var targetChr = World.GetCharacter(recipient, false);
			if (targetChr == null)
			{
				SendChatPlayerNotFoundReply(sender.Client, recipient);
				return;
			}

			if (targetChr.Faction.Group != sender.Faction.Group)
			{
				SendChatPlayerWrongTeamReply(sender.Client);
				return;
			}

			if (targetChr.IsIgnoring(sender))
			{
				using (var packetOut = CreateCharChatMessage(ChatMsgType.Ignored, ChatLanguage.Universal, targetChr, sender, null, msg))
				{
					sender.Send(packetOut);
				}
			}
			else
			{
				using (var packetOut = CreateCharChatMessage(ChatMsgType.Whisper, ChatLanguage.Universal, sender, targetChr, null, msg))
				{
					targetChr.Send(packetOut);
				}
			}

			using (var packetOut = CreateCharChatMessage(ChatMsgType.MsgReply, ChatLanguage.Universal, targetChr, targetChr, null, msg, sender.ChatTag))
			{
				sender.Send(packetOut);
			}

			// handle afk/dnd situations
			if (targetChr.IsAFK)
			{
				using (var packetOut = CreateCharChatMessage(ChatMsgType.AFK, ChatLanguage.Universal, targetChr, sender, null, targetChr.AFKReason, targetChr.ChatTag))
				{
					sender.Send(packetOut);
				}
			}

			if (targetChr.IsDND)
			{
				using (var packetOut = CreateCharChatMessage(ChatMsgType.DND, ChatLanguage.Universal, targetChr, sender, null, string.Empty, targetChr.ChatTag))
				{
					sender.Send(packetOut);
				}
			}
		}

		/// <summary>
		/// Parses any incoming channel messages.
		/// </summary>
		/// <param name="type">the type of chat message indicated by the client</param>
		/// <param name="language">the chat language indicated by the client</param>
		/// <param name="packet">the actual chat message packet</param>
		private static void ChannelParser(Character sender, ChatMsgType type, ChatLanguage language, RealmPacketIn packet)
		{
			var channel = packet.ReadCString();
			var message = packet.ReadCString();

			if (RealmCommandHandler.HandleCommand(sender, message, sender.Target as Character))
				return;

			var chan = ChatChannelGroup.RetrieveChannel(sender, channel);
			if (chan == null)
				return;

			chan.SendMessage(sender, message);
		}

		private static void AFKParser(Character sender, ChatMsgType type, ChatLanguage language, RealmPacketIn packet)
		{
			var reason = packet.ReadCString();

			if (type == ChatMsgType.AFK)
			{
				// flip their AFK flag
				sender.IsAFK = !sender.IsAFK;

				sender.AFKReason = (sender.IsAFK ? reason : "");
			}

			if (type == ChatMsgType.DND)
			{
				// flip their DND flag
				sender.IsDND = !sender.IsDND;

				sender.DNDReason = (sender.IsDND ? reason : "");
			}
		}

		#endregion

		#region Send/Reply methods

		/// <summary>
		/// Sends a message that the whisper target wasn't found.
		/// </summary>
		/// <param name="client">the client to send to</param>
		/// <param name="recipient">the name of the target player</param>
		public static void SendChatPlayerNotFoundReply(IPacketReceiver client, string recipient)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHAT_PLAYER_NOT_FOUND))
			{
				packet.WriteCString(recipient);
				client.Send(packet);
			}
		}

		/// <summary>
		/// Sends a message that the whisper target isn't the same faction.
		/// </summary>
		/// <param name="client">the client to send to</param>
		/// <param name="recipient">the name of the target player</param>
		public static void SendChatPlayerWrongTeamReply(IPacketReceiver client)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHAT_WRONG_FACTION))
			{
				// just guessing here.
				client.Send(packet);
			}
		}

		#endregion

		#region Send Helpers
		private static RealmPacketOut CreateObjectChatMessage(ChatMsgType type, ChatLanguage language, INamedEntity obj)
		{
			var name = obj.Name;

			var packet = new RealmPacketOut(RealmServerOpCode.SMSG_MESSAGECHAT, 31 + name.Length + 50);
			packet.Write((byte)type);															// 1
			packet.Write((uint)language);														// 5
			packet.Write(obj.EntityId);																// 13
			packet.Write(0);																	// 17
			packet.WriteUIntPascalString(name);														// 21 + nameLength
			packet.Write((long)0);																// 29 + nameLength
			//packet.Write(obj.EntityId);

			return packet;
		}

		/// <summary>
		/// Creates a chat message packet for a non-player object.
		/// </summary>
		/// <param name="type">the type of chat message</param>
		/// <param name="language">the language the message is in</param>
		/// <param name="obj">the object "saying" the message</param>
		/// <param name="msg">the message itself</param>
		/// <param name="tag">any chat tags for the object</param>
		/// <returns>the generated chat packet</returns>
		private static RealmPacketOut CreateObjectChatMessage(ChatMsgType type,
			ChatLanguage language, INamedEntity obj, string msg, ChatTag tag)
		{
			var packet = CreateObjectChatMessage(type, language, obj);
			//packet.Write(obj.EntityId);	
			packet.WriteUIntPascalString(msg);														// 30 + nameLength + msg.Length
			packet.Write((byte)tag);															// 31 + ...

			return packet;
		}

		/// <summary>
		/// Creates a chat message packet for a player.
		/// </summary>
		/// <param name="type">the type of chat message</param>
		/// <param name="language">the language the message is in</param>
		/// <param name="id1">the ID of the chatter</param>
		/// <param name="id2">the ID of the receiver</param>
		/// <param name="target">the target or null (if its an area message)</param>
		/// <param name="msg">the message itself</param>
		/// <param name="tag">the chat tag of the chatter</param>
		private static RealmPacketOut CreateCharChatMessage(ChatMsgType type, ChatLanguage language, IEntity id1, IEntity id2,
			string target, string msg, ChatTag tag)
		{
			return CreateCharChatMessage(type, language, id1.EntityId, id2.EntityId, target, msg, tag);
		}

		/// <summary>
		/// Creates a chat message packet for a player.
		/// </summary>
		/// <param name="type">the type of chat message</param>
		/// <param name="language">the language the message is in</param>
		/// <param name="id1">the ID of the chatter</param>
		/// <param name="id2">the ID of the receiver</param>
		/// <param name="target">the target or null (if its an area message)</param>
		/// <param name="msg">the message itself</param>
		private static RealmPacketOut CreateCharChatMessage(ChatMsgType type, ChatLanguage language, IChatter id1, IChatter id2,
			string target, string msg)
		{
			return CreateCharChatMessage(type, language, id1.EntityId, id2.EntityId, target, msg, id1.ChatTag);
		}

		/// <summary>
		/// Creates a chat message packet for a player.
		/// </summary>
		/// <param name="type">the type of chat message</param>
		/// <param name="language">the language the message is in</param>
		/// <param name="id1">the ID of the chatter</param>
		/// <param name="id2">the ID of the receiver</param>
		/// <param name="target">the target or null (if its an area message)</param>
		/// <param name="msg">the message itself</param>
		/// <param name="tag">the chat tag of the chatter</param>
		/// <returns>Might return null</returns>
		private static RealmPacketOut CreateCharChatMessage(ChatMsgType type, ChatLanguage language, EntityId id1, EntityId id2,
			string target, string msg, ChatTag tag)
		{
			var packet = new RealmPacketOut(RealmServerOpCode.SMSG_MESSAGECHAT);
			packet.Write((byte)type);
			packet.Write((uint)language);
			packet.Write(id1);
			packet.Write(0);
			if (target != null)
				packet.WriteUIntPascalString(target);
			packet.Write(id2);
			packet.WriteUIntPascalString(msg);
			packet.Write((byte)tag);

			return packet;
		}

		#region SendSystemMessage
		/// <summary>
		/// Sends a system message.
		/// </summary>
		/// <param name="target">the receiver of the message</param>
		/// <param name="message">the message to send</param>
		public static void SendSystemMessage(IPacketReceiver target, string message)
		{
			using (var packet = CreateCharChatMessage(ChatMsgType.System, ChatLanguage.Universal, EntityId.Zero, EntityId.Zero, null, message, ChatTag.None))
			{
				target.Send(packet);
			}
		}

		/// <summary>
		/// Sends a system message.
		/// TODO: Improve performance
		/// </summary>
		/// <param name="targets">an enumerable collection of players to send the message to</param>
		/// <param name="message">the message to send</param>
		public static void SendSystemMessage(this IEnumerable<Character> targets, TranslatableItem item)
		{
			SendSystemMessage(targets, item.Key, item.Args);
		}

		public static void SendSystemMessage(this IEnumerable<Character> targets, string[] texts, params object[] args)
		{
			foreach (var target in targets)
			{
				if (target != null)
				{
					target.SendSystemMessage(texts.Localize(target.Locale, args));
				}
			}
		}
		/// <summary>
		/// Sends a system message.
		/// </summary>
		/// <param name="targets">an enumerable collection of players to send the message to</param>
		/// <param name="message">the message to send</param>
		public static void SendSystemMessage(this IEnumerable<Character> targets, RealmLangKey langKey, params object[] args)
		{
			foreach (var target in targets)
			{
				if (target != null)
				{
					target.SendSystemMessage(langKey, args);
				}
			}
		}

		/// <summary>
		/// Sends a system message.
		/// </summary>
		/// <param name="targets">an enumerable collection of players to send the message to</param>
		/// <param name="message">the message to send</param>
		public static void SendSystemMessage(this IEnumerable<Character> targets, string message, params object[] args)
		{
			SendSystemMessage(targets, string.Format(message, args));
		}

		/// <summary>
		/// Sends a system message.
		/// </summary>
		/// <param name="targets">an enumerable collection of players to send the message to</param>
		/// <param name="message">the message to send</param>
		public static void SendSystemMessage(this IEnumerable<Character> targets, string message)
		{
			using (var packet = CreateCharChatMessage(ChatMsgType.System, ChatLanguage.Universal, EntityId.Zero, EntityId.Zero, null, message, ChatTag.None))
			{
				foreach (var target in targets)
				{
					if (target != null)
					{
						target.Send(packet);
					}
				}
			}
		}
		#endregion


		/// <summary>
		/// Sends the amount of experience gained to the characters combat log.
		/// </summary>
		/// <param name="target">the character to receieve the combat log message</param>
		/// <param name="message">the message to display in the characters combat log</param>
		public static void SendCombatLogExperienceMessage(IPacketReceiver target, ClientLocale locale, RealmLangKey key, params object[] args)
		{
			using (var packet = CreateCharChatMessage(ChatMsgType.CombatXPGain, ChatLanguage.Universal, EntityId.Zero, EntityId.Zero, null,
				RealmLocalizer.Instance.Translate(locale, key, args), ChatTag.None))
			{
				target.Send(packet);
			}
		}

		/// <summary>
		/// Sends a whisper from one player to another.
		/// </summary>
		/// <param name="sender">the sender of the whisper</param>
		/// <param name="receiver">the target of the whisper</param>
		/// <param name="message">the message to send</param>
		public static void SendWhisper(IChatter sender, IChatter receiver, string message)
		{
			if (message.Length == 0)
				return;

			using (var chatPacket = CreateCharChatMessage(ChatMsgType.Whisper, sender.SpokenLanguage, sender, receiver, null, message))
			{
				receiver.Send(chatPacket);
			}

			using (var chatPacket = CreateCharChatMessage(ChatMsgType.WhisperInform, sender.SpokenLanguage, sender, receiver, null, message))
			{
				sender.Send(chatPacket);
			}
		}

		/// <summary>
		/// Sends a whisper from one player to another.
		/// </summary>
		/// <param name="sender">the sender of the whisper</param>
		/// <param name="receiver">the target of the whisper</param>
		/// <param name="message">the message to send</param>
		public static void SendRaidBossWhisper(WorldObject sender, IChatter receiver, string message)
		{
			if (message.Length == 0)
				return;

			using (var chatPacket = CreateCharChatMessage(ChatMsgType.RaidBossWhisper, ChatLanguage.Universal, sender.EntityId, receiver.EntityId, sender.Name, message, ChatTag.None))
			{
				receiver.Send(chatPacket);
			}
		}

		/// <summary>
		/// Sends a message to guild chat.
		/// </summary>
		/// <param name="sender">the sender/guild member of the message</param>
		/// <param name="message">the message to send</param>
		internal static void SendGuildMessage(IChatter sender, Guild guild, string message)
		{
			using (var chatPacket = CreateCharChatMessage(ChatMsgType.Guild, sender.SpokenLanguage, sender, sender, null, message))
			{
				guild.SendToChatListeners(chatPacket);
			}
		}

		/// <summary>
		/// Sends an officer-only message to guild chat.
		/// </summary>
		/// <param name="sender">the sender/guild member of the message</param>
		/// <param name="message">the message to send</param>
		internal static void SendGuildOfficerMessage(IChatter sender, Guild guild, string message)
		{
			using (var chatPacket = CreateCharChatMessage(ChatMsgType.Officer, sender.SpokenLanguage, sender, sender, null, message))
			{
				guild.SendToOfficers(chatPacket);
			}
		}

		/// <summary>
		/// Sends a monster message.
		/// </summary>
		/// <param name="obj">the monster the message is being sent from</param>
		/// <param name="chatType">the type of message</param>
		/// <param name="language">the language to send the message in</param>
		/// <param name="message">the message to send</param>
		public static void SendMonsterMessage(WorldObject obj, ChatMsgType chatType, ChatLanguage language, string message)
		{
			SendMonsterMessage(obj, chatType, language, message, chatType == ChatMsgType.MonsterYell ? YellRadius : ListeningRadius);
		}

		/// <summary>
		/// Sends a monster message.
		/// </summary>
		/// <param name="obj">the monster the message is being sent from</param>
		/// <param name="chatType">the type of message</param>
		/// <param name="language">the language to send the message in</param>
		/// <param name="message">the message to send</param>
		public static void SendMonsterMessage(WorldObject obj, ChatMsgType chatType, ChatLanguage language, string[] localizedMsgs)
		{
			SendMonsterMessage(obj, chatType, language, localizedMsgs, chatType == ChatMsgType.MonsterYell ? YellRadius : ListeningRadius);
		}

		/// <summary>
		/// Sends a monster message.
		/// </summary>
		/// <param name="obj">the monster the message is being sent from</param>
		/// <param name="chatType">the type of message</param>
		/// <param name="language">the language to send the message in</param>
		/// <param name="message">the message to send</param>
		/// <param name="radius">The radius or -1 to be heard by everyone in the Map</param>
		public static void SendMonsterMessage(WorldObject obj, ChatMsgType chatType, ChatLanguage language, string message, float radius)
		{
			if (obj == null || !obj.IsAreaActive)
				return;

			using (var packetOut = CreateObjectChatMessage(chatType, language, obj, message, obj is Unit ? ((Unit)obj).ChatTag : ChatTag.None))
			{
				obj.SendPacketToArea(packetOut, radius, true);
			}
		}

		public static void SendMonsterMessage(WorldObject chatter, ChatMsgType chatType, ChatLanguage language, string[] localizedMsgs, float radius)
		{
			if (chatter == null || !chatter.IsAreaActive)
				return;

			using (var packet = CreateObjectChatMessage(chatType, language, chatter))
			{
				chatter.IterateEnvironment(radius, obj =>
				{
					if (obj is Character)
					{
						packet.WriteUIntPascalString(localizedMsgs.Localize(((Character)obj).Client.Info.Locale));
						packet.Write((byte)(chatter is Unit ? ((Unit)chatter).ChatTag : ChatTag.None));

						((Character)obj).Send(packet.GetFinalizedPacket());
					}
					return true;
				});
			}
		}

		#endregion

		#region Helper methods

		/// <summary>
		/// Converts chat channel flags from DBC format to client format.
		/// </summary>
		/// <param name="dbcFlags">the DBC chat channel flags</param>
		/// <returns>converted client chat channel flags</returns>
		public static ChatChannelFlagsClient Convert(ChatChannelFlags dbcFlags)
		{
			var flags = ChatChannelFlagsClient.Predefined;

			if (dbcFlags.HasFlag(ChatChannelFlags.Trade))
			{
				flags |= ChatChannelFlagsClient.Trade;
			}
			if (dbcFlags.HasFlag(ChatChannelFlags.CityOnly))
			{
				flags |= ChatChannelFlagsClient.CityOnly;
			}
			if (dbcFlags.HasFlag(ChatChannelFlags.LookingForGroup))
			{
				flags |= ChatChannelFlagsClient.LFG;
			}
			else
			{
				flags |= ChatChannelFlagsClient.FFA;
			}

			return flags;
		}

		/// <summary>
		/// Reads a string from a packet, and treats it like a chat message, purifying it.
		/// </summary>
		/// <param name="packet">the packet to read from</param>
		/// <returns>the purified chat message</returns>
		private static string ReadMessage(RealmPacketIn packet)
		{
			var msg = packet.ReadCString();

			ChatUtility.Purify(ref msg);

			return msg;
		}

		/// <summary>
		/// Takes a string, and scrambles any letters or numbers with random letters.
		/// </summary>
		/// <param name="originalMsg">the original unscrambled string</param>
		/// <returns>the randomized/scrambled string</returns>
		private static string ScrambleMessage(string originalMsg)
		{
			Random rng = new Random(1132532542);

			StringBuilder scramMsg = new StringBuilder(originalMsg.Length);

			for (int pos = 0; pos < originalMsg.Length; pos++)
			{
				if (Char.IsLetterOrDigit(originalMsg[pos]))
				{
					scramMsg.Append((char)rng.Next(LowercaseAlphabetStart, LowercaseAlphabetEnd));
				}
				else
				{
					scramMsg.Append(originalMsg[pos]);
				}
			}

			return scramMsg.ToString();
		}

		#endregion

		/// <summary>
		/// Triggers a chat notification event.
		/// </summary>
		/// <param name="chatter">the person chatting</param>
		/// <param name="message">the chat message</param>
		/// <param name="language">the chat language</param>
		/// <param name="chatType">the type of chat</param>
		/// <param name="target">the target of the message (channel, whisper, etc)</param>
		public static void ChatNotify(IChatter chatter, string message, ChatLanguage language, ChatMsgType chatType, IGenericChatTarget target)
		{
			var chatNotify = MessageSent;

			if (chatNotify != null)
			{
				chatNotify(chatter, message, language, chatType, target);
			}
		}

		public static bool IsYell(this ChatMsgType type)
		{
			return type == ChatMsgType.Yell || type == ChatMsgType.MonsterYell;
		}
	}
}