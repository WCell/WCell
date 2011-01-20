/*************************************************************************
 *
 *   file		: SkillRaceClassInfo.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-07 07:58:12 +0100 (l? 07 mar 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 784 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using WCell.Constants;

namespace WCell.RealmServer.Skills
{

	/// <summary>
	/// Completely unknown set of flags
	/// </summary>
	[Flags]
	public enum SkillRaceClassFlags
	{
		#region Generic
		FLAG_0x1 = 0x1,
		FLAG_0x2 = 0x2,
		FLAG_0x4 = 0x4,
		FLAG_0x8 = 0x8,

		FLAG_0x10 = 0x10,
		FLAG_0x20 = 0x20,
		FLAG_0x40 = 0x40,
		FLAG_0x80 = 0x80,

		FLAG_0x100 = 0x100,
		FLAG_0x200 = 0x200,
		FLAG_0x400 = 0x400,
		FLAG_0x800 = 0x800,

		FLAG_0x1000 = 0x1000,
		FLAG_0x2000 = 0x2000,
		FLAG_0x4000 = 0x4000,
		FLAG_0x8000 = 0x8000,

		FLAG_0x10000 = 0x10000,
		FLAG_0x20000 = 0x20000,
		FLAG_0x40000 = 0x40000,
		FLAG_0x80000 = 0x80000,

		FLAG_0x100000 = 0x100000,
		FLAG_0x200000 = 0x200000,
		FLAG_0x400000 = 0x400000,
		FLAG_0x800000 = 0x800000,

		FLAG_0x1000000 = 0x1000000,
		FLAG_0x2000000 = 0x2000000,
		FLAG_0x4000000 = 0x4000000,
		FLAG_0x8000000 = 0x8000000,

		FLAG_0x10000000 = 0x10000000,
		FLAG_0x20000000 = 0x20000000,
		FLAG_0x40000000 = 0x40000000,
		//FLAG_0x80000000 = 0x80000000,
		#endregion
	}

	/// <summary>
	/// Race/Class-restrictions, the Tier and initial amount of a skill.
	/// </summary>
	public class SkillRaceClassInfo
	{
		public uint Id;//0
		public SkillLine SkillLine;//1
		public RaceMask RaceMask;//2
		public ClassMask ClassMask;//3
		public SkillRaceClassFlags Flags;//4
		public uint MinimumLevel;//5
		public SkillTiers Tiers;//6

		/// <summary>
		/// SkillCostsData.dbc
		/// </summary>
		public uint SkillCostIndex;//7

		public override string ToString()
		{
			return ToString("");
		}

		public string ToString(string indent)
		{
			return 
				indent + "Id: " + Id + "\n" +
				indent + "Skill: " + SkillLine + "\n" +
				indent + "Races: " + RaceMask + "\n" +
				indent + "Classes: " + ClassMask + "\n" +
				indent + "Flags: " + Flags + "\n" +
				indent + "MinLevel: " + MinimumLevel + "\n" +
				indent + "Tier: " + Tiers + "\n" +
				indent + "SkillCostsIndex: " + SkillCostIndex + "\n";
		}
	}
}