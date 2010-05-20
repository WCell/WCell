/*************************************************************************
 *
 *   file		: Constants.Factions.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-06-06 19:18:11 +0800 (Fri, 06 Jun 2008) $
 *   last author	: $LastChangedBy: Nivelo $
 *   revision		: $Rev: 456 $
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

namespace WCell.Constants.Factions
{
	[Flags]
    public enum AreaOwnership : uint
    {
		/// <summary>
		/// Doesn't belong to anyone
		/// </summary>
		Contested = 0,
        Alliance = 2,
        Horde = 4,

		/// <summary>
		/// Belongs to both sides (Peace eventually!)
		/// Only Shattrath City and Dalaran
		/// </summary>
        Sanctuary = 6
    }

    /// <summary>
    /// A mask of the values from FactionGroup.dbc
    /// </summary>
    [Flags]
    public enum FactionGroupMask
    {
        /// <summary>
        /// Not aggressive
        /// </summary>
        None = 0,

        Player = 1,
        /// <summary>
        /// A member of the Alliance faction
        /// </summary>
        Alliance = 2,
        /// <summary>
        /// A member of the Horde faction
        /// </summary>
        Horde = 4,
        Monster = 8
    };

    /// <summary>
    /// FactionGroup
    /// </summary>
    public enum FactionGroup
    {
        Alliance = 1,
        Horde = 2,
        Invalid = 3
    }
}