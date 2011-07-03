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

	/// <summary>
	/// Flags used in Faction Template DBCs
	/// </summary>
	[Flags]
	public enum FactionTemplateFlags : uint
	{
		None = 0x0,
		Flagx1 = 0x1,
		Flagx2 = 0x2,
		Flagx4 = 0x4,
		Flagx8 = 0x8,
		Flagx10 = 0x10,
		Flagx20 = 0x20,
		Flagx40 = 0x40,
		Flagx80= 0x80,
		Flagx200 = 0x200,
		Flagx400 = 0x400,

		/// <summary>
		/// Flagged for PvP
		/// </summary>
		PvP = 0x800,

		/// <summary>
		/// Attacks players that have been involved in PvP
		/// </summary>
		ContestedGuard = 0x1000,

		Flagx2000 = 0x2000
	}

	[Flags]
	public enum FactionFlags : byte
	{
		None = 0x00,
		Visible = 0x01,
		AtWar = 0x02,
		Hidden = 0x04,
		Inivisible = 0x08,
		Peace = 0x10,
		Inactive = 0x20,
		Rival = 0x40
	}
}