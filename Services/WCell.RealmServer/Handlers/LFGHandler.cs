using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.LFG;
using WCell.Core;
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
			var roles = packet.ReadUInt32();
			packet.SkipBytes(2);

			var dungeonsCount = packet.ReadByte();
			if (dungeonsCount == 0)
				return;

			for (byte i = 0; i < dungeonsCount; ++i)
			{
				// dungeons id/type
				var packedDungeon = packet.ReadUInt32(); 
				var id = packedDungeon & 0x00FFFFFF;
				var type = packedDungeon & 0xFF000000;
			}

			byte counter2 = packet.ReadByte();
			packet.SkipBytes(counter2); // lua: GetLFGInfoLocal

			string comment = packet.ReadCString();

			//SendLfgJoinResult();
			//SendLfgUpdate();
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_LFG_LEAVE)]
		public static void HandleLeave(IRealmClient client, RealmPacketIn packet)
		{
			var dungeonId = packet.ReadUInt32();
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_LFG_SEARCH_JOIN)]
		public static void HandleSearchJoin(IRealmClient client, RealmPacketIn packet)
		{
			var dungeonId = packet.ReadUInt32();
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_LFG_SEARCH_LEAVE)]
		public static void HandleSearchLeave(IRealmClient client, RealmPacketIn packet)
		{

		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_LFG_PROPOSAL_RESPONSE)]
		public static void HandleProposalResponse(IRealmClient client, RealmPacketIn packet)
		{
			var lfgGroupId = packet.ReadUInt32();
			var accept = packet.ReadBoolean();
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_SET_LFG_COMMENT)]
		public static void HandleSetComment(IRealmClient client, RealmPacketIn packet)
		{
			var comment = packet.ReadCString();
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_LFG_SET_ROLES)]
		public static void HandleSetRoles(IRealmClient client, RealmPacketIn packet)
		{
			var roles = packet.ReadByte();
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_LFG_SET_NEEDS)]
		public static void HandleSetNeeds(IRealmClient client, RealmPacketIn packet)
		{

		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_LFG_BOOT_PLAYER_VOTE)]
		public static void HandleBootPlayerVote(IRealmClient client, RealmPacketIn packet)
		{
			var passVote = packet.ReadBoolean();
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_LFG_GET_PLAYER_INFO)]
		public static void HandleGetPlayerInfo(IRealmClient client, RealmPacketIn packet)
		{
			SendPlayerInfo(client);
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_LFG_TELEPORT)]
		public static void HandleTeleport(IRealmClient client, RealmPacketIn packet)
		{
			var isTeleportingOut = packet.ReadBoolean();
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_LFG_GET_PARTY_INFO)]
		public static void HandleGetPartyInfo(IRealmClient client, RealmPacketIn packet)
		{
			SendPartyInfo(client);
		}

		public static void SendSearchResults(IRealmClient client)
		{
			// SMSG_LFG_SEARCH_RESULTS
			// uint, uint, if (byte) { uint count, for (count) { long} }, uint count2, uint, for (count2) { long, uint flags, if (flags & 0x2) {string}, if (flags & 0x10) {for (3) byte}, if (flags & 0x80) {long, uint}}, uint count3, uint, for (count3) {long, uint flags, if (flags & 0x1) {byte, byte, byte, for (3) byte, uint, uint, uint, uint, uint, uint, float, float, uint, uint, uint, uint, uint, float, uint, uint, uint, uint, uint, uint}, if (flags&0x2) string, if (flags&0x4) byte, if (flags&0x8) long, if (flags&0x10) byte, if (flags&0x20) uint, if (flags&0x40) byte, if (flags& 0x80) {long, uint}}
		}

		public static void SendProposalUpdate(IRealmClient client)
		{
			// SMSG_LFG_PROPOSAL_UPDATE
			// uint, byte, uint, uint, byte, for (byte) {uint, byte, byte, byte, byte}
		}

		public static void SendRoleCheckUpdate(IRealmClient client)
		{
			// SMSG_LFG_ROLE_CHECK_UPDATE
			// uint, byte, for (byte) uint, byte, for (byte) { long, byte, uint, byte, }
		}

		public static void SendJoinResult(IRealmClient client)
		{
			// SMSG_LFG_JOIN_RESULT
			// uint unk, uint, if (unk == 6) { byte count, for (count) long }
		}

		public static void SendQueueStatus(IRealmClient client)
		{
			// SMSG_LFG_QUEUE_STATUS
			// uint dungeon, uint lfgtype, uint, uint, uint, uint, byte, byte, byte, byte
		}

		public static void SendUpdatePlayer(IRealmClient client)
		{
			// SMSG_LFG_UPDATE_PLAYER
			// byte, if (byte) { byte, byte, byte, byte, if (byte) for (byte) uint, string}
		}

		public static void SendUpdateParty(IRealmClient client)
		{
			// SMSG_LFG_UPDATE_PARTY
			// byte, if (byte) { byte, byte, byte, for (3) byte, byte, if (byte) for (byte) uint, string}
		}

		public static void SendUpdateSearch(IRealmClient client)
		{
			// SMSG_LFG_UPDATE_SEARCH
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_LFG_UPDATE_SEARCH))
			{
				packet.Write(true);
				client.Send(packet);
			}
		}

		public static void SendBootPlayer(IRealmClient client)
		{
			// SMSG_LFG_BOOT_PLAYER
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_LFG_BOOT_PROPOSAL_UPDATE))
			{
				packet.WriteByte(true);							// Vote in progress
				packet.WriteByte(false);						// Did player Vote
				packet.WriteByte(true);							// Did player pass the motion
				packet.Write(EntityId.Zero);					// EntityID of player being voted on
				packet.WriteUInt(0);							// Total votes
				packet.WriteUInt(0);							// Count of votes to boot
				packet.WriteUInt(0);							// Time left in seconds
				packet.WriteUInt(0);							// Needed Votes
				packet.Write("Too noobzor for this l33t grpz");	// Kick reason
				client.Send(packet);
			}
		}

		public static void SendPlayerInfo(IRealmClient client)
		{
			// SMSG_LFG_PLAYER_INFO
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_LFG_PARTY_INFO))
			{
				byte dungeonsCount = 0;
				packet.Write(dungeonsCount);
				uint packedDungeonId = 0;
				packet.Write(packedDungeonId);
				//something to do with quests! :\
				packet.WriteByte(false); // is quest complete?
				packet.WriteUInt(0); //quest reward/required money
				packet.WriteUInt(0); //quest reward xp
				packet.WriteUInt(0); //lfg dungeon reward cash
				packet.WriteUInt(0); //lfg dungeon reward xp
				var questItemsCount = 0;
				packet.WriteByte(questItemsCount);
				for (byte i = 0; i < questItemsCount; i++)
				{
					packet.WriteUInt(0); //item id
					packet.WriteUInt(0); //display id
					packet.WriteUInt(0); //count of items
				}

				uint lockCount = 0;
				packet.Write(lockCount);
				for (uint i = 0; i < lockCount; i++)
				{
					packet.Write(packedDungeonId);
					packet.Write((uint)LFGLockStatusType.RaidLocked);
				}

				client.Send(packet);
			}
		}

		public static void SendPartyInfo(IRealmClient client)
		{
			// SMSG_LFG_PARTY_INFO
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_LFG_PARTY_INFO))
			{
				byte count = 0;
				packet.Write(count);
				for(byte i = 0; i < count; i++)
					packet.Write(EntityId.Zero);

				client.Send(packet);
			}
		}
	}
}