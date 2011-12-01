/*************************************************************************
 *
 *   file		: GroupInviteRelation.cs
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

using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Interaction
{
    /// <summary>
    /// Represents a group invite relationship between two <see cref="Character"/> entities.
    /// </summary>
    public sealed class GroupInviteRelation : BaseRelation
    {
        public GroupInviteRelation(uint charId, uint relatedCharId)
            : base(charId, relatedCharId)
        { 
        }

        public override bool RequiresOnlineNotification
        {
            get { return false; }
        }

        public override CharacterRelationType Type
        {
            get { return CharacterRelationType.GroupInvite; }
        }
    }
}