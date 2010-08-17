using System;
using NLog;
using WCell.Constants;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Network;
using WCell.Util;
using WCell.RealmServer.ArenaTeams;

namespace WCell.RealmServer.Handlers
{
    public static class ArenaTeamHandler
    {
        private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Sends a arena team query response to the client.
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
            packet.WriteUInt(team.Stats.rating);
            packet.WriteUInt(team.Stats.gamesWeek);
            packet.WriteUInt(team.Stats.winsWeek);
            packet.WriteUInt(team.Stats.gamesSeason);
            packet.WriteUInt(team.Stats.winsSeason);
            packet.WriteUInt(team.Stats.rank);

            return packet;
        }
    }
}
