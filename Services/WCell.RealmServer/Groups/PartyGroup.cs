/*************************************************************************
 *
 *   file		: PartyGroup.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-27 10:06:23 +0100 (on, 27 jan 2010) $

 *   revision		: $Rev: 1227 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Groups
{
    /// <summary>
    /// Represents a party group.
    /// </summary>
    public sealed class PartyGroup : Group, IGroupConverter<RaidGroup>
	{
		#region Fields

		/// <summary>
		/// Max Amount of allowed sub-groups
		/// </summary>
		public const byte MaxSubGroupCount = 1;

		/// <summary>
		/// Max amount of allowed members in a Party
		/// </summary>
		public new const int MaxMemberCount = MaxSubGroupCount * SubGroup.MaxMemberCount;

		#endregion

		/// <summary>
		/// Creates a party group with the given character as the leader.
		/// </summary>
		/// <param name="leader"></param>
		public PartyGroup(Character leader)
            : base(leader, MaxSubGroupCount)
        {
		}

		#region Properties

		/// <summary>
		/// Tye type of group.
		/// </summary>
        public override GroupFlags Flags
        {
            get { return GroupFlags.Party; }
		}

		#endregion

		#region IGroupConverter<RaidGroup> Members

		/// <summary>
		/// Converts the group into a raid group.
		/// </summary>
		/// <returns>a <see cref"RaidGroup" /> object with all the members from the original party group present.</returns>
        public RaidGroup ConvertTo()
        {
            return new RaidGroup(this);
        }

        #endregion
    }
}