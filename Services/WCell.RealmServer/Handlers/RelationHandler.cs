using WCell.Constants;
using WCell.Constants.Relations;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
	public static class RelationHandler
	{
        /// <summary>
        /// Handles an incoming friend list request
        /// </summary>
		/// <param name="client">the client that sent the packet</param>
		/// <param name="packet">the packet we received</param>
        [ClientPacketHandler(RealmServerOpCode.CMSG_CONTACT_LIST)]
        public static void ContactListRequest(IRealmClient client, RealmPacketIn packet)
        {
			var flags = (RelationTypeFlag)packet.ReadUInt32();

			RelationMgr.Instance.SendRelationList(client.ActiveCharacter, flags);
        }

        /// <summary>
        /// Handles an incoming add friend request
        /// </summary>
		/// <param name="client">the client that sent the packet</param>
		/// <param name="packet">the packet we received</param>
        [ClientPacketHandler(RealmServerOpCode.CMSG_ADD_FRIEND)]
        public static void AddFriendRequest(IRealmClient client, RealmPacketIn packet)
        {
            string relCharacterName = packet.ReadCString();
			string note = packet.ReadCString();

            RelationMgr.Instance.AddRelation(client.ActiveCharacter, relCharacterName, note, 
                                             CharacterRelationType.Friend);
        }

        /// <summary>
        /// Handles an incoming remove friend request
        /// </summary>
		/// <param name="client">the client that sent the packet</param>
		/// <param name="packet">the packet we received</param>
        [ClientPacketHandler(RealmServerOpCode.CMSG_DEL_FRIEND)]
        public static void RemoveFriendRequest(IRealmClient client, RealmPacketIn packet)
        {
            var relCharId = packet.ReadEntityId();

            RelationMgr.Instance.RemoveRelation(client.ActiveCharacter.EntityId.Low, relCharId.Low, 
                                                CharacterRelationType.Friend);
        }

        /// <summary>
        /// Handles an incoming friend set note request
        /// </summary>
		/// <param name="client">the client that sent the packet</param>
		/// <param name="packet">the packet we received</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_SET_CONTACT_NOTES)]
        public static void SetRelationNoteRequest(IRealmClient client, RealmPacketIn packet)
        {
			EntityId characterId = packet.ReadEntityId();
			string note = packet.ReadCString();

			RelationMgr.Instance.SetRelationNote(client.ActiveCharacter.EntityId.Low, characterId.Low, 
												note, CharacterRelationType.Friend);
        }
		
        /// <summary>
        /// Handles an incoming add ignore request
        /// </summary>
		/// <param name="client">the client that sent the packet</param>
		/// <param name="packet">the packet we received</param>
        [ClientPacketHandler(RealmServerOpCode.CMSG_ADD_IGNORE)]
        public static void AddIgnoreRequest(IRealmClient client, RealmPacketIn packet)
        {
            string relCharacterName = packet.ReadCString();

            RelationMgr.Instance.AddRelation(client.ActiveCharacter, relCharacterName, 
											 string.Empty, CharacterRelationType.Ignored);
        }

        /// <summary>
        /// Handles an incoming remove ignore request
        /// </summary>
		/// <param name="client">the client that sent the packet</param>
		/// <param name="packet">the packet we received</param>
        [ClientPacketHandler(RealmServerOpCode.CMSG_DEL_IGNORE)]
        public static void RemoveIgnoreRequest(IRealmClient client, RealmPacketIn packet)
        {
            EntityId relCharId = packet.ReadEntityId();

			RelationMgr.Instance.RemoveRelation(client.ActiveCharacter.EntityId.Low, relCharId.Low,
                                                CharacterRelationType.Ignored);
        }

        /*/// <summary>
        /// Handles an incoming add muted request
        /// </summary>
		/// <param name="client">the client that sent the packet</param>
		/// <param name="packet">the packet we received</param>
        [ClientPacketHandler(RealmServerOpCode.CMSG_ADD_MUTED)]
        public static void AddMutedRequest(IRealmClient client, RealmPacketIn packet)
        {
            string relCharacterName = packet.ReadCString();

            RelationMgr.Instance.AddRelation(client.ActiveCharacter, relCharacterName,
											 string.Empty, CharacterRelationType.Muted);
        }

        /// <summary>
        /// Handles an incoming remove muted request
        /// </summary>
		/// <param name="client">the client that sent the packet</param>
		/// <param name="packet">the packet we received</param>
        [ClientPacketHandler(RealmServerOpCode.CMSG_DEL_MUTED)]
        public static void RemoveMutedRequest(IRealmClient client, RealmPacketIn packet)
        {
            EntityId relCharId = packet.ReadEntityId();

			RelationMgr.Instance.RemoveRelation(client.ActiveCharacter.EntityId.Low, relCharId.Low,
                                                CharacterRelationType.Muted);
        }*/

        
        [ClientPacketHandler(RealmServerOpCode.CMSG_BUG)]
        public static void BugRequest(IRealmClient client, RealmPacketIn packet)
        {
            var isSuggestion = packet.ReadUInt32();
            var contentLen = packet.ReadUInt32();
            var content = packet.ReadString();
            var typeLen = packet.ReadUInt32();
            var type = packet.ReadString();

            // TO-DO : Escape strings
            //BugReport.CreateNewBugReport(type, content);
        }
        
	}
}