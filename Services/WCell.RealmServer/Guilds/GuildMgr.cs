/*************************************************************************
 *
 *   file		: GuildMgr.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate$
 *   last author	: $LastChangedBy$
 *   revision		: $Rev$
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using Castle.ActiveRecord;
using NLog;
using WCell.Util;
using WCell.Util.Collections;
using WCell.Constants;
using WCell.Constants.Guilds;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.NPCs;

namespace WCell.RealmServer.Guilds
{
	/// <summary>
	/// </summary>
	public sealed class GuildMgr : Manager<GuildMgr>
	{
		private static uint guildCharterCost = 1000;

		public const int MIN_GUILD_RANKS = 5;
		public const int MAX_GUILD_RANKS = 10;
		public static int MaxGuildNameLength = 20;
		public static int MaxGuildRankNameLength = 10;
		public static int MaxGuildMotdLength = 100;
		public static int MaxGuildInfoLength = 500;
		public static int MaxGuildMemberNoteLength = 100;

		/// <summary>
		/// Cost (in copper) of a new Guild Tabard
		/// </summary>
		public static uint GuildTabardCost = 100000;

		/// <summary>
		/// The delay (in hours) before Guild Members' BankMoneyWithdrawlAllowance resets.
		/// </summary>
		public const int BankMoneyAllowanceResetDelay = 24;

		public const uint UNLIMITED_BANK_MONEY_WITHDRAWL = UInt32.MaxValue;
		public const uint UNLIMITED_BANK_SLOT_WITHDRAWL = UInt32.MaxValue;
		public const int MAX_BANK_TABS = 6;
		public const int MAX_BANK_TAB_SLOTS = 98;

		public static uint GuildCharterCost
		{
			get { return guildCharterCost; }
			set
			{
				guildCharterCost = value;
				PetitionerEntry.GuildPetitionEntry.Cost = value;
			}
		}

		private static int requiredCharterSignature = 9;

		public static int RequiredCharterSignature
		{
			get { return requiredCharterSignature; }
			set
			{
				requiredCharterSignature = value;
				PetitionerEntry.GuildPetitionEntry.RequiredSignatures = value;
			}
		}

		/// <summary>
		/// Maps char-id to the corresponding GuildMember object so it can be looked up when char reconnects
		/// </summary>
		public static readonly IDictionary<uint, GuildMember> OfflineMembers;
		public static readonly IDictionary<uint, Guild> GuildsById;
		public static readonly IDictionary<string, Guild> GuildsByName;
		private static readonly ReaderWriterLockWrapper guildsLock = new ReaderWriterLockWrapper();
		private static readonly ReaderWriterLockWrapper membersLock = new ReaderWriterLockWrapper();

		#region Init
		static GuildMgr()
		{
			GuildsById = new SynchronizedDictionary<uint, Guild>();
			GuildsByName = new SynchronizedDictionary<string, Guild>(StringComparer.InvariantCultureIgnoreCase);
			OfflineMembers = new SynchronizedDictionary<uint, GuildMember>();
		}

		private GuildMgr()
		{
		}

		[Initialization(InitializationPass.Fifth, "Initialize Guilds")]
		public static bool Initialize()
		{
			return Instance.Start();
		}

		private bool Start()
		{
			Guild[] guilds = null;

#if DEBUG
			try
			{
#endif
				guilds = ActiveRecordBase<Guild>.FindAll();
#if DEBUG
			}
			catch (Exception e)
			{
				RealmDBMgr.OnDBError(e);
				guilds = ActiveRecordBase<Guild>.FindAll();
			}
#endif

			if (guilds != null)
			{
				foreach (var guild in guilds)
				{
					guild.InitAfterLoad();
				}
			}

			return true;
		}
		#endregion

		public static ImmutableList<GuildRank> CreateDefaultRanks(Guild guild)
		{
			var ranks = new ImmutableList<GuildRank>();
			var ranksNum = 0;
			var gmRank = new GuildRank(guild, "Guild Master", GuildPrivileges.ALL, ranksNum++);
			ranks.Add(gmRank);
			ranks.Add(new GuildRank(guild, "Officer", GuildPrivileges.ALL, ranksNum++));
			ranks.Add(new GuildRank(guild, "Veteran", GuildPrivileges.DEFAULT, ranksNum++));
			ranks.Add(new GuildRank(guild, "Member", GuildPrivileges.DEFAULT, ranksNum++));
			ranks.Add(new GuildRank(guild, "Initiate", GuildPrivileges.DEFAULT, ranksNum));
			return ranks;

		}

		internal void OnCharacterLogin(Character chr)
		{
			GuildMember member;
			using (membersLock.EnterWriteLock())
			{
				if (OfflineMembers.TryGetValue(chr.EntityId.Low, out member))
				{
					OfflineMembers.Remove(chr.EntityId.Low);
					member.Character = chr;
				}
			}

			if (member != null)
			{
				chr.GuildMember = member;
				if (member.Guild != null)
				{
					GuildHandler.SendEventToGuild(member.Guild, GuildEvents.ONLINE, member);
				}
				else
				{
					// now this is bad
					LogManager.GetCurrentClassLogger().Warn("Found orphaned GuildMember for character \"{0}\" during logon.");
				}
			}
		}

		/// <summary>
		/// Cleanup character invitations and group leader, looter change on character logout/disconnect
		/// </summary>
		/// <param name="member">The GuildMember logging out / disconnecting (or null if the corresponding Character is not in a Guild)</param>
		internal void OnCharacterLogout(GuildMember member)
		{
			if (member == null)
			{
				// null check is only required because we stated in the documentation of this method that memeber is allowed to be null
				return;
			}

			var chr = member.Character;
			var listInviters = RelationMgr.Instance.GetPassiveRelations(chr.EntityId.Low, CharacterRelationType.GuildInvite);

			foreach (GroupInviteRelation inviteRelation in listInviters)
			{
				RelationMgr.Instance.RemoveRelation(inviteRelation);
			}

			var guild = member.Guild;

			if (guild == null) // ???
				return;

			member.LastLogin = DateTime.Now;
			var zone = member.Character.Zone;
			member.ZoneId = zone != null ? (int)zone.Id : 0;
			member.Class = member.Character.Class;
			member.Level = member.Character.Level;

			member.Character = null;

			member.UpdateLater();

			using (membersLock.EnterWriteLock())
			{
				OfflineMembers[chr.EntityId.Low] = member;
			}

			GuildHandler.SendEventToGuild(member.Guild, GuildEvents.OFFLINE, member);
		}

		/// <summary>
		/// New or loaded Guild
		/// </summary>
		/// <param name="guild"></param>
		internal void RegisterGuild(Guild guild)
		{
			using (guildsLock.EnterWriteLock())
			{
				GuildsById.Add(guild.Id, guild);
				GuildsByName.Add(guild.Name, guild);
				using (membersLock.EnterWriteLock())
				{
					foreach (var gm in guild.Members.Values)
					{
						if (gm.Character == null && !OfflineMembers.ContainsKey(gm.Id))
						{
							OfflineMembers.Add(gm.Id, gm);
						}
					}
				}
			}
		}

		internal void UnregisterGuild(Guild guild)
		{
			using (guildsLock.EnterWriteLock())
			{
				GuildsById.Remove(guild.Id);
				GuildsByName.Remove(guild.Name);
				// no need to remove offline members, since, at this point
				// all members have already been evicted
			}
		}

		internal void RegisterGuildMember(GuildMember gm)
		{
			if (gm.Character == null)
			{
				using (membersLock.EnterWriteLock())
				{
					OfflineMembers.Add(gm.Id, gm);
				}
			}
		}

		internal void UnregisterGuildMember(GuildMember gm)
		{
			using (membersLock.EnterWriteLock())
			{
				OfflineMembers.Remove(gm.Id);
			}
		}

		public static Guild GetGuild(uint guildId)
		{
			using (guildsLock.EnterReadLock())
			{
				Guild guild;
				GuildsById.TryGetValue(guildId, out guild);
				return guild;
			}
		}

		public static Guild GetGuild(string name)
		{
			using (guildsLock.EnterReadLock())
			{
				Guild guild;
				GuildsByName.TryGetValue(name, out guild);
				return guild;
			}
		}

		#region Checks
		public static bool CanUseName(string name)
		{
			if (IsValidGuildName(name))
			{
				return GetGuild(name) == null;
			}
			return false;
		}

		public static bool DoesGuildExist(string name)
		{
			return GetGuild(name) != null;
		}

		public static bool IsValidGuildName(string name)
		{
			name = name.Trim();
			if (name.Length < 3 && name.Length > MaxGuildNameLength || name.Contains(" "))
			{
				return false;
			}

			return true;
		}

		//private static bool CheckIsLeader(Character member, Guild guild)
		//{
		//    if (guild.Leader != member.GuildMember)
		//    {
		//        // you dont have permission
		//        Guild.SendResult(member.Client, GuildCommands.PROMOTE, GuildResult.PERMISSIONS);
		//        return false;
		//    }
		//    return true;
		//}

		//private static bool CheckNotSelf(Character member1, Character member2)
		//{
		//    if (member1 == member2)
		//    {
		//        Guild.SendResult(member1.Client, GuildCommands.INVITE, GuildResult.PERMISSIONS);
		//        return false;
		//    }
		//    return true;
		//}

		////private static bool CheckForPermission(Character member, Group group)
		////{
		////    EntityId memberId = member.EntityId;

		////    if (!group.IsLeader(memberId.Low) && !group.IsAssistant(memberId.Low))
		////    {
		////        // you dont have permission
		////        Group.SendResult(member.Client, GroupResult.DontHavePermission);
		////        return false;
		////    }
		////    return true;
		////}

		//private static bool CheckMemberInGuild(Character requester, string memberName)
		//{
		//    if (requester.GuildMember == null || string.IsNullOrEmpty(memberName))
		//        return false;

		//    GuildMember guildMember = requester.GuildMember.Guild[memberName];

		//    if (guildMember == null)
		//    {
		//        Guild.SendResult(requester.Client, GuildCommands.CREATE, guildMember.Name,
		//            GuildResult.PLAYER_NOT_IN_GUILD);
		//        return false;
		//    }
		//    return true;
		//}

		//private static bool CheckMemberInGuild(Character requester, GuildMember guildMember)
		//{
		//    if (guildMember == null)
		//        return false;

		//    if (requester.GuildMember == null || requester.GuildMember.Guild != guildMember.Guild)
		//    {
		//        Guild.SendResult(requester.Client, GuildCommands.CREATE, guildMember.Name,
		//            GuildResult.PLAYER_NOT_IN_GUILD);
		//        return false;
		//    }
		//    return true;
		//}

		////private static bool CheckCharacterExist(Character requester, Character character)
		////{
		////    return CheckCharacterExist(requester, character, string.Empty);
		////}

		//private static bool CheckCharacterExist(Character requester, Character character, string targetName)
		//{
		//    if (character == null)
		//    {
		//        // Character is offline or doesn't exist
		//        Guild.SendResult(requester.Client, GuildCommands.INVITE, targetName,
		//            GuildResult.PLAYER_NOT_FOUND);
		//        return false;
		//    }
		//    return true;
		//}

		//private static bool CheckSameFaction(Character inviter, Character invitee)
		//{
		//    //Check if the inviter and invitee are from the same faction
		//    if (inviter.Faction.Group != invitee.Faction.Group)
		//    {
		//        // you can't invite a Character from other faction
		//        Guild.SendResult(inviter.Client, GuildCommands.INVITE, GuildResult.NOT_ALLIED);
		//        return false;
		//    }
		//    return true;
		//}


		//private static bool CheckAlreadyInvited(Character inviter, Character invitee)
		//{
		//    if (inviter.IsInvitedToGuild)
		//    {
		//        Guild.SendResult(inviter.Client, GuildCommands.INVITE, inviter.Name, GuildResult.ALREADY_IN_GUILD);
		//        return false;
		//    }
		//    return true;
		//}

		//private static bool CheckInvitingSelf(Character inviter, Character invitee)
		//{
		//    return (inviter != invitee);
		//}

		//private static bool CheckInGuild(Character requester)
		//{
		//    if (requester.GuildMember == null)
		//    {
		//        Guild.SendResult(requester.Client, GuildCommands.CREATE, GuildResult.PLAYER_NOT_IN_GUILD);
		//        return false;
		//    }
		//    return true;
		//}

		//private static bool CheckInviteeInGuild(Character inviter, Character invitee)
		//{
		//    if (invitee.GuildMember != null)
		//    {
		//        Guild.SendResult(inviter.Client, GuildCommands.INVITE, GuildResult.ALREADY_IN_GUILD);
		//        return false;
		//    }
		//    return true;
		//}

		//private static bool CheckSameGuild(Character requester, Character character)
		//{
		//    if (requester.GuildMember == null || character.GuildMember == null)
		//        return false;

		//    if (requester.GuildMember.Guild != character.GuildMember.Guild)
		//    {
		//        return false;
		//    }
		//    return true;
		//}

		//private static bool CheckIsIgnored(Character ignored, Character ignoring)
		//{
		//    //if (RelationMgr.Instance.ExistRelation(ignoring.EntityId, ignored.EntityId,
		//    //    CharacterRelationType.Ignored))

		//    if (ignoring.Ignores(ignored.EntityId.Low))
		//    {
		//        Guild.SendResult(ignored.Client, GuildCommands.INVITE, GuildResult.PLAYER_IGNORING_YOU);
		//        return false;
		//    }
		//    return true;
		//}

		//private static bool CheckHaveInvitePrivileges(Character inviter)
		//{
		//    if (inviter.GuildMember == null)
		//        return false;

		//    Guild guild = inviter.GuildMember.Guild;
		//    GuildMember guildMember = inviter.GuildMember;

		//    if (!guildMember.HasRight(GuildPrivileges.INVITE))
		//    {
		//        Guild.SendResult(inviter.Client, GuildCommands.INVITE, GuildResult.PERMISSIONS);
		//        return false;
		//    }
		//    return true;
		//}

		//private static bool CheckHavePrivileges(Character character, GuildPrivileges privileges,
		//    GuildCommands command)
		//{
		//    GuildMember guildMember = character.GuildMember;

		//    if (guildMember == null)
		//        return false;

		//    if (!guildMember.HasRight(privileges))
		//    {
		//        Guild.SendResult(character.Client, command, GuildResult.PERMISSIONS);
		//        return false;
		//    }
		//    return true;
		//}
		#endregion
	}
}