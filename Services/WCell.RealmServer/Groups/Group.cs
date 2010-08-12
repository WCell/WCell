/*************************************************************************
 *
 *   file		: Group.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-04-16 10:32:12 +0200 (fr, 16 apr 2010) $
 *   last author	: $LastChangedBy: XTZGZoReX $
 *   revision		: $Rev: 1278 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;
using NLog;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Instances;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;
using WCell.Util.NLog;
using WCell.RealmServer.Looting;
using WCell.Constants.Factions;
using WCell.Constants.Looting;
using WCell.Util.Threading;

namespace WCell.RealmServer.Groups
{
	/// <summary>
	/// Base group class for every group type.
	/// Don't forget to lock the SyncRoot while iterating over a Group.
	/// </summary>
	public abstract partial class Group : IInstanceHolderSet, ICharacterSet
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// The time after which a member who left a Group will be teleported out of a Group owned instance in seconds.
		/// </summary>
		public static int GroupInstanceKickDelayMillis = 60000;

		protected const byte MinGroupMemberCount = 2;
		protected const byte TargetIconCount = 8;

		protected SubGroup[] m_subGroups;
		protected GroupMember m_leader;
		protected GroupMember m_masterLooter, m_mainAssistant, m_mainTank;
		protected LootMethod m_lootMethod;
		protected ItemQuality m_lootThreshold;
		protected uint m_DungeonDifficulty;
		protected EntityId[] m_targetIcons;
		protected int m_Count;
		protected GroupMember m_roundRobinMember;

		internal protected GroupMember m_firstMember, m_lastMember;
		internal protected ReaderWriterLockSlim m_syncLock;

		#region Constructors
		protected Group(Character leader, byte maxGroupUnits)
		{
			// m_syncLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
			m_syncLock = new ReaderWriterLockSlim();
			m_subGroups = new SubGroup[maxGroupUnits];

			for (byte i = 0; i < maxGroupUnits; i++)
			{
				m_subGroups[i] = new SubGroup(this, i);
			}

			m_targetIcons = new EntityId[TargetIconCount];
			for (int i = 0; i < m_targetIcons.Length; i++)
				m_targetIcons[i] = EntityId.Zero;

			m_lootMethod = LootMethod.GroupLoot;
			m_lootThreshold = ItemQuality.Uncommon;
			m_DungeonDifficulty = 0;

			var member = AddMember(leader, false);
			Leader = member;
			m_masterLooter = null;
			// update will follow when the 2nd guy joins
		}
		#endregion

		#region Properties
		/// <summary>
		/// The chosen LootMethod.
		/// Make sure to send an Update after changing this, so the Group is informed about the change.
		/// </summary>
		public LootMethod LootMethod
		{
			get { return m_lootMethod; }
			set
			{
				if (value >= LootMethod.End)
				{
					m_lootMethod = LootMethod.GroupLoot;
				}
				else
				{
					m_lootMethod = value;
				}
			}
		}

		/// <summary>
		/// The least Quality of Items to be handled by the MasterLooter or to be rolled for.
		/// Make sure to send an Update after changing this, so the Group is informed about the change.
		/// </summary>
		public ItemQuality LootThreshold
		{
			get { return m_lootThreshold; }
			set
			{
				m_lootThreshold = value;
			}
		}

		/// <summary>
		/// The DungeonDifficulty.
		/// Make sure to send an Update after changing this, so the Group is informed about the change.
		/// </summary>
		public uint DungeonDifficulty
		{
			get { return m_DungeonDifficulty; }
			set
			{
				if (value != m_DungeonDifficulty)
				{
					m_DungeonDifficulty = value;
					if (!IsBattleGroup)
					{
						foreach (var member in GetCharacters())
						{
							if (Flags.HasFlag(GroupFlags.Raid))
							{
								InstanceHandler.SendDungeonDifficulty(member);
							}
							else
							{
								InstanceHandler.SendRaidDifficulty(member);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// All SubGroups of this Group. 
		/// By default there is only 1 SubGroup, except for RaidGroups which always have 8 (some might be empty)
		/// </summary>
		public SubGroup[] SubGroups
		{
			get { return m_subGroups; }
		}

		public bool IsLeader(Character chr)
		{
			var leader = m_leader;
			if (leader != null)
			{
				var role = chr.Role;
				if (role != null && role.IsStaff)
				{
					return true;
				}
				return leader.Character == chr;
			}
			return false;
		}

		/// <summary>
		/// The GroupMember who is leader of this Group.
		/// Is null if the Group has no online members.
		/// This will only ever be null if no one in the Group is online.
		/// </summary>
		public GroupMember Leader
		{
			get { return m_leader; }
			set
			{
				if (value != m_leader)
				{
					var oldLeader = m_leader;
					m_leader = value;
					if (value != null)
					{
						OnLeaderChanged(oldLeader);
					}
				}
			}
		}

		/// <summary>
		/// The GroupMember who is the looter of the group. Returns null if there isnt one.
		/// </summary>
		public GroupMember MasterLooter
		{
			get { return m_masterLooter; }
			set
			{
				if (value != m_masterLooter)
				{
					m_masterLooter = value;
					SendUpdate();
				}
			}
		}

		/// <summary>
		/// Returns true if this is a BattleGroup. //TODO: 
		/// </summary>
		public virtual bool IsBattleGroup
		{
			get { return false; }
		}

		/// <summary>
		/// The MainAssistant of this Group
		/// </summary>
		public GroupMember MainAssistant
		{
			get { return m_mainAssistant; }
			set
			{
				if (value != m_mainAssistant)
				{
					if (value != null)
					{
						value.Flags |= GroupMemberFlags.MainAssistant;
					}

					if (m_mainAssistant != null)
					{
						m_mainAssistant.Flags &= ~GroupMemberFlags.MainAssistant;
					}
					m_mainAssistant = value;
					SendUpdate();
				}
			}
		}

		/// <summary>
		/// The MainTank of this Group
		/// </summary>
		public GroupMember MainTank
		{
			get { return m_mainTank; }
			set
			{
				if (value != m_mainTank)
				{
					if (value != null)
					{
						value.Flags |= GroupMemberFlags.MainTank;
					}
					if (m_mainTank != null)
					{
						m_mainTank.Flags &= ~GroupMemberFlags.MainTank;
					}
					m_mainTank = value;
					SendUpdate();
				}
			}
		}

		/// <summary>
		/// Whether this Group is full 
		/// </summary>
		public bool IsFull
		{
			get
			{
				return m_Count >= MaxMemberCount;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public FactionGroup FactionGroup
		{
			get { return m_leader.Character.FactionGroup; }
		}

		/// <summary>
		/// WARNING: Does not ensure to execute within the Character's context
		/// </summary>
		public void ForeachCharacter(Action<Character> callback)
		{
			foreach (var chr in GetCharacters())
			{
				callback(chr);
			}
		}

		/// <summary>
		/// Executes the given callback for every online group member.
		/// Callback is called immediately on everyone who is in the current context and delayed for everyone who is not.
		/// Callback will only execute if character is still in group at the time of execution.
		/// Execution is not guaranteed!
		/// </summary>
		public void CallOnAllInContext(Action<Character> callback)
		{
			foreach (var chr in GetCharacters())
			{
				var c = chr;
				chr.ExecuteInContext(() =>
				{
					if (c.Group == this)
					{
						callback(c);
					}
				});
			}
		}

		/// <summary>
		/// Executes the given callback immediately for every online group member in the given context.
		/// IMPORTANT: Must be called from within context.
		/// </summary>
		public void CallOnAllInSameContext(IContextHandler context, Action<Character> callback)
		{
			context.EnsureContext();
			foreach (var chr in GetCharacters())
			{
				if (chr.ContextHandler == context)
				{
					callback(chr);
				}
			}
		}

		/// <summary>
		/// The amount of Members in this Group
		/// </summary>
		public int Count
		{
			get { return m_Count; }
		}


		/// <summary>
		/// The maximum amount of Members in this Group
		/// </summary>
		public int MaxMemberCount
		{
			get
			{
				return (SubGroups.Length * SubGroup.MaxMemberCount);
			}
		}

		/// <summary>
		/// The Member who joined this Group earliest, of everyone who is currently in this Group
		/// </summary>
		public GroupMember FirstMember
		{
			get
			{
				return m_firstMember;
			}
		}

		/// <summary>
		/// The Member who joined this Group last, of everyone who is currently in this Group
		/// </summary>
		public GroupMember LastMember
		{
			get
			{
				return m_lastMember;
			}
		}

		/// <summary>
		/// Free spots left in this group (= MaxCount - CurrentCount)
		/// </summary>
		public byte InvitesLeft
		{
			get
			{
				return (byte)(MaxMemberCount - Count);
			}
		}

		public abstract GroupFlags Flags
		{
			get;
		}

		/// <summary>
		/// The member whose turn it is in RoundRobin
		/// </summary>
		public GroupMember RoundRobinMember
		{
			get
			{
				return m_roundRobinMember;
			}
		}

		/// <summary>
		/// The SyncRoot against which to synchronize this group (when iterating over it or making certain changes)
		/// </summary>
		public ReaderWriterLockSlim SyncRoot
		{
			get
			{
				return m_syncLock;
			}
		}
		#endregion

		#region Group Management
		/// <summary>
		/// Add member to Group
		/// </summary>
		/// <param name="update">Indicates if this group needs to be updated after adding the 
		/// new member</param>
		/// <returns>True if the member was added successfully. False otherwise.</returns>
		public GroupMember AddMember(Character chr, bool update)
		{
			GroupMember newMember = null;

			m_syncLock.EnterWriteLock();

			try
			{
				// look for the first SubGroup with space left
				foreach (var groupUnit in m_subGroups)
				{
					if (!groupUnit.IsFull)
					{
						newMember = new GroupMember(chr, GroupMemberFlags.Normal);
						groupUnit.AddMember(newMember);
						break;
					}
				}
			}
			catch (Exception e)
			{
				LogUtil.ErrorException(e, string.Format("Could not add member {0} to group {1}", chr, this));
			}
			finally
			{
				m_syncLock.ExitWriteLock();
			}

			OnAddMember(newMember);
			if (newMember != null && update)
			{
				SendUpdate();
			}
			return newMember;
		}

		/// <summary>
		/// Remove member from this Group
		/// </summary>
		public virtual void RemoveMember(GroupMember member)
		{
			if (Count <= MinGroupMemberCount)
			{
				Disband();
			}
			else
			{
				m_syncLock.EnterWriteLock();

				var leaderChanged = false;
				try
				{
					var next = member.Next;
					OnMemberRemoved(member);


					// fix the links in the linked list:
					if (m_firstMember == member)
					{
						// set the new FirstMember if we removed the prevous one
						m_firstMember = next;
					}
					else
					{
						// set the previous GroupMember's Next member
						var previous = m_firstMember;
						while (previous.Next != member)
						{
							previous = previous.Next;
						}

						// previous should never be null here
						previous.Next = next;
						if (member == m_lastMember)
						{
							m_lastMember = previous;
						}
					}

					//Set new Leader if we removed the previous one
					if (m_leader == member)
					{
						m_leader = GetFirstOnlineMemberUnlocked();
						leaderChanged = true;
					}

					//Set new MasterLooter if we removed the previous one
					if (m_masterLooter == member)
					{
						m_masterLooter = m_firstMember;
					}

					//Unset MainAssistant if we removed the previous one
					if (m_mainAssistant == member)
					{
						member.Flags &= ~GroupMemberFlags.MainAssistant;

						m_mainAssistant = null;
					}

					//Unset MainTank if we removed the previous one
					if (m_mainTank == member)
					{
						member.Flags &= ~GroupMemberFlags.MainTank;

						m_mainTank = null;
					}

					// Skip the member in RoundRobin
					if (m_roundRobinMember == member)
					{
						m_roundRobinMember = next;
					}
				}
				finally
				{
					m_syncLock.ExitWriteLock();
				}

				if (leaderChanged)
				{
					// make sure to call this after the lock has been released
					OnLeaderChanged(member);
				}
			}

			SendUpdate();
		}

		public GroupMember GetFirstOnlineMember()
		{
			m_syncLock.EnterReadLock();
			try
			{
				return GetFirstOnlineMemberUnlocked();
			}
			finally
			{
				m_syncLock.ExitReadLock();
			}
		}

		internal GroupMember GetFirstOnlineMemberUnlocked()
		{
			var member = m_firstMember;

			while (member != null)
			{
				if (member.IsOnline)
				{
					return member;
				}
				member = member.Next;
			}
			return member;
		}

		/// <summary>
		/// Called when the given member is added
		/// </summary>
		protected virtual void OnAddMember(GroupMember member)
		{
			m_Count++;
			var chr = member.Character;
			if (chr != null)
			{
				chr.GroupMember = member;
			}

			if (m_firstMember == null)
			{
				// first member:
				m_firstMember = m_lastMember = member;
			}
			else
			{
				m_lastMember.Next = member;
				m_lastMember = member;
			}

			var handler = MemberAdded;
			if (handler != null)
			{
				handler(member);
			}
		}

		/// <summary>
		/// Called before the given member is removed to clean up everything related to the given member
		/// </summary>
		protected void OnMemberRemoved(GroupMember member)
		{
			var chr = member.Character;
			if (chr != null && chr.IsInWorld)
			{
				if (!chr.IsInContext)
				{
					chr.ExecuteInContext(() => OnMemberRemoved(member));
					return;
				}

				var handler = MemberRemoved;
				if (handler != null)
				{
					handler(member);
				}
				m_Count--;

				//Send an empty party list to the member
				SendEmptyUpdate(chr);

				chr.GroupMember = null;

				// notify user:
				// SendGroupDestroyed(chr);
				GroupHandler.SendResult(chr.Client, GroupResult.NoError);

				//Remove the member from the subgroup
				member.SubGroup.RemoveMember(member);

				member.Character = null;

				// Teleport out of group-owned instances within 1 minute
				if (chr.Region is BaseInstance)
				{
					var instance = (BaseInstance)chr.Region;
					chr.Region.CallDelayed(GroupInstanceKickDelayMillis, () =>
					{
						if (chr.IsInWorld && chr.Region == instance && !instance.CanEnter(chr))
						{
							// chr is still inside and not allowed
							//chr.Region.TeleportOutside(chr);
							chr.TeleportToNearestGraveyard();
						}
					});
				}
			}
			else
			{
				var handler = MemberRemoved;
				if (handler != null)
				{
					handler(member);
				}
				m_Count--;

				GroupMgr.Instance.OfflineChars.Remove(member.Id);
				member.m_subGroup = null;
			}
			member.m_nextMember = null;
		}

		private void OnLeaderChanged(GroupMember oldLeader)
		{
			// if everyone is offline, the Group has no leader
			if (m_leader != null)
			{
				GroupHandler.SendLeaderChanged(m_leader);
			}

			var evt = LeaderChanged;
			if (evt != null)
			{
				evt(oldLeader, m_leader);
			}
		}

		/// <summary>
		/// Disbands this Group
		/// </summary>
		public virtual void Disband()
		{
			m_syncLock.EnterWriteLock();

			try
			{
				foreach (var subGroup in m_subGroups)
				{
					foreach (var member in subGroup.Members)
					{
						var chr = member.Character;
						if (chr != null)
						{
							var m = member;
							//Send member left group result //TODO: Check the GroupType cuz this is different
							// SendResult(chr.Client, GroupResult.NoError);
							SendGroupDestroyed(chr);
						}
						OnMemberRemoved(member);
					}
				}
			}
			finally
			{
				m_syncLock.ExitWriteLock();
			}
		}

		public GroupMember this[uint lowMemberId]
		{
			get
			{
				m_syncLock.EnterReadLock();
				try
				{
					GroupMember member = null;

					for (var i = 0; i < m_subGroups.Length; i++)
					{
						var groupUnit = m_subGroups[i];
						member = groupUnit[lowMemberId];

						if (member != null)
							return member;
					}
				}
				finally
				{
					m_syncLock.ExitReadLock();
				}
				return null;
			}
		}

		public GroupMember this[string name]
		{
			get
			{
				m_syncLock.EnterReadLock();
				try
				{
					GroupMember member = null;

					foreach (SubGroup groupUnit in m_subGroups)
					{
						member = groupUnit[name];

						if (member != null)
							return member;
					}
				}
				finally
				{
					m_syncLock.ExitReadLock();
				}
				return null;
			}
		}
		#endregion

		#region Checks
		/// <summary>
		/// Check whether the given inviter may invite the given target
		/// </summary>
		public static GroupResult CheckInvite(Character inviter, out Character target, string targetName)
		{
			GroupResult err;
			var inviterMember = inviter.GroupMember;
			var group = inviterMember != null ? inviterMember.Group : null;

			if (group != null && group.IsFull)
			{
				// your group is full
				err = GroupResult.GroupIsFull;
				target = null;
				targetName = string.Empty;
			}
			else if ((inviterMember != null && !inviterMember.IsAtLeastAssistant))
			{
				target = null;
				err = GroupResult.DontHavePermission;
			}
			else
			{
				target = World.GetCharacter(targetName, false);
				if (target == null || inviter == target ||
					(target.Role.IsStaff && !inviter.Role.IsStaff))	// cannot invite staff members without authorization
				{
					// Character is offline or doesn't exist
					err = GroupResult.OfflineOrDoesntExist;
				}
				else if (inviter.Faction.Group != target.Faction.Group)
				{
					// you can't invite anyone from another faction
					err = GroupResult.TargetIsUnfriendly;
				}
				else if (target.Group != null || target.IsInvitedToGroup)
				{
					err = GroupResult.AlreadyInGroup;
				}
				else if (target.IsIgnoring(inviter) && !inviter.Role.IsStaff)
				{
					err = GroupResult.TargetIsIgnoringYou;
				}
				else
				{
					return GroupResult.NoError;
				}
			}

			SendResult(inviter.Client, err, 0, targetName);
			return err;
		}

		/// <summary>
		/// Checks whether the given target exists in this group and whether the given requestMember has the given privs
		/// </summary>
		public GroupResult CheckAction(GroupMember requestMember, GroupMember target, string targetName, GroupPrivs reqPrivs)
		{
			GroupResult err;
			if (target == null || (target.Group) != requestMember.Group)
			{
				// Character is offline or doesn't exist
				err = GroupResult.NotInYourParty;
			}
			else if ((reqPrivs == GroupPrivs.Leader && m_leader != requestMember) ||
				(reqPrivs == GroupPrivs.MainAsisstant && !requestMember.IsAtLeastMainAssistant) ||
				(reqPrivs == GroupPrivs.Assistant && !requestMember.IsAtLeastAssistant))
			{
				err = GroupResult.DontHavePermission;
				targetName = string.Empty;
			}
			else
			{
				return GroupResult.NoError;
			}

			var requester = requestMember.Character;
			if (requester != null)
			{
				SendResult(requester.Client, err, 0, targetName);
			}
			return err;
		}

		public bool CheckPrivs(GroupMember member, GroupPrivs reqPrivs)
		{
			//var flags = member.Flags;
			if ((reqPrivs == GroupPrivs.Leader && m_leader != member) ||
				(reqPrivs == GroupPrivs.MainAsisstant && !member.IsAtLeastMainAssistant) ||
				(reqPrivs == GroupPrivs.Assistant && !member.IsAtLeastAssistant))
			{
				var requester = member.Character;
				if (requester != null)
				{
					GroupHandler.SendResult(requester.Client, GroupResult.DontHavePermission);
				}
				return false;
			}
			return true;
		}

		public bool CheckFull(GroupMember member, SubGroup group)
		{
			if (group.IsFull)
			{
				var chr = member.Character;
				if (chr != null)
				{
					GroupHandler.SendResult(chr.Client, GroupResult.GroupIsFull);
				}
				return false;
			}
			return true;
		}
		#endregion

		#region Send Methods

		/// <summary>
		/// Send the Updated list of the group state to each group member
		/// </summary>
		public virtual void SendUpdate()
		{
			if (m_leader == null)
			{
				// group is not active
				return;
			}

			m_syncLock.EnterReadLock();

			try
			{
				Character chr;

				foreach (var groupUnit in m_subGroups)
				{
					foreach (var member in groupUnit.Members)
					{
						var maxLen = 35 + ((11 + CharacterHandler.MaxCharNameLength) * (Count - 1));
						using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GROUP_LIST, maxLen))
						{
							if ((chr = member.Character) == null)
								continue;

							packet.Write((byte)Flags);
							packet.Write(IsBattleGroup); //Is BG Group ?? : 0 == false, 1 == true;
							packet.Write(groupUnit.Id);
							packet.Write((byte)member.Flags);
							if (Flags.HasFlag(GroupFlags.LFD))		// since 3.3
							{
								packet.Write((byte)0);
								packet.Write(0);
							}
							packet.Write(0x50000000FFFFFFFEul);
							packet.Write(0);                        // since 3.3: Some kind of sequence id
							packet.Write(Count - 1);

							foreach (var memberSubGroup in m_subGroups)
							{
								foreach (var groupMember in memberSubGroup.Members)
								{
									if (member == groupMember)
										continue;

									packet.WriteCString(groupMember.Name);
									packet.Write(EntityId.GetPlayerId(groupMember.Id));

									if (groupMember.Character != null)
									{
										packet.Write((byte)CharacterStatus.ONLINE);
									}
									else
									{
										packet.Write((byte)CharacterStatus.OFFLINE);
									}

									packet.Write(memberSubGroup.Id);
									packet.Write((byte)groupMember.Flags);
									packet.Write((byte)0); // 3.3
								}
							}

							packet.Write(EntityId.GetPlayerId(Leader.Id));
							packet.Write((byte)LootMethod);

							if (MasterLooter != null)
							{
								packet.Write(EntityId.GetPlayerId(MasterLooter.Id).Full);
							}
							else
							{
								packet.Write(0L);
							}

							packet.Write((byte)LootThreshold);
							//packet.Write((byte)DungeonDifficulty); // normal
							packet.Write((byte)0);    // replace with ^
							packet.Write((byte)0);    // since 3.3: Raid difficulty
							packet.Write((byte)0);    // 3.3, dynamic difficulty?

							chr.Client.Send(packet);
						}
					}
				}
			}
			finally
			{
				m_syncLock.ExitReadLock();
			}
		}

		/// <summary>
		/// Send a packet to each group member
		/// </summary>
		/// <param name="packet">Realm Packet</param>
		public virtual void SendAll(RealmPacketOut packet)
		{
			SendAll(packet, null);
		}

		/// <summary>
		/// Send a packet to each group member except one specified
		/// </summary>
		/// <param name="packet">Realm Packet</param>
		/// <param name="ignored">Member that won't receive the message</param>
		protected virtual void SendAll(RealmPacketOut packet, GroupMember ignored)
		{
			foreach (var groupUnit in m_subGroups)
			{
				groupUnit.Send(packet, ignored);
			}
		}

		/// <summary>
		/// Send Empty Group List
		/// </summary>
		protected virtual void SendEmptyUpdate(Character chr)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GROUP_LIST))
			{
				//packet.ContentLength = 24;
				packet.Fill(0, 24);

				chr.Client.Send(packet);
			}
		}

		/// <summary>
		/// Send Group Uninvite packet
		/// </summary>
		public static void SendGroupUninvite(Character chr)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GROUP_UNINVITE, 0))
			{
				chr.Client.Send(packet);
			}
		}

		/// <summary>
		/// Send Party Disband Packet
		/// </summary>
		protected virtual void SendGroupDestroyed(Character chr)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GROUP_DESTROYED))
			{
				chr.Client.Send(packet);
			}
		}

		/// <summary>
		/// Sends result of actions connected with groups
		/// </summary>
		/// <param name="client">the client to send to</param>
		/// <param name="resultType">The result type</param>
		/// <param name="resultCode">The <see cref="GroupResult"/> result code</param>
		/// <param name="name">name of player event has happened to</param>
		public static void SendResult(IPacketReceiver client, GroupResult resultCode, uint resultType,
			string name)
		{
			// TODO: add enum for resultType
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PARTY_COMMAND_RESULT))
			{
				packet.Write(resultType);
				packet.WriteCString(name);
				packet.Write((uint)resultCode);
				packet.Write((uint)0); // 3.3.3, lfg cooldown?

				client.Send(packet);
			}
		}

		/// <summary>
		/// Sends result of actions connected with groups
		/// </summary>
		/// <param name="client">the client to send to</param>
		/// <param name="resultCode">The <see cref="GroupResult"/> result code</param>
		/// <param name="name">name of player event has happened to</param>
		public static void SendResult(IPacketReceiver client, GroupResult resultCode,
			string name)
		{
			SendResult(client, resultCode, 0, name);
		}

		/// <summary>
		/// Sends ping to the group, except pinger
		/// </summary>
		/// <param name="pinger">The group member who pingged the minimap</param>
		/// <param name="x">x coordinate of ping</param>
		/// <param name="y">y coordinate of ping</param>
		public virtual void SendPing(GroupMember pinger, float x, float y)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_MINIMAP_PING))
			{
				packet.Write(EntityId.GetPlayerId(pinger.Id));
				packet.WriteFloat(x);
				packet.WriteFloat(y);

				SendAll(packet, pinger);
			}
		}

		/// <summary>
		/// Sends roll results to the group
		/// </summary>
		/// <param name="min">minimal value</param>
		/// <param name="max">maximal value</param>
		/// <param name="roll">value rolled out</param>
		/// <param name="guid">guid of roller</param>
		public virtual void SendRoll(int min, int max, int roll, EntityId guid)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_RANDOM_ROLL))
			{
				packet.Write(min);
				packet.Write(max);
				packet.Write(roll);
				packet.Write(guid.Full);

				SendAll(packet);
			}
		}

		/// <summary>
		/// Sends all info about set icons to the client
		/// </summary>
		/// <param name="requester">The character requesting the target icon list</param>
		public virtual void SendTargetIconList(Character requester)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_RAID_TARGET_UPDATE))
			{
				packet.WriteByte(1); // flag, meaning that it's full list of target icons

				for (byte iconId = 0; iconId < TargetIconCount; iconId++)
				{
					if (m_targetIcons[iconId] == EntityId.Zero)
						continue;

					packet.WriteByte(iconId);
					packet.Write(m_targetIcons[iconId].Full);
				}

				if (requester != null)
				{
					requester.Client.Send(packet);
				}
				else
				{
					SendAll(packet);
				}
			}
		}

		/// <summary>
		/// Sends all info about set icons to the client
		/// </summary>
		public virtual void SendTargetIconList()
		{
			SendTargetIconList(null);
		}

		/// <summary>
		/// Sends info about change of single target icons info to all the party
		/// </summary>
		/// <param name="iconId">what element of array has changed</param>
		/// <param name="targetId">new value</param>
		public virtual void SetTargetIcon(byte iconId, EntityId whoId, EntityId targetId)
		{
			if (iconId >= TargetIconCount)
				return;

			bool requiresIconUpdate = ClearTargetIconList(iconId, targetId);

			m_targetIcons[iconId] = targetId;

			if (requiresIconUpdate)
			{
				SendTargetIconList();
				return;
			}

			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_RAID_TARGET_UPDATE))
			{
				packet.WriteByte(0); // flag, meaning that it's single target icon change
				// (i.e. don't need to reset client-side list before applying it)?

				packet.Write(whoId);        // 3.3.x
				packet.WriteByte(iconId);
				packet.Write(targetId.Full);

				SendAll(packet);
			}
		}

		private bool ClearTargetIconList(byte iconId, EntityId targetId)
		{
			bool cleared = false;

			m_syncLock.EnterReadLock();
			try
			{
				if (targetId != EntityId.Zero)
				{
					for (int i = 0; i < m_targetIcons.Length; i++)
					{
						if (m_targetIcons[i] == targetId)
						{
							if (iconId != i)
								cleared = true;

							m_targetIcons[i] = EntityId.Zero;
						}
					}
				}
			}
			finally
			{
				m_syncLock.ExitReadLock();
			}
			return cleared;
		}

		#endregion

		#region IChatTarget
		/// <summary>
		/// The name of the this ChatTarget
		/// </summary>
		public string Name
		{
			get
			{
				return string.Empty;
			}
		}

		/// <summary>
		/// The EntityId (only set for Owner)
		/// </summary>
		public EntityId EntityId
		{
			get
			{
				return EntityId.Zero;
			}
		}

		public void SendSystemMsg(string message)
		{
			m_syncLock.EnterReadLock();
			try
			{
				foreach (SubGroup unit in SubGroups)
				{
					foreach (GroupMember member in unit)
					{
						if (member.Character != null)
						{
							member.Character.SendSystemMessage(message);
						}
					}
				}
			}
			finally
			{
				m_syncLock.ExitReadLock();
			}
		}

		public void SendMessage(IChatter sender, ChatLanguage language, string message)
		{
			throw new NotImplementedException();
		}

		public GroupMember GetMember(uint lowId)
		{
			m_syncLock.EnterReadLock();
			try
			{
				foreach (var unit in SubGroups)
				{
					foreach (var member in unit)
					{
						if (member.Id == lowId)
						{
							return member;
						}
					}
				}
			}
			finally
			{
				m_syncLock.ExitReadLock();
			}
			return null;
		}

		/// <summary>
		/// All online characters. 
		/// Don't forget to lock the SyncRoot while iterating over a Group.
		/// </summary>
		public Character[] GetCharacters()
		{
			var chrs = new Character[m_Count];
			var c = 0;

			m_syncLock.EnterReadLock();
			try
			{
				for (var i = 0; i < SubGroups.Length; i++)
				{
					var subGroup = SubGroups[i];
					foreach (var member in subGroup)
					{
						if (member.Character != null)
						{
							chrs[c++] = member.Character;
						}

					}
				}
			}
			finally
			{
				m_syncLock.ExitReadLock();
			}

			if (chrs.Length > c)
			{
				Array.Resize(ref chrs, c);
			}
			return chrs;
		}

		/// <summary>
		/// Returns all online Characters in a <see cref="SynchronizedCharacterList"/>.
		/// </summary>
		/// <returns></returns>
		public SynchronizedCharacterList GetCharacterSet()
		{
			var chrs = new SynchronizedCharacterList(m_Count, FactionGroup);

			m_syncLock.EnterReadLock();
			try
			{
				for (var i = 0; i < SubGroups.Length; i++)
				{
					var subGroup = SubGroups[i];
					foreach (var member in subGroup)
					{
						if (member.Character != null)
						{
							chrs.Add(member.Character);
						}

					}
				}
			}
			finally
			{
				m_syncLock.ExitReadLock();
			}
			return chrs;
		}

		/// <summary>
		/// Sends the given packet to all online characters
		/// </summary>
		public void Send(RealmPacketOut packet)
		{
			foreach (var chr in GetCharacters())
			{
				chr.Client.Send(packet);
			}
		}
		#endregion

		/// <summary>
		/// Selects and returns the next online Member whose turn it is in RoundRobin.
		/// </summary>
		/// <returns>null if all members of this Group are offline.</returns>
		public GroupMember GetNextRoundRobinMember()
		{
			m_syncLock.EnterWriteLock();
			try
			{
				if (m_roundRobinMember == null)
				{
					m_roundRobinMember = m_firstMember;
				}
				else
				{
					m_roundRobinMember = m_roundRobinMember.Next;
				}
				while (m_roundRobinMember.Character == null)
				{
					m_roundRobinMember = m_roundRobinMember.Next;
					if (m_roundRobinMember == m_firstMember)
					{
						return null;
					}
				}
			}
			finally
			{
				m_syncLock.ExitWriteLock();
			}
			return m_roundRobinMember;
		}

		public void GetNearbyLooters(ILootable lootable, WorldObject initialLooter,
			ICollection<LooterEntry> looters)
		{
			WorldObject center;
			if (lootable is WorldObject)
			{
				center = (WorldObject)lootable;
			}
			else
			{
				center = initialLooter;
			}

			GroupMember otherMember;
			foreach (Character chr in center.GetObjectsInRadius(LootMgr.LootRadius, ObjectTypes.Player, false, 0))
			{
				if (chr.IsAlive && (chr == initialLooter ||
					((otherMember = chr.GroupMember) != null && otherMember.Group == this)))
				{
					looters.Add(chr.LooterEntry);
				}
			}
		}

		/// <summary>
		/// Sets the given Looting-parameters and updates the Group.
		/// </summary>
		public void SetLootMethod(LootMethod method, GroupMember masterLooter, ItemQuality lootThreshold)
		{
			m_syncLock.EnterWriteLock();
			try
			{
				LootMethod = method;
				m_masterLooter = masterLooter;
				LootThreshold = lootThreshold;
			}
			finally
			{
				m_syncLock.ExitWriteLock();
			}

			SendUpdate();
		}

		/// <summary>
		/// Update the stats of the given <see cref="GroupMember"/> to all 
		/// out of range members of this group. 
		/// </summary>
		/// <remarks>Method requires Group-synchronization.</remarks>
		/// <param name="member">The <see cref="GroupMember"/> who needs to send
		/// the update</param>
		internal void UpdateOutOfRangeMembers(GroupMember member)
		{
			if (member.Character == null)
				return;

			if (member.Group != this)
				return;

			foreach (var chr in GetCharacters())
			{
				if (chr != member.Character && chr != null
					&& !chr.IsInUpdateRange(member.Character)
					&& member.Character.GroupUpdateFlags != GroupUpdateFlags.None)
				{
					GroupHandler.SendPartyMemberStats(chr.Client, member,
						member.Character.GroupUpdateFlags);
				}
			}
			member.Character.GroupUpdateFlags = GroupUpdateFlags.None;
		}

		/// <summary>
		/// All members of this Group and all its SubGroups.
		/// Don't forget to lock the SyncRoot while iterating over a Group.
		/// </summary>
		public IEnumerator<GroupMember> GetEnumerator()
		{
			foreach (var unit in SubGroups)
			{
				foreach (var member in unit)
				{
					yield return member;
				}
			}
		}

		public void LockRead(Action action)
		{
			m_syncLock.EnterReadLock();

			try
			{
				action();
			}
			finally
			{
				m_syncLock.ExitReadLock();
			}
		}

		public void LockWrite(Action action)
		{
			m_syncLock.EnterWriteLock();

			try
			{
				action();
			}
			finally
			{
				m_syncLock.ExitWriteLock();
			}
		}

		#region Implementation of IInstanceHolder

		public Character InstanceLeader
		{
			get { return m_leader != null ? m_leader.Character : null; }
		}

		public InstanceCollection InstanceLeaderCollection
		{
			get { return m_leader != null ? m_leader.Character.Instances : null; }
		}

		public void ForeachInstanceHolder(Action<InstanceCollection> callback)
		{
			foreach (var chr in GetCharacters())
			{
				var log = chr.Instances;
				if (log != null)
				{
					callback(log);
				}
			}
		}

		/// <summary>
		/// Gets the Instance of the given Region of either the Leader or any member
		/// if anyone is already in it.
		/// </summary>
		public BaseInstance GetActiveInstance(RegionTemplate region)
		{
			// Need to be careful, since we are quite probably not in the corresponding Character's context:
			var leader = m_leader;
			if (leader != null)
			{
				var leaderChr = leader.Character;
				if (leaderChr != null)
				{
					var instances = leaderChr.Instances;
					if (instances != null)
					{
						var instance = instances.GetActiveInstance(region);
						if (instance != null)
						{
							return instance;
						}
					}
				}
			}

			// check all other members
			foreach (var chr in GetCharacters())
			{
				var instance = chr.GetActiveInstance(region);
				if (instance != null)
				{
					return instance;
				}
			}
			return null;
		}

		#endregion

		public void DistributeGroupHonor(Character earner, Character victim, uint honorPoints)
		{
			if (Count < 1) return;
			var bonus = honorPoints / (uint)Count;

			ForeachCharacter((chr) =>
			{
				if (chr.IsInRange(new SimpleRange(0.0f, 100.0f), earner))
				{
					chr.GiveHonorPoints(bonus);
					chr.KillsToday++;
					chr.LifetimeHonorableKills++;
					HonorHandler.SendPVPCredit(chr, bonus * 10, victim);
				}
			});
		}

		public void OnKill(Character killer, NPC victim)
		{
			if (Count < 1) return;

			ForeachCharacter((chr) =>
			{
				if (chr.Region == victim.Region && chr.IsInRange(new SimpleRange(0.0f, 100.0f), killer))
				{
					chr.QuestLog.OnNPCInteraction(victim);
					chr.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.KillCreature, victim.EntryId, 1);
				}
			});
		}
	}
}