/*************************************************************************
 *
 *   file		: FriendRelation.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-07 07:58:12 +0100 (lø, 07 mar 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 784 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Interaction
{
    /// <summary>
    /// Represents a friend relationship between two <see cref="Character"/> entities.
    /// </summary>
    public sealed class FriendRelation : PersistedRelation
    {
		public FriendRelation(uint charId, uint relatedCharId)
            : base(charId, relatedCharId)
        { 
        }

        public override bool RequiresOnlineNotification
        {
            get { return true; }
        }

        public override CharacterRelationType Type
        {
            get { return CharacterRelationType.Friend; }
        }

        public override bool Validate(CharacterRecord charInfo, CharacterRecord relatedCharInfo, out RelationResult relResult)
        {
            //Check if the character exists. This should always be true in theory
            if (charInfo == null)
            {
                relResult = RelationResult.FRIEND_DB_ERROR;
                return false;
            }

            //Checks if the relation target char exist 
            if (relatedCharInfo == null)
            {
                relResult = RelationResult.FRIEND_NOT_FOUND;
                return false;
            }

            //Checks if the target char is the same as the related one
            if (charInfo.EntityLowId == relatedCharInfo.EntityLowId)
            {
                relResult = RelationResult.FRIEND_SELF;
                return false;
            }

            //Checks if both chars are in the same faction
			if (FactionMgr.GetFactionGroup(charInfo.Race) != FactionMgr.GetFactionGroup(relatedCharInfo.Race))
            {
                relResult = RelationResult.FRIEND_ENEMY;
                return false;
            }

            //Checks if the relation currently exist
            if (RelationMgr.Instance.HasRelation(charInfo.EntityLowId, relatedCharInfo.EntityLowId, Type))
            {
                relResult = RelationResult.FRIEND_ALREADY;
                return false;
            }

            //All checks are ok so check if the related char is online
            if (World.GetCharacter(relatedCharInfo.EntityLowId) != null)
            {
                relResult = RelationResult.FRIEND_ADDED_ONLINE;
            }
            else
            {
                relResult = RelationResult.FRIEND_ADDED_OFFLINE;
            }
            return true;
        }
    }
}