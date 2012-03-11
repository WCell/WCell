using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras.Handlers;

namespace WCell.RealmServer.Spells.Auras
{
    /// <summary>
    /// AuraCollection for Character objects.
    /// Contains a lot of modifications and bookkeeping that is not required for NPCs.
    /// </summary>
    public class PlayerAuraCollection : AuraCollection
    {
        #region Fields

        /// <summary>
        /// Amount of currently added modifiers that require charges.
        /// If > 0, will iterate over modifiers and remove charges after SpellCasts.
        /// </summary>
        public int ModifierWithChargesCount
        {
            get;
            protected set;
        }

        /// <summary>
        /// Flat modifiers of spells
        /// </summary>
        internal readonly List<AddModifierEffectHandler> SpellModifiersFlat = new List<AddModifierEffectHandler>(5);

        /// <summary>
        /// Percent modifiers of spells
        /// </summary>
        internal readonly List<AddModifierEffectHandler> SpellModifiersPct = new List<AddModifierEffectHandler>(5);

        /// <summary>
        /// Mask of spells that are allowed to crit hit, although they are not allowed to, by default
        /// </summary>
        internal readonly uint[] CriticalStrikeEnabledMask = new uint[SpellConstants.SpellClassMaskSize];

        /// <summary>
        /// Set of Auras that are only applied when certain items are equipped
        /// </summary>
        List<Aura> itemRestrictedAuras;

        /// <summary>
        /// Set of Auras that are only applied in certain shapeshift forms
        /// </summary>
        List<Aura> shapeshiftRestrictedAuras;

        /// <summary>
        /// Set of Auras that are only applied in certain AuraStates
        /// </summary>
        List<Aura> auraStateRestrictedAuras;

        /// <summary>
        /// Set of Auras which have effects that depend on other Auras
        /// </summary>
        List<Aura> aurasWithAuraDependentEffects;

        #endregion Fields

        public PlayerAuraCollection(Character owner)
            : base(owner)
        {
        }

        #region Overrides

        public override void AddAura(Aura aura, bool start)
        {
            base.AddAura(aura, start);
            OnAuraAddedOrRemoved();
            if (aura.Spell.IsPassive)
            {
                if (aura.Spell.HasItemRequirements)
                {
                    ItemRestrictedAuras.Add(aura);
                }
                if (aura.Spell.IsModalShapeshiftDependentAura)
                {
                    ShapeshiftRestrictedAuras.Add(aura);
                }
                if (aura.Spell.RequiredCasterAuraState != 0)
                {
                    AuraStateRestrictedAuras.Add(aura);
                }
            }
            if (aura.Spell.HasAuraDependentEffects)
            {
                AurasWithAuraDependentEffects.Add(aura);
            }
        }

        protected internal override void Remove(Aura aura)
        {
            base.Remove(aura);
            OnAuraAddedOrRemoved();
            if (aura.Spell.IsPassive)
            {
                if (aura.Spell.HasItemRequirements)
                {
                    ItemRestrictedAuras.Remove(aura);
                }
                if (aura.Spell.IsModalShapeshiftDependentAura)
                {
                    ShapeshiftRestrictedAuras.Remove(aura);
                }
                if (aura.Spell.RequiredCasterAuraState != 0)
                {
                    AuraStateRestrictedAuras.Remove(aura);
                }
            }
            if (aura.Spell.HasAuraDependentEffects)
            {
                AurasWithAuraDependentEffects.Remove(aura);
            }
        }

        #endregion Overrides

        #region Spell Modifiers

        public void AddSpellModifierPercent(AddModifierEffectHandler modifier)
        {
            if (modifier.Charges > 0)
            {
                ModifierWithChargesCount++;
            }
            SpellModifiersPct.Add(modifier);
            OnModifierChange(modifier);
            AuraHandler.SendModifierUpdate((Character)m_owner, modifier.SpellEffect, true);
        }

        public void AddSpellModifierFlat(AddModifierEffectHandler modifier)
        {
            if (modifier.Charges > 0)
            {
                ModifierWithChargesCount++;
            }
            SpellModifiersFlat.Add(modifier);
            OnModifierChange(modifier);
            AuraHandler.SendModifierUpdate((Character)m_owner, modifier.SpellEffect, false);
        }

        public void RemoveSpellModifierPercent(AddModifierEffectHandler modifier)
        {
            if (modifier.Charges > 0)
            {
                ModifierWithChargesCount--;
            }
            OnModifierChange(modifier);
            AuraHandler.SendModifierUpdate((Character)m_owner, modifier.SpellEffect, true);
            SpellModifiersPct.Remove(modifier);
        }

        public void RemoveSpellModifierFlat(AddModifierEffectHandler modifier)
        {
            if (modifier.Charges > 0)
            {
                ModifierWithChargesCount--;
            }
            OnModifierChange(modifier);
            AuraHandler.SendModifierUpdate((Character)m_owner, modifier.SpellEffect, false);
            SpellModifiersFlat.Remove(modifier);
        }

