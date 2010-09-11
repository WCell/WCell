using System;
using NLog;
using WCell.Constants;
using WCell.Constants.ArenaTeams;
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
