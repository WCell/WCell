/*************************************************************************
 *
 *   file		: ModAttackPower.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-31 03:46:31 +0100 (sø, 31 jan 2010) $

 *   revision		: $Rev: 1238 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    #region Melee

    public class ModMeleeAttackPowerHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            if (EffectValue > 0)
            {
                m_aura.Auras.Owner.MeleeAttackPowerModsPos += EffectValue;
            }
            else
            {
                m_aura.Auras.Owner.MeleeAttackPowerModsNeg += EffectValue;
            }
        }

        protected override void Remove(bool cancelled)
        {
            if (EffectValue > 0)
            {
                m_aura.Auras.Owner.MeleeAttackPowerModsPos -= EffectValue;
            }
            else
            {
                m_aura.Auras.Owner.MeleeAttackPowerModsNeg -= EffectValue;
            }
        }
    }

    public class ModMeleeAttackPowerPercentHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            m_aura.Auras.Owner.MeleeAttackPowerMultiplier += EffectValue / 100f;
        }

        protected override void Remove(bool cancelled)
        {
            m_aura.Auras.Owner.MeleeAttackPowerMultiplier -= EffectValue / 100f;
        }
    }

    #endregion Melee

    #region Ranged

    public class ModRangedAttackPowerHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            if (EffectValue > 0)
            {
                m_aura.Auras.Owner.RangedAttackPowerModsPos += EffectValue;
            }
            else
            {
                m_aura.Auras.Owner.RangedAttackPowerModsNeg += EffectValue;
            }
        }

        protected override void Remove(bool cancelled)
        {
            if (EffectValue > 0)
            {
                m_aura.Auras.Owner.RangedAttackPowerModsPos -= EffectValue;
            }
            else
            {
                m_aura.Auras.Owner.RangedAttackPowerModsNeg -= EffectValue;
            }
        }
    }

    public class ModRangedAttackPowerPercentHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            m_aura.Auras.Owner.RangedAttackPowerMultiplier += EffectValue / 100f;
        }

        protected override void Remove(bool cancelled)
        {
            m_aura.Auras.Owner.RangedAttackPowerMultiplier -= EffectValue / 100f;
        }
    }

    #endregion Ranged
};