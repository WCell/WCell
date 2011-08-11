/*************************************************************************
 *
 *   file		: TalentEntry.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-12-29 05:43:08 +0100 (ti, 29 dec 2009) $

 *   revision		: $Rev: 1160 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants.Talents;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Talents
{
	public class TalentEntry
	{
		public TalentId Id;
		public TalentTree Tree;			// 1
		public uint Row;				// 2
		public uint Col;				// 3

		/// <summary>
		/// Spells indexed by Talent-Rank
		/// </summary>
		public Spell[] Spells;	// 4-12  (9)

		/// <summary>
		/// The id of the talent that must be learnt before this one can be learnt
		/// </summary>
		public TalentId RequiredId;		// 13[-15 (3)]

		/// <summary>
		/// The required rank of the required talent
		/// </summary>
		public uint RequiredRank;			// 16

		public uint Index;

		/// <summary>
		/// Required amount of points spent within the same TalentTree to activate this talent
		/// </summary>
		public uint RequiredTreePoints
		{
			get { return Row * 5; }
		}

		/// <summary>
		/// The highest rank that this talent has
		/// </summary>
		public int MaxRank
		{
			get { return Spells.Length; }
		}

		public string Name
		{
			get { return Spells[0].Name; }
		}

		/// <summary>
		/// Complete name of this talent (includes the FullName of the tree)
		/// </summary>
		public string FullName
		{
			get { return Tree.FullName + " " + Name; }
		}

		public override string ToString()
		{
			return FullName + " (Id: " + (int)Id + ", Ranks: " + Spells.Length + ", Required: " + RequiredId + ")";
		}
	}
}