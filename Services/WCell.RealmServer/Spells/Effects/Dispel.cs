using System;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.Util;

namespace WCell.RealmServer.Spells.Effects
{
    public class DispelEffectHandler : SpellEffectHandler
    {
        public DispelEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        protected override void Apply(WorldObject target)
        {
            var dispelType = (DispelType)Effect.MiscValue;
            if (dispelType == DispelType.None)
            {
                throw new Exception("Invalid DispelType None in Spell: " + Effect.Spell);
            }

            var caster = Cast.CasterUnit;
            var max = CalcEffectValue();

            foreach (var aura in ((Unit)target).Auras)
            {
                if (aura.Spell.DispelType == dispelType)
                {
                    // Check dispel resistance
                    var auraCaster = aura.CasterUnit;
                    if (caster != null && auraCaster != null && caster.MayAttack(auraCaster))
                    {
                        // trying to remove buff from enemy or debuff from friend

                        var dispelResistance = auraCaster.Auras.GetModifiedInt(SpellModifierType.DispelResistance, aura.Spell, 1);	// base chance of 1%

                        // "Reduces the chance [auras] will be dispelled by x%"
                        if (dispelResistance > Utility.Random(100))
                        {
                            if (--max == 0)		// one less to dispel
                            {
                                break;
                            }
                            continue;
                        }
                    }
                    aura.Remove();
                    if (--max == 0)
                    {
                        break;
                    }
                }
            }
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Unit; }
        }
    }
}