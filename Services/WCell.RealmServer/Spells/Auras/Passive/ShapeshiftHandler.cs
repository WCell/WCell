/*************************************************************************
 *
 *   file		: Shapeshift.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-28 13:29:18 +0100 (to, 28 jan 2010) $

 *   revision		: $Rev: 1230 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>
    /// Changes the owner's form.
    /// TODO: The act of shapeshifting frees the caster of Polymorph and Movement Impairing effects.
    /// </summary>
    public class ShapeshiftHandler : AuraEffectHandler
    {
        ShapeshiftForm form;

        protected internal override void CheckInitialize(SpellCast creatingCast, ObjectReference casterReference, Unit target, ref SpellFailedReason failReason)
        {
            form = (ShapeshiftForm)SpellEffect.MiscValue;
            if (target.ShapeshiftForm == form)
            {
                // stances can't be undone:
                if (form != ShapeshiftForm.BattleStance &&
                    form != ShapeshiftForm.BerserkerStance &&
                    form != ShapeshiftForm.DefensiveStance)
                {
                    if (Aura != null)
                    {
                        target.Auras.RemoveWhere(aura => aura.Spell.Id == Aura.Spell.Id);
                    }
                    failReason = SpellFailedReason.DontReport;
                }
            }
        }

        protected override void Apply()
        {
            Owner.ShapeshiftForm = form;
        }

        protected override void Remove(bool cancelled)
        {
            Owner.ShapeshiftForm = ShapeshiftForm.Normal;
        }
    }
};