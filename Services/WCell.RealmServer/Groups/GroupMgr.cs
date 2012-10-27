/*************************************************************************
 *
 *   file		: GroupMgr.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-27 10:06:23 +0100 (on, 27 jan 2010) $

 *   revision		: $Rev: 1227 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.Collections.Generic;
using WCell.Constants;
using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Interaction;
using WCell.Util.Collections;

namespace WCell.RealmServer.Groups
{
	/// <summary>
	/// TODO: Group-Tracking (including buffs/debuffs)
	/// </summary>
	public sealed class GroupMgr : Manager<GroupMgr>
	{
		/// <summary>
		/// Maps char-id to the corresponding GroupMember object so it can be looked up when char reconnects
		/// </summary>
		internal readonly IDictionary<uint, GroupMember> OfflineChars;

		private GroupMgr()
		{
			OfflineChars = new SynchronizedDictionary<uint, GroupMember>(100);
		}

		#region Login/Logout
		/// <summary>
		/// Removes an offline Character with the given Id
		/// from his/her Group
		/// </summary>
		/// <param name="id"></param>
		public bool RemoveOfflineCharacter(uint id)
		{
			GroupMember member;
			if (OfflineChars.TryGetValue(id, out member))
			{
				member.LeaveGroup();
				return true;
			}
			return false;
		}

		internal void OnCharacterLogin(Character chr)
		{
			GroupMember member;
			if (OfflineChars.TryGetValue(chr.EntityId.Low, out member))
			{
				OfflineChars.Remove(chr.EntityId.Low);
				member.Character = chr;
				var group = member.Group;
				if (group.Leader == null)
				{
					// complete Group was offline, make a new Leader
					group.Leader = member;
				}
				else
				{
					group.SendUpdate();
				}
				chr.GroupUpdateFlags |= GroupUpdateFlags.Status;
			}
		}

		/// <summary>
		/// Cleanup character invitations and group leader, looter change on character logout/disconnect
		/// </summary>
		/// <param name="member">The GroupMember logging out / disconnecting (or null if the corresponding Character is not in a Group)</param>
		internal void OnCharacterLogout(GroupMember member)
		{
			if (member == null)
			{
				// null check is only required because we stated in the documentation of this method that memeber is allowed to be null
				return;
			}

			var chr = member.Character;
			var listInviters = RelationMgr.Instance.GetRelations(chr.EntityId.Low,
				CharacterRelationType.GroupInvite);

			foreach (GroupInviteRelation inviteRelation in listInviters)
			{
				RelationMgr.Instance.RemoveRelation(inviteRelation);
			}

			var group = member.Group;

			member.Position = member.Character.Position;
			member.Map = member.Character.Map;
			member.Character.GroupUpdateFlags |= GroupUpdateFlags.Status;
			member.Character = null;

			OfflineChars.Add(chr.EntityId.Low, member);

			group.SendUpdate();
		}
		#endregion

		#region Checks
		private static bool CheckIsLeader(GroupMember member)
		{
			if (!member.IsLeader)
			{
				// you dont have permission
				var chr = member.Character;
				if (chr != null)
				{
					GroupHandler.SendResult(chr.Client, GroupResultType.Invite, GroupResult.DontHavePermission);
				}
				return false;
			}
			return true;
		}

		private static bool CheckMemberInGroup(Character requester, Group group, uint memberLowId)
		{
			GroupMember member = group[memberLowId];

			if (member == null)
			{
				// you can't uninvite people not from your group
				GroupHandler.SendResult(requester.Client, GroupResultType.Leave, GroupResult.NotInYourParty);
				return false;
			}
			return true;
		}

		private static bool CheckSameGroup(Character requester, Character character)
		{
			if (requester.Group != character.Group)
			{
				GroupHandler.SendResult(requester.Client, GroupResult.NotInYourParty, character.Name);
				return false;
			}
			return true;
		}
		#endregion
	}

	public static class GroupUtil
	{
		#region Staff Management
		public static void EnsurePureStaffGroup(this Character chr)
		{
			var group = chr.Group;
			if (group == null) return;
			group.EnsurePureStaffGroup();
		}
		#endregion
	}
}