using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras;
using WCell.Util;
using WCell.Util.NLog;

namespace WCell.RealmServer.Spells
{
    /// <summary>
    /// Aura-related information of a Spell
    /// </summary>
    public partial class Spell
    {
        #region Auto generated Aura Fields

        /// <summary>
        /// Whether this Spell is an Aura
        /// </summary>
        public bool IsAura;

        /// <summary>
        /// AuraUID, the same for all Auras that may not stack
        /// </summary>
        public uint AuraUID;

        public bool HasPeriodicAuraEffects;

        public bool HasNonPeriodicAuraEffects;

        /// <summary>
        /// AuraFlags to be applied to all Auras resulting from this Spell
        /// </summary>
        public AuraFlags DefaultAuraFlags;

        /// <summary>
        /// General Amplitude for Spells that represent Auras (can only have one for the time being)
        /// </summary>
        public int AuraAmplitude;

        /// <summary>
        /// whether this Spell is an AreaAura
        /// </summary>
        public bool IsAreaAura;

        /// <summary>
        /// Modal Auras cannot be updated, but must be replaced
        /// </summary>
        public bool IsAutoRepeating;

        /// <summary>
        /// General Amplitude for Spells that represent AreaAuras (can only have one per spell)
        /// </summary>
        public int AreaAuraAmplitude;

        /// <summary>
        /// All effects that belong to an Aura
        /// </summary>
        public SpellEffect[] AuraEffects;

        /// <summary>
        /// All effects that belong to an AreaAura
        /// </summary>
        public SpellEffect[] AreaAuraEffects;

        /// <summary>
        /// Whether the Aura's effects should multiply it's effect value by the amount of its Applications
        /// </summary>
        public bool CanStack;

        /// <summary>
        /// The amount of initial Aura-Applications
        /// </summary>
        public int InitialStackCount;

        /// <summary>
        /// Only has Aura effects
        /// </summary>
        public bool IsPureAura;

        /// <summary>
        /// Only has positive Aura effects
        /// </summary>
        public bool IsPureBuff;

        /// <summary>
        /// Only has negative Aura effects
        /// </summary>
        public bool IsPureDebuff;

        /// <summary>
        /// whether this Spell applies the death effect
        /// </summary>
        public bool IsGhost;

        /// <summary>
        /// Whether this is a proc and whether its own effects handle procs (or false, if not a proc or custom proc handlers have been added)
        /// </summary>
        public bool IsAuraProcHandler
        {
            get { return IsProc && ProcHandlers == null; }
        }

        public bool IsVehicle;

        /// <summary>
        /// Spell lets one shapeshift into another creature
        /// </summary>
        public bool IsShapeshift;

        /// <summary>
        /// whether this spell applies makes the targets fly
        /// </summary>
        public bool HasFlyEffect;

        /// <summary>
        /// Does this Spell apply a Mount-Aura?
        /// </summary>
        public bool IsMount
        {
            get { return Mechanic == SpellMechanic.Mounted; }
        }

        /// <summary>
        /// Does this Spell apply a Flying-Mount Aura?
        /// </summary>
        public bool IsFlyingMount;

        /// <summary>
        ///
        /// </summary>
        public bool CanApplyMultipleTimes;

        /// <summary>
        /// Whether the Aura has effects that depend on the wearer's Shapeshift form
        /// </summary>
        public bool HasShapeshiftDependentEffects;

        /// <summary>
        /// Whether the Aura is in any way dependent on the wearer's shapeshift form
        /// </summary>
        public bool IsModalShapeshiftDependentAura;

        /// <summary>
        ///
        /// </summary>
        public AuraCasterGroup AuraCasterGroup;

        #endregion Auto generated Aura Fields

        #region InitAura

