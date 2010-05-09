/*************************************************************************
 *
 *   file		: VoiceHandler.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-04-28 12:55:13 +0800 (Mon, 28 Apr 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 301 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.Core;

namespace WCell.RealmServer.Handlers
{
    public static class VoiceChatHandler
    {
        [ClientPacketHandler(RealmServerOpCode.CMSG_VOICE_SESSION_ENABLE, IsGamePacket = false, RequiresLogin = false)]
        public static void HandleStatusUpdate(IRealmClient client, RealmPacketIn packet)
        {
            var voiceEnabled = packet.ReadBoolean();
            var micEnabled = packet.ReadBoolean();
        }

        [ClientPacketHandler(RealmServerOpCode.CMSG_SET_ACTIVE_VOICE_CHANNEL, IsGamePacket = false, RequiresLogin = false)]
        public static void HandleQuery(IRealmClient client, RealmPacketIn packet)
        {

        }
		

        public static void SendSystemStatus(Character chr, VoiceSystemStatus status)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_FEATURE_SYSTEM_STATUS))
            {
                packet.WriteByte(2); // what is this
                packet.WriteByte((byte)status);

                chr.Client.Send(packet);
            }
        }

        public static void SendVoiceData(ChatChannel chatChannel)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_VOICE_SESSION_ROSTER_UPDATE))
            {
            }
        }
    }
}