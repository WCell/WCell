/*************************************************************************
 *
 *   file		: ModifierChange.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-02-05 11:19:26 +0100 (ti, 05 feb 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 108 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Modifiers.ChangeUpdates;

namespace WCell.RealmServer.Modifiers
{
    /// <summary>
    /// A delegate that is used for updating the enhancement fields of a <see cref="Unit" /> when
    /// the unit's enhancement changes are processed.
    /// </summary>
    /// <param name="updateUnit">the <see cref="Unit" /> to update</param>
    /// <param name="updateType">the <see cref="ModifierTypes" /> to update for</param>
    /// <param name="col">the collection holding all the enhancement values for the <see cref="Unit" /></param>
    public delegate void UpdateModifierDelegate(Unit updateUnit, ModifierType updateType, ModifierCollection col);

    /// <summary>
    /// Defines an enahancement change event, where a field that affects an enahancement has changed, 
    /// and the modified value must be updated for the client.
    /// </summary>
	internal static class ModifierChange
	{
        private static Dictionary<ModifierType, UpdateModifierDelegate> m_updateDelegates;

        static ModifierChange()
        {
            m_updateDelegates = new Dictionary<ModifierType, UpdateModifierDelegate>();

            m_updateDelegates.Add(ModifierType.Power, Power.UpdatePower);
            m_updateDelegates.Add(ModifierType.Health, Health.UpdateHealth);
            m_updateDelegates.Add(ModifierType.Agility, Agility.UpdateAgility);
            m_updateDelegates.Add(ModifierType.Strength, Strength.UpdateStrength);
            m_updateDelegates.Add(ModifierType.Intellect, Intellect.UpdateIntellect);
            m_updateDelegates.Add(ModifierType.Spirit, Spirit.UpdateSpirit);
            m_updateDelegates.Add(ModifierType.Stamina, Stamina.UpdateStamina);
			m_updateDelegates.Add(ModifierType.AllStats, AllStats.UpdateAllStats);
			m_updateDelegates.Add(ModifierType.AllResists, AllResists.UpdateAllResists);
			m_updateDelegates.Add(ModifierType.ArcaneResist, ArcaneResist.UpdateArcaneResist);
			m_updateDelegates.Add(ModifierType.FireResist, FireResist.UpdateFireResist);
            m_updateDelegates.Add(ModifierType.FrostResist, FrostResist.UpdateFrostResist);
            m_updateDelegates.Add(ModifierType.HolyResist, HolyResist.UpdateHolyResist);
            m_updateDelegates.Add(ModifierType.NatureResist, NatureResist.UpdateNatureResist);
            m_updateDelegates.Add(ModifierType.PhysicalResist, PhysicalResist.UpdatePhysicalResist);
            m_updateDelegates.Add(ModifierType.ShadowResist, ShadowResist.UpdateShadowResist);
			m_updateDelegates.Add(ModifierType.AttackPower, AttackPower.UpdateAttackPower);
			m_updateDelegates.Add(ModifierType.RangedAttackPower, RangedAttackPower.UpdateRangedAttackPower);
			m_updateDelegates.Add(ModifierType.Armor, Armor.UpdateArmor);
			m_updateDelegates.Add(ModifierType.WeaponSkillRating, WeaponSkillRating.UpdateWeaponSkillRating);

			// regen
			m_updateDelegates.Add(ModifierType.HealthRegen, HealthRegen.UpdateHealthRegen);
			m_updateDelegates.Add(ModifierType.HealthRegenInCombat, HealthRegen.UpdateCombatHealthRegen);
			m_updateDelegates.Add(ModifierType.HealthRegenNoCombat, HealthRegen.UpdateNormalHealthRegen);
			m_updateDelegates.Add(ModifierType.PowerRegen, PowerRegen.UpdatePowerRegen);
			m_updateDelegates.Add(ModifierType.ManaRegen, PowerRegen.UpdatePowerRegen);
			m_updateDelegates.Add(ModifierType.ManaInterruptRegen, PowerRegen.UpdateInterruptedManaRegen);

			// speeds
			m_updateDelegates.Add(ModifierType.SpeedFactor, Speed.UpdateSpeedFactor);
			m_updateDelegates.Add(ModifierType.FlightSpeedFactor, Speed.UpdateFlySpeedFactor);
			m_updateDelegates.Add(ModifierType.SwimSpeedFactor, Speed.UpdateSwimSpeedFactor);
			m_updateDelegates.Add(ModifierType.RunSpeedFactor, Speed.UpdateMountSpeedFactor);


			foreach(ModifierType statType in Enum.GetValues(typeof(ModifierType)))
			{
                if (!m_updateDelegates.ContainsKey(statType))
				{
                    m_updateDelegates.Add(statType, Nothing.UpdateNothing);
				}
			}
        }

        /// <summary>
        /// Invokes a <see cref="UpdateEnahancementDelegate" /> delegate to update the neccessary fields
        /// for the given change.
        /// </summary>
        /// <param name="updateUnit">the <see cref="Unit" /> to update</param>
        /// <param name="changeType">the change type</param>
        /// <param name="statsCollection">the enhancements collection</param>
        internal static void CommitChange(Unit updateUnit, ModifierType changeType, 
                                            ModifierCollection statsCollection)
        {
            m_updateDelegates[changeType].Invoke(updateUnit, changeType, statsCollection);
        }
    }
}