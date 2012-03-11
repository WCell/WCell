using NLog;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects.Custom
{
    /// <summary>
    /// Removes the cooldown for the SpellEffect.AffectSpellSet
    /// </summary>
    public class RemoveCooldownEffectHandler : SpellEffectHandler
    {
        public RemoveCooldownEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override SpellFailedReason Initialize()
        {
            if (Effect.AffectSpellSet == null)
            {
                LogManager.GetCurrentClassLogger().Warn("Tried to use {0} in Spell \"{1}\" with an empty SpellEffect.AffectSpellSet", GetType(), Effect.Spell);
                return SpellFailedReason.Error;
            }
            return SpellFailedReason.Ok;
        }

        protected override void Apply(WorldObject target)
        {
            if (target is Unit)
            {
                foreach (var spell in Effect.AffectSpellSet)
                {
                    ((Unit)target).Spells.ClearCooldown(spell, false);
                }
            }
        }
    }
}