        private void InitAura()
        {
            // procs
            if (ProcTriggerFlags != ProcTriggerFlags.None || CasterProcSpells != null)
            {
                ProcTriggerEffects = Effects.Where(effect => effect.IsProc).ToArray();
                if (ProcTriggerEffects.Length == 0)
                {
                    // no proc-specific effects -> all effects are triggered on proc
                    ProcTriggerEffects = null;
                }
                //else if (ProcTriggerEffects.Length > 1)
                //{
                //    log.Warn("Spell {0} had more than one ProcTriggerEffect", this);
                //}

                IsProc = ProcTriggerEffects != null || ProcHandlers != null || CasterProcSpells != null ||
                    ProcCharges > 0;
            }

            IsAura = IsProc || HasEffectWith(effect =>
            {
                if (effect.AuraType != AuraType.None)
                {
                    return true;
                }
                return false;
            });

            ForeachEffect(effect =>
            {
                if (effect.IsAuraEffect)
                {
                    HasNonPeriodicAuraEffects = HasNonPeriodicAuraEffects || !effect.IsPeriodic;
                    HasPeriodicAuraEffects = HasPeriodicAuraEffects || effect.IsPeriodic;
                }
            });

            IsAutoRepeating = AttributesExB.HasFlag(SpellAttributesExB.AutoRepeat);

            HasManaShield = HasEffectWith(effect => effect.AuraType == AuraType.ManaShield);

            AuraEffects = GetEffectsWhere(effect => effect.AuraEffectHandlerCreator != null);
            AreaAuraEffects = GetEffectsWhere(effect => effect.IsAreaAuraEffect);

            IsAreaAura = AreaAuraEffects != null;
            IsPureAura = !IsDamageSpell && !HasEffectWith(effect => effect.EffectType != SpellEffectType.ApplyAura ||
                                                                    effect.EffectType != SpellEffectType.ApplyAuraToMaster ||
                                                                    effect.EffectType != SpellEffectType.ApplyStatAura ||
                                                                    effect.EffectType != SpellEffectType.ApplyStatAuraPercent);

            IsPureBuff = IsPureAura && HasBeneficialEffects && !HasHarmfulEffects;
            IsPureDebuff = IsPureAura && HasHarmfulEffects && !HasBeneficialEffects;

            IsVehicle = HasEffectWith(effect => effect.AuraType == AuraType.Vehicle);
            IsShapeshift = HasEffectWith(effect =>
            {
                //if (effect.AuraType == AuraType.ModShapeshift)
                //{
                //    var info = SpellHandler.ShapeshiftEntries.Get((uint)effect.MiscValue);
                //    return info.CreatureType > 0;
                //}
                return effect.AuraType == AuraType.ModShapeshift || effect.AuraType == AuraType.Transform;
            });

            // charges and stacks:
            CanStack = MaxStackCount > 0;
            if (ProcCharges > 0)
            {
                // applications will be used up by procs
                InitialStackCount = ProcCharges;
            }
            else
            {
                // applications can be added by re-applying
                InitialStackCount = 1;
            }

            IsGhost = HasEffectWith(effect => effect.AuraType == AuraType.Ghost);

            HasFlyEffect = HasEffectWith(effect => effect.AuraType == AuraType.Fly);

            IsFlyingMount = IsMount &&
                            HasEffectWith(effect => effect.AuraType == AuraType.ModSpeedMountedFlight);

            CanApplyMultipleTimes = Attributes == (SpellAttributes.InvisibleAura | SpellAttributes.Passive) &&
                                    Ability == null && Talent == null;

            HasShapeshiftDependentEffects = HasEffectWith(effect => effect.RequiredShapeshiftMask != 0);
            IsModalShapeshiftDependentAura = IsPassive && (RequiredShapeshiftMask != 0 || HasShapeshiftDependentEffects);

            if (AuraUID == 0)
            {
                CreateAuraUID();
            }
        }

        #endregion InitAura

        #region AuraUID Evaluation

        private void CreateAuraUID()
        {
            var count = AuraHandler.AuraIdEvaluators.Count;
            //for (var i = count-1; i >= 0; i--)
            for (var i = 0u; i < count; i++)
            {
                var eval = AuraHandler.AuraIdEvaluators[(int)i];
                if (eval(this))
                {
                    AuraUID = (uint)SpellLineId.End + i;
                    break;
                }
            }

            if (AuraUID == 0)
            {
                // by default the uid is the id of all spells in one line
                // and single spells get single unique ids
                if (Line != null)
                {
                    AuraUID = Line.AuraUID;
                }
                else
                {
                    AuraUID = AuraHandler.GetNextAuraUID();
                }
            }
        }

        #endregion AuraUID Evaluation

        #region Caster Proc Spells

        /// <summary>
        /// Add Spells which, when casted by the owner of this Aura, can cause it to trigger this spell's procs.
        /// Don't add damage spells (they will generate a Proc event anyway).
        /// </summary>
        public void AddCasterProcSpells(params SpellId[] spellIds)
        {
            var spells = new Spell[spellIds.Length];
            for (var i = 0; i < spellIds.Length; i++)
            {
                var id = spellIds[i];
                var spell = SpellHandler.Get(id);
                if (spell == null)
                {
                    throw new InvalidSpellDataException("Invalid SpellId: " + id);
                }
                spells[i] = spell;
            }
            AddCasterProcSpells(spells);
        }

        /// <summary>
        /// Add Spells which, when casted by the owner of this Aura, can cause it to trigger this spell's procs.
        /// Don't add damage spells (they will generate a Proc event anyway).
        /// </summary>
        public void AddCasterProcSpells(params SpellLineId[] spellSetIds)
        {
            var list = new List<Spell>(spellSetIds.Length * 6);
            foreach (var id in spellSetIds)
            {
                var line = SpellLines.GetLine(id);
                list.AddRange(line);
            }
            AddCasterProcSpells(list.ToArray());
        }

        /// <summary>
        /// Add Spells which, when casted by the owner of this Aura, can cause it to trigger this spell's procs.
        /// Don't add damage spells (they will generate a Proc event anyway).
        /// </summary>
        public void AddCasterProcSpells(params Spell[] spells)
        {
            if (CasterProcSpells == null)
            {
                CasterProcSpells = new HashSet<Spell>();
            }
            foreach (var spell in spells)
            {
                spell.GeneratesProcEventOnCast = true;
            }
            CasterProcSpells.AddRange(spells);
            //ProcTriggerFlags = ProcTriggerFlags.SpellCast;
        }

        #endregion Caster Proc Spells

        #region Target Proc Spells

        /// <summary>
        /// Add Spells which, when casted by others on the owner of this Aura, can cause it to trigger it's procs.
        /// Don't add damage spells (they will generate a Proc event anyway).
        /// </summary>
        public void AddTargetProcSpells(params SpellId[] spellIds)
        {
            var spells = new Spell[spellIds.Length];
            for (var i = 0; i < spellIds.Length; i++)
            {
                var id = spellIds[i];
                var spell = SpellHandler.Get(id);
                if (spell == null)
                {
                    throw new InvalidSpellDataException("Invalid SpellId: " + id);
                }
                spells[i] = spell;
            }
            AddTargetProcSpells(spells);
        }

        /// <summary>
        /// Add Spells which, when casted by others on the owner of this Aura, can cause it to trigger it's procs
        /// Don't add damage spells (they will generate a Proc event anyway).
        /// </summary>
        public void AddTargetProcSpells(params SpellLineId[] spellSetIds)
        {
            var list = new List<Spell>(spellSetIds.Length * 6);
            foreach (var id in spellSetIds)
            {
                var line = SpellLines.GetLine(id);
                list.AddRange(line);
            }
            AddTargetProcSpells(list.ToArray());
        }

        /// <summary>
        /// Add Spells which, when casted by others on the owner of this Aura, can cause it to trigger it's procs
        /// Don't add damage spells (they will generate a Proc event anyway).
        /// </summary>
        public void AddTargetProcSpells(params Spell[] spells)
        {
            if (TargetProcSpells == null)
            {
                TargetProcSpells = new HashSet<Spell>();
            }
            foreach (var spell in spells)
            {
                spell.GeneratesProcEventOnCast = true;
            }
            TargetProcSpells.AddRange(spells);
            //ProcTriggerFlags = ProcTriggerFlags.SpellCast;
        }

        #endregion Target Proc Spells

        #region CreateAuraEffectHandlers

        public List<AuraEffectHandler> CreateAuraEffectHandlers(
            ObjectReference caster,
            Unit target,
            bool beneficial)
        {
            return CreateAuraEffectHandlers(AuraEffects, caster, target, beneficial);
        }

        public static List<AuraEffectHandler> CreateAuraEffectHandlers(SpellEffect[] effects, ObjectReference caster,
            Unit target, bool beneficial)
        {
            if (effects == null)
                return null;

            try
            {
                List<AuraEffectHandler> effectHandlers = null;
                var failReason = SpellFailedReason.Ok;

                for (var i = 0; i < effects.Length; i++)
                {
                    var effect = effects[i];
                    if (effect.HarmType == HarmType.Beneficial || !beneficial)
                    {
                        var effectHandler = effect.CreateAuraEffectHandler(caster, target, ref failReason);
                        if (failReason != SpellFailedReason.Ok)
                        {
                            return null;
                        }

                        if (effectHandlers == null)
                        {
                            effectHandlers = new List<AuraEffectHandler>(3);
                        }
                        effectHandlers.Add(effectHandler);
                    }
                }
                return effectHandlers;
            }
            catch (Exception e)
            {
                LogUtil.ErrorException(e, "Failed to create AuraEffectHandlers for: " + effects.GetWhere(effect => effect != null).Spell);
                return null;
            }
        }

        #endregion CreateAuraEffectHandlers

        public bool CanOverride(Spell spell)
        {
            if (CanOverrideEqualAuraRank)
            {
                return Rank >= spell.Rank;
            }
            else
            {
                return Rank > spell.Rank;
            }
        }

        public AuraIndexId GetAuraUID(ObjectReference caster, WorldObject target)
        {
            return GetAuraUID(IsBeneficialFor(caster, target));
        }

        public AuraIndexId GetAuraUID(bool positive)
        {
            return new AuraIndexId
            {
                AuraUID = !CanApplyMultipleTimes ? AuraUID : AuraHandler.lastAuraUid + ++AuraHandler.randomAuraId,
                IsPositive = positive
            };
        }
    }
}