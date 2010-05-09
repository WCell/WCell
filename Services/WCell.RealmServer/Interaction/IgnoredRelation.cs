/*************************************************************************
 *
 *   file		: IgnoredRelation.cs
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

namespace WCell.RealmServer.Interaction
{
    /// <summary>
    /// Represents a friend relationship between two <see cref="Character"/> entities.
    /// </summary>
    public sealed class IgnoredRelation : PersistedRelation
    {
		public IgnoredRelation(uint charId, uint relatedCharId)
            : base(charId, relatedCharId)
        { 
        }

        public override bool RequiresOnlineNotification
        {
            get { return false; }
        }

        public override CharacterRelationType Type
        {
            get { return CharacterRelationType.Ignored; }
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
                relResult = RelationResult.IGNORE_NOT_FOUND;
                return false;
            }

            //Checks if the target char is the same as the related one
            if (charInfo.EntityLowId == relatedCharInfo.EntityLowId)
            {
                relResult = RelationResult.IGNORE_SELF;
                return false;
            }

            //Checks if the relation currently exist
            if (RelationMgr.Instance.HasRelation(charInfo.EntityLowId, relatedCharInfo.EntityLowId, this.Type))
            {
                relResult = RelationResult.IGNORE_ALREADY;
                return false;
            }

            relResult = RelationResult.IGNORE_ADDED;
            return true;
        }
    }
}