        private void OnModifierChange(AddModifierEffectHandler modifier)
        {
            foreach (var aura in Owner.Auras)
            {
                if (aura.IsActivated && !aura.Spell.IsEnhancer && modifier.SpellEffect.MatchesSpell(aura.Spell))
                {
                    // activated, passive Aura, affected by this modifier -> Needs to re-apply
                    aura.ReApplyNonPeriodicEffects();
                }
            }
        }

        /// <summary>
        /// Returns the modified value (modified by certain talent bonusses) of the given type for the given spell (as int)
        /// </summary>
        public override int GetModifiedInt(SpellModifierType type, Spell spell, int value)
        {
            var flatMod = GetModifierFlat(type, spell);
            var percentMod = GetModifierPercent(type, spell);
            return (((value + flatMod) * (100 + percentMod)) + 50) / 100;		// rounded
        }

        /// <summary>
        /// Returns the given value minus bonuses through certain talents, of the given type for the given spell (as int)
        /// </summary>
        public override int GetModifiedIntNegative(SpellModifierType type, Spell spell, int value)
        {
            var flatMod = GetModifierFlat(type, spell);
            var percentMod = GetModifierPercent(type, spell);
            return (((value - flatMod) * (100 - percentMod)) + 50) / 100;		// rounded
        }

        /// <summary>
        /// Returns the modified value (modified by certain talents) of the given type for the given spell (as float)
        /// </summary>
        public override float GetModifiedFloat(SpellModifierType type, Spell spell, float value)
        {
            var flatMod = GetModifierFlat(type, spell);
            var percentMod = GetModifierPercent(type, spell);
            return (value + flatMod) * (1 + (percentMod / 100f));
        }

        /// <summary>
        /// Returns the percent modifier (through certain talents) of the given type for the given spell
        /// </summary>
        public int GetModifierPercent(SpellModifierType type, Spell spell)
        {
            var amount = 0;
            for (var i = 0; i < SpellModifiersPct.Count; i++)
            {
                var modifier = SpellModifiersPct[i];
                if ((SpellModifierType)modifier.SpellEffect.MiscValue == type &&
                    modifier.SpellEffect.MatchesSpell(spell))
                {
                    amount += modifier.SpellEffect.ValueMin;
                }
            }
            return amount;
        }

        /// <summary>
        /// Returns the flat modifier (through certain talents) of the given type for the given spell
        /// </summary>
        public int GetModifierFlat(SpellModifierType type, Spell spell)
        {
            var amount = 0;
            for (var i = 0; i < SpellModifiersFlat.Count; i++)
            {
                var modifier = SpellModifiersFlat[i];
                if ((SpellModifierType)modifier.SpellEffect.MiscValue == type &&
                    modifier.SpellEffect.MatchesSpell(spell))
                {
                    amount += modifier.SpellEffect.ValueMin;
                }
            }
            return amount;
        }

        #endregion Spell Modifiers

        #region OnCasted

        public override void OnCasted(SpellCast cast)
        {
            // remove one of the stack of all stacking SpellModifiers (flat & percent)
            var spell = cast.Spell;
            if (ModifierWithChargesCount > 0)
            {
                List<IAura> toRemove = null;
                foreach (var modifier in SpellModifiersFlat)
                {
                    var effect = modifier.SpellEffect;
                    if (effect.MatchesSpell(spell) &&
                        cast.Spell != effect.Spell &&
                        (cast.TriggerEffect == null || cast.TriggerEffect.Spell != effect.Spell))
                    {
                        if (modifier.Charges > 0)
                        {
                            modifier.Charges--;
                            if (modifier.Charges < 1)
                            {
                                if (toRemove == null)
                                {
                                    toRemove = SpellCast.AuraListPool.Obtain();
                                }
                                toRemove.Add(modifier.Aura);
                            }
                        }
                    }
                }
                foreach (var modifier in SpellModifiersPct)
                {
                    var effect = modifier.SpellEffect;
                    if (effect.MatchesSpell(spell) &&
                        cast.Spell != effect.Spell &&
                        (cast.TriggerEffect == null || cast.TriggerEffect.Spell != effect.Spell))
                    {
                        if (modifier.Charges > 0)
                        {
                            modifier.Charges--;
                            if (modifier.Charges < 1)
                            {
                                if (toRemove == null)
                                {
                                    toRemove = SpellCast.AuraListPool.Obtain();
                                }
                                toRemove.Add(modifier.Aura);
                            }
                        }
                    }
                }

                if (toRemove != null)
                {
                    foreach (var aura in toRemove)
                    {
                        aura.Remove(false);
                    }
                    toRemove.Clear();
                    SpellCast.AuraListPool.Recycle(toRemove);
                }
            }
        }

        #endregion OnCasted

        #region Item Restrictions

        private List<Aura> ItemRestrictedAuras
        {
            get
            {
                if (itemRestrictedAuras == null)
                {
                    itemRestrictedAuras = new List<Aura>(3);
                }
                return itemRestrictedAuras;
            }
        }

