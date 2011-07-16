/*************************************************************************
 *
 *   file		: RelationMgr.Handlers.cs
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
using WCell.Core.Network;
using WCell.RealmServer;
using WCell.RealmServer.Entities;
using WCell.RealmServer.World;
using WCell.Core;

namespace WCell.RealmServer.Interaction
{
    public sealed partial class RelationMgr
	{
        /// <summary>
        /// Handles an incoming friend list request
        /// </summary>
		/// <param name="client">the client that sent the packet</param>
		/// <param name="packet">the packet we received</param>
        [PacketHandler(RealmServerOpCode.CMSG_FRIEND_LIST)]
        public static void FriendListRequest(RealmClient client, RealmPacketIn packet)
        {
            RelationMgr.Instance.SendRelationList(client.ActiveCharacter, CharacterRelationType.Friend);
            RelationMgr.Instance.SendIgnoreList(client.ActiveCharacter);
        }

        /// <summary>
        /// Handles an incoming add friend request
        /// </summary>
		/// <param name="client">the client that sent the packet</param>
		/// <param name="packet">the packet we received</param>
        [PacketHandler(RealmServerOpCode.CMSG_ADD_FRIEND)]
        public static void AddFriendRequest(RealmClient client, RealmPacketIn packet)
        {
            string relCharacterName = packet.ReadString();

            RelationMgr.Instance.AddRelation(client.ActiveCharacter, relCharacterName, 
                                             CharacterRelationType.Friend);
        }

        /// <summary>
        /// Handles an incoming remove friend request
        /// </summary>
		/// <param name="client">the client that sent the packet</param>
		/// <param name="packet">the packet we received</param>
        [PacketHandler(RealmServerOpCode.CMSG_DEL_FRIEND)]
        public static void RemoveFriendRequest(RealmClient client, RealmPacketIn packet)
        {
            EntityId relCharId = packet.ReadEntityId();

            RelationMgr.Instance.RemoveRelation(client.ActiveCharacter.EntityId, relCharId, 
                                                CharacterRelationType.Friend);
        }

        /// <summary>
        /// Handles an incoming add ignore request
        /// </summary>
		/// <param name="client">the client that sent the packet</param>
		/// <param name="packet">the packet we received</param>
        [PacketHandler(RealmServerOpCode.CMSG_ADD_IGNORE)]
        public static void AddIgnoreRequest(RealmClient client, RealmPacketIn packet)
        {
            string relCharacterName = packet.ReadString();

            RelationMgr.Instance.AddRelation(client.ActiveCharacter, relCharacterName, 
                                             CharacterRelationType.Ignored);
        }

        /// <summary>
        /// Handles an incoming remove ignore request
        /// </summary>
		/// <param name="client">the client that sent the packet</param>
		/// <param name="packet">the packet we received</param>
        [PacketHandler(RealmServerOpCode.CMSG_DEL_IGNORE)]
        public static void RemoveIgnoreRequest(RealmClient client, RealmPacketIn packet)
        {
            EntityId relCharId = packet.ReadEntityId();

            RelationMgr.Instance.RemoveRelation(client.ActiveCharacter.EntityId, relCharId,
                                                CharacterRelationType.Ignored);
        }

        /// <summary>
        /// Handles an incoming add muted request
        /// </summary>
		/// <param name="client">the client that sent the packet</param>
		/// <param name="packet">the packet we received</param>
        [PacketHandler(RealmServerOpCode.CMSG_ADD_MUTED)]
        public static void AddMutedRequest(RealmClient client, RealmPacketIn packet)
        {
            string relCharacterName = packet.ReadString();

            RelationMgr.Instance.AddRelation(client.ActiveCharacter, relCharacterName,
                                             CharacterRelationType.Muted);
        }

        /// <summary>
        /// Handles an incoming remove muted request
        /// </summary>
		/// <param name="client">the client that sent the packet</param>
		/// <param name="packet">the packet we received</param>
        [PacketHandler(RealmServerOpCode.CMSG_DEL_MUTED)]
        public static void RemoveMutedRequest(RealmClient client, RealmPacketIn packet)
        {
            EntityId relCharId = packet.ReadEntityId();

            RelationMgr.Instance.RemoveRelation(client.ActiveCharacter.EntityId, relCharId,
                                                CharacterRelationType.Muted);
        }

        /*
        [PacketHandler(RealmServerOpCode.CMSG_BUG)]
        public static void BugRequest(RealmClient client, RealmPacketIn packet)
        {
        }
        */
    }
}