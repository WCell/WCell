/*************************************************************************
 *
 *   file		: Hunter.cs
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

using WCell.Core;
using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.RacesClasses
{
    /// <summary>
    /// Defines the basics of the Hunter class.
    /// </summary>
    public class Hunter : BaseClass
    {
        /// <summary>
        /// Creates a new <see cref="Hunter" /> object with the given base health/power values.
        /// </summary>
        /// <param name="baseHealth">the amount of health this class starts with</param>
        /// <param name="basePower">the amount of power this class starts with</param>
        public Hunter()
		{
        }

        /// <summary>
        /// Calculates attack power for the class at a specific level, Strength and Agility.
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <param name="strength">the player's Strength</param>
        /// <param name="agility">the player's Agility</param>
        /// <returns>the total attack power</returns>
        public override uint CalculateMeleeAP(uint level, uint strength, uint agility)
        {
            return ((((level * 2) + strength) + agility) - 20);
        }

        /// <summary>
        /// Calculates the dodge amount for the class at a specific level and Agility.
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <param name="agility">the player's Agility</param>
        /// <returns>the total dodge amount</returns>
        public override float CalculateDodge(uint level, uint agility)
        {
            return (agility / ((level * 0.41f) + 1.476f));
        }

        public override float CalculateMagicCritChance(uint level, uint intellect)
        {
            return (intellect / 80f) + /*(Spell Critical Strike Rating/22.08)*/ +3.6f;
        }

        /// <summary>
        /// Calculates the amount of power regeneration for the class at a specific level and Spirit.
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <param name="spirit">the player's Spirit</param>
        /// <returns>the total power regeneration amount</returns>
        public override float CalculatePowerRegen(uint level, uint spirit)
        {
            return (15f + (spirit / 5f));
        }

        public override int CalculateHealthRegen(uint level, uint spirit)
        {
            return (int)(spirit * 0.25f) + 6;
        }

        /// <summary>
        /// Calculates ranged attack power for the class at a specific level, Strength and Agility.
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <param name="strength">the player's Strength</param>
        /// <param name="agility">the player's Agility</param>
        /// <returns>the total ranged attack power</returns>
        public override uint CalculateRangedAP(uint level, uint strength, uint agility)
        {
            return (((level * 2) + (agility * 2)) - 20);
        }

        /// <summary>
        /// Gets the amount of health gained at a specific level.
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <returns>the total health gained at the given level</returns>
        public override uint GetHealthGain(uint level)
        {
            return (level <= 13 ? 17 : level + 4);
        }

        /// <summary>
        /// Gets the amount of power gained at a specific level.
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <returns>the total power gained at the given level</returns>
        public override uint GetPowerGain(uint level)
        {
            return (level <= 27 ? level + 18 : 42);
        }

        /// <summary>
        /// Runs any needed initialization for a player that has just been created.
        /// </summary>
		/// <param name="character">the <see cref="Character"/>character</see> that needs to be initialized</param>
        public override void InitializeStartingPlayer(Character character)
        {
            base.InitializeStartingPlayer(character);

            //base.AddSpell((ushort)2973, 4);
            //base.AddSpell((ushort)75, 3);
            //base.AddSkill((ushort)0, (ushort)50, 1, 1);
            //base.AddSkill((ushort)0, (ushort)163, 1, 1);
            //base.AddSkill((ushort)0, (ushort)51, 1, 1);
            //base.AddSkill(SPELLSKILL.CLOTH, SKILL.CLOTH, 1, 1);
            //base.AddSkill(SPELLSKILL.LEATHER, SKILL.LEATHER, 1, 1);

        }

        public override ClassType ClassID
        {
            get { return ClassType.Hunter; }
        }
    }
}