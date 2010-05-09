/*************************************************************************
 *
 *   file		: HonorHandler.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-04-18 02:42:44 +0200 (lø, 18 apr 2009) $
 *   last author	: $LastChangedBy: fubecao $
 *   revision		: $Rev: 876 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    public static class HonorHandler
    {
        /*[ClientPacketHandler(RealmServerOpCode.CMSG_SET_PVP_TITLE)]
        public static void HandleSetPvPTitle(IRealmClient client, RealmPacketIn packet)
        {
        }*/

        [ClientPacketHandler(RealmServerOpCode.CMSG_SET_TITLE)]
        public static void HandleChooseTitle(IRealmClient client, RealmPacketIn packet)
        {
            uint title = packet.ReadUInt32();

            // TODO: Check whether title can be used
            client.ActiveCharacter.ChosenTitle = title;
        }

        public static void SendPVPCredit(IPacketReceiver receiver, uint points, Character victim)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PVP_CREDIT))
            {
                packet.Write(points);
                packet.Write(victim.EntityId);
                packet.Write((int)victim.PvPRank);

                receiver.Send(packet);
            }
        }
    }
}