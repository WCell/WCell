/*************************************************************************
 *
 *   file		    : FactionHandler.cs
 *   copyright		: (C) The WCell Team
 *   email		    : info@wcell.org
 *   last changed	: $LastChangedDate: 2008-02-19 00:18:43 -0800 (Tue, 19 Feb 2008) $
 *   last author	: $LastChangedBy: anonemous $
 *   revision		: $Rev: 148 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    public static class FactionHandler
    {
        /// <summary>
        /// User starts/ends war with a faction
        /// </summary>
        [ClientPacketHandler(RealmServerOpCode.CMSG_SET_FACTION_ATWAR)]
        public static void HandleStartWar(IRealmClient client, RealmPacketIn packet)
        {
            var reputationIndex = (FactionReputationIndex)packet.ReadUInt32();
            var declaredWar = packet.ReadBoolean();
            client.ActiveCharacter.Reputations.DeclareWar(reputationIndex, declaredWar, true);
        }

        /// <summary>
        /// User watches Faction-status
        /// </summary>
        [ClientPacketHandler(RealmServerOpCode.CMSG_SET_WATCHED_FACTION)]
        public static void HandleSetWatchedFaction(IRealmClient client, RealmPacketIn packet)
        {
            var reputationIndex = packet.ReadInt32();

            client.ActiveCharacter.WatchedFaction = reputationIndex;
        }

        /// <summary>
        /// Sets the specified faction to the inactive state
        /// </summary>
        [ClientPacketHandler(RealmServerOpCode.CMSG_SET_FACTION_INACTIVE)]
        public static void HandleStopWatchingFaction(IRealmClient client, RealmPacketIn packet)
        {
            var reputationIndex = (FactionReputationIndex)packet.ReadInt32();
            bool isInactive = packet.ReadBoolean();

            client.ActiveCharacter.Reputations.SetInactive(reputationIndex, isInactive);
        }

        /// <summary>
        /// Makes the given faction visible to the client.
        /// </summary>
        public static void SendVisible(IPacketReceiver client, FactionReputationIndex reputationIndex)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SET_FACTION_VISIBLE, 4))
            {
                packet.Write((int)reputationIndex);

                client.Send(packet);
            }
        }

        /// <summary>
        /// Lets player know they are at war with a certain faction.
        /// </summary>
        public static void SendSetAtWar(IPacketReceiver client, Reputation rep)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SET_FACTION_ATWAR, 5))
            {
                packet.Write((int)rep.Faction.ReputationIndex);

                packet.Write((byte)rep.Flags); // rep flags

                client.Send(packet);
            }
        }

        /// <summary>
        /// Sends a reputation update.
        /// </summary>
        public static void SendReputationStandingUpdate(IPacketReceiver client, Reputation rep)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SET_FACTION_STANDING, 16))
            {
                packet.Write(0f); // Refer-A-Friend bonus reputation
                packet.Write((byte)0);
                packet.Write(1);							// count (we only ever send 1)
                packet.Write((uint)rep.Faction.ReputationIndex);
                packet.Write(rep.Value);

                client.Send(packet);
            }
        }

        /// <summary>
        /// Sends all known factions to the client (only used right after connecting).
        /// </summary>
        public static void SendFactionList(Character chr)
        {
            const int count = 128;
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_INITIALIZE_FACTIONS, (count * 5) + 4))
            {
                packet.Write(count);
                for (var i = 0; i < count; i++)
                {
                    var reps = chr.Reputations;
                    var rep = reps[(FactionReputationIndex)i];
                    if (rep != null)
                    {
                        packet.Write((byte)rep.Flags);
                        packet.Write(rep.Value);
                    }
                    else
                    {
                        packet.Write((byte)0);
                        packet.Write(0);
                    }
                }

                chr.Client.Send(packet);
            }
        }

        // TODO: SMSG_CHAT_WRONG_FACTION
    }
}