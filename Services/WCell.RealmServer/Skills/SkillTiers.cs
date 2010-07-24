/*************************************************************************
 *
 *   file		: SkillTier.cs
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

using WCell.Constants.Skills;
using WCell.Util;

namespace WCell.RealmServer.Skills
{
	/// <summary>
	/// Represents the requirements that are needed to advance a Skill to the next tier
	/// </summary>
	public struct SkillTiers
	{
		//public static SkillTier DefaultTier = new SkillTier(75, 150, 225, 300, 375);

		public uint Id;
		/// <summary>
		/// The cost of each tier
		/// </summary>
		public uint[] Costs;
		/// <summary>
		/// The limit of each tier
		/// </summary>
		public uint[] MaxValues;

		public uint GetCost(SkillTierId id)
		{
			return Costs[(uint)id];
		}

		public uint GetMaxValue(SkillTierId id)
		{
			return MaxValues[(uint)id];
		}

		public override string ToString()
		{
			return Id > 0 ? ("Id: " + Id + ", MaxValues: " + MaxValues.ToString(", ")) : "0";
		}

		public static bool operator ==(SkillTiers tiers, SkillTiers tier2)
		{
			return tiers.Equals(tier2);
		}

		public static bool operator !=(SkillTiers tiers, SkillTiers tier2)
		{
			return !(tiers == tier2);
		}

		public override bool Equals(object obj)
		{
			return
				obj is SkillTiers &&
				(MaxValues == null || MaxValues.Equals(((SkillTiers)obj).MaxValues));
		}

		public override int GetHashCode()
		{
			return (MaxValues != null ? MaxValues.GetHashCode() : 1);
		}
	}
}