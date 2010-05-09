/*************************************************************************
 *
 *   file		: RelationFactory.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-02-19 19:42:32 +0100 (ti, 19 feb 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 151 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using NLog;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Database;
using WCell.RealmServer.World;
using WCell.Core;
using System.Linq;

namespace WCell.RealmServer.Interaction
{
    /// <summary>
    /// Factory class used to create the different relations objects
    /// </summary>
    public static class RelationFactory
    {
        public static BaseRelation CreateRelation(EntityId charId, EntityId relatedCharId,
            CharacterRelationType relationType)
        {
            switch (relationType)
            {
                case CharacterRelationType.Friend:
                    return new FriendRelation(charId, relatedCharId);
                case CharacterRelationType.Ignored:
                    return new IgnoredRelation(charId, relatedCharId);
                case CharacterRelationType.Muted:
                    return new MutedRelation(charId, relatedCharId);
                case CharacterRelationType.GroupInvite:
                    return new GroupInviteRelation(charId, relatedCharId);
            }
            return null;
        }

        public static BaseRelation CreateRelation(CharacterRelationRecord relationRecord)
        {
            if (relationRecord == null)
                return null;

            EntityId charId = EntityId.GetPlayerId(relationRecord.CharacterId);
            EntityId relatedCharId = EntityId.GetPlayerId(relationRecord.RelatedCharacterId);
            CharacterRelationType relationType = relationRecord.RelationType;

            return CreateRelation(charId, relatedCharId, relationType);
        }
    }
}
