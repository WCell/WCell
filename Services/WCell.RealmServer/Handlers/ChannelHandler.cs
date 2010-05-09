/*************************************************************************
 *
 *   file		    : Channel.Handlers.cs
 *   copyright		: (C) The WCell Team
 *   email		    : info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-22 22:18:21 +0100 (fr, 22 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1210 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.Constants.Chat;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
	public static class ChannelHandler
	{
		#region Packet Handlers

		/// <summary>
		/// Handles an incoming channel join request
		/// </summary>
		/// <param name="client">the client the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_JOIN_CHANNEL)]
		public static void HandleJoinChannel(IRealmClient client, RealmPacketIn packet)
		{
			//int channelNumber = packet.ReadInt32();
			uint channelId = packet.ReadUInt32(); // used for lookup in dbc for flags

			byte unk = packet.ReadByte(); // Unknown
            byte unk2 = packet.ReadByte(); // Unknown

		    string channelName = packet.ReadCString();
			string password = packet.ReadCString();

			var group = ChatChannelGroup.GetGroup(client.ActiveCharacter.Faction.Group);
			if (group == null)
				return;

			if (string.IsNullOrEmpty(channelName))
				return;

			var chan = group.GetChannel(channelName, channelId, true);

			chan.TryJoin(client.ActiveCharacter, password, false);
		}

		/// <summary>
		/// Handles an incoming channel leave request
		/// </summary>
		/// <param name="client">the client the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_LEAVE_CHANNEL)]
		public static void HandleLeaveChannel(IRealmClient client, RealmPacketIn packet)
		{
			var channelNumber = packet.ReadUInt32(); // unk
			var channelName = packet.ReadCString();

			var chan = ChatChannelGroup.RetrieveChannel(client.ActiveCharacter, channelName);

			if (chan != null)
			{
				chan.Leave(client.ActiveCharacter, false);
			}
		}

		/// <summary>
		/// Handles an incoming channel list request
		/// </summary>
		/// <param name="client">the client the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_CHANNEL_LIST)]
		public static void HandleListChannel(IRealmClient client, RealmPacketIn packet)
		{
			var channelName = packet.ReadCString();
			var chan = ChatChannelGroup.RetrieveChannel(client.ActiveCharacter, channelName);

			if (chan != null)
			{
				SendChannelList(client, chan);
			}
			else
			{
				SendNotOnChannelReply(client, channelName);
			}
		}

		/// <summary>
		/// Handles an incoming channel password change request
		/// </summary>
		/// <param name="client">the client the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_CHANNEL_PASSWORD)]
		public static void HandlePasswordChange(IRealmClient client, RealmPacketIn packet)
		{
			var channelName = packet.ReadCString();
			var password = packet.ReadCString();

			ChannelMember member;
			var chan = ChatChannel.EnsurePresence(client.ActiveCharacter, channelName, out member);

			if (chan != null)
			{
				if (!member.IsModerator)
				{
					SendNotModeratorReply(client, channelName);
				}
				else
				{
					chan.Password = password;
					SendPasswordChangedToEveryone(chan, client.ActiveCharacter.EntityId);
				}
			}
			else
			{
				SendNotOnChannelReply(client, channelName);
			}
		}

		/// <summary>
		/// Handles an incoming request of current owner
		/// </summary>
		/// <param name="client">the client the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_CHANNEL_OWNER)]
		public static void HandleCurrentOwnerRequest(IRealmClient client, RealmPacketIn packet)
		{
			string channelName = packet.ReadCString();

			var chan = ChatChannel.EnsureModerator(client.ActiveCharacter, channelName);
			if (chan != null)
			{
				SendCurrentOwner(client, chan);
			}
		}

		/// <summary>
		/// Handles an incoming owner set request
		/// </summary>
		/// <param name="client">the client the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_CHANNEL_SET_OWNER)]
		public static void HandleOwnerChange(IRealmClient client, RealmPacketIn packet)
		{
			var channelName = packet.ReadCString();
			var newOwner = packet.ReadCString();

			ChannelMember senderMember, targetMember;
			var chan = ChatChannel.EnsurePresence(client.ActiveCharacter, channelName, newOwner, out senderMember, out targetMember);

			if (chan != null)
			{
				chan.MakeOwner(senderMember, targetMember);
			}
		}

		/// <summary>
		/// Handles a request of making channel member a moderator
		/// </summary>
		/// <param name="client">the client the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_CHANNEL_MODERATOR)]
		public static void HandleSetModeratorRequest(IRealmClient client, RealmPacketIn packet)
		{
			var channelName = packet.ReadCString();
			var targetName = packet.ReadCString();

			ChannelMember senderMember, targetMember;
			var chan = ChatChannel.EnsurePresence(client.ActiveCharacter, channelName, targetName, out senderMember, out targetMember);

			if (chan != null)
			{
				chan.SetModerator(senderMember, targetMember, true);
			}
		}

		/// <summary>
		/// Handles a request of making channel member a non-moderator
		/// </summary>
		/// <param name="client">the client the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_CHANNEL_UNMODERATOR)]
		public static void HandleUnsetModeratorRequest(IRealmClient client, RealmPacketIn packet)
		{
			var channelName = packet.ReadCString();
			var targetName = packet.ReadCString();

			ChannelMember senderMember, targetMember;
			var chan = ChatChannel.EnsurePresence(client.ActiveCharacter, channelName, targetName, out senderMember, out targetMember);

			if (chan != null)
			{
				chan.SetModerator(senderMember, targetMember, false);
			}
		}

		/// <summary>
		/// Handles a request of muting a channel member
		/// </summary>
		/// <param name="client">the client the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_CHANNEL_MUTE)]
		public static void HandleMuteRequest(IRealmClient client, RealmPacketIn packet)
		{
			var channelName = packet.ReadCString();
			var targetName = packet.ReadCString();

			ChannelMember senderMember, targetMember;
			var chan = ChatChannel.EnsurePresence(client.ActiveCharacter, channelName, targetName, out senderMember, out targetMember);

			if (chan != null)
			{
				chan.SetMuted(senderMember, targetMember, true);
			}
		}

		/// <summary>
		/// Handles a request of unmuting a channel member
		/// </summary>
		/// <param name="client">the client the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_CHANNEL_UNMUTE)]
		public static void HandleUnMuteRequest(IRealmClient client, RealmPacketIn packet)
		{
			var channelName = packet.ReadCString();
			var targetName = packet.ReadCString();

			ChannelMember senderMember, targetMember;
			var chan = ChatChannel.EnsurePresence(client.ActiveCharacter, channelName, targetName, out senderMember, out targetMember);

			if (chan != null)
			{
				chan.SetMuted(senderMember, targetMember, false);
			}
		}

		/// <summary>
		/// Handles a invite to channel packet
		/// </summary>
		/// <param name="client">the client the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_CHANNEL_INVITE)]
		public static void HandleChannelInvite(IRealmClient client, RealmPacketIn packet)
		{
			var channelName = packet.ReadCString();
			var targetName = packet.ReadCString();

			ChannelMember inviter;
			var chan = ChatChannel.EnsurePresence(client.ActiveCharacter, channelName, out inviter);

			if (chan != null)
			{
				chan.Invite(inviter, targetName);
			}
		}

		/// <summary>
		/// Handles a request of kicking a channel member
		/// </summary>
		/// <param name="client">the client the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_CHANNEL_KICK)]
		public static void HandleKickRequest(IRealmClient client, RealmPacketIn packet)
		{
			var channelName = packet.ReadCString();
			var targetName = packet.ReadCString();
			var chr = client.ActiveCharacter;

			ChannelMember member;
			var chan = ChatChannel.EnsurePresence(chr, channelName, out member);

			if (chan != null)
			{
				chan.Kick(member, targetName);
			}
		}

		/// <summary>
		/// Handles a request of banning a channel member
		/// </summary>
		/// <param name="client">the client the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_CHANNEL_BAN)]
		public static void HandleBanRequest(IRealmClient client, RealmPacketIn packet)
		{
			var channelName = packet.ReadCString();
			var targetName = packet.ReadCString();
			var chr = client.ActiveCharacter;

			ChannelMember member;
			var chan = ChatChannel.EnsurePresence(chr, channelName, out member);

			if (chan != null)
			{
				chan.SetBanned(member, targetName, true);
			}
		}

		/// <summary>
		/// Handles a request of unbanning a channel member
		/// </summary>
		/// <param name="client">the client the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_CHANNEL_UNBAN)]
		public static void HandleUnbanRequest(IRealmClient client, RealmPacketIn packet)
		{
			var channelName = packet.ReadCString();
			var targetName = packet.ReadCString();

			ChannelMember member;
			var chan = ChatChannel.EnsurePresence(client.ActiveCharacter, channelName, out member);
			if (chan != null)
			{
				chan.SetBanned(member, targetName, false);
			}
		}

		/// <summary>
		/// Handles a request of toggling the announce mode of the channel
		/// </summary>
		/// <param name="client">the client the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_CHANNEL_ANNOUNCEMENTS)]
		public static void HandleAnnouncementsRequest(IRealmClient client, RealmPacketIn packet)
		{
			string channelName = packet.ReadCString();
			var chan = ChatChannel.EnsureModerator(client.ActiveCharacter, channelName);

			if (chan != null)
			{
				chan.Announces = !chan.Announces;

				SendAnnouncementToEveryone(chan, client.ActiveCharacter.EntityId, chan.Announces);
			}
		}

		/// <summary>
		/// Handles a request of toggling the moderate mode of the channel
		/// </summary>
		/// <param name="client">the client the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_CHANNEL_MODERATE)]
		public static void HandleModerateRequest(IRealmClient client, RealmPacketIn packet)
		{
			var channelName = packet.ReadCString();

			ChannelMember member;
			var chan = ChatChannel.EnsurePresence(client.ActiveCharacter, channelName, out member);

			if (chan != null)
			{
				chan.ToggleModerated(member);
			}
		}

		/// <summary>
		/// Handles a request of getting the number of members in a channel
		/// </summary>
		/// <param name="client">the client the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_GET_CHANNEL_MEMBER_COUNT, IsGamePacket = false, RequiresLogin = false)]
		public static void HandleMemberCountRequest(IRealmClient client, RealmPacketIn packet)
		{
			var channelName = packet.ReadCString();

			var chr = client.ActiveCharacter;
			if (chr == null)
			{
				// Queried before finishing login: Reply with 0 for now.
				SendMemberCountReply(client, channelName, 0, 0);
			}
			else
			{
				var chan = ChatChannelGroup.RetrieveChannel(client.ActiveCharacter, channelName);

				if (chan == null) return;

				SendMemberCountReply(chr, chan.Name, (byte) chan.Flags, (uint) chan.MemberCount);
			}
		}

		#endregion

		#region Send Methods
        //public static void SendChannelNotify(IPacketReceiver client, string chan, EntityId targetChr, ChannelNotification notification)
        //{
        //    using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHANNEL_NOTIFY))
        //    {
        //        packet.WriteByte((byte)ChannelNotification.AlreadyOnChannel);
        //        packet.WriteCString(chan);
        //        packet.Write(targetChr);

        //        client.Send(packet);
        //    }
        //}

		/// <summary>
		/// Send the "already on channel" reply
		/// </summary>
		/// <param name="client">the client the outdoing packet belongs to</param>
		/// <param name="chan">name of channel</param>
		public static void SendAlreadyOnChannelReply(IPacketReceiver client, string chan, EntityId entityId)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHANNEL_NOTIFY))
			{
				packet.WriteByte((byte)ChannelNotification.AlreadyOnChannel);
				packet.WriteCString(chan);
				packet.Write(entityId);

				client.Send(packet);
			}
		}

		/// <summary>
		/// Send the "you are banned" reply
		/// </summary>
		/// <param name="client">the client the outdoing packet belongs to</param>
		/// <param name="chan">name of channel</param>
		public static void SendBannedReply(IPacketReceiver client, string chan)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHANNEL_NOTIFY))
			{
				packet.WriteByte((byte)ChannelNotification.YouAreBanned);
				packet.WriteCString(chan);

				client.Send(packet);
			}
		}

		/// <summary>
		/// Send the "wrong password" reply
		/// </summary>
		/// <param name="client">the client the outdoing packet belongs to</param>
		/// <param name="chan">name of channel</param>
		public static void SendWrongPassReply(IPacketReceiver client, string chan)
		{
			using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHANNEL_NOTIFY))
			{
				packet.WriteByte((byte)ChannelNotification.WrongPassword);
				packet.WriteCString(chan);

				client.Send(packet);
			}
		}

		/// <summary>
		/// Send the "wrong password" reply
		/// </summary>
		/// <param name="client">the client the outdoing packet belongs to</param>
		/// <param name="chan">name of channel</param>
		public static void SendWrongFaction(IPacketReceiver client, string chan, string invitedName)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHANNEL_NOTIFY))
			{
				packet.WriteByte((byte)ChannelNotification.InviteWrongFaction);
				packet.WriteCString(chan);
				packet.WriteCString(invitedName);

				client.Send(packet);
			}
		}

		/// <summary>
		/// Send the "you have joined channel" reply
		/// </summary>
		/// <param name="client">the client the outdoing packet belongs to</param>
		/// <param name="chan">name of channel</param>
		public static void SendYouJoinedReply(IPacketReceiver client, ChatChannel chan)
		{
			using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHANNEL_NOTIFY))
			{
				packet.WriteByte((byte)ChannelNotification.YouJoined);
				packet.WriteCString(chan.Name);
                packet.Write((byte)chan.Flags);
                packet.WriteUInt(chan.ChannelId);
				packet.WriteUInt(0);

				client.Send(packet);
			}
		}

		/// <summary>
		/// Send the "you are not on the channel" reply
		/// </summary>
		/// <param name="client">the client the outdoing packet belongs to</param>
		/// <param name="chan">name of channel</param>
		public static void SendNotOnChannelReply(IPacketReceiver client, string chan)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHANNEL_NOTIFY))
			{
				packet.WriteByte((byte)ChannelNotification.NotOnChannel);
				packet.WriteCString(chan);

				client.Send(packet);
			}
		}

		/// <summary>
		/// Send the "name is not on the channel" reply
		/// </summary>
		/// <param name="client">the client the outdoing packet belongs to</param>
		/// <param name="chan">name of channel</param>
		public static void SendTargetNotOnChannelReply(IPacketReceiver client, string chan, string playerName)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHANNEL_NOTIFY))
			{
				packet.WriteByte((byte)ChannelNotification.PlayerNotOnChannel);
				packet.WriteCString(chan);
				packet.WriteCString(playerName);

				client.Send(packet);
			}
		}

		/// <summary>
		/// Send the "you have left the channel" reply
		/// </summary>
		/// <param name="client">the client the outdoing packet belongs to</param>
		/// <param name="chan">name of channel</param>
		/// <param name="channelId">Id of official channel in client (or 0 for custom channels)</param>
		public static void SendYouLeftChannelReply(IPacketReceiver client, string chan, int channelId)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHANNEL_NOTIFY))
			{
				packet.WriteByte((byte)ChannelNotification.YouLeft);
				packet.WriteCString(chan);
				packet.WriteUInt(channelId);
				packet.WriteByte(0);

				client.Send(packet);
			}
		}

		/// <summary>
		/// Send the "you are not a moderator" reply
		/// </summary>
		/// <param name="client">the client the outdoing packet belongs to</param>
		/// <param name="chan">name of channel</param>
		public static void SendNotModeratorReply(IPacketReceiver client, string chan)
		{
			using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHANNEL_NOTIFY))
			{
				packet.WriteByte((byte)ChannelNotification.NotModerator);
				//packet.WriteCString(chan);

				client.Send(packet);
			}
		}

		/// <summary>
		/// Send the "you have invited ... to channel" reply
		/// </summary>
		/// <param name="client">the client the outdoing packet belongs to</param>
		/// <param name="chan">name of channel</param>
		public static void SendYouInvitedReply(IPacketReceiver client, string chan, string invitedName)
		{
			using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHANNEL_NOTIFY))
			{
				packet.WriteByte((byte)ChannelNotification.HaveInvitedToChannel);
				packet.WriteCString(chan);
				packet.WriteCString(invitedName);

				client.Send(packet);
			}
		}

		/// <summary>
		/// Send the "... has invited you to channel" message
		/// </summary>
		/// <param name="client">the client the outdoing packet belongs to</param>
		/// <param name="chan">name of channel</param>
		/// <param name="sender">entityid of sender</param>
		public static void SendInvitedMessage(IPacketReceiver client, string chan, EntityId sender)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHANNEL_NOTIFY))
			{
				packet.WriteByte((byte)ChannelNotification.BeenInvitedToChannel);
				packet.WriteCString(chan);
				packet.Write(sender);

				client.Send(packet);
			}
		}

		/// <summary>
		/// Send the "you are not an owner" reply
		/// </summary>
		/// <param name="client">the client the outdoing packet belongs to</param>
		/// <param name="chan">name of channel</param>
		public static void SendNotOwnerReply(IPacketReceiver client, string chan)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHANNEL_NOTIFY))
			{
				packet.WriteByte((byte)ChannelNotification.NotOwner);
				packet.WriteCString(chan);

				client.Send(packet);
			}
		}

		/// <summary>
		/// Send the "you are muted" reply
		/// </summary>
		/// <param name="client">the client the outdoing packet belongs to</param>
		/// <param name="chan">name of channel</param>
		public static void SendMutedReply(IPacketReceiver client, string chan)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHANNEL_NOTIFY))
			{
				packet.WriteByte((byte)ChannelNotification.YouAreMuted);
				packet.WriteCString(chan);

				client.Send(packet);
			}
		}

		/// <summary>
		/// Send the name of current of the channel
		/// </summary>
		/// <param name="client">the client the outdoing packet belongs to</param>
		/// <param name="chan">the channel</param>
		public static void SendCurrentOwner(IPacketReceiver client, ChatChannel chan)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHANNEL_NOTIFY))
			{
				packet.WriteByte((byte)ChannelNotification.CurrentOwner);
				packet.WriteCString(chan.Name);

				if (chan.IsConstant || chan.Owner == null)
				{
					packet.WriteCString("Nobody");
				}
				else
				{
					packet.WriteCString(chan.Owner.User.Name);
				}

				client.Send(packet);
			}
		}

		/// <summary>
		/// Send the list of channel members
		/// </summary>
		/// <param name="client">the client the outdoing packet belongs to</param>
		/// <param name="chan">channel to be listed</param>
		public static void SendChannelList(IPacketReceiver client, ChatChannel chan)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHANNEL_LIST))
			{
				packet.Write((byte)0x01); // unk - 0x03?
                packet.WriteCString(chan.Name);
                packet.Write((byte)chan.Flags);

                // Note: InsertIntAt might be used here later if we want
                // to do some sort of "GM shouldn't be included" check.
                packet.WriteUInt(chan.Members.Count);
				foreach (var mi in chan.Members.Values)
				{
					packet.Write(mi.User.EntityId);
                    packet.Write((byte)mi.Flags);
				}

				client.Send(packet);
			}
		}

		/// <summary>
		/// Send the "name has joined channel" reply to everyone
		/// </summary>
		/// <param name="chan">name of channel</param>
		/// <param name="sender">sender (to check the ignore list)</param>
		public static void SendJoinedReplyToEveryone(ChatChannel chan, ChannelMember sender)
		{
            // the packet has the same size no matter the opcode, so it's really
            // a retarded way of doing it... but blame blizz.
            var opcode = chan.IsConstant ? RealmServerOpCode.SMSG_USERLIST_ADD
                : RealmServerOpCode.SMSG_USERLIST_UPDATE;

			using (RealmPacketOut packet = new RealmPacketOut(opcode))
			{
				//packet.WriteByte((byte)ChannelNotification.PlayerJoined);
				packet.Write(sender.User.EntityId);
                packet.Write((byte)sender.Flags);
                packet.Write((byte)chan.ClientFlags);
                packet.WriteUInt(chan.MemberCount);
                packet.WriteCString(chan.Name);

				SendPacketToChannel(chan, packet, sender.User.EntityId);
			}
		}

		/// <summary>
		/// Send the "name has left channel" reply to everyone
		/// </summary>
		/// <param name="chan">name of channel</param>
		/// <param name="sender">sender (to check the ignore list)</param>
		public static void SendLeftReplyToEveryone(ChatChannel chan, EntityId sender)
		{
			using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHANNEL_NOTIFY))
			{
				packet.WriteByte((byte)ChannelNotification.PlayerLeft);
				packet.WriteCString(chan.Name);
				packet.Write(sender);

				SendPacketToChannel(chan, packet, sender);
			}
		}

		/// <summary>
		/// Send owner changed message to everyone
		/// </summary>
		/// <param name="chan">name of channel</param>
		/// <param name="sender">sender (to check the ignore list)</param>
		/// <param name="newOwner">new owner</param>
		public static void SendOwnerChangedToEveryone(ChatChannel chan, EntityId sender, EntityId newOwner)
		{
			using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHANNEL_NOTIFY))
			{
				packet.WriteByte((byte)ChannelNotification.OwnerChanged);
				packet.WriteCString(chan.Name);
				packet.Write(newOwner);

				SendPacketToChannel(chan, packet, sender);
			}
		}

		/// <summary>
		/// Send the password changed message to everyone
		/// </summary>
		/// <param name="chan">name of channel</param>
		/// <param name="sender">sender (to check the ignore list)</param>
		public static void SendPasswordChangedToEveryone(ChatChannel chan, EntityId sender)
		{
			using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHANNEL_NOTIFY))
			{
				packet.WriteByte((byte)ChannelNotification.PasswordChanged);
				packet.WriteCString(chan.Name);
				packet.Write(sender);

				SendPacketToChannel(chan, packet, sender);
			}
		}

		/// <summary>
		/// Send the ban message to everyone
		/// </summary>
		/// <param name="chan">name of channel</param>
		/// <param name="sender">sender (aka banner)</param>
		/// <param name="banned">banned</param>
		public static void SendBannedToEveryone(ChatChannel chan, EntityId sender, EntityId banned)
		{
			using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHANNEL_NOTIFY))
			{
				packet.WriteByte((byte)ChannelNotification.Banned);
				packet.WriteCString(chan.Name);
				packet.Write(sender);
				packet.Write(banned);

				SendPacketToChannel(chan, packet, sender);
			}
		}

		/// <summary>
		/// Send the kick message to everyone
		/// </summary>
		/// <param name="chan">name of channel</param>
		/// <param name="sender">sender (aka kicker)</param>
		/// <param name="kicked">kicked</param>
		public static void SendKickedToEveryone(ChatChannel chan, EntityId sender, EntityId kicked)
		{
			using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHANNEL_NOTIFY))
			{
				packet.WriteByte((byte)ChannelNotification.Kicked);
				packet.WriteCString(chan.Name);
				packet.Write(sender);
				packet.Write(kicked);

				SendPacketToChannel(chan, packet, sender);
			}
		}

		/// <summary>
		/// Send the unbanned message to everyone
		/// </summary>
		/// <param name="chan">name of channel</param>
		/// <param name="sender">sender (aka unbanner)</param>
		/// <param name="unbanned">unbanned</param>
		public static void SendUnbannedToEveryone(ChatChannel chan, EntityId sender, EntityId unbanned)
		{
			using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHANNEL_NOTIFY))
			{
				packet.WriteByte((byte)ChannelNotification.Unbanned);
				packet.WriteCString(chan.Name);
				packet.Write(sender);
				packet.Write(unbanned);

				SendPacketToChannel(chan, packet, sender);
			}
		}

		/// <summary>
		/// Send the moderate status change message to everyone
		/// </summary>
		/// <param name="chan">name of channel</param>
		/// <param name="sender">sender (status changer)</param>
		public static void SendModerateToEveryone(ChatChannel chan, EntityId sender)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHANNEL_NOTIFY))
			{
				if (chan.IsModerated)
				{
					packet.WriteByte((byte)ChannelNotification.Moderated);
				}
				else
				{
					packet.WriteByte((byte)ChannelNotification.NotModerated);
				}

				packet.WriteCString(chan.Name);
				packet.Write(sender);

				SendPacketToChannel(chan, packet, sender);
			}
		}

		/// <summary>
		/// Send the announce status change message to everyone
		/// </summary>
		/// <param name="chan">name of channel</param>
		/// <param name="sender">sender (status changer)</param>
		/// <param name="newStatus">new announcements status</param>
		public static void SendAnnouncementToEveryone(ChatChannel chan, EntityId sender, bool newStatus)
		{
			using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHANNEL_NOTIFY))
			{
				if (newStatus)
				{
					packet.WriteByte((byte)ChannelNotification.Announcing);
				}
				else
				{
					packet.WriteByte((byte)ChannelNotification.NotAnnouncing);
				}

				packet.WriteCString(chan.Name);
				packet.Write(sender);

				SendPacketToChannel(chan, packet, sender);
			}
		}

		/// <summary>
		/// Send the message about change of moderator status of player to everyone
		/// </summary>
		/// <param name="chan">name of channel</param>
		/// <param name="sender">sender (to check the ignore list)</param>
		/// <param name="target">one who has changed his status</param>
		public static void SendModeratorStatusToEveryone(ChatChannel chan, EntityId sender, EntityId target, bool newStatus)
		{
			using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHANNEL_NOTIFY))
			{
				packet.WriteByte((byte)ChannelNotification.Moderator);
				packet.WriteCString(chan.Name);
				packet.Write(target);

				if (!newStatus)
				{
					packet.WriteByte(0x02);
					packet.WriteByte(0x00);
				}
				else
				{
					packet.WriteByte(0x00);
					packet.WriteByte(0x02);
				}

				SendPacketToChannel(chan, packet, sender);
			}
		}

		/// <summary>
		/// Send the message about change of muted status of player to everyone
		/// </summary>
		/// <param name="chan">name of channel</param>
		/// <param name="sender">sender (to check the ignore list)</param>
		/// <param name="target">one who has changed his status</param>
		/// <param name="newStatus">the new status</param>
		public static void SendMuteStatusToEveryone(ChatChannel chan, EntityId sender, EntityId target, bool newStatus)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHANNEL_NOTIFY))
			{
				packet.WriteByte((byte)ChannelNotification.Mute);
				packet.WriteCString(chan.Name);
				packet.Write(target);

				if (!newStatus)
				{
					packet.WriteByte(0x04);
					packet.WriteByte(0x00);
				}
				else
				{
					packet.WriteByte(0x00);
					packet.WriteByte(0x04);
				}

				SendPacketToChannel(chan, packet, sender);
			}
		}

		/// <summary>
		/// Send a packet to everyone on a channel.
		/// </summary>
		/// <param name="chan">the channel to which the packet is sent</param>
		/// <param name="packet">the packet to send</param>
		public static void SendPacketToChannel(ChatChannel chan, RealmPacketOut packet)
		{
			foreach (var member in chan.Members.Values)
			{
				member.User.Send(packet);
			}
		}

		/// <summary>
		/// Send a packet to everyone on a channel that does not ignore the given sender.
		/// </summary>
		/// <param name="chan">the channel to which the packet is sent</param>
		/// <param name="packet">the packet to send</param>
		/// <param name="sender">sender (to check the ignore list)</param>
		public static void SendPacketToChannel(ChatChannel chan, RealmPacketOut packet, EntityId sender)
		{
			foreach (var member in chan.Members.Values)
			{
				if (!RelationMgr.Instance.HasRelation(member.User.EntityId.Low, sender.Low, CharacterRelationType.Ignored))
				{
					member.User.Send(packet);
				}
			}
		}

		/// <summary>
		/// Send a reply to the number of members request
		/// </summary>
		/// <param name="client">the client the outdoing packet belongs to</param>
		public static void SendMemberCountReply(IPacketReceiver client, string channelName, byte channelFlags, uint memberCount)
		{
			using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHANNEL_MEMBER_COUNT))
			{
				packet.WriteCString(channelName);
				packet.WriteByte(channelFlags);
				packet.WriteUInt(memberCount);

				client.Send(packet);
			}
		}

		#endregion
	}
}