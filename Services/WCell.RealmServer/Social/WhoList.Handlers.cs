/*************************************************************************
 *
 *   file		: WhoList.Handlers.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-01-31 12:35:36 +0100 (to, 31 jan 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 87 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core.Network;
using WCell.RealmServer;
using WCell.RealmServer.Entities;
using WCell.RealmServer.World;

namespace WCell.RealmServer.Interaction
{
    public static partial class WhoList
    {
        /// <summary>
        /// Handles an incoming who list request
        /// </summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        [PacketHandler(RealmServerOpCode.CMSG_WHO)]
        public static void WhoListRequest(RealmClient client, RealmPacketIn packet)
        {
            WhoSearch search = new WhoSearch();

            search.MaxResultCount = MAX_RESULT_COUNT;
            search.Faction = client.ActiveCharacter.Faction.Group;
            search.MinLevel = (byte)packet.ReadUInt32();
            search.MaxLevel = (byte)packet.ReadUInt32();
            search.Name = packet.ReadString();

            byte unkown1 = packet.ReadByte();
            uint unkown2 = packet.ReadUInt32();
            uint unkown3 = packet.ReadUInt32();

            uint zoneCount = packet.ReadUInt32();
            if (zoneCount > 0 && zoneCount <= 10)
            {
                for (int i = 0; i < zoneCount; i++)
                    search.Zones.Add((ZoneId)packet.ReadUInt32());
            }

            uint nameCount = packet.ReadUInt32();
            if (nameCount > 0 && nameCount <= 10)
            {
                for (int i = 0; i < nameCount; i++)
                    search.Names.Add(packet.ReadString().ToLower());
            }

            uint totalMatches;
            //Performs the search and retrieves matching characters
            List<Character> characters = search.RetrieveMatchedCharacters(out totalMatches);

            //Send the character list to the client
            SendWhoList(client, characters, totalMatches);
        }
    }
}