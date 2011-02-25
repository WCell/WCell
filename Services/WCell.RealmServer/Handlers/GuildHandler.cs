using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Guilds;
using WCell.Constants.Items;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Guilds;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.Items;
using WCell.RealmServer.Network;
using WCell.Util;

namespace WCell.RealmServer.Handlers
{
	public static class GuildHandler
	{
		#region Queries
		/// <summary>
		/// Handles an incoming guild query
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[PacketHandler(RealmServerOpCode.CMSG_GUILD_QUERY)]
		public static void Query(IRealmClient client, RealmPacketIn packet)
		{
			var guildId = packet.ReadUInt32();

			Guild guild;
			var character = client.ActiveCharacter;

			if (character != null && character.Guild != null && character.Guild.Id == guildId)
				guild = character.Guild;
			else
			{
				guild = GuildMgr.GetGuild(guildId);
			}

			if (guild != null)
			{
				SendGuildQueryResponse(client, guild);
			}
		}

		/// <summary>
		/// Handles an incoming guild roster query
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[PacketHandler(RealmServerOpCode.CMSG_GUILD_ROSTER)]
		public static void Roster(IRealmClient client, RealmPacketIn packet)
		{
			var guildMember = client.ActiveCharacter.GuildMember;

			if (guildMember == null)
				return;

			var guild = guildMember.Guild;

			if (guild == null)
				return;

			SendGuildRoster(client, guild, client.ActiveCharacter.GuildMember.HasRight(GuildPrivileges.VIEWOFFNOTE));
		}

		/// <summary>
		/// Handles an incoming guild roster query
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[PacketHandler(RealmServerOpCode.MSG_GUILD_EVENT_LOG_QUERY)]
		public static void EventLog(IRealmClient client, RealmPacketIn packet)
		{
			var guildMember = client.ActiveCharacter.GuildMember;
			if (guildMember == null)
				return;

			var guild = guildMember.Guild;
			if (guild == null)
				return;

			SendGuildEventLog(client, guild);
		}
		#endregion

		#region Guild Entering / Leaving
		/// <summary>
		/// Handles an incoming guild invite request (/ginvite Player)
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[PacketHandler(RealmServerOpCode.CMSG_GUILD_INVITE)]
		public static void InviteRequest(IRealmClient client, RealmPacketIn packet)
		{
			string inviteeName = packet.ReadCString();

			Character inviter = client.ActiveCharacter;
			Character invitee = World.GetCharacter(inviteeName, false);
			Guild guild = inviter.Guild;

			if (Guild.CheckInvite(inviter, invitee, inviteeName) == GuildResult.SUCCESS)
			{
				var inviteRelation = RelationMgr.CreateRelation(inviter.EntityId.Low,
																invitee.EntityId.Low, CharacterRelationType.GuildInvite);

				RelationMgr.Instance.AddRelation(inviteRelation);

				SendResult(inviter.Client, GuildCommandId.INVITE, invitee.Name, GuildResult.SUCCESS);

				guild.EventLog.AddInviteEvent(inviter.EntityId.Low, invitee.EntityId.Low);

				// Target has been invited
				SendGuildInvite(invitee.Client, inviter);
			}
		}

		/// <summary>
		/// Handles an incoming accept on guild invite request
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[PacketHandler(RealmServerOpCode.CMSG_GUILD_ACCEPT)]
		public static void Accept(IRealmClient client, RealmPacketIn packet)
		{
			Character invitee = client.ActiveCharacter;

			var listInviters = RelationMgr.Instance.GetPassiveRelations(invitee.EntityId.Low,
																		CharacterRelationType.GuildInvite);

			var invite = listInviters.FirstOrDefault();

			if (invite == null)
				return;

			Character inviter = World.GetCharacter(invite.CharacterId);
			if (inviter == null)
				return;

			//Removes the guild invite relation between the inviter and invitee
			RelationMgr.Instance.RemoveRelation(invite);

			GuildMember guildMember = inviter.GuildMember;

			if (guildMember == null)
				return;

			var guild = guildMember.Guild;
			//Add the invitee to the guild
			guild.AddMember(invitee);
		}

		/// <summary>
		/// Handles an incoming decline on guild invite request
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[PacketHandler(RealmServerOpCode.CMSG_GUILD_DECLINE)]
		public static void Decline(IRealmClient client, RealmPacketIn packet)
		{
			Character invitee = client.ActiveCharacter;

			var listInviters = RelationMgr.Instance.GetPassiveRelations(invitee.EntityId.Low,
																		CharacterRelationType.GuildInvite);

			var invite = listInviters.FirstOrDefault();

			if (invite == null)
				return;

			Character inviter = World.GetCharacter(invite.CharacterId);
			if (inviter == null)
				return;

			//Removes the guild invite relation between the inviter and invitee
			RelationMgr.Instance.RemoveRelation(invite);

			SendGuildDecline(inviter.Client, invitee.Name);
		}

		/// <summary>
		/// Handles an incoming guild leave request
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[PacketHandler(RealmServerOpCode.CMSG_GUILD_LEAVE)]
		public static void Leave(IRealmClient client, RealmPacketIn packet)
		{
			var guildMember = client.ActiveCharacter.GuildMember;

			if (guildMember == null)
			{
				SendResult(client, GuildCommandId.QUIT, GuildResult.PLAYER_NOT_IN_GUILD);
			}
			else
			{
				if (guildMember.Rank.RankIndex != 0)
				{
					guildMember.LeaveGuild();
				}
			}
		}

		/// <summary>
		/// Handles an incoming guild member remove request
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[PacketHandler(RealmServerOpCode.CMSG_GUILD_REMOVE)]
		public static void Remove(IRealmClient client, RealmPacketIn packet)
		{
			string targetName = packet.ReadCString();

			Character remover = client.ActiveCharacter;

			GuildMember targetGM;

			if (Guild.CheckAction(remover, targetName, out targetGM,
					  GuildCommandId.CREATE, GuildPrivileges.REMOVE, false) == GuildResult.SUCCESS)
			{
				var guild = remover.Guild;
				var removerGM = remover.GuildMember;

				if (targetGM.Rank.RankIndex == 0 || targetGM.Rank.RankIndex < removerGM.Rank.RankIndex)
					return;

				SendEventToGuild(guild, GuildEvents.REMOVED, removerGM, targetGM);

				guild.EventLog.AddRemoveEvent(removerGM.Id, targetGM.Id);

				guild.RemoveMember(targetGM, false);
			}
		}
		#endregion

		#region Guild Information (MOTD, guild info, member notes)
		/// <summary>
		/// Handles an incoming guild information (one in guild roster) change request
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[PacketHandler(RealmServerOpCode.CMSG_GUILD_INFO_TEXT)]
		public static void ChangeInfo(IRealmClient client, RealmPacketIn packet)
		{
			var newInfo = packet.ReadCString();
			var guild = Guild.CheckPrivs(client.ActiveCharacter, GuildCommandId.INVITE, GuildPrivileges.EGUILDINFO);

			if (guild != null)
			{
				guild.Info = newInfo;
			}
		}

		/// <summary>
		/// Handles an incoming guild information (/ginfo) request
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[PacketHandler(RealmServerOpCode.CMSG_GUILD_INFO)]
		public static void InfoRequest(IRealmClient client, RealmPacketIn packet)
		{
			var guildMember = client.ActiveCharacter.GuildMember;

			if (guildMember == null)
			{
				SendResult(client, GuildCommandId.CREATE, GuildResult.PLAYER_NOT_IN_GUILD);
			}
			else
			{
				SendGuildInformation(client, guildMember.Guild);
			}
		}

		/// <summary>
		/// Handles an incoming guild MOTD change request
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[PacketHandler(RealmServerOpCode.CMSG_GUILD_MOTD)]
		public static void ChangeMOTD(IRealmClient client, RealmPacketIn packet)
		{
			var newMotd = packet.ReadCString();
			var guild = Guild.CheckPrivs(client.ActiveCharacter, GuildCommandId.INVITE, GuildPrivileges.SETMOTD);
			if (guild != null)
			{
				guild.MOTD = newMotd;
			}
		}

		/// <summary>
		/// Handles an incoming public guild note change request
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[PacketHandler(RealmServerOpCode.CMSG_GUILD_SET_PUBLIC_NOTE)]
		public static void ChangePublicNote(IRealmClient client, RealmPacketIn packet)
		{
			string targetName = packet.ReadCString();
			string newNote = packet.ReadCString();

			Character reqChar = client.ActiveCharacter;
			Character targetChar = World.GetCharacter(targetName, false);

			if (Guild.CheckAction(reqChar, targetChar,
								  targetName, GuildCommandId.MEMBER, GuildPrivileges.EPNOTE, true) == GuildResult.SUCCESS)
			{
				var member = targetChar.GuildMember;

				member.PublicNote = newNote;

				SendResult(client, GuildCommandId.PUBLIC_NOTE_CHANGED, targetName, GuildResult.SUCCESS);
			}
		}

		/// <summary>
		/// Handles an incoming officer guild note change request
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[PacketHandler(RealmServerOpCode.CMSG_GUILD_SET_OFFICER_NOTE)]
		public static void ChangeOfficerNote(IRealmClient client, RealmPacketIn packet)
		{
			string targetName = packet.ReadCString();
			string newNote = packet.ReadCString();

			Character reqChar = client.ActiveCharacter;
			Character targetChar = World.GetCharacter(targetName, false);

			if (Guild.CheckAction(reqChar, targetChar,
					  targetName, GuildCommandId.MEMBER, GuildPrivileges.EOFFNOTE, true) == GuildResult.SUCCESS)
			{
				var member = targetChar.GuildMember;

				member.OfficerNote = newNote;

				SendResult(client, GuildCommandId.OFFICER_NOTE_CHANGED, targetName, GuildResult.SUCCESS);
			}
		}
		#endregion

		#region GuildBank, Emblems, GuildBankLog
		[PacketHandler(RealmServerOpCode.MSG_GUILD_BANK_MONEY_WITHDRAWN)]
		public static void HandleGuildBankMoneyWithdrawnRequest(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			if (chr.Guild != null && chr.GuildMember != null)
			{
				var amt = chr.GuildMember.BankMoneyWithdrawlAllowance;
				SendMemberRemainingDailyWithdrawlAllowance(chr, amt);
			}
		}

		[PacketHandler(RealmServerOpCode.MSG_SAVE_GUILD_EMBLEM)]
		public static void HandleSetGuildTabard(IRealmClient client, RealmPacketIn packet)
		{
			var vendorEntityId = packet.ReadEntityId();
			var emblemStyle = packet.ReadUInt32();
			var emblemColor = packet.ReadUInt32();
			var borderStyle = packet.ReadUInt32();
			var borderColor = packet.ReadUInt32();
			var backgroundColor = packet.ReadUInt32();

			var chr = client.ActiveCharacter;
			var vendor = chr.Map.GetObject(vendorEntityId) as NPC;
			var tabard = new GuildTabard()
			{
				BackgroundColor = (int)backgroundColor,
				BorderColor = (int)borderColor,
				BorderStyle = (int)borderStyle,
				EmblemColor = (int)emblemColor,
				EmblemStyle = (int)emblemStyle
			};

			if (chr.Guild != null && chr.GuildMember != null)
			{
				chr.Guild.TrySetTabard(chr.GuildMember, vendor, tabard);
			}
			else
			{
				//"You are not part of a guild!"
				SendTabardResult(chr, GuildTabardResult.NoGuild);
			}
		}

		[PacketHandler(RealmServerOpCode.MSG_GUILD_PERMISSIONS)]
		public static void HandleGuildPermissionsQuery(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;

			if (chr.Guild != null && chr.GuildMember != null)
			{
				SendGuildBankPermissions(chr);
			}
		}

		/// <summary>
		/// Called when the GuildBank vault GameObject is clicked
		/// </summary>
		[PacketHandler(RealmServerOpCode.CMSG_GUILD_BANKER_ACTIVATE)]
		public static void HandleGuildBankActivate(IRealmClient client, RealmPacketIn packet)
		{
			var bankEntityId = packet.ReadEntityId();
			var unkown = packet.ReadByte();

			var chr = client.ActiveCharacter;
			var bank = chr.Map.GetObject(bankEntityId) as GameObject;

			if (chr.Guild != null && chr.GuildMember != null)
			{
				SendGuildBankTabNames(chr, bank);
			}
			else
			{
				SendResult(chr, GuildCommandId.BANK, GuildResult.PLAYER_NOT_IN_GUILD);
			}
		}

		[PacketHandler(RealmServerOpCode.CMSG_GUILD_BANK_QUERY_TAB)]
		public static void HandleGuildBankTabQuery(IRealmClient client, RealmPacketIn packet)
		{
			var bankEntityId = packet.ReadEntityId();
			var tabId = packet.ReadByte();
			var unknown = packet.ReadByte();

			var chr = client.ActiveCharacter;
			var bank = chr.Map.GetObject(bankEntityId) as GameObject;

			if (chr.Guild != null && chr.GuildMember != null)
			{
				GetBankTabContent(chr, bank, tabId);
			}
		}

		[PacketHandler(RealmServerOpCode.CMSG_GUILD_BANK_DEPOSIT_MONEY)]
		public static void HandleGuildBankDepositMoney(IRealmClient client, RealmPacketIn packet)
		{
			var bankEntityId = packet.ReadEntityId();
			var deposit = packet.ReadUInt32();

			var chr = client.ActiveCharacter;
			var bank = chr.Map.GetObject(bankEntityId) as GameObject;

			if (chr.Guild != null && chr.GuildMember != null)
			{
				chr.Guild.Bank.DepositMoney(chr, bank, deposit);
			}
		}

		[PacketHandler(RealmServerOpCode.CMSG_GUILD_BANK_WITHDRAW_MONEY)]
		public static void HandleGuildBankWithdrawMoney(IRealmClient client, RealmPacketIn packet)
		{
			var bankEntityId = packet.ReadEntityId();
			var withdrawl = packet.ReadUInt32();

			var chr = client.ActiveCharacter;
			var bank = chr.Map.GetObject(bankEntityId) as GameObject;

			if (chr.Guild != null && chr.GuildMember != null)
			{
				chr.Guild.Bank.WithdrawMoney(chr, bank, withdrawl);
			}
		}

		[PacketHandler(RealmServerOpCode.CMSG_GUILD_BANK_SWAP_ITEMS)]
		public static void HandleGuildBankSwapItems(IRealmClient client, RealmPacketIn packet)
		{
			var bankEntityId = packet.ReadEntityId();
			var isBankToBank = packet.ReadByte() > 0 ? true : false;

			var toBankTab = (byte)1;
			var toTabSlot = (byte)1;
			var isAutoStore = false;
			var autoStoreCount = (byte)0;
			var bagSlot = (byte)0;
			var slot = (byte)0;
			var unknown1 = (uint)1;
			var fromBankTab = (byte)0;
			var fromTabSlot = (byte)0;
			var itemEntryId = (uint)0;
			var unknown2 = (byte)1;
			var isBankToChar = true;
			var amount = (byte)0;

			if (!isBankToBank)
			{
				fromBankTab = packet.ReadByte();
				fromTabSlot = packet.ReadByte();
				itemEntryId = packet.ReadUInt32();
				isAutoStore = packet.ReadByte() > 0 ? true : false;
				autoStoreCount = (byte)0;
				if (isAutoStore)
				{
					autoStoreCount = packet.ReadByte();
				}

				bagSlot = packet.ReadByte();
				slot = packet.ReadByte();

				if (!isAutoStore)
				{
					isBankToChar = packet.ReadByte() > 0 ? true : false;
					amount = packet.ReadByte();
				}

				if ((fromTabSlot >= GuildMgr.MAX_BANK_TAB_SLOTS) && fromTabSlot != 0xFF) return;
			}
			else
			{
				toBankTab = packet.ReadByte();
				toTabSlot = packet.ReadByte();
				unknown1 = packet.ReadUInt32();
				fromBankTab = packet.ReadByte();
				fromTabSlot = packet.ReadByte();
				itemEntryId = packet.ReadUInt32();
				unknown2 = packet.ReadByte();
				amount = packet.ReadByte();

				if (toTabSlot >= GuildMgr.MAX_BANK_TAB_SLOTS) return;
				if ((toBankTab == fromBankTab) && (toTabSlot == fromTabSlot)) return;
			}

			var chr = client.ActiveCharacter;
			var bank = chr.Map.GetObject(bankEntityId) as GameObject;
			if (chr.Guild == null) return;

			if (isBankToBank)
			{
				chr.Guild.Bank.SwapItemsManualBankToBank(chr, bank, fromBankTab, fromTabSlot, toBankTab,
															 toTabSlot, itemEntryId, amount);
			}
			else if (isBankToChar)
			{
				if (isAutoStore)
				{
					chr.Guild.Bank.SwapItemsAutoStoreBankToChar(chr, bank, fromBankTab, fromTabSlot, itemEntryId,
																autoStoreCount);
				}
				else
				{
					chr.Guild.Bank.SwapItemsManualBankToChar(chr, bank, fromBankTab, fromTabSlot, bagSlot, slot,
															 itemEntryId, amount);
				}
			}
			else
			{
				if (isAutoStore)
				{
					chr.Guild.Bank.SwapItemsAutoStoreCharToBank(chr, bank, fromBankTab, bagSlot, slot, itemEntryId, autoStoreCount);
				}
				else
				{
					chr.Guild.Bank.SwapItemsManualCharToBank(chr, bank, bagSlot, slot, itemEntryId, fromBankTab,
															 fromTabSlot, amount);
				}
			}
		}

		[PacketHandler(RealmServerOpCode.CMSG_GUILD_BANK_BUY_TAB)]
		public static void HandleGuildBankBuyTab(IRealmClient client, RealmPacketIn packet)
		{
			var bankEntityId = packet.ReadEntityId();
			var newTabId = packet.ReadByte();

			var chr = client.ActiveCharacter;
			var bank = chr.Map.GetObject(bankEntityId) as GameObject;

			if (chr.Guild != null && chr.GuildMember != null)
			{
				chr.Guild.Bank.BuyTab(chr, bank, newTabId);
			}
		}

		[PacketHandler(RealmServerOpCode.CMSG_GUILD_BANK_UPDATE_TAB)]
		public static void HandleGuildBankModifyTabInfo(IRealmClient client, RealmPacketIn packet)
		{
			var bankEntityId = packet.ReadEntityId();
			var tabId = packet.ReadByte();
			var newName = packet.ReadCString();
			var newIcon = packet.ReadCString();

			if (newName.Length == 0) return;
			if (newIcon.Length == 0) return;

			var chr = client.ActiveCharacter;
			var bank = chr.Map.GetObject(bankEntityId) as GameObject;

			if (chr.Guild != null && chr.GuildMember != null)
			{
				chr.Guild.Bank.ModifyTabInfo(chr, bank, tabId, newName, newIcon);
			}
		}

		[PacketHandler(RealmServerOpCode.MSG_QUERY_GUILD_BANK_TEXT)]
		public static void HandleQueryGuildBankTabText(IRealmClient client, RealmPacketIn packet)
		{
			var tabId = packet.ReadByte();

			var chr = client.ActiveCharacter;
			if (chr.Guild != null && chr.GuildMember != null)
			{
				chr.Guild.Bank.GetBankTabText(chr, tabId);
			}
		}

		[PacketHandler(RealmServerOpCode.CMSG_SET_GUILD_BANK_TEXT)]
		public static void HandleSetGuildBankTabText(IRealmClient client, RealmPacketIn packet)
		{
			var tabId = packet.ReadByte();
			var newText = packet.ReadCString();

			var chr = client.ActiveCharacter;
			if (chr.Guild != null && chr.GuildMember != null)
			{
				chr.Guild.Bank.SetBankTabText(chr, tabId, newText);
			}
		}

		[PacketHandler(RealmServerOpCode.MSG_GUILD_BANK_LOG_QUERY)]
		public static void HandleQueryGuildBankLog(IRealmClient client, RealmPacketIn packet)
		{
			var tabId = packet.ReadByte();

			var chr = client.ActiveCharacter;
			if (chr.Guild != null && chr.GuildMember != null)
			{
				chr.Guild.Bank.QueryBankLog(chr, tabId);
			}
		}
		#endregion

		#region Ranks (adding/removing/changing ranks, promoting/demoting, changing leader)
		/// <summary>
		/// Handles an incoming add rank request
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[PacketHandler(RealmServerOpCode.CMSG_GUILD_ADD_RANK)]
		public static void AddRank(IRealmClient client, RealmPacketIn packet)
		{
			var rankName = packet.ReadCString().Trim();
			if (rankName.Length < 2 || rankName.Length > GuildMgr.MaxGuildRankNameLength)
				return;

			var guild = Guild.CheckPrivs(client.ActiveCharacter, GuildCommandId.CREATE, GuildPrivileges.ALL);
			if (guild != null)
			{
				guild.AddRank(rankName, GuildPrivileges.DEFAULT, true);
			}
		}

		/// <summary>
		/// Handles an incoming rank remove request
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[PacketHandler(RealmServerOpCode.CMSG_GUILD_DEL_RANK)]
		public static void DeleteRank(IRealmClient client, RealmPacketIn packet)
		{
			var guild = Guild.CheckPrivs(client.ActiveCharacter, GuildCommandId.PROMOTE, GuildPrivileges.ALL);
			if (guild != null)
			{
				guild.RemoveRank(true);
			}
		}

		/// <summary>
		/// Handles an incoming rank change request
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[PacketHandler(RealmServerOpCode.CMSG_GUILD_RANK)]
		public static void ChangeRank(IRealmClient client, RealmPacketIn packet)
		{
			int rankId = packet.ReadInt32();
			var newPrivileges = (GuildPrivileges)packet.ReadUInt32();
			string newName = packet.ReadCString();

			// TODO: needs lots of additional checks, such as staff checks and in case of demoting even more
			var guild = Guild.CheckPrivs(client.ActiveCharacter, GuildCommandId.PROMOTE, GuildPrivileges.PROMOTE | newPrivileges);
			if (guild != null)
			{
				guild.ChangeRank(rankId, newName, newPrivileges, true);
			}
		}

		/// <summary>
		/// Handles an incoming guild member promote request
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[PacketHandler(RealmServerOpCode.CMSG_GUILD_PROMOTE)]
		public static void Promote(IRealmClient client, RealmPacketIn packet)
		{
			var targetName = packet.ReadCString();
			var reqChar = client.ActiveCharacter;

			GuildMember target;

			if (Guild.CheckAction(reqChar, targetName, out target,
				GuildCommandId.PROMOTE, GuildPrivileges.PROMOTE, false) == GuildResult.SUCCESS)
			{
				var requester = reqChar.GuildMember;
				var guild = target.Guild;

				if (guild.Promote(target))
				{
					SendEventToGuild(guild, GuildEvents.PROMOTION, target, requester);

					guild.EventLog.AddPromoteEvent(requester.Id, target.Id, target.RankId);
				}
			}
		}

		/// <summary>
		/// Handles an incoming guild member demote request
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[PacketHandler(RealmServerOpCode.CMSG_GUILD_DEMOTE)]
		public static void Demote(IRealmClient client, RealmPacketIn packet)
		{
			var targetName = packet.ReadCString();
			var reqChar = client.ActiveCharacter;
			GuildMember target;

			if (Guild.CheckAction(reqChar, targetName, out target,
				GuildCommandId.PROMOTE, GuildPrivileges.DEMOTE, false) == GuildResult.SUCCESS)
			{
				var requester = reqChar.GuildMember;
				var guild = target.Guild;
				if (guild.Demote(target))
				{
					SendEventToGuild(guild, GuildEvents.DEMOTION, target, requester);

					guild.EventLog.AddPromoteEvent(requester.Id, target.Id, target.RankId);
				}
			}
		}
		#endregion

		#region Control (creating, disbanding, changing leader)
		/// <summary>
		/// Handles an incoming guild disband request
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[PacketHandler(RealmServerOpCode.CMSG_GUILD_DISBAND)]
		public static void Disband(IRealmClient client, RealmPacketIn packet)
		{
			var guild = Guild.CheckPrivs(client.ActiveCharacter, GuildCommandId.CREATE, GuildPrivileges.ALL);
			if (guild != null)
			{
				guild.Disband();
			}
		}

		/// <summary>
		/// Handles an incoming guild leader change request
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[PacketHandler(RealmServerOpCode.CMSG_GUILD_LEADER)]
		public static void ChangeLeader(IRealmClient client, RealmPacketIn packet)
		{
			string targetName = packet.ReadCString();

			Character reqChar = client.ActiveCharacter;
			Character targetChar = World.GetCharacter(targetName, false);

			if (Guild.CheckIsLeader(reqChar, targetChar, GuildCommandId.CREATE, targetName) == GuildResult.SUCCESS)
			{
				var guild = reqChar.Guild;

				guild.SyncRoot.Enter();
				try
				{
					reqChar.Guild.ChangeLeader(targetChar.GuildMember);
				}
				finally
				{
					guild.SyncRoot.Exit();
				}
			}
		}
		#endregion

		/// <summary>
		/// Sends a guild query response to the client.
		/// </summary>
		/// <param name="client">the client to send to</param>
		/// <param name="guild">guild to be sent</param>
		public static void SendGuildQueryResponse(IPacketReceiver client, Guild guild)
		{
			using (var packet = CreateGuildQueryResponsePacket(guild))
			{
				client.Send(packet);
			}
		}

		/// <summary>
		/// Sends a guild query response to all member of this guild
		/// </summary>
		/// <param name="guild">guild to be sent</param>
		public static void SendGuildQueryToGuildMembers(Guild guild)
		{
			using (var packet = CreateGuildQueryResponsePacket(guild))
			{
				guild.Broadcast(packet);
			}
		}

		/// <summary>
		/// Sends a guild invite packet to a client
		/// </summary>
		/// <param name="client">the client to send to</param>
		/// <param name="inviter">inviter</param>
		public static void SendGuildInvite(IPacketReceiver client, Character inviter)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GUILD_INVITE))
			{
				packet.WriteCString(inviter.Name);
				packet.WriteCString(inviter.GuildMember.Guild.Name);
				client.Send(packet);
			}
		}

		private static RealmPacketOut CreateGuildQueryResponsePacket(Guild guild)
		{
			var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GUILD_QUERY_RESPONSE);

			packet.WriteUInt((byte)guild.Id);
			packet.WriteCString(guild.Name);

			var guildRanks = guild.Ranks;

			if (guildRanks.Length >= GuildMgr.MAX_GUILD_RANKS)
				return null;

			for (int i = 0; i < guildRanks.Length; i++)
			{
				packet.WriteCString(guildRanks[i].Name);
			}

			packet.Fill(0, GuildMgr.MAX_GUILD_RANKS - guildRanks.Length);

			packet.Write(guild.Tabard.EmblemStyle);
			packet.Write(guild.Tabard.EmblemColor);
			packet.Write(guild.Tabard.BorderStyle);
			packet.Write(guild.Tabard.BorderColor);
			packet.Write(guild.Tabard.BackgroundColor);
			packet.Write(0); // NEW 3.0.2

			return packet;
		}

		/// <summary>
		/// Sends guild invitation decline
		/// </summary>
		/// <param name="client">the client to send to</param>
		/// <param name="decliner">player who has declined your request</param>
		public static void SendGuildDecline(IPacketReceiver client, string decliner)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GUILD_DECLINE, decliner.Length + 1))
			{
				packet.WriteCString(decliner);
				client.Send(packet);
			}
		}

		/// <summary>
		/// Sends guild roster
		/// </summary>
		/// <param name="client">the client to send to</param>
		/// <param name="guild">guild</param>
		public static void SendGuildRoster(IPacketReceiver client, Guild guild, bool showOfficerNotes)
		{
			using (var packet = CreateGuildRosterPacket(guild, showOfficerNotes))
			{
				client.Send(packet);
			}
		}

		/// <summary>
		/// Sends guild roster to guild members
		/// </summary>
		/// <param name="guild">guild</param>
		public static void SendGuildRosterToGuildMembers(Guild guild)
		{
			using (var guildRosterNormal = CreateGuildRosterPacket(guild, false))
			{
				using (var guildRosterOfficer = CreateGuildRosterPacket(guild, true))
				{
					foreach (var member in guild.Members.Values)
					{
						if (member.Character == null)
							continue;

						if (member.HasRight(GuildPrivileges.VIEWOFFNOTE))
						{
							member.Character.Send(guildRosterOfficer);
						}
						else
						{
							member.Character.Send(guildRosterNormal);
						}
					}
				}
			}
		}

		private static RealmPacketOut CreateGuildRosterPacket(Guild guild, bool showOfficerNotes)
		{
			var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GUILD_ROSTER);

			packet.Write(guild.MemberCount);

			if (guild.MOTD != null)
			{
				packet.WriteCString(guild.MOTD);
			}
			else
			{
				packet.WriteByte(0);
			}

			if (guild.Info != null)
			{
				packet.WriteCString(guild.Info);
			}
			else
			{
				packet.WriteByte(0);
			}

			var guildRanks = guild.Ranks;

			packet.Write(guildRanks.Length);

			for (var i = 0; i < guildRanks.Length; i++)
			{
				var rank = guildRanks[i];

				packet.Write((uint)rank.Privileges);

				packet.Write(rank.DailyBankMoneyAllowance);

				for (var j = 0; j < GuildMgr.MAX_BANK_TABS; j++)
				{
					var tabPrivileges = rank.BankTabRights[j];
					packet.WriteInt((uint)tabPrivileges.Privileges);
					packet.WriteInt(tabPrivileges.WithdrawlAllowance);
				}
			}

			Character chr;
			foreach (var member in guild.Members.Values)
			{
				chr = member.Character;

				packet.Write(EntityId.GetPlayerId(member.Id));

				if (chr != null)
					packet.WriteByte((byte)chr.Status);
				else
					packet.WriteByte((byte)CharacterStatus.OFFLINE);

				packet.WriteCString(member.Name);
				packet.Write(member.Rank.RankIndex);

				packet.Write((byte)member.Level);
				packet.Write((byte)member.Class);
				packet.Write((byte)0);
				packet.Write(member.ZoneId);

				if (chr == null)
				{
					packet.Write((float)(DateTime.Now - member.LastLogin).TotalDays);
				}

				if (member.PublicNote != null)
					packet.WriteCString(member.PublicNote);
				else
					packet.Write((byte)0);

				if (showOfficerNotes && member.OfficerNote != null)
					packet.WriteCString(member.OfficerNote);
				else
					packet.Write((byte)0);
			}

			return packet;
		}

		/// <summary>
		/// Sends a guild information to a client
		/// </summary>
		/// <param name="client">the client to send to</param>
		/// <param name="guild">guild</param>
		public static void SendGuildInformation(IPacketReceiver client, Guild guild)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GUILD_INFO))
			{
				packet.WriteCString(guild.Name);
				packet.WriteInt((byte)guild.Created.Year);
				packet.WriteInt((byte)guild.Created.Month);
				packet.WriteInt((byte)guild.Created.Day);
				packet.WriteInt((byte)guild.MemberCount);
				packet.WriteInt((byte)guild.MemberCount); // number of accounts

				client.Send(packet);
			}
		}

		/// <summary>
		/// Sends result of actions connected with guilds
		/// </summary>
		/// <param name="client">the client to send to</param>
		/// <param name="commandId">command executed</param>
		/// <param name="name">name of player event has happened to</param>
		/// <param name="resultCode">The <see cref="GuildResult"/> result code</param>
		public static void SendResult(IPacketReceiver client, GuildCommandId commandId, string name,
									  GuildResult resultCode)
		{
			// TODO: change params to enums
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GUILD_COMMAND_RESULT))
			{
				packet.WriteUInt((uint)commandId);
				packet.WriteCString(name);
				packet.WriteUInt((uint)resultCode);

				client.Send(packet);
			}
		}

		/// <summary>
		/// Sends result of actions connected with guilds
		/// </summary>
		/// <param name="client">the client to send to</param>
		/// <param name="commandId">command executed</param>
		/// <param name="resultCode">The <see cref="GuildResult"/> result code</param>
		public static void SendResult(IPacketReceiver client, GuildCommandId commandId, GuildResult resultCode)
		{
			SendResult(client, commandId, string.Empty, resultCode);
		}

		#region Events
		/// <summary>
		/// Sends events connected with guilds
		/// </summary>
		/// <param name="client">the client to send to</param>
		/// <param name="guild">guild</param>
		/// <param name="guildEvent">event that happened</param>
		/// <param name="affectedMember">The <see cref="GuildMember"/> which was affected</param>
		public static void SendEvent(IPacketReceiver client, Guild guild, GuildEvents guildEvent, GuildMember affectedMember)
		{
			using (RealmPacketOut packet = CreateEventPacket(guild, guildEvent, affectedMember))
			{
				client.Send(packet);
			}
		}

		/// <summary>
		/// Sends event connected with guilds to all guild members
		/// </summary>
		/// <param name="guild">guild</param>
		/// <param name="guildEvent">event that happened</param>
		public static void SendEventToGuild(Guild guild, GuildEvents guildEvent)
		{
			using (var packet = CreateEventPacket(guild, guildEvent, null))
			{
				guild.Broadcast(packet);
			}
		}

		/// <summary>
		/// Sends event connected with guilds to all guild members. Use this one for promotion/demotion events.
		/// </summary>
		/// <param name="guild">guild</param>
		/// <param name="guildEvent">event that happened</param>
		/// <param name="affectedMember">The <see cref="GuildMember"/> which was affected</param>
		/// <param name="influencer">one who caused this event</param>
		public static void SendEventToGuild(Guild guild, GuildEvents guildEvent, GuildMember affectedMember, GuildMember influencer)
		{
			using (var packet = CreateEventPacket(guild, guildEvent, affectedMember, influencer))
			{
				guild.Broadcast(packet);
			}
		}

		/// <summary>
		/// Sends event connected with guilds to all guild members
		/// </summary>
		/// <param name="guild">guild</param>
		/// <param name="guildEvent">event that happened</param>
		/// <param name="affectedMember">The <see cref="GuildMember"/> which was affected</param>
		public static void SendEventToGuild(Guild guild, GuildEvents guildEvent, GuildMember affectedMember)
		{
			using (var packet = CreateEventPacket(guild, guildEvent, affectedMember))
			{
				guild.Broadcast(packet);
			}
		}

		/// <summary>
		/// Sends event connected with guilds to all guild members, except one
		/// </summary>
		/// <param name="guild">guild</param>
		/// <param name="guildEvent">event that happened</param>
		/// <param name="ignoredCharacter">character to be ignored</param>
		public static void SendEventToGuild(Guild guild, GuildEvents guildEvent, Character ignoredCharacter)
		{
			using (RealmPacketOut packet = CreateEventPacket(guild, guildEvent, null))
			{
				guild.Broadcast(packet, ignoredCharacter);
			}
		}

		/// <summary>
		/// Sends event connected with guilds to all guild members, except one
		/// </summary>
		/// <param name="guild">guild</param>
		/// <param name="guildEvent">event that happened</param>
		/// <param name="affectedMember">The <see cref="GuildMember"/> which was affected</param>
		/// <param name="ignoredCharacter">character to be ignored</param>
		public static void SendEventToGuild(Guild guild, GuildEvents guildEvent, GuildMember affectedMember, Character ignoredCharacter)
		{
			using (RealmPacketOut packet = CreateEventPacket(guild, guildEvent, affectedMember))
			{
				guild.Broadcast(packet, ignoredCharacter);
			}
		}

		private static RealmPacketOut CreateEventPacket(Guild guild, GuildEvents guildEvent, GuildMember affectedMember)
		{
			return CreateEventPacket(guild, guildEvent, affectedMember, null);
		}

		/// <summary>
		/// TODO: Fix for 3.3
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="guildEvent"></param>
		/// <param name="affectedMember"></param>
		/// <param name="influencer"></param>
		/// <returns></returns>
		private static RealmPacketOut CreateEventPacket(Guild guild, GuildEvents guildEvent, GuildMember affectedMember, GuildMember influencer)
		{
			var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GUILD_EVENT);

			packet.WriteByte((byte)guildEvent);

			// Next byte is strings count

			switch (guildEvent)
			{
				case GuildEvents.PROMOTION:
				case GuildEvents.DEMOTION:
					packet.Write((byte)3);
					packet.WriteCString(influencer.Name);
					packet.WriteCString(affectedMember.Name);
					packet.WriteCString(affectedMember.Rank.Name);
					break;

				case GuildEvents.MOTD:
					if (guild.MOTD != null)
					{
						packet.Write((byte)1);
						packet.WriteCString(guild.MOTD);
					}
					else
					{
						packet.Write((byte)0);
					}
					break;

				case GuildEvents.JOINED:
				case GuildEvents.LEFT:
				case GuildEvents.ONLINE:
				case GuildEvents.OFFLINE:
					packet.Write((byte)1);
					packet.WriteCString(affectedMember.Name);
					break;

				case GuildEvents.REMOVED:
					packet.Write((byte)2);
					packet.WriteCString(influencer.Name);
					packet.WriteCString(affectedMember.Name);
					break;

				case GuildEvents.LEADER_CHANGED:
					packet.Write((byte)2);
					packet.WriteCString(affectedMember.Name);
					packet.WriteCString(influencer.Name);
					break;

				default:
					packet.Write((byte)0);
					break;
			}

			return packet;
		}

		/// <summary>
		/// Sends a guild log to a client
		/// </summary>
		/// <param name="client">the client to send to</param>
		/// <param name="guild">guild</param>
		public static void SendGuildEventLog(IPacketReceiver client, Guild guild)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_GUILD_EVENT_LOG_QUERY))
			{
				var eventLog = guild.EventLog;

				packet.WriteByte(eventLog.Entries.Count);

				foreach (var entry in eventLog.Entries)
				{
					packet.WriteByte((byte)entry.Type);
					packet.Write(EntityId.GetPlayerId((uint)entry.Character1LowId));

					if (entry.Type != GuildEventLogEntryType.JOIN_GUILD &&
						entry.Type != GuildEventLogEntryType.LEAVE_GUILD)
						packet.Write(EntityId.GetPlayerId((uint)entry.Character2LowId));

					if (entry.Type == GuildEventLogEntryType.PROMOTE_PLAYER ||
						entry.Type == GuildEventLogEntryType.DEMOTE_PLAYER)
						packet.WriteByte(entry.NewRankId);

					packet.Write((DateTime.Now - entry.TimeStamp).Seconds);
				}

				client.Send(packet);
			}
		}
		#endregion

		#region Guild Bank
		public static void SendGuildBankPermissions(Character chr)
		{
			if (chr.Guild == null || chr.GuildMember == null) return;

			var client = chr.Client;
			var member = chr.GuildMember;
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_GUILD_PERMISSIONS))
			{
				packet.Write(member.Rank.RankIndex);
				packet.Write((uint)member.Rank.Privileges);
				packet.Write(member.BankMoneyWithdrawlAllowance);
				packet.Write(chr.Guild.PurchasedBankTabCount);
				for (var i = 0; i < GuildMgr.MAX_BANK_TABS; ++i)
				{
					packet.Write((uint)member.Rank.BankTabRights[i].Privileges);
					packet.Write(member.Rank.BankTabRights[i].WithdrawlAllowance);
				}

				client.Send(packet);
			}
		}

		public static void SendGuildBankTabNames(Character chr, GameObject bank)
		{
			SendGuildBankList(chr, bank, 0, true, false);
		}

		public static void SendGuildBankTabContents(Character chr, GameObject bank, byte tabId)
		{
			SendGuildBankList(chr, bank, tabId, false, true);
		}

		public static void SendGuildBankTabContentUpdateToAll(this Guild guild, byte tabId, int tabSlot)
		{
			guild.SendGuildBankTabContentUpdateToAll(tabId, tabSlot, -1);
		}

		public static void SendGuildBankTabContentUpdateToAll(this Guild guild, byte tabId, int slot1, int slot2)
		{
			guild.SyncRoot.Enter();

			try
			{
				foreach (var member in guild.Members.Values)
				{
					if (!member.Rank.BankTabRights[tabId].Privileges.HasFlag(GuildBankTabPrivileges.ViewTab))
					{
						return;
					}

					using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GUILD_BANK_LIST))
					{
						packet.Write(guild.Money);
						packet.Write(tabId);

						packet.Write(member.Rank.BankTabRights[tabId].WithdrawlAllowance);
						packet.Write(false); // hasTabnames = false;

						var list = new List<int> {
                            slot1
                        };
						if (slot2 != -1)
						{
							list.Add(slot2);
						}

						packet.WriteByte((byte)list.Count);
						foreach (var slot in list)
						{
							packet.Write(slot);
							var item = member.Guild.Bank[tabId][slot];
							packet.Write(item.EntryId);

							var randomPropId = item.RandomProperty;
							packet.Write(randomPropId);
							if (randomPropId != 0)
							{
								packet.Write(item.RandomSuffix);
							}

							packet.Write(item.Amount);
							packet.Write((uint)0);
							packet.Write((byte)0);

							if (item.EnchantIds == null)
							{
								packet.Write((byte)0);
								continue;
							}

							var count = 0;
							var enchantments = new int[3];
							for (var i = 0; i < 3; ++i)
							{
								if (item.EnchantIds[i] == 0) continue;
								count++;
								enchantments[i] = item.EnchantIds[i];
							}

							packet.Write((byte)count);
							for (var i = 0; i < 3; ++i)
							{
								if (enchantments[i] == 0) continue;
								packet.Write((byte)i);
								packet.Write(list[i]);
							}
						} // end foreach

						member.Character.Client.Send(packet);
					} // end using
				}
			}
			finally
			{
				guild.SyncRoot.Exit();
			}
		}

		public static void SendGuildBankMoneyUpdate(Character chr, GameObject bank)
		{
			SendGuildBankList(chr, bank, 0, false, false);
		}

		private static void SendGuildBankList(Character chr, GameObject bank, byte tabId, bool hasTabNames, bool hasItemInfo)
		{
			if (bank == null) return;
			if (chr.Guild == null || chr.GuildMember == null) return;
			if (!bank.CanBeUsedBy(chr)) return;

			var guild = chr.Guild;
			var gBank = guild.Bank;

			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GUILD_BANK_LIST))
			{
				packet.Write(guild.Money);
				packet.Write(tabId);
				packet.Write(chr.GuildMember.Rank.BankTabRights[tabId].WithdrawlAllowance);
				packet.Write(hasTabNames);

				if (hasTabNames)
				{
					if (tabId == 0)
					{
						packet.Write((byte)guild.PurchasedBankTabCount);
						for (var i = 0; i < guild.PurchasedBankTabCount; ++i)
						{
							packet.Write(gBank[i].Name);
							packet.Write(gBank[i].Icon);
						}
					}
				}

				packet.Write(hasItemInfo);
				if (!hasItemInfo)
				{
					chr.Client.Send(packet);
					return;
				}

				var bankTab = gBank[tabId];
				for (var slot = 0; slot < GuildMgr.MAX_BANK_TAB_SLOTS; ++slot)
				{
					var item = (slot < bankTab.ItemRecords.Count) ? bankTab.ItemRecords[slot] : null;

					packet.Write((byte)slot);
					if (item == null)
					{
						packet.Write((uint)0);
						continue;
					}

					packet.Write(item.EntryId);
					var randPropId = item.RandomProperty;
					packet.Write(randPropId);
					if (randPropId > 0)
					{
						packet.Write(item.RandomSuffix);
					}

					packet.Write(item.Amount);
					packet.Write((uint)0);
					packet.Write((byte)0);

					if (item.EnchantIds == null)
					{
						packet.Write((byte)0);
						continue;
					}

					var count = 0;
					var list = new int[3];
					for (var i = 0; i < 3; ++i)
					{
						if (item.EnchantIds[i] == 0) continue;
						count++;
						list[i] = item.EnchantIds[i];
					}

					packet.Write((byte)count);
					for (var i = 0; i < 3; ++i)
					{
						if (list[i] == 0) continue;
						packet.Write((byte)i);
						packet.Write(list[i]);
					}
				}

				chr.Client.Send(packet);
			}
		}

		public static void GetBankTabContent(Character chr, GameObject bank, byte tabId)
		{
			if (bank == null) return;
			if (chr.Guild == null || chr.GuildMember == null) return;
			if (!bank.CanBeUsedBy(chr)) return;

			var bankTab = chr.Guild.Bank[tabId];
			if (bankTab == null) return;
			if (!chr.GuildMember.HasBankTabRight(tabId, GuildBankTabPrivileges.ViewTab)) return;


			SendMemberRemainingDailyWithdrawlAllowance(chr, chr.GuildMember.BankMoneyWithdrawlAllowance);

			SendGuildBankTabContents(chr, bank, tabId);
		}

		public static void SendGuildBankTabText(Character chr, byte tabId, string text)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_QUERY_GUILD_BANK_TEXT))
			{
				packet.Write(tabId);
				packet.Write(text);

				chr.Client.Send(packet);
			}
		}

		public static RealmPacketOut CreateBankTabTextPacket(byte tabId, string text)
		{
			var packet = new RealmPacketOut(RealmServerOpCode.MSG_QUERY_GUILD_BANK_TEXT);
			packet.Write(tabId);
			packet.Write(text);

			return packet;
		}

		public static void SendGuildBankLog(Character chr, GuildBankLog log, byte tabId)
		{
            if (log == null)
                return;
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_GUILD_BANK_LOG_QUERY))
			{
				var entries = log.GetBankLogEntries(tabId);

				packet.Write(tabId);
				packet.Write((byte)entries.Count);
				foreach (var entry in entries)
				{
					packet.Write((byte)entry.Type);
					packet.Write(entry.Actor.EntityId);
					if (entry.Type == GuildBankLogEntryType.DepositMoney ||
						entry.Type == GuildBankLogEntryType.WithdrawMoney ||
						entry.Type == GuildBankLogEntryType.MoneyUsedForRepairs ||
						entry.Type == GuildBankLogEntryType.Unknown1 ||
						entry.Type == GuildBankLogEntryType.Unknown2)
					{
						packet.Write(entry.Money);
					}
					else
					{
						packet.Write(entry.ItemEntryId);
						packet.Write(entry.ItemStackCount);
						if (entry.Type == GuildBankLogEntryType.MoveItem ||
							entry.Type == GuildBankLogEntryType.MoveItem_2)
						{
							packet.Write((byte)entry.DestinationTabId);
						}
					}
					var time = Utility.GetDateTimeToGameTime(DateTime.Now) -
							   Utility.GetDateTimeToGameTime(entry.TimeStamp);
					packet.Write(time);
				}

				chr.Client.Send(packet);
			}
		}

		public static void SendMemberRemainingDailyWithdrawlAllowance(IPacketReceiver client, uint remainingAllowance)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_GUILD_BANK_MONEY_WITHDRAWN))
			{
				packet.Write(remainingAllowance);

				client.Send(packet);
			}
		}
		#endregion

		public static void SendTabardResult(IPacketReceiver client, GuildTabardResult result)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_SAVE_GUILD_EMBLEM))
			{
				packet.Write((uint)result);

				client.Send(packet);
			}
		}
	}
}