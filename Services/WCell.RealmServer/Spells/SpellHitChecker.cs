using System.Linq;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.Util;

namespace WCell.RealmServer.Spells
{
    internal sealed class SpellHitChecker
    {
        private Spell spell;
        private WorldObject caster;

        private Unit target;

        public void Initialize(Spell spell, WorldObject caster)
        {
            this.spell = spell;
            this.caster = caster;
        }

        public CastMissReason CheckHitAgainstTarget(Unit target)
        {
            this.target = target;
            return CheckHitAgainstTarget();
        }

        private CastMissReason CheckHitAgainstTarget()
        {
            if (target.IsEvading)
            {
                return CastMissReason.Evade;
            }

            if (spell.IsAffectedByInvulnerability ||
                (target is Character && ((Character)target).Role.IsStaff))
            {
                if (target.IsInvulnerable)
                {
                    return CastMissReason.Immune_2;
                }

                if (spell.IsAffectedByInvulnerability && spell.Schools.All(target.IsImmune))
                {
                    return CastMissReason.Immune;
                }
            }

            bool missed = CheckMiss();
            if (missed)
            {
                return CastMissReason.Miss;
            }

            // TODO: Resist

            return CastMissReason.None;
        }

        private bool CheckMiss()
        {
            float hitChance = CalculateHitChanceAgainstTargetInPercentage();
            float roll = Utility.Random(SpellConstants.MinHitChance, SpellConstants.MaxHitChance + 1);
            return hitChance < roll;
        }

        private float CalculateHitChanceAgainstTargetInPercentage()
        {
            float minHitChance = SpellConstants.MinHitChance;
            float hitChance = CalculateBaseHitChanceAgainstTargetInPercentage();

            if (caster is Unit)
            {
                hitChance += (caster as Unit).GetHighestSpellHitChanceMod(spell.Schools);

                if (caster is Character)
                {
                    minHitChance = SpellConstants.CharacterMinHitChance;
                    hitChance += (caster as Character).SpellHitChanceFromHitRating;
                }
            }

            return MathUtil.ClampMinMax(hitChance, minHitChance, SpellConstants.MaxHitChance);
        }

        private int CalculateBaseHitChanceAgainstTargetInPercentage()
        {
            int levelDifference = target.Level - caster.SharedReference.Level;

            if (levelDifference < 3)
            {
                int baseHitChance = SpellConstants.HitChanceForEqualLevel - levelDifference;
                return baseHitChance > SpellConstants.MaxHitChance ? SpellConstants.MaxHitChance : baseHitChance;
            }

            int unitFactor;
            if (target is Character)
            {
                unitFactor = SpellConstants.HitChancePerLevelPvP;
            }
            else
            {
                unitFactor = SpellConstants.HitChancePerLevelPvE;
            }

            return 94 - (levelDifference - 2) * unitFactor;
        }
    }
}
