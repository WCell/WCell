/*************************************************************************
 *
 *   file		: RaidGroup.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-27 10:06:23 +0100 (on, 27 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1227 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Groups
{
	public enum ReadyCheckStatus
	{
		NotReady = 0,
		Ready = 1,
	}

	/// <summary>
	/// Represents a raid group.
	/// </summary>
	public sealed class RaidGroup : Group
	{
		#region Fields

		public const byte MaxSubGroupCount = 8;
		private GroupMember m_readyCheckRequester;

		#endregion

		/// <summary>
		/// Creates a raid group with the given character as the leader.
		/// </summary>
		/// <param name="leader">the character to be the leader</param>
		public RaidGroup(Character leader)
			: base(leader, MaxSubGroupCount)
		{
			m_readyCheckRequester = null;
		}

		/// <summary>
		/// Creates a raid group from an existing party group.
		/// TODO: This looks wrong
		/// </summary>
		/// <param name="group">the group to convert</param>
		public RaidGroup(Group group)
			: this(group.Leader.Character)
		{
			foreach (var member in group)
			{
				if (member != m_firstMember)
				{
					AddMember(member.Character, false);
				}
			}
		}

		#region Properties

		/// <summary>
		/// The type of group.
		/// </summary>
		public override GroupFlags Flags
		{
			get { return GroupFlags.Raid; }
		}

		#endregion

		#region Method

		/// <summary>
		/// Sets whether or not the given player is an assistant.
		/// </summary>
		/// <param name="member">the member to set</param>
		/// <param name="isAssistant">whether or not the member is an assistant</param>
		public void SetAssistant(Character member, bool isAssistant)
		{
			GroupMember assistant = this[member.EntityId.Low];

			if (assistant != null)
			{
				if (isAssistant)
				{
					assistant.Flags |= GroupMemberFlags.Assistant;
				}
				else
				{
					assistant.Flags &= ~GroupMemberFlags.Assistant;
				}
			}
		}

		/// <summary>
		/// Sets whether or not the given player is the main assistant.
		/// </summary>
		/// <param name="member">the member to set</param>
		/// <param name="add">whether or not the member is an assistant</param>
		public void SetMainAssistant(Character member, bool add)
		{
			GroupMember mainAssistant = this[member.EntityId.Low];

			if (mainAssistant != null)
			{
				if (add)
				{
					mainAssistant.Flags |= GroupMemberFlags.MainAssistant;
				}
				else
				{
					mainAssistant.Flags &= ~GroupMemberFlags.MainAssistant;
				}
			}
		}

		/// <summary>
		/// Moves a member of the raid group into another subgroup.
		/// </summary>
		/// <param name="member">the member to move</param>
		/// <param name="group">the target subgroup</param>
		/// <returns>Whether the move was successful or false if the target group was full</returns>
		public bool MoveMember(GroupMember member, SubGroup group)
		{
			if (group.IsFull)
				return false;

			var oldGroup = member.SubGroup;
			oldGroup.m_members.Remove(member);
			group.AddMember(member);
			return true;
		}

		/// <summary>
		/// Sends a ready check request to the members of the raid group.
		/// </summary>
		/// <param name="member">the member who requested the check</param>
		public void SendReadyCheckRequest(GroupMember member)
		{
			m_readyCheckRequester = member;

			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_RAID_READY_CHECK))
			{
				packet.Write(EntityId.GetPlayerId(member.Id));

				SendAll(packet, member);
			}
		}

		/// <summary>
		/// Sends the response of a member to the original ready check requester.
		/// </summary>
		/// <param name="member">the responding member</param>
		/// <param name="status">their ready status</param>
		public void SendReadyCheckResponse(GroupMember member, ReadyCheckStatus status)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_RAID_READY_CHECK_CONFIRM))
			{
				packet.Write(EntityId.GetPlayerId(member.Id));
				packet.WriteByte((byte)status);

				Character checkRequester = World.GetCharacter(m_readyCheckRequester.Id);
				if (checkRequester != null)
					checkRequester.Client.Send(packet);
			}
		}

		#endregion
	}
}
