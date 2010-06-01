/*************************************************************************
 *
 *   file		: Rogue.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-04-08 17:02:58 +0800 (Tue, 08 Apr 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 244 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.RacesClasses
{
    /// <summary>
    /// Defines the basics of the Rogue class.
    /// </summary>
    public class RogueClass : BaseClass
	{
		public override ClassId Id
		{
			get { return ClassId.Rogue; }
		}

		public override PowerType PowerType
		{
			get
			{
				return PowerType.Energy;
			}
		}

        public override int CalculateHealthRegen(int level, int spirit)
        {
			return (int)(spirit * 0.50f) + 2;
        }

        /// <summary>
        /// Calculates attack power for the class at a specific level, Strength and Agility.
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <param name="strength">the player's Strength</param>
        /// <param name="agility">the player's Agility</param>
        /// <returns>the total attack power</returns>
        public override int CalculateMeleeAP(int level, int strength, int agility)
        {
            return ((level * 2 + strength + agility) - 20);
		}

		/// <summary>
		/// Calculates ranged attack power for the class at a specific level, Strength and Agility.
		/// </summary>
		/// <param name="level">the player's level</param>
		/// <param name="strength">the player's Strength</param>
		/// <param name="agility">the player's Agility</param>
		/// <returns>the total ranged attack power</returns>
		public override int CalculateRangedAP(int level, int strength, int agility)
		{
			return level + agility - 10;
		}

        /// <summary>
        /// Calculates the dodge amount for the class at a specific level and Agility.
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <param name="agility">the player's Agility</param>
        /// <returns>the total dodge amount</returns>
        public override float CalculateDodge(int level, int agility, int baseAgility, int defenseSkill, int dodgeRating, int defense)
        {
            return (agility / ((level * 0.226f) + 0.838f));
        }

        public override float CalculateMagicCritChance(int level, int intellect)
        {
            return 0;
        }

        /// <summary>
        /// Calculates the amount of power regeneration for the class at a specific level and Spirit.
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <param name="spirit">the player's Spirit</param>
        /// <returns>the total power regeneration amount</returns>
        public override int CalculatePowerRegen(Character chr)
        {
            return 20;
        }

		public override int GetPowerForLevel(int level)
		{
			return 100;
		}
    }
}
