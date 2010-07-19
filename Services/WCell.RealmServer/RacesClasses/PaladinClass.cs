/*************************************************************************
 *
 *   file		: Paladin.cs
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

namespace WCell.RealmServer.RacesClasses
{
    /// <summary>
    /// Defines the basics of the Paladin class.
    /// </summary>
    public class PaladinClass : BaseClass
	{
		public override ClassId Id
		{
			get { return ClassId.Paladin; }
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
            return (level * 3 + strength * 2 - 20);
		}

        public override float CalculateMagicCritChance(int level, int intellect)
        {
            return (intellect / 80f) + /*(Spell Critical Strike Rating/22.08)*/ +3.336f;
        }
    }
}