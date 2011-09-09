/*************************************************************************
 *
 *   file		    : GroupHandlers.cs
 *   copyright		: (C) The WCell Team
 *   email		    : info@wcell.org
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
using System.Linq;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Looting;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.Network;
using WCell.RealmServer.Spells.Auras;
using WCell.Util;

namespace WCell.RealmServer.Handlers
{
	public static class GroupHandler
	{
		/// <summary>
		/// Handles an incoming group invite request (/invite Player)
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_GROUP_INVITE)]
		public static void GroupInviteRequest(IRealmClient client, RealmPacketIn packet)
		{
			var inviteeName = packet.ReadCString();

			var inviter = client.ActiveCharacter;
			var group = inviter.Group;

			Character invitee;
			if (Group.CheckInvite(inviter, out invitee, inviteeName) == GroupResult.NoError)
			{
				var listInvitees = Singleton<RelationMgr>.Instance.GetRelations(inviter.EntityId.Low,
																				CharacterRelationType.GroupInvite);

				if (group == null || listInvitees.Count < group.InvitesLeft)
				{
					BaseRelation inviteRelation = RelationMgr.CreateRelation(inviter.EntityId.Low,
																			 invitee.EntityId.Low, CharacterRelationType.GroupInvite);

					Singleton<RelationMgr>.Instance.AddRelation(inviteRelation);

					// Target has been invited
					Group.SendResult(inviter.Client, GroupResult.NoError, inviteeName);
					SendGroupInvite(invitee.Client, inviter.Name);
				}
			}
		}

		/// <summary>
		/// Handles an incoming accept on group invite request
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_GROUP_ACCEPT)]
		public static void GroupAccept(IRealmClient client, RealmPacketIn packet)
		{
			var invitee = client.ActiveCharacter;

			var listInviters = Singleton<RelationMgr>.Instance.GetPassiveRelations(invitee.EntityId.Low,
																				   CharacterRelationType.GroupInvite);

			var invite = listInviters.FirstOrDefault();

			//Check if we got invited
			if (invite == null)
				return;

			//Removes the group invite relation between the inviter and invitee
			RelationMgr.Instance.RemoveRelation(invite);

			var inviter = World.GetCharacter(invite.CharacterId);
			if (inviter == null)
				return;

			//If the inviter isnt in a group already we create a new one
			var inviterGroup = inviter.Group ?? new PartyGroup(inviter);

			//Add the invitee to the group
			inviterGroup.AddMember(invitee, true);
		}

		/// <summary>
		/// Handles an incoming decline on group invite request
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_GROUP_DECLINE)]
		public static void GroupDecline(IRealmClient client, RealmPacketIn packet)
		{
			Character invitee = client.ActiveCharacter;

			var listInviters = Singleton<RelationMgr>.Instance.GetPassiveRelations(invitee.EntityId.Low,
																				   CharacterRelationType.GroupInvite);

			var invite = listInviters.FirstOrDefault();

			//Check if we got invited
			if (invite == null)
				return;

			//Removes the group invite relation between the inviter and invitee
			Singleton<RelationMgr>.Instance.RemoveRelation(invite);

			Character inviter = World.GetCharacter(invite.CharacterId);
			if (inviter == null)
				return;

			SendGroupDecline(inviter.Client, invitee.Name);
		}

		/// <summary>
		/// Handles an incoming request on uninviting a Character from the group by name
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_GROUP_UNINVITE)]
		public static void GroupUninviteByName(IRealmClient client, RealmPacketIn packet)
		{
			var uninviteeName = packet.ReadCString();

			if (uninviteeName.Length == 0)
				return;

			var uninviter = client.ActiveCharacter;
			var uninviterMember = uninviter.GroupMember;
			if (uninviterMember == null || !uninviterMember.IsLeader)
			{
				return;
			}

			var group = uninviterMember.Group;
			var target = group[uninviteeName];

			if (target != null)
			{
				group.RemoveMember(target);
			}
		}

		/// <summary>
		/// Handles an incoming request on uninviting a Character from the group by GUID
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_GROUP_UNINVITE_GUID)]
		public static void HandleGroupUninviteByGUID(IRealmClient client, RealmPacketIn packet)
		{
			var uninviteeId = packet.ReadEntityId();
		    packet.ReadByte(); // 3.3.3a

			var uninviter = client.ActiveCharacter;
			var member = uninviter.GroupMember;
			if (member == null || !member.IsLeader)
			{
				return;
			}

			var group = member.Group;
			var uninvitee = group[uninviteeId.Low];

			if (uninvitee != null)
			{
				var uninviteeGroup = uninvitee.SubGroup.Group;
				if (uninviteeGroup != group)
					return;

				group.RemoveMember(uninvitee);
			}
		}

		/// <summary>
		/// Handles an incoming request on leader change
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_GROUP_SET_LEADER)]
		public static void GroupSetLeader(IRealmClient client, RealmPacketIn packet)
		{
			EntityId guid = packet.ReadEntityId();

			GroupMember member = client.ActiveCharacter.GroupMember;
			if (member == null)
				return;

			var group = member.Group;
			var targetMember = group[guid.Low];

			if (group.CheckAction(member, targetMember,
				targetMember != null ? targetMember.Name : String.Empty, GroupPrivs.Leader) == GroupResult.NoError)
			{
				group.Leader = targetMember;
				group.SendUpdate();
			}
		}

		/// <summary>
		/// Handles an incoming group disband packet (used for leaving party in fact)
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_GROUP_DISBAND)]
		public static void GroupDisband(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var member = chr.GroupMember;

			if (member == null)
				return;

			member.LeaveGroup();
		}

		/// <summary>
		/// Handles an incoming request on loot method change
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_LOOT_METHOD)]
		public static void GroupSetLootMethod(IRealmClient client, RealmPacketIn packet)
		{
			var member = client.ActiveCharacter.GroupMember;

			if (member == null)
				return;

			var lootMethod = (LootMethod)packet.ReadUInt32();
			var looterGuid = packet.ReadEntityId();
			var lootThreshold = (ItemQuality)packet.ReadUInt32();

			var group = member.Group;
			var looterMember = group[looterGuid.Low];

			bool isValid;
			if (looterMember != null)
			{
				isValid = (group.CheckAction(member, looterMember, looterMember.Name,
											 GroupPrivs.Leader) == GroupResult.NoError);
			}
			else
			{
				isValid = group.CheckPrivs(member, GroupPrivs.Leader);
			}

			if (isValid)
			{
				group.SetLootMethod(lootMethod, looterMember, lootThreshold);
			}
		}

		/// <summary>
		/// Handles an incoming minimap ping
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.MSG_MINIMAP_PING)]
		public static void MinimapPing(IRealmClient client, RealmPacketIn packet)
		{
			var x = packet.ReadFloat();
			var y = packet.ReadFloat();

			var member = client.ActiveCharacter.GroupMember;

			if (member == null)
				return;

			member.Group.SendPing(member, x, y);
		}

		/// <summary>
		/// Handles an incoming request on random roll
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.MSG_RANDOM_ROLL)]
		public static void RandomRollRequest(IRealmClient client, RealmPacketIn packet)
		{
			int min = packet.ReadInt32();
			int max = packet.ReadInt32();

			if (min > max || max > 10000)
				return;

			var random = new Random();
			var roll = random.Next(min, max);
			var group = client.ActiveCharacter.Group;

			if (group == null)
			{
				// no group, so send only to the requester
				SendRoll(client, min, max, roll, client.ActiveCharacter.EntityId);
			}
			else
				group.SendRoll(min, max, roll, client.ActiveCharacter.EntityId);
		}

		/// <summary>
		/// Handles an incoming request on random roll
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.MSG_RAID_TARGET_UPDATE)]
		public static void RaidIconTarget(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var member = chr.GroupMember;
			if (member == null)
				return;

			var group = member.Group;

			byte iconId = packet.ReadByte();

			if (iconId == Byte.MaxValue) // request for full list of icons
			{
				group.SendTargetIconList(client.ActiveCharacter);
			}
			else // updating icon
			{
				if (!group.CheckPrivs(member, GroupPrivs.Assistant))
					return;

				EntityId targetId = packet.ReadEntityId();

				group.SetTargetIcon(iconId, client.ActiveCharacter.EntityId, targetId);
			}
		}

		/// <summary>
		/// Handles an incoming convert party to raid request
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_GROUP_RAID_CONVERT)]
		public static void ConvertToRaidRequest(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var member = chr.GroupMember;
			if (member == null)
				return;

			var group = member.Group;

			if ((group is PartyGroup))
			{
				if (group.CheckPrivs(member, GroupPrivs.Leader))
				{
					Group newGroup = ((PartyGroup)group).ConvertTo();

					SendResult(client, GroupResult.NoError); // Successful converted to raid
					newGroup.SendUpdate();
				}
			}
		}

		/// <summary>
		/// Handles an incoming request on moving Character from one subgroup to another
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_GROUP_CHANGE_SUB_GROUP)]
		public static void ChangeSubgroupRequest(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var member = chr.GroupMember;

			if (member == null)
			{
				return;
			}

			var group = member.Group as RaidGroup;
			if (group == null)
				return;

			var targetName = packet.ReadCString();
			var targetGroupId = packet.ReadByte();

			var targetMember = group[targetName];
			if (group.CheckAction(member, targetMember, targetName, GroupPrivs.Assistant) == GroupResult.NoError)
			{
				var subGroup = group.SubGroups.Get(targetGroupId);
				if (subGroup == null)
				{
					// Invalid subgroup
				}
				else if (!group.MoveMember(targetMember, subGroup))
				{
					SendResult(client, GroupResult.GroupIsFull);
				}
				else
				{
					group.SendUpdate();
				}
			}
		}

		/// <summary>
		/// Handles an incoming request on changing Assistant flag of specified Character
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_GROUP_ASSISTANT_LEADER)]
		public static void ChangeAssistantFlagRequest(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var member = chr.GroupMember;

			if (member == null)
				return;

			var group = member.Group as RaidGroup;
			if (group == null)
				return;

			EntityId targetGuid = packet.ReadEntityId();

			var targetMember = group[targetGuid.Low];

			if (group.CheckAction(member, targetMember, targetMember != null ? targetMember.Name : String.Empty, GroupPrivs.Leader)
				== GroupResult.NoError)
			{
				bool isSet = packet.ReadBoolean();
				targetMember.IsAssistant = isSet;
				group.SendUpdate();
			}
		}

		/// <summary>
		/// Handles an incoming request for changing the Main Tank or Main Assistant flag 
		/// of specified Character
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.MSG_PARTY_ASSIGNMENT)]
		public static void GroupPromoteFlagRequest(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var member = chr.GroupMember;
			if (member == null)
				return;

			var group = member.Group as RaidGroup;
			if (group == null)
				return;

			byte promotionType = packet.ReadByte();
			bool add = packet.ReadBoolean();
			EntityId targetGuid = packet.ReadEntityId();

			var targetMember = group[targetGuid.Low];

			if (group.CheckAction(member, targetMember, targetMember != null ? targetMember.Name : String.Empty, GroupPrivs.Leader) ==
				GroupResult.NoError)
			{
				if (promotionType == 0)
				{
					// group.SetMainTank(promotedMember, add);
				}
				else
				{
					group.MainAssistant = targetMember;
				}

				group.SendUpdate();
			}
		}

		/// <summary>
		/// Handles an incoming request or answer to raid ready check
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.MSG_RAID_READY_CHECK)]
		public static void RaidReadyCheck(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var member = chr.GroupMember;
			if (member == null)
				return;

			var group = member.Group as RaidGroup;
			if (group == null)
				return;

			if (packet.RemainingLength == 0) // request for ready check
			{
				if (!group.CheckPrivs(member, GroupPrivs.Assistant))
					return;

				group.SendReadyCheckRequest(member);
			}
			else // answer to ready check
			{
				//byte status = packet.ReadByte();
				ReadyCheckStatus status = (ReadyCheckStatus)packet.ReadByte();

				group.SendReadyCheckResponse(member, status);
			}
		}

		/// <summary>
		/// Handles an incoming request for a group member status
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_REQUEST_PARTY_MEMBER_STATS)]
		public static void RequestPartyMemberStats(IRealmClient client, RealmPacketIn packet)
		{
			var targetId = packet.ReadEntityId();
			var group = client.ActiveCharacter.Group;
			if (group != null)
			{
				var target = group.GetMember(targetId.Low);
				if (target != null)
				{
					var flags = GroupUpdateFlags.None;

					if (target.Character != null)
					{
						flags |= GroupUpdateFlags.UpdateFull;
					}
					else
					{
						flags |= GroupUpdateFlags.Status;
					}

					SendPartyMemberStatsFull(client, target, flags);
				}
			}
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_MEETINGSTONE_INFO)]
		public static void HandleMeetingStoneInfoRequest(IRealmClient client, RealmPacketIn packet)
		{
			SendMeetingStoneSetQueue(client.ActiveCharacter);
		}

        [ClientPacketHandler(RealmServerOpCode.CMSG_SET_ALLOW_LOW_LEVEL_RAID1)]
        public static void HandleSetAllowLowLevelRaid1(IRealmClient client, RealmPacketIn packet)
        {
            Character character = client.ActiveCharacter;
            bool allow = packet.ReadBoolean();
            character.IsAllowedLowLevelRaid = allow;
        }

		#region Out

		public static void SendLeaderChanged(GroupMember leader)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GROUP_SET_LEADER))
			{
				packet.WriteCString(leader.Name);
				leader.SubGroup.Group.SendAll(packet);
			}
		}

		/// <summary>
		/// Sends result of actions connected with groups
		/// </summary>
		/// <param name="client">the client to send to</param>
		/// <param name="resultCode">The <see cref="GroupResult"/> result code</param>
		public static void SendResult(IPacketReceiver client, GroupResult resultCode)
		{
			Group.SendResult(client, resultCode, 0, String.Empty);
		}

		/// <summary>
		/// Send Group Invite packet
		/// </summary>
		/// <param name="client">realm client</param>
		/// <param name="inviter">nick of player invited you</param>
		public static void SendGroupInvite(IPacketReceiver client, string inviter)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GROUP_INVITE, inviter.Length + 10))
			{
				packet.Write((byte)1);		// unknown since Wotlk
				packet.WriteCString(inviter);
                packet.Write((uint)0); //333a
                packet.Write((byte)0); //333a
			    packet.Write((uint)0); //333a
				client.Send(packet);
			}
		}

		/// <summary>
		/// Sends group invitation decline
		/// </summary>
		/// <param name="client">the client to send to</param>
		/// <param name="decliner">player who has declined your request</param>
		public static void SendGroupDecline(IPacketReceiver client, string decliner)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GROUP_DECLINE, decliner.Length + 1))
			{
				packet.WriteCString(decliner);
				client.Send(packet);
			}
		}

		/// <summary>
		/// Sends roll results to the group
		/// </summary>
		/// <param name="client">realm client</param>
		/// <param name="min">minimal value</param>
		/// <param name="max">maximal value</param>
		/// <param name="value">value rolled out</param>
		/// <param name="guid">guid of roller</param>
		public static void SendRoll(IPacketReceiver client, int min, int max, int value, EntityId guid)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_RANDOM_ROLL, 20))
			{
				packet.Write(min);
				packet.Write(max);
				packet.Write(value);
				packet.Write(guid.Full);

				client.Send(packet);
			}
		}

		/// <summary>
		/// Sends the requested party member stats data to the client
		/// </summary>
		/// <param name="client">realm client</param>
		/// <param name="member">The character whose stats is going to be retrieved</param>
		/// <param name="flags">The stats to be retrieved from the <paramref name="member"/></param>
		public static void SendPartyMemberStatsFull(IPacketReceiver client, GroupMember member, GroupUpdateFlags flags)
		{
			SendPartyMemberStatsInternal(client, member, flags, RealmServerOpCode.SMSG_PARTY_MEMBER_STATS_FULL);
		}

		/// <summary>
		/// Sends the requested party member stats data to the client
		/// </summary>
		/// <param name="client">realm client</param>
		/// <param name="member">The character whose stats is going to be retrieved</param>
		/// <param name="flags">The stats to be retrieved from the <paramref name="member"/></param>
		public static void SendPartyMemberStats(IPacketReceiver client, GroupMember member, GroupUpdateFlags flags)
		{
			SendPartyMemberStatsInternal(client, member, flags, RealmServerOpCode.SMSG_PARTY_MEMBER_STATS);
		}

		/// <summary>
		/// Sends the requested party member stats data to the client
		/// </summary>
		/// <param name="client">realm client</param>
		/// <param name="member">The character whose stats is going to be retrieved</param>
		/// <param name="flags">The stats to be retrieved from the <paramref name="member"/></param>
		private static void SendPartyMemberStatsInternal(IPacketReceiver client, GroupMember member, GroupUpdateFlags flags,
														 RealmServerOpCode opcode)
		{
			using (var packet = new RealmPacketOut(opcode))
			{
				if (opcode == RealmServerOpCode.SMSG_PARTY_MEMBER_STATS_FULL)
				{
					packet.Write((byte)0); //arena something
				}

				member.WriteIdPacked(packet);
				if (!member.IsOnline)
				{
					packet.WriteUShort((ushort)CharacterStatus.OFFLINE);
					client.Send(packet);
					return;
				}
				packet.Write((uint)flags);

				var chr = member.Character;
				if (flags.HasFlag(GroupUpdateFlags.Status))
				{
					packet.Write((ushort)chr.Status);
				}
				if (flags.HasFlag(GroupUpdateFlags.Health))
				{
					packet.Write(chr.Health);
				}
				if (flags.HasFlag(GroupUpdateFlags.MaxHealth))
				{
					packet.Write(chr.MaxHealth);
				}
				if (flags.HasFlag(GroupUpdateFlags.PowerType))
				{
					packet.Write((byte)chr.PowerType);
				}
				if (flags.HasFlag(GroupUpdateFlags.Power))
				{
					packet.Write((ushort)chr.Power);
				}
				if (flags.HasFlag(GroupUpdateFlags.MaxPower))
				{
					packet.Write((ushort)chr.MaxPower);
				}
				if (flags.HasFlag(GroupUpdateFlags.Level))
				{
					packet.Write((ushort)chr.Level);
				}
				if (flags.HasFlag(GroupUpdateFlags.ZoneId))
				{
					packet.Write((ushort)(chr.Zone != null ? chr.Zone.Id : ZoneId.None));
				}
				if (flags.HasFlag(GroupUpdateFlags.Position))
				{
					packet.Write((ushort)chr.Position.X);
					packet.Write((ushort)chr.Position.Y);
				}
				if (flags.HasFlag(GroupUpdateFlags.Auras))
				{
					ulong auraMask = chr.AuraUpdateMask;
					packet.Write(auraMask);
					Aura currAura;

					for (byte i = 0; i < AuraHandler.MaxAuras; ++i)
					{
						if ((auraMask & ((ulong)1 << i)) != 0)
						{
							currAura = chr.Auras.GetAt(i);

							packet.Write(currAura.Spell.Id);
							packet.Write((byte)currAura.Flags);
						}
					}
				}

				NPC targetPet = chr.ActivePet;

				if (targetPet == null) //no pet
				{
					packet.Write((byte)0); //name
					packet.Write(0UL); //auras
					client.Send(packet);
					return;
				}

				if (flags.HasFlag(GroupUpdateFlags.PetGuid))
				{
					packet.Write(targetPet.EntityId);
				}

				if (flags.HasFlag(GroupUpdateFlags.PetName))
				{
					packet.WriteCString(targetPet.Name);
				}

				if (flags.HasFlag(GroupUpdateFlags.PetDisplayId))
				{
					packet.Write((ushort)targetPet.DisplayId);
				}

				if (flags.HasFlag(GroupUpdateFlags.PetHealth))
				{
					packet.Write(targetPet.Health);
				}

				if (flags.HasFlag(GroupUpdateFlags.PetMaxHealth))
				{
					packet.Write(targetPet.MaxHealth);
				}

				if (flags.HasFlag(GroupUpdateFlags.PetPowerType))
				{
					packet.Write((byte)targetPet.PowerType);
				}

				if (flags.HasFlag(GroupUpdateFlags.PetPower))
				{
					packet.Write((ushort)targetPet.Power);
				}

				if (flags.HasFlag(GroupUpdateFlags.PetMaxPower))
				{
					packet.Write((ushort)targetPet.MaxPower);
				}

				if (flags.HasFlag(GroupUpdateFlags.PetAuras))
				{
					ulong auraMask = targetPet.AuraUpdateMask;
					packet.Write(auraMask);
					Aura currAura;

					for (byte i = 0; i < AuraHandler.MaxAuras; ++i)
					{
						if ((auraMask & ((ulong)1 << i)) != 0)
						{
							currAura = targetPet.Auras.GetAt(i);//chr.Auras.GetAt(i);

							packet.Write(currAura.Spell.Id);
							packet.Write((byte)currAura.Flags);
						}
					}
				}
				client.Send(packet);
			}
		}

		private static void SendMeetingStoneSetQueue(IPacketReceiver client)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_MEETINGSTONE_SETQUEUE))
			{
				packet.Write((uint)0);
				packet.Write((byte)6);

				client.Send(packet);
			}
		}
		#endregion
	}
}