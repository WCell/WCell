/*************************************************************************
 *
 *   file		: SkillAbility.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-28 05:18:24 +0100 (to, 28 jan 2010) $

 *   revision		: $Rev: 1229 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Spells;
using WCell.Util;

namespace WCell.RealmServer.Skills
{
	/// <summary>
	/// Skill Abilities, any kind of skill-related action 
	/// </summary>
	public class SkillAbility
	{
		public static int SuccessChanceGrey = 1000;
		public static int SuccessChanceGreen = 1000;
		public static int SuccessChanceYellow = 700;
		public static int SuccessChanceOrange = 300;

		public static int GainChanceGreen = 250;
		public static int GainChanceYellow = 500;
		public static int GainChanceOrange = 1000;

		public static int GainAmount = 1;

		// Diff values for Locks that have no color values
		public static uint GreyDiff = 100;
		public static uint GreenDiff = 50;
		public static uint YellowDiff = 25;

		public uint AbilityId;
		public SkillLine Skill;
		public Spell Spell;
		public RaceMask RaceMask;		//3
		public ClassMask ClassMask;	//4

		/// <summary>
		/// The spell that superceeds this one
		/// </summary>
		public SpellId NextSpellId;

		/// <summary>
		/// The Ability that superceeds this one
		/// </summary>
		public SkillAbility NextAbility;

		/// <summary>
		/// The Ability that this one superceeded
		/// </summary>
		public SkillAbility PreviousAbility;

		public uint OrangeValue;
		public uint YellowValue;
		public uint GreenValue;
		public uint GreyValue;
		public uint RedValue;

		/// <summary>
		/// For pets
		/// </summary>
		public uint ReqTrainPts;

		public SkillAcquireMethod AcquireMethod;

		public bool CanGainSkill;

		public int Gain(int skillValue)
		{
			int chance;
			if (skillValue >= GreyValue)
			{
				return 0;
			}
			else if (skillValue >= GreenValue)
			{
				chance = GainChanceGreen;
			}
			else if (skillValue >= YellowValue)
			{
				chance = GainChanceYellow;
			}
			else
			{
				chance = GainChanceOrange;
			}

			return (Utility.Random() % 1000) < chance ? GainAmount : 0;
		}

		public bool CheckSuccess(uint skillValue)
		{
			int chance;
			if (skillValue >= GreyValue)
			{
				chance = SuccessChanceGrey;
			}
			else if (skillValue >= GreenValue)
			{
				chance = SuccessChanceGreen;
			}
			else if (skillValue >= YellowValue)
			{
				chance = SuccessChanceYellow;
			}
			else if (skillValue > RedValue)
			{
				chance = SuccessChanceOrange;
			}
			else
			{
				return false;
			}

			return (Utility.Random() % 1000) < chance;
		}

		public string SkillInfo
		{
			get
			{
				return string.Format(Skill.Name + " (Levels: {0}, {1}, {2})", YellowValue, GreenValue, GreyValue);
			}
		}

		public override string ToString()
		{
			return Spell + string.Format(" - {3}(Skill: {0}, Yellow: {1}, Grey: {2})", Skill.Name, YellowValue, GreyValue, AcquireMethod != 0 ? AcquireMethod + " " : "");
		}
	}
}