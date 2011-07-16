/*************************************************************************
 *
 *   file		: GMCommands.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-05-09 02:21:57 +0800 (Fri, 09 May 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 326 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.Constants.World;
using WCell.Core.Network;
using WCell.RealmServer.Global;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
	/// <summary>
	/// These commands can be used by GMs through WoW's internal console
	/// </summary>
    public static class ClientConsoleCommandHandler
    {
        // console command "setrawpos x y z o"
        [ClientPacketHandler(RealmServerOpCode.CMSG_MOVE_SET_RAW_POSITION)]
        public static void HandleMoveSetRawPosition(IRealmClient client, RealmPacketIn packet)
        {
            var pos = packet.ReadVector3();
            float orientation = packet.ReadFloat();

			var map = client.ActiveCharacter.Map;
			if (map != null)
			{
				client.ActiveCharacter.TeleportTo(map, ref pos);
			}
        }

        // console command "worldport mapId x y z o"
        [ClientPacketHandler(RealmServerOpCode.CMSG_WORLD_TELEPORT)]
        public static void HandleWorldTeleport(IRealmClient client, RealmPacketIn packet)
        {
            uint time = packet.ReadUInt32();
			MapId mapId = (MapId)packet.ReadUInt32();

			var pos = packet.ReadVector3();
            float orientation = packet.ReadFloat(); // in client specified as degrees

			var map = World.GetNonInstancedMap(mapId);
			if (map != null)
			{
				client.ActiveCharacter.TeleportTo(map, ref pos);
			}
        }

        // console command "whois accountName"
        [ClientPacketHandler(RealmServerOpCode.CMSG_WHOIS)]
        public static void HandleWhoIs(IRealmClient client, RealmPacketIn packet)
        {
            // in official, you would do a whois ACCOUNTNAME to bring up real name, etc
            string personToFind = packet.ReadCString();
        }

        // console command "port unitName". could also be port GUID depending on how we want to handle it
        [ClientPacketHandler(RealmServerOpCode.CMSG_TELEPORT_TO_UNIT)]
        public static void HandleTeleportToUnit(IRealmClient client, RealmPacketIn packet)
        {
            string teleportTo = packet.ReadCString();
        }
    }
}