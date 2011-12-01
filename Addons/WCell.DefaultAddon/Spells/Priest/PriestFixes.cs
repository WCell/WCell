using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;

namespace WCell.Addons.Default.Spells.Priest
{
	public static class PriestFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixPriest()
		{
            SpellLineId.PriestPowerWordShield.Apply(spell => spell.AddTriggerSpellEffect(SpellId.WeakenedSoul));

            SpellLineId.PriestDispelMagic.Apply(spell =>
            {
                var effect = spell.GetEffect(SpellEffectType.Dispel);
                effect.SpellEffectHandlerCreator = (cast, eff) => new DispelMagicHandler(cast, eff);
            });
		}

	}
    #region DispelMagicHandler
    class DispelMagicHandler : SpellEffectHandler
    {
        public DispelMagicHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        protected override void Apply(WorldObject target)
        {
            var chr = target as Character;

            if (chr != null)
            {
                if (target.IsFriendlyWith(Cast.CasterChar))
                {
                    chr.Auras.RemoveFirstVisibleAura(aura => aura.Spell.HasHarmfulEffects);
                    //if (Cast.Spell.Id == (int)SpellId.ClassSkillDispelMagicRank2)
                    //{
                    //    chr.Auras.RemoveFirstVisibleAura(aura => aura.Spell.HasHarmfulEffects);
                    //}
                    if (Cast.CasterChar.Spells.Contains(SpellId.GlyphOfDispelMagic))
                    {
                        int amountToHeal = (chr.Health * 3) / 100;
                        chr.Target.Heal(amountToHeal, Cast.CasterChar, Effect);
                    }
                }
                else
                {
                    chr.Auras.RemoveFirstVisibleAura(aura => aura.Spell.HasBeneficialEffects);
                    //if (Cast.Spell.Id == (int)SpellId.ClassSkillDispelMagicRank2)
                    //{
                    //    chr.Auras.RemoveFirstVisibleAura(aura => aura.Spell.HasHarmfulEffects);
                    //}
                }
            }
        }
    }
#endregion
}