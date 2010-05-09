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

using WCell.Util;

namespace WCell.RealmServer.Skills
{
	/// <summary>
	/// Represents the requirements that are needed to advance a Skill to the next tier
	/// </summary>
	public struct SkillTier
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
        public uint[] Values;

		public override string ToString()
		{
			return Id > 0 ? ("Id: " + Id + ", MaxValues: " + Values.ToString(", ")) : "0";
		}

		public static bool operator ==(SkillTier tier, SkillTier tier2)
		{
			return tier.Equals(tier2);
		}

		public static bool operator !=(SkillTier tier, SkillTier tier2)
		{
			return !(tier == tier2);
		}

		public override bool Equals(object obj)
		{
			return
				obj is SkillTier &&
				(((SkillTier)obj).Values == null) == (((SkillTier)obj).Values == null) &&
				(Values == null || Values.Equals(((SkillTier)obj).Values));
		}

		public override int GetHashCode()
		{
			return (Values != null ? Values.GetHashCode() : 1);
		}
	}
}
