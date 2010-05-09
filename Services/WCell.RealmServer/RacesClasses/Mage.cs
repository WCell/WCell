/*************************************************************************
 *
 *   file		: Mage.cs
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
    /// Defines the basics of the Mage class.
    /// </summary>
    public class Mage : BaseClass
    {
        /// <summary>
        /// Creates a new <see cref="Mage" /> object with the given base health/power values.
        /// </summary>
        /// <param name="baseHealth">the amount of health this class starts with</param>
        /// <param name="basePower">the amount of power this class starts with</param>
        public Mage()
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
            return (strength - 10);
        }

        public override float CalculateMagicCritChance(uint level, uint intellect)
        {
            return (intellect / 80f) + /*(Spell Critical Strike Rating/22.08)*/ +0.91f;
        }

        /// <summary>
        /// Calculates the amount of power regeneration for the class at a specific level and Spirit.
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <param name="spirit">the player's Spirit</param>
        /// <returns>the total power regeneration amount</returns>
        public override float CalculatePowerRegen(uint level, uint spirit)
        {
            return (12.5f + (spirit / 4f));
        }

        public override int CalculateHealthRegen(uint level, uint spirit)
        {
            return (int)(spirit * 0.10f);
        }

        /// <summary>
        /// Gets the amount of health gained at a specific level.
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <returns>the total health gained at the given level</returns>
        public override uint GetHealthGain(uint level)
        {
            return (level <= 25 ? 15 : level - 8);
        }

        /// <summary>
        /// Gets the amount of power gained at a specific level.
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <returns>the total power gained at the given level</returns>
        public override uint GetPowerGain(uint level)
        {
            return (level <= 27 ? level + 23 : 51);
        }

        /// <summary>
        /// Runs any needed initialization for a player that has just been created.
        /// </summary>
		/// <param name="character">the <see cref="Character">Character</see> that needs to be initialized</param>
        public override void InitializeStartingPlayer(Character character)
        {
            base.InitializeStartingPlayer(character);

            //base.AddSpell((ushort)133, 3);
            //base.AddSpell((ushort)168, 4);
            //base.AddSpell(SPELLSKILL.SHOOT, 2);
            //base.AddSkill((ushort)0, (ushort)6, 1, 1);
            //base.AddSkill((ushort)0, (ushort)8, 1, 1);
            //base.AddSkill(SPELLSKILL.STAFF, SKILL.STAFF, 1, 300);
            //base.AddSkill(SPELLSKILL.WAND, SKILL.WAND, 1, 300);
            //base.AddSkill(SPELLSKILL.CLOTH, SKILL.CLOTH, 1, 1);
        }

        public override ClassType ClassID
        {
            get { return ClassType.Mage; }
        }
    }
}