        internal void OnEquip(Item item)
        {
            if (itemRestrictedAuras != null)
            {
                var plr = (Character)m_owner;				// PlayerAuraCollection always has Character owner
                foreach (var aura in itemRestrictedAuras)
                {
                    if (!aura.IsActivated)
                    {
                        aura.IsActivated =
                            MayActivate(aura, false) &&
                            aura.Spell.CheckItemRestrictions(item, plr.Inventory) == SpellFailedReason.Ok;
                    }
                }
            }
        }

        internal void OnBeforeUnEquip(Item item)
        {
            if (itemRestrictedAuras != null)
            {
                var plr = (Character)m_owner;				// PlayerAuraCollection always has Character owner
                foreach (var aura in itemRestrictedAuras)
                {
                    if (aura.IsActivated)
                    {
                        aura.IsActivated =
                            MayActivate(aura, false) &&
                            aura.Spell.CheckItemRestrictionsWithout(plr.Inventory, item) == SpellFailedReason.Ok;
                    }
                }
            }
        }

        #endregion Item Restrictions

        #region Shapeshift Restrictions

        private List<Aura> ShapeshiftRestrictedAuras
        {
            get
            {
                if (shapeshiftRestrictedAuras == null)
                {
                    shapeshiftRestrictedAuras = new List<Aura>(3);
                }
                return shapeshiftRestrictedAuras;
            }
        }

        internal void OnShapeshiftFormChanged()
        {
            if (shapeshiftRestrictedAuras != null)
            {
                foreach (var aura in shapeshiftRestrictedAuras)
                {
                    if (aura.Spell.RequiredShapeshiftMask != 0)
                    {
                        // toggle Aura
                        aura.IsActivated = MayActivate(aura);
                    }
                    else if (aura.Spell.HasShapeshiftDependentEffects)
                    {
                        // the Aura activation state itself did not change
                        aura.ReEvaluateNonPeriodicHandlerRequirements();
                    }
                }
            }
        }

        #endregion Shapeshift Restrictions

        #region AuraState Restrictions

        private List<Aura> AuraStateRestrictedAuras
        {
            get
            {
                if (auraStateRestrictedAuras == null)
                {
                    auraStateRestrictedAuras = new List<Aura>(2);
                }
                return auraStateRestrictedAuras;
            }
        }

        internal void OnAuraStateChanged()
        {
            if (auraStateRestrictedAuras != null)
            {
                foreach (var aura in auraStateRestrictedAuras)
                {
                    aura.IsActivated = MayActivate(aura);
                }
            }
        }

        #endregion AuraState Restrictions

        #region Auras dependent on other Auras

        private List<Aura> AurasWithAuraDependentEffects
        {
            get
            {
                if (aurasWithAuraDependentEffects == null)
                {
                    aurasWithAuraDependentEffects = new List<Aura>(2);
                }
                return aurasWithAuraDependentEffects;
            }
        }

        internal void OnAuraAddedOrRemoved()
        {
            if (aurasWithAuraDependentEffects != null)
            {
                foreach (var aura in aurasWithAuraDependentEffects)
                {
                    foreach (var handler in aura.Handlers)
                    {
                        // Toggle activation
                        if (handler.SpellEffect.IsDependentOnOtherAuras)
                        {
                            handler.IsActivated = MayActivate(handler);
                        }
                    }
                }
            }
        }

        #endregion Auras dependent on other Auras

        #region Actual restriction checks

        /// <summary>
        /// Check all restrictions on the given Aura (optionally, exclude item check)
        /// </summary>
        private bool MayActivate(Aura aura, bool inclItemCheck)
        {
            if (inclItemCheck && aura.Spell.CheckItemRestrictions(((Character)m_owner).Inventory) != SpellFailedReason.Ok)
            {
                return false;
            }

            // ShapeShiftMask & Items & AuraState
            if (aura.Spell.RequiredShapeshiftMask != 0 && !aura.Spell.RequiredShapeshiftMask.HasAnyFlag(m_owner.ShapeshiftMask))
            {
                return false;
            }
            if (aura.Spell.RequiredCasterAuraState != 0 && !m_owner.AuraState.HasAnyFlag(aura.Spell.RequiredCasterAuraState))
            {
                return false;
            }
            return true;
        }

        protected internal override bool MayActivate(Aura aura)
        {
            if (MayActivate(aura, true))
            {
                return true;
            }
            return base.MayActivate(aura);
        }

        protected internal override bool MayActivate(AuraEffectHandler handler)
        {
            // ShapeShiftMask & RequiredActivationAuras
            var effect = handler.SpellEffect;
            if ((effect.RequiredShapeshiftMask == 0 ||
                        (effect.RequiredShapeshiftMask.HasAnyFlag(Owner.ShapeshiftMask))) &&
                (effect.RequiredActivationAuras == null || ContainsAny(effect.RequiredActivationAuras)))
            {
                return true;
            }
            return base.MayActivate(handler);
        }

        #endregion Actual restriction checks

        /// <summary>
        /// Returns wehther the given spell is allowed to crit, if it was not
        /// allowed to crit by default. (Due to Talents that override Spell behavior)
        /// </summary>
        public bool CanSpellCrit(Spell spell)
        {
            return spell.MatchesMask(CriticalStrikeEnabledMask);
        }
    }
}