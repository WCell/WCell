/*************************************************************************
 *
 *   file			: Channel.cs
 *   copyright		: (C) The WCell Team
 *   email			: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-06-30 21:56:12 +0800 (Mon, 30 Jun 2008) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 548 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.Collections.Generic;
using System.Linq;
using WCell.Util.Collections;
using WCell.Constants;
using WCell.Constants.Chat;
using WCell.Constants.Misc;
using WCell.Core;
using WCell.RealmServer.Commands;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Chat
{
	///<summary>
	/// TODO: Implement Channel ids correctly: Send channel id and hope its used by the client to lookup the correct name locally
	/// TODO: Have Zone channels stored in a list per ZoneInfo object and updated correctly
	/// TODO: Cleanup/move special handling to seperate handlers 
	/// TODO: Get rid of ImmutableDictionary and instead put channel-specific stuff into its own message-loop
	/// 
	/// The chat channel class
	///</summary>
	public class ChatChannel : IChatTarget
	{
		/// <summary>
		/// Delegate for join validators.
		/// </summary>
		/// <param name="chatChannel">the channel being joined</param>
		/// <param name="user">the user joining the channel</param>
		/// <returns>whether or not the user can join the channel</returns>
		public delegate bool JoinValidationHandler(ChatChannel chatChannel, IUser user);

		/// <summary>
		/// Default validator for staff-only channels.
		/// </summary>
		public static readonly JoinValidationHandler StaffValidator = (chan, user) => user.Role.IsStaff;

		/// <summary>
		/// Default staff-only channel.
		/// </summary>
		public static readonly ChatChannel Staff = new ChatChannel(ChatChannelGroup.Global, "Staff",
			ChatChannelFlags.Global, true, StaffValidator);

	    private string m_name;
		private ChannelMember m_owner;
	    private ChatChannelFlagsEntry m_flagsEntry;
		private readonly ChatChannelGroup m_group;
		private readonly ImmutableDictionary<uint, ChannelMember> m_members;
		private readonly List<uint> m_bannedEntities;
		private JoinValidationHandler m_joinValidator;

		public readonly int ChannelId;

		/// <summary>
		/// Default Ctor
		/// </summary>
		public ChatChannel(ChatChannelGroup group)
		{
			m_group = group;
			m_members = new ImmutableDictionary<uint, ChannelMember>();

			Name = Password = string.Empty;
			Announces = true;
			m_bannedEntities = new List<uint>();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">name of channel</param>
		public ChatChannel(ChatChannelGroup group, string name)
			: this(group)
		{
			Name = name;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">name of channel</param>
		public ChatChannel(ChatChannelGroup group, string name, ChatChannelFlagsEntry flags, bool constant, JoinValidationHandler joinValidator)
			: this(group, name)
		{
			m_flagsEntry = flags;
			m_joinValidator = joinValidator;
			IsConstant = constant;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">name of channel</param>
		public ChatChannel(ChatChannelGroup group, string name, ChatChannelFlags flags, bool constant, JoinValidationHandler joinValidator)
			: this(group, name)
		{
			m_flagsEntry = new ChatChannelFlagsEntry
			{
				Flags = flags,
				ClientFlags = ChatMgr.Convert(flags)
			};
			m_joinValidator = joinValidator;
			IsConstant = constant;
		}

		public ChatChannel(ChatChannelGroup group, uint channelId, string name)
			: this(group, name)
		{
			ChatChannelGroup.DefaultChannelFlags.TryGetValue(channelId, out m_flagsEntry);
		}

		public ChatChannel(ChatChannelGroup group, string name, ChatChannelFlags flags, bool constant)
			: this(group, name)
		{
			m_flagsEntry = new ChatChannelFlagsEntry
			{
				Flags = flags,
				ClientFlags = ChatMgr.Convert(flags)
			};
			IsConstant = constant;
		}

		#region Properties

		/// <summary>
		/// Gets the chat channel manager that this channel resides under.
		/// </summary>
		public ChatChannelGroup Manager
		{
			get { return m_group; }
		}

		/// <summary>
		/// The join validator for this channel.
		/// </summary>
		public JoinValidationHandler JoinValidator
		{
			get { return m_joinValidator; }
		}

		/// <summary>
		/// A map of all users in the channel, sorted by low entity ID.
		/// </summary>
		public IDictionary<uint, ChannelMember> Members
		{
			get { return m_members; }
		}

		/// <summary>
		/// Gets or sets the owner of this channel.
		/// </summary>
		public ChannelMember Owner
		{
			get { return m_owner; }
			set
			{
				if (m_owner != null)
				{
					m_owner.IsOwner = false;
				}
				if (value != null)
				{
					m_owner = value;
					m_owner.Flags = ChannelMemberFlags.Moderator | ChannelMemberFlags.Owner | ChannelMemberFlags.Voiced;
				}
			}
		}

		/// <summary>
		/// Gets the name of the channel.
		/// </summary>
		public string Name
		{
			get { return m_name; }
			set { m_name = value; }
		}

	    /// <summary>
	    /// Gets or sets the password for the channel.
	    /// </summary>
	    public string Password { get; set; }

	    /// <summary>
		/// Determines whether or not joins/parts of the channel 
		/// are announced to all the channel users.
		/// </summary>
		public bool Announces
		{
			get;
			set;
		}

	    /// <summary>
	    /// Whether or not the channel is constant. That is, General/LFG/Guild Rec., etc.
	    /// </summary>
	    public bool IsConstant { get; set; }

	    /// <summary>
	    /// Whether or not the channel is moderated.
	    /// </summary>
	    public bool IsModerated { get; set; }

	    public ChatChannelFlags Flags
		{
			get { return m_flagsEntry.Flags; }
		}

		public ChatChannelFlagsClient ClientFlags
		{
			get { return m_flagsEntry.ClientFlags; }
		}

		public List<uint> BannedEntities
		{
			get { return m_bannedEntities; }
		}

		public int MemberCount
		{
			get { return m_members.Count; }
		}

		public int RequiredRank
		{
			get;
			set;
		}

		public bool IsCityOnly
		{
			get { return m_flagsEntry.Flags.HasFlag(ChatChannelFlags.CityOnly); }
		}

		public bool IsZoneSpecific
		{
			get { return m_flagsEntry.Flags.HasFlag(ChatChannelFlags.ZoneSpecific); }
		}

		/// <summary>
		/// TODO: Looking for Group
		/// </summary>
		public bool IsLFG
		{
            get { return m_flagsEntry.Flags.HasFlag(ChatChannelFlags.LookingForGroup); }
		}

		/// <summary>
		/// TODO: Trade
		/// </summary>
		public bool IsTrade
		{
            get { return m_flagsEntry.Flags.HasFlag(ChatChannelFlags.Trade); }
		}

		/// <summary>
		/// TODO: Guild Recruitment
		/// </summary>
		public bool RequiresUnguilded
		{
			get { return m_flagsEntry.Flags.HasFlag(ChatChannelFlags.RequiresUnguilded); }
		}

		//public uint ChannelId
		//{
		//    get { return m_channelId; }
		//}

		#endregion

		#region Methods

		public void ToggleModerated(ChannelMember toggler)
		{
			if (toggler.IsOwner || toggler.User.Role.IsStaff)
			{
				IsModerated = !IsModerated;
				ChannelHandler.SendModerateToEveryone(this, toggler.User.EntityId);
			}
			else
			{
				ChannelHandler.SendNotOwnerReply(toggler.User, m_name);
			}
		}

		/// <summary>
		/// Whether the given Entity-id is present in this channel
		/// </summary>
		/// <param name="lowId">the player to look for</param>
		/// <returns>true if the player is in this channel; false otherwise</returns>
		public bool IsPresent(uint lowId)
		{
			return m_members.ContainsKey(lowId);
		}

		/// <summary>
		/// Whether the given Entity-id is present in this channel
		/// </summary>
		/// <returns>true if the player is in this channel; false otherwise</returns>
		public bool EnsurePresence(IUser user, out ChannelMember member)
		{
			if (!m_members.TryGetValue(user.EntityId.Low, out member))
			{
				ChannelHandler.SendNotOnChannelReply(user, m_name);
				member = null;
				return false;
			}
			return true;
		}

		/// <summary>
		/// Adds a player to this channel.
		/// </summary>
		public void TryJoin(IUser chatter)
		{
			TryJoin(chatter, null, false);
		}

		/// <summary>
		/// Adds a player to this channel.
		/// </summary>
		public void TryJoin(IUser user, string password, bool silent)
		{
			if (IsTrade)
			{
				return;
			}

			if (RequiresUnguilded)
			{
				return;
			}

			if (IsLFG)
			{
				// TODO: Add looking-for-group support
				// don't join since they aren't lfg, since we have no lfg stuff!
				return;
			}

			var lowId = user.EntityId.Low;
			if (IsBanned(lowId) || (m_joinValidator != null && !m_joinValidator(this, user)))
			{
				ChannelHandler.SendBannedReply(user, Name);
				return;
			}

			if (IsPresent(lowId))
			{
				if (!IsConstant)
				{
					ChannelHandler.SendAlreadyOnChannelReply(user, Name, user.EntityId);
				}
				return;
			}

			if (Password.Length > 0 && Password != password)
			{
				ChannelHandler.SendWrongPassReply(user, Name);
				return;
			}

			ChannelHandler.SendYouJoinedReply(user, this);

			var member = user.Role.IsStaff ? new ChannelMember(user, ChannelMemberFlags.Moderator) : new ChannelMember(user);

            if (Announces)
            {
                ChannelHandler.SendJoinedReplyToEveryone(this, member);
            }

			if (!IsConstant && m_owner == null)
			{
				Owner = member;
			}

			m_members.Add(lowId, member);
			user.ChatChannels.Add(this);
		}

		/// <summary>
		/// Removes a player from a channel.
		/// </summary>
		/// <param name="silent">whether or not to tell the player/channel they have left</param>
		public void Leave(IUser user, bool silent)
		{
			var lowId = user.EntityId.Low;

			ChannelMember member;
			if (!m_members.TryGetValue(lowId, out member))
			{
				if (!silent)
				{
					ChannelHandler.SendNotOnChannelReply(user, m_name);
				}
			}
			else
			{
				if (!silent)
				{
					ChannelHandler.SendYouLeftChannelReply(user, m_name, ChannelId);
				}

				OnUserLeft(member);
			}
		}

		private void OnUserLeft(ChannelMember member)
		{
			var changeOwner = member.IsOwner;

			member.User.ChatChannels.Remove(this);
			m_members.Remove(member.User.EntityId.Low);

			if (changeOwner)
			{
				member = m_members.Values.FirstOrDefault();
				if (member != null)
				{
					if (Announces)
					{
						ChannelHandler.SendLeftReplyToEveryone(this, member.User.EntityId);
					}
					Owner = member;
				}
				else
				{
					m_group.DeleteChannel(this);
					m_owner = null;
				}
			}
		}

		/// <summary>
		/// Deletes this channel
		/// </summary>
		public void Delete()
		{
			foreach (var member in m_members.Values)
			{
				m_owner.IsOwner = false;
				Leave(member.User, false);
				m_group.DeleteChannel(this);
			}
		}

		/// <summary>
		/// Kicks and -maybe- bans a player from the channel.
		/// </summary>
		/// <returns>Whether the player was found and kicked or false if not found or privs were insufficient.</returns>
		public bool Kick(ChannelMember member, string targetName)
		{
			if (!string.IsNullOrEmpty(targetName))
			{
				var target = World.GetNamedEntity(targetName, true) as IUser;
				if (target != null)
				{
					return Kick(member, target);
				}
			}
			return false;
		}

		/// <summary>
		/// Kicks and -maybe- bans a player from the channel.
		/// </summary>
		/// <returns>Whether the player was kicked or false if privs were insufficient.</returns>
		public bool Kick(ChannelMember member, IUser targetUser)
		{
			var user = member.User;
			ChannelMember targetMember;
			if (!EnsurePresence(targetUser, out targetMember))
			{
				ChannelHandler.SendTargetNotOnChannelReply(user, m_name, targetUser.Name);
			}
			else
			{
				var targetEntitiyId = targetUser.EntityId;
				if (CheckPrivs(member, targetMember, false))
				{
					ChannelHandler.SendKickedToEveryone(this, user.EntityId, targetEntitiyId);
				}

				m_members.Remove(targetEntitiyId.Low);
				OnUserLeft(targetMember);
				return true;
			}
			return false;
		}

		#endregion

		#region IChatTarget
		public EntityId EntityId
		{
			get
			{
				return EntityId.Zero;
			}
		}

		public IEnumerable<IUser> GetUsers()
		{
			foreach (var memberInfo in m_members.Values)
			{
				yield return memberInfo.User;
			}
		}

		public void Send(RealmPacketOut packet)
		{
			foreach (var user in GetUsers())
			{
				user.Send(packet);
			}
		}
		#endregion

		#region Manage Privs
		public bool MakeOwner(ChannelMember oldOwner, ChannelMember newOwner)
		{
			if (!newOwner.IsOwner)
			{
				if (!oldOwner.IsOwner && !oldOwner.User.Role.IsStaff)
				{
					ChannelHandler.SendNotOwnerReply(oldOwner.User, m_name);
				}
				else
				{
					ChannelHandler.SendModeratorStatusToEveryone(this, oldOwner.User.EntityId, newOwner.User.EntityId, true);
					ChannelHandler.SendOwnerChangedToEveryone(this, oldOwner.User.EntityId, newOwner.User.EntityId);
					Owner = newOwner;
					return true;
				}
			}
			return false;
		}

		public bool SetModerator(ChannelMember mod, ChannelMember newMod, bool makeMod)
		{
			if (newMod.IsModerator != makeMod)
			{
				if (!mod.IsOwner && newMod.IsOwner && !mod.User.Role.IsStaff)
				{
					ChannelHandler.SendNotOwnerReply(mod.User, m_name);
				}
				else
				{
					newMod.IsModerator = makeMod;
					ChannelHandler.SendModeratorStatusToEveryone(this, mod.User.EntityId, newMod.User.EntityId, makeMod);
					return true;
				}
			}
			return false;
		}
		#endregion

		#region Muting
		public void SetMuted(ChannelMember member, ChannelMember targetMember, bool muted)
		{
			if (targetMember.IsMuted != muted && CheckPrivs(member, targetMember, !muted))
			{
				targetMember.IsMuted = muted;
				ChannelHandler.SendMuteStatusToEveryone(this, member.User.EntityId, targetMember.User.EntityId, muted);
			}
		}
		#endregion

		#region Banning
		/// <summary>
		/// Whether given Entity-id is banned from this channel
		/// </summary>
		/// <param name="lowId">the player to look for</param>
		/// <returns>true if the player is banned; false otherwise</returns>
		public bool IsBanned(uint lowId)
		{
			return m_bannedEntities.Contains(lowId);
		}

		public bool SetBanned(ChannelMember member, string targetName, bool addBan)
		{
			var target = World.GetNamedEntity(targetName, true) as IUser;
			if (target != null)
			{
				return SetBanned(member, target, false);
			}

		    ChatMgr.SendChatPlayerNotFoundReply(member.User, targetName);
		    return false;
		}

		/// <summary>
		/// Returns whether the Ban could be added/removed or false if privs were insufficient.
		/// </summary>
		/// <param name="member"></param>
		/// <param name="target"></param>
		/// <param name="addBan"></param>
		/// <returns></returns>
		public bool SetBanned(ChannelMember member, IUser target, bool addBan)
		{
			if (!member.IsModerator && !member.User.Role.IsStaff)
			{
				return false;
			}

			if (addBan)
			{
				ChannelMember targetMember;
				if (EnsurePresence(target, out targetMember))
				{
					// if target is on the channel
					if (targetMember > member)										// harmful actions require you to have higher privs than the target
					{
						if (targetMember.IsModerator)
						{
							ChannelHandler.SendNotOwnerReply(member.User, m_name);
						}
						return false;
					}
				}
			}

			if (addBan)
			{
				if (!m_bannedEntities.Contains(target.EntityId.Low))
				{
					ChannelHandler.SendTargetNotOnChannelReply(member.User, m_name, target.Name);
				}
				else
				{
					m_bannedEntities.Add(target.EntityId.Low);
					ChannelHandler.SendBannedToEveryone(this, member.User.EntityId, target.EntityId);
				}
			}
			else
			{
				if (!m_bannedEntities.Remove(target.EntityId.Low))
				{
					ChannelHandler.SendTargetNotOnChannelReply(member.User, m_name, target.Name);
				}
				else
				{
					ChannelHandler.SendUnbannedToEveryone(this, member.User.EntityId, target.EntityId);
				}
			}

			return true;
		}
		#endregion

		#region User Retrieval & Priv ensurance
		/// <summary>
		/// Ensures that the given user is on the channel and has mod status.
		/// If not, sends corresponding error messages to user.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="channelName"></param>
		/// <returns></returns>
		public static ChatChannel EnsureModerator(IUser user, string channelName)
		{
			var chan = ChatChannelGroup.RetrieveChannel(user, channelName);

			if (chan != null)
			{
				var chatterLowId = user.EntityId.Low;
				ChannelMember member;
				if (!chan.Members.TryGetValue(chatterLowId, out member))
				{
					ChannelHandler.SendNotOnChannelReply(user, channelName);
				}
				else
				{
					if (!member.IsModerator)
					{
						ChannelHandler.SendNotModeratorReply(user, channelName);
					}
					else
					{
						return chan;
					}
				}
				return null;
			}
			return null;
		}

		/// <summary>
		/// Retrieves the given ChannelMember and the Channel it is on. 
		/// If not, sends corresponding error messages to user and returns null.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="channelName"></param>
		/// <returns></returns>
		public static ChatChannel EnsurePresence(IUser user, string channelName, out ChannelMember member)
		{
			var chan = ChatChannelGroup.RetrieveChannel(user, channelName);

			if (chan != null)
			{
				var chatterLowId = user.EntityId.Low;
				if (!chan.Members.TryGetValue(chatterLowId, out member))
				{
					ChannelHandler.SendNotOnChannelReply(user, channelName);
				}
				else
				{
					return chan;
				}
				return null;
			}
			member = null;
			return null;
		}

		/// <summary>
		/// Retrieve the given two ChannelMembers and the Channel they are on, if they
		/// are all on the channel. If not, sends corresponding error messages to user.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="channelName"></param>
		/// <param name="targetName"></param>
		/// <param name="userMember"></param>
		/// <param name="targetMember"></param>
		/// <returns></returns>
		public static ChatChannel EnsurePresence(IUser user, string channelName, string targetName, out ChannelMember userMember, out ChannelMember targetMember)
		{
			if (!string.IsNullOrEmpty(targetName))
			{
				var chan = ChatChannelGroup.RetrieveChannel(user, channelName);
				if (chan != null)
				{
					var chatterLowId = user.EntityId.Low;
					if (!chan.Members.TryGetValue(chatterLowId, out userMember))
					{
						ChannelHandler.SendNotOnChannelReply(user, channelName);
					}
					else
					{
						var targetUser = World.GetNamedEntity(targetName, false) as IUser;
						if (targetUser == null || !chan.Members.TryGetValue(targetUser.EntityId.Low, out targetMember))
						{
							ChannelHandler.SendTargetNotOnChannelReply(user, channelName, targetName);
						}
						else if (targetUser != user)
						{
							return chan;
						}
					}
				}
			}
			userMember = null;
			targetMember = null;
			return null;
		}

		/// <summary>
		/// Checks for whether the given member can perform a beneficial or harmful action on the targetMember
		/// within this channel.
		/// </summary>
		/// <param name="member"></param>
		/// <param name="targetMember"></param>
		/// <param name="beneficial"></param>
		/// <returns></returns>
		public bool CheckPrivs(ChannelMember member, ChannelMember targetMember, bool beneficial)
		{
			if (beneficial)
			{
				if (!(member.IsModerator && !member.User.Role.IsStaff))			// beneficial actions can be done by any mod or staff member
				{
					ChannelHandler.SendNotModeratorReply(member.User, m_name);
					return false;
				}
			}
			else if (targetMember > member)										// harmful actions require you to have higher privs than the target
			{
				if (targetMember.IsModerator)
				{
					ChannelHandler.SendNotOwnerReply(member.User, m_name);
				}
				else
				{
					ChannelHandler.SendNotModeratorReply(member.User, m_name);
				}
				return false;
			}
			return true;
		}
		#endregion

		public void Invite(ChannelMember inviter, string targetName)
		{
			var target = World.GetNamedEntity(targetName, false) as IUser;
			if (target != null)
			{
				Invite(inviter, target);
			}
			else
			{
				ChatMgr.SendChatPlayerNotFoundReply(inviter.User, targetName);
			}
		}

		public void Invite(ChannelMember inviter, IUser target)
		{
			if (!target.IsIgnoring(inviter.User) || inviter.User.Role.IsStaff)
			{
				if (IsPresent(target.EntityId.Low))
				{
					ChannelHandler.SendAlreadyOnChannelReply(inviter.User, target.Name, target.EntityId);
				}
				else if (target.FactionGroup != inviter.User.FactionGroup)
				{
					ChannelHandler.SendWrongFaction(inviter.User, m_name, target.Name);
				}
				else
				{
					// TODO: all other checks (eg ban etc)
					ChannelHandler.SendInvitedMessage(target, m_name, inviter.User.EntityId);
					ChannelHandler.SendYouInvitedReply(inviter.User, m_name, target.Name);
				}
			}
			else
			{
				ChatMgr.SendChatPlayerNotFoundReply(inviter.User, target.Name);
			}
		}

		#region IPlayerChatTarget Members

		public void SendMessage(string message)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_MESSAGECHAT))
			{
				packet.Write((byte)ChatMsgType.Channel);
				packet.Write((uint)ChatLanguage.Common);
				packet.Write(EntityId.Zero);
				packet.WriteCString("");
				packet.Write(EntityId.Zero);
				packet.WriteUIntPascalString(message);
				packet.Write((byte)ChatTag.None); // chat tag

				ChannelHandler.SendPacketToChannel(this, packet);
			}

			ChatMgr.ChatNotify(null, message, ChatLanguage.Common, ChatMsgType.Channel, this);
		}

		/// <summary>
		/// Sends a message to this channel.
		/// </summary>
		/// <param name="sender">the chatter saying the message</param>
		public void SendMessage(IChatter sender, string message)
		{
		    ChannelMember mi;
			if (!Members.TryGetValue(sender.EntityId.Low, out mi))
			{
				ChannelHandler.SendNotOnChannelReply(sender, m_name);
				return;
			}

			if (mi.IsMuted)
			{
				ChannelHandler.SendMutedReply(sender, m_name);
				return;
			}

			bool isMod = mi.IsModerator;
			if (IsModerated && !isMod)
			{
				ChannelHandler.SendNotOnChannelReply(sender, m_name);
				return;
			}

			if (sender is IUser)
			{
				if (RealmCommandHandler.HandleCommand((IUser)sender, message, this))
					return;
			}

			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_MESSAGECHAT))
			{
				packet.Write((byte)ChatMsgType.Channel);
				packet.Write((uint)sender.SpokenLanguage);
				packet.Write(sender.EntityId);
				packet.Write(0);			// unknown
				packet.WriteCString(Name);
				packet.Write(sender.EntityId);
				packet.WriteUIntPascalString(message);
				packet.Write((byte)sender.ChatTag); // chat tag

				if (isMod)
				{
					ChannelHandler.SendPacketToChannel(this, packet);
				}
				else
				{
					ChannelHandler.SendPacketToChannel(this, packet, sender.EntityId);
				}

				ChatMgr.ChatNotify(sender, message, sender.SpokenLanguage, ChatMsgType.Channel, this);
			}
		}

		#endregion
	}
}