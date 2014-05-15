using System;
using WCell.Constants;
using WCell.Constants.ArenaTeams;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Database;
using WCell.RealmServer.Network;
using WCell.Util;
using WCell.RealmServer.Battlegrounds.Arenas;
using WCell.RealmServer.Global;
using WCell.Util.Logging;
using WCell.RealmServer.Database.Entities;

namespace WCell.RealmServer.Handlers
{
    public static class ArenaTeamHandler
    {
        private static readonly Logger s_log = LogManager.GetCurrentClassLogger();


        [ClientPacketHandler(RealmServerOpCode.CMSG_ARENA_TEAM_QUERY)]
        public static void HandleArenaTeamQuery(IRealmClient client, RealmPacketIn packet)
        {
            var arenaTeamId = packet.ReadUInt32();
            var team = ArenaMgr.GetArenaTeam(arenaTeamId);
            if (team != null)
            {
                SendArenaTeamQueryResponse(client, team);
            }
        }

        [ClientPacketHandler(RealmServerOpCode.CMSG_ARENA_TEAM_ROSTER)]
        public static void HandleArenaTeamRoster(IRealmClient client, RealmPacketIn packet)
        {
            var arenaTeamId = packet.ReadUInt32();
            var team = ArenaMgr.GetArenaTeam(arenaTeamId);
            if (team != null)
            {
                SendArenaTeamRosterResponse(client, team);
            }
        }

        /// <summary>
        /// Sends an arena team query response to the client.
        /// </summary>
        /// <param name="client">the client to send to</param>
        /// <param name="team">arena team to be sent</param>
        public static void SendArenaTeamQueryResponse(IPacketReceiver client, ArenaTeam team)
        {
            using (var packet = CreateArenaTeamQueryResponsePacket(team))
            {
                client.Send(packet);
            }
            using (var packet = CreateArenaTeamStatsResponsePacket(team))
            {
                client.Send(packet);
            }
        }

        public static void SendArenaTeamRosterResponse(IPacketReceiver client, ArenaTeam team)
        {
            using (var packet = CreateArenaTeamRosterResponsePacket(team))
            {
                client.Send(packet);
            }
        }
        private static RealmPacketOut CreateArenaTeamQueryResponsePacket(ArenaTeam team)
		{
			var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ARENA_TEAM_QUERY_RESPONSE, 4*7+team.Name.Length+1);

            packet.WriteUInt((byte)team.Id);
            packet.WriteCString(team.Name);
            packet.WriteUInt(team.Type);

            /* TO-DO : Implement Emblem
             * packet.WriteUInt(team.Emblem.backgroundColor);
            packet.WriteUInt(team.Emblem.emblemStyle);
            packet.WriteUInt(team.Emblem.emblemColor);
            packet.WriteUInt(team.Emblem.borderStyle);
            packet.WriteUInt(team.Emblem.borderColor);
             */

            return packet;
        }

        private static RealmPacketOut CreateArenaTeamStatsResponsePacket(ArenaTeam team)
        {
            var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ARENA_TEAM_STATS, 4*7);

            packet.WriteUInt((byte)team.Id);
            packet.WriteUInt(team.Stats.Rating);
            packet.WriteUInt(team.Stats.GamesWeek);
            packet.WriteUInt(team.Stats.WinsWeek);
            packet.WriteUInt(team.Stats.GamesSeason);
            packet.WriteUInt(team.Stats.WinsSeason);
            packet.WriteUInt(team.Stats.Rank);

            return packet;
        }

        private static RealmPacketOut CreateArenaTeamRosterResponsePacket(ArenaTeam team)
        {
            var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ARENA_TEAM_ROSTER, 100);

            packet.WriteUInt(team.Id);
            packet.WriteByte(0);
            packet.WriteUInt(team.MemberCount);
            packet.WriteUInt(team.Type);

            foreach (var member in team.Members.Values)
            {
                packet.WriteULong(member.Character.EntityId.Full);
                var pl = World.GetCharacter(member.Character.EntityId.Low);
                packet.WriteByte((pl != null) ? 1 : 0);
                packet.WriteCString(member.Character.Name);
                packet.WriteByte((team.Leader == member) ? 0 : 1);
                packet.WriteByte((pl != null) ? pl.Level : 0);
                packet.WriteUInt((uint)member.Class);
                packet.WriteUInt(member.GamesWeek);
                packet.WriteUInt(member.WinsWeek);
                packet.WriteUInt(member.GamesSeason);
                packet.WriteUInt(member.WinsSeason);
                packet.WriteUInt(member.PersonalRating);
                packet.WriteFloat(0.0f);
                packet.WriteFloat(0.0f);
            }
            return packet;
        }
        /// <summary>
        /// Sends result of actions connected with arenas
        /// </summary>
        /// <param name="client">the client to send to</param>
        /// <param name="commandId">command executed</param>
        /// <param name="name">name of player event has happened to</param>
        /// <param name="resultCode">The <see cref="ArenaTeamResult"/> result code</param>
        public static void SendResult(IPacketReceiver client, ArenaTeamCommandId commandId, string team, string player,
                                      ArenaTeamResult resultCode)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ARENA_TEAM_COMMAND_RESULT))
            {
                packet.WriteUInt((uint)commandId);
                packet.WriteCString(team);
                packet.WriteCString(player);
                packet.WriteUInt((uint)resultCode);

                client.Send(packet);
            }
        }

        /// <summary>
        /// Sends result of actions connected with arenas
        /// </summary>
        /// <param name="client">the client to send to</param>
        /// <param name="commandId">command executed</param>
        /// <param name="resultCode">The <see cref="ArenaTeamResult"/> result code</param>
        public static void SendResult(IPacketReceiver client, ArenaTeamCommandId commandId, ArenaTeamResult resultCode)
        {
            SendResult(client, commandId, string.Empty, string.Empty, resultCode);
        }
    }
}
