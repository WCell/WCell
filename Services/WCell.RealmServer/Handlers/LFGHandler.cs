using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
	/// <summary>
	/// When opening the LFG panel, player sends:
	///		CMSG_LFG_GET_PLAYER_INFO
	///		CMSG_LFG_GET_PARTY_INFO
	/// </summary>
	public static class LFGHandler
	{
		[ClientPacketHandler(RealmServerOpCode.CMSG_LFG_JOIN)]
		public static void HandleJoin(IRealmClient client, RealmPacketIn packet)
		{

		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_LFG_LEAVE)]
		public static void HandleLeave(IRealmClient client, RealmPacketIn packet)
		{

		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_LFG_SEARCH_JOIN)]
		public static void HandleSearchJoin(IRealmClient client, RealmPacketIn packet)
		{

		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_LFG_SEARCH_LEAVE)]
		public static void HandleSearchLeave(IRealmClient client, RealmPacketIn packet)
		{

		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_LFG_PROPOSAL_RESPONSE)]
		public static void HandleProposalResponse(IRealmClient client, RealmPacketIn packet)
		{

		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_SET_LFG_COMMENT)]
		public static void HandleSetComment(IRealmClient client, RealmPacketIn packet)
		{

		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_LFG_SET_ROLES)]
		public static void HandleSetRoles(IRealmClient client, RealmPacketIn packet)
		{

		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_LFG_SET_NEEDS)]
		public static void HandleSetNeeds(IRealmClient client, RealmPacketIn packet)
		{

		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_LFG_BOOT_PLAYER_VOTE)]
		public static void HandleBootPlayerVote(IRealmClient client, RealmPacketIn packet)
		{

		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_LFG_GET_PLAYER_INFO)]
		public static void HandleGetPlayerInfo(IRealmClient client, RealmPacketIn packet)
		{

		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_LFG_TELEPORT)]
		public static void HandleTeleport(IRealmClient client, RealmPacketIn packet)
		{

		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_LFG_GET_PARTY_INFO)]
		public static void HandleGetPartyInfo(IRealmClient client, RealmPacketIn packet)
		{

		}


		public static void SendSearchResults()
		{
			// SMSG_LFG_SEARCH_RESULTS
		}

		public static void SendProposalUpdate()
		{
			// SMSG_LFG_PROPOSAL_UPDATE
		}

		public static void SendRoleCheckUpdate()
		{
			// SMSG_LFG_ROLE_CHECK_UPDATE
		}

		public static void SendJoinResult()
		{
			// SMSG_LFG_JOIN_RESULT
		}

		public static void SendQueueStatus()
		{
			// SMSG_LFG_QUEUE_STATUS
		}

		public static void SendUpdatePlayer()
		{
			// SMSG_LFG_UPDATE_PLAYER
		}

		public static void SendUpdateParty()
		{
			// SMSG_LFG_UPDATE_PARTY
		}

		public static void SendUpdateSearch()
		{
			// SMSG_LFG_UPDATE_SEARCH
		}

		public static void SendBootPlayer()
		{
			// SMSG_LFG_BOOT_PLAYER
		}

		public static void SendPlayerInfo()
		{
			// SMSG_LFG_PLAYER_INFO
		}

		public static void SendPartyInfo()
		{
			// SMSG_LFG_PARTY_INFO
		}
	}
}