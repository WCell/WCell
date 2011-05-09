/*************************************************************************
 *
 *   file		    : ChatHandler.cs
 *   copyright		: (C) The WCell Team
 *   email		    : info@wcell.org
 *   last changed	: $LastChangedDate: 2008-02-19 07:46:37 -0800 (Tue, 19 Feb 2008) $
 *   last author	: $LastChangedBy: anonemous $
 *   revision		: $Rev: 148 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;
using WCell.Util;

namespace WCell.RealmServer.Chat
{
	public static partial class ChatMgr
	{
		/// <summary>
		/// Handles an incoming chat message.
		/// </summary>
		/// <param name="client">the client that sent to us</param>
		/// <param name="packet">the full packet</param>
        [ClientPacketHandler(RealmServerOpCode.CMSG_MESSAGECHAT_SAY, RequiresLogin = false)] // one can also chat while logging out
        [ClientPacketHandler(RealmServerOpCode.CMSG_MESSAGECHAT_YELL, RequiresLogin = false)]
        [ClientPacketHandler(RealmServerOpCode.CMSG_MESSAGECHAT_CHANNEL, RequiresLogin = false)]
        [ClientPacketHandler(RealmServerOpCode.CMSG_MESSAGECHAT_GUILD, RequiresLogin = false)]
        [ClientPacketHandler(RealmServerOpCode.CMSG_MESSAGECHAT_OFFICER, RequiresLogin = false)]
        [ClientPacketHandler(RealmServerOpCode.CMSG_MESSAGECHAT_AFK, RequiresLogin = false)]
        [ClientPacketHandler(RealmServerOpCode.CMSG_MESSAGECHAT_DND, RequiresLogin = false)]
        [ClientPacketHandler(RealmServerOpCode.CMSG_MESSAGECHAT_EMOTE, RequiresLogin = false)]
        [ClientPacketHandler(RealmServerOpCode.CMSG_MESSAGECHAT_PARTY, RequiresLogin = false)]
        [ClientPacketHandler(RealmServerOpCode.CMSG_MESSAGECHAT_PARTY_LEADER, RequiresLogin = false)]
        [ClientPacketHandler(RealmServerOpCode.CMSG_MESSAGECHAT_RAID, RequiresLogin = false)]
        [ClientPacketHandler(RealmServerOpCode.CMSG_MESSAGECHAT_RAID_LEADER, RequiresLogin = false)]
        [ClientPacketHandler(RealmServerOpCode.CMSG_MESSAGECHAT_BATTLEGROUND, RequiresLogin = false)]
        [ClientPacketHandler(RealmServerOpCode.CMSG_MESSAGECHAT_BATTLEGROUND_LEADER, RequiresLogin = false)]
		[ClientPacketHandler(RealmServerOpCode.CMSG_MESSAGECHAT_RAID_WARNING, RequiresLogin = false)]
		public static void HandleChatMessage(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;

			if (chr.IsLoggingOut && !chr.IsPlayerLogout)
			{
				// forced logout -> can't talk
				ItemHandler.SendCantDoRightNow(client);
			}
			else
			{
				var type = GetTypeFromOpcode((RealmServerOpCode)packet.PacketId.RawId);

				var parseHandler = ChatParsers.Get((uint) type);
				if (parseHandler == null)
					return;

				var language = chr.SpokenLanguage;
				if (language == ChatLanguage.Universal)
				{
					// language is not forced
					language = (ChatLanguage) packet.ReadUInt32();
					if (!chr.GodMode && (!chr.CanSpeak(language)))
					{
						// TODO: Cheater
						return;
					}
				}
				else
				{
					// spoken language is forced
					packet.ReadUInt32();
				}

				parseHandler(chr, type, language, packet);
			}
		}

		/// <summary>
		/// Creates a packet
		/// </summary>
		public static RealmPacketOut CreateChatPacket(ChatMsgType type, ChatLanguage language, string msg, ChatTag tag)
		{
			var packet = new RealmPacketOut(RealmServerOpCode.SMSG_MESSAGECHAT, 23 + msg.Length);
			packet.WriteByte((byte)type);			// 1
			packet.WriteUInt((uint)language);		// 5
			packet.WriteUIntPascalString(msg);			// 22 + msg.Length
			packet.WriteByte((byte)tag);			// 23 + msg.Length

			return packet;
		}

        public static ChatMsgType GetTypeFromOpcode(RealmServerOpCode chatOpcode)
        {
            var type = ChatMsgType.End;
            switch (chatOpcode)
            {
                case RealmServerOpCode.CMSG_MESSAGECHAT_SAY:
                    type = ChatMsgType.Say;
                    break;
                case RealmServerOpCode.CMSG_MESSAGECHAT_YELL:
                    type = ChatMsgType.Yell;
                    break;
                case RealmServerOpCode.CMSG_MESSAGECHAT_CHANNEL:
                    type = ChatMsgType.Channel;
                    break;
                case RealmServerOpCode.CMSG_MESSAGECHAT_WHISPER:
                    type = ChatMsgType.Whisper;
                    break;
                case RealmServerOpCode.CMSG_MESSAGECHAT_GUILD:
                    type = ChatMsgType.Guild;
                    break;
                case RealmServerOpCode.CMSG_MESSAGECHAT_OFFICER:
                    type = ChatMsgType.Officer;
                    break;
                case RealmServerOpCode.CMSG_MESSAGECHAT_AFK:
                    type = ChatMsgType.AFK;
                    break;
                case RealmServerOpCode.CMSG_MESSAGECHAT_DND:
                    type = ChatMsgType.DND;
                    break;
                case RealmServerOpCode.CMSG_MESSAGECHAT_EMOTE:
                    type = ChatMsgType.Emote;
                    break;
                case RealmServerOpCode.CMSG_MESSAGECHAT_PARTY:
                    type = ChatMsgType.Party;
                    break;
                case RealmServerOpCode.CMSG_MESSAGECHAT_PARTY_LEADER:
                    type = ChatMsgType.PartyLeader;
                    break;
                case RealmServerOpCode.CMSG_MESSAGECHAT_RAID:
                    type = ChatMsgType.Raid;
                    break;
                case RealmServerOpCode.CMSG_MESSAGECHAT_RAID_LEADER:
                    type = ChatMsgType.RaidLeader;
                    break;
                case RealmServerOpCode.CMSG_MESSAGECHAT_BATTLEGROUND:
                    type = ChatMsgType.Battleground;
                    break;
                case RealmServerOpCode.CMSG_MESSAGECHAT_BATTLEGROUND_LEADER:
                    type = ChatMsgType.BattlegroundLeader;
                    break;
                case RealmServerOpCode.CMSG_MESSAGECHAT_RAID_WARNING:
                    type = ChatMsgType.RaidWarn;
                    break;
            }
            return type;
        }
	}
}