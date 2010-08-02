using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;

namespace WCell.Addons.Default.Spells.Priest
{
	public static class PriestFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixPriest()
		{
			// only proc on kill that rewards xp or honor
			SpellLineId.PriestShadowSpiritTap.Apply(spell => spell.ProcTriggerFlags = ProcTriggerFlags.GainExperience);

			// Holy Inspiration can be proced when priest casts the given spells
			// TODO: Only cast on crit
			SpellLineId.PriestHolyInspiration.Apply(spell => spell.AddCasterProcSpells(
				SpellLineId.PriestFlashHeal,
				SpellLineId.PriestHeal,
				SpellLineId.PriestGreaterHeal,
				SpellLineId.PriestBindingHeal,
				SpellLineId.PriestDisciplinePenance,
				SpellLineId.PriestPrayerOfMending,
				SpellLineId.PriestPrayerOfHealing,
				SpellLineId.PriestHolyCircleOfHealing));

			// Mind Flay: Assault the target's mind with Shadow energy, causing ${$m3*3} Shadow damage over $d and slowing their movement speed by $s2%.
			SpellLineId.PriestShadowMindFlay.Apply(spell =>
			{
				var effect = spell.AddAuraEffect(AuraType.PeriodicDamage, ImplicitTargetType.SingleEnemy);
				effect.Amplitude = spell.Effects[2].Amplitude;
			});

			// Shadow Weaving applies to caster and can also be proc'ed by Mind Flay
			SpellLineId.PriestShadowShadowWeaving.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.AddTargetTrigger);
				effect.ImplicitTargetA = ImplicitTargetType.Self;
				effect.AddAffectingSpells(SpellLineId.PriestShadowMindFlay);
			});

			// Dispersion also regenerates Mana
			SpellLineId.PriestShadowDispersion.Apply(spell =>
			{
				var effect = spell.AddPeriodicTriggerSpellEffect(SpellId.Dispersion_2, ImplicitTargetType.Self);
				effect.Amplitude = 1000;
			});

			// Vampiric Embrace can be proc'ed by a certain set of spells, and has a custom healing AuraEffectHandler
			SpellLineId.PriestShadowVampiricEmbrace.Apply(spell =>
			{
				// change Dummy to proc effect
				var effect = spell.Effects[0];
				effect.IsProc = true;
				effect.AuraEffectHandlerCreator = () => new AuraVampiricEmbracerHandler();

				// Set correct flags and set of spells to trigger the proc
				spell.ProcTriggerFlags = ProcTriggerFlags.SpellCast;
				spell.AddCasterProcSpells(
					SpellLineId.PriestShadowMindFlay,
					SpellLineId.PriestShadowWordPain,
					SpellLineId.PriestShadowWordDeath,
					SpellLineId.PriestMindBlast,
					SpellLineId.PriestManaBurn,
					SpellLineId.PriestDevouringPlague,
					SpellLineId.PriestShadowVampiricTouch,
					SpellLineId.PriestMindSear);
			});

            SpellLineId.PriestDispelMagic.Apply(spell =>
            {
                var effect = spell.GetEffect(SpellEffectType.Dispel);
                effect.SpellEffectHandlerCreator = (cast, eff) => new DispelMagicHandler(cast, eff);
            });
		}

	}

	#region AuraVampiricEmbracerHandler
	public class AuraVampiricEmbracerHandler : AuraEffectHandler
	{
		public override void OnProc(Unit triggerer, IUnitAction action)
		{
			if (action is IDamageAction)
			{
				var owner = Owner;
				var dmgAction = ((IDamageAction)action);

				var healSelfAmount = ((dmgAction.Damage * EffectValue) + 50) / 100;	// don't forget rounding
				var healPartyAmount = (healSelfAmount + 3) / 5; // don't forget rounding

				owner.Heal(healSelfAmount, owner, SpellEffect);
				if (owner is Character)
				{
					var chr = (Character)owner;
					var group = chr.Group;
					if (group != null)
					{
						// heal all group members in same context (ie same Region in current implementation)
						group.CallOnAllInSameContext(chr.ContextHandler, (member) =>
						{
							member.Heal(healPartyAmount, owner, SpellEffect);
						});
					}
				}
			}
		}
	}
	#endregion

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
                    if (Cast.Spell.Id == (int)SpellId.ClassSkillDispelMagicRank2)
                    {
                        chr.Auras.RemoveFirstVisibleAura(aura => aura.Spell.HasHarmfulEffects);
                    }
                    if (Cast.CasterChar.Spells.Contains(SpellId.GlyphOfDispelMagic) || Cast.CasterChar.Spells.Contains(SpellId.GlyphOfDispelMagic_2))
                    {
                        int amountToHeal = (chr.Health * 3) / 100;
                        chr.Target.Heal(amountToHeal, Cast.CasterChar, Effect);
                    }
                }
                else
                {
                    chr.Auras.RemoveFirstVisibleAura(aura => aura.Spell.HasBeneficialEffects);
                    if (Cast.Spell.Id == (int)SpellId.ClassSkillDispelMagicRank2)
                    {
                        chr.Auras.RemoveFirstVisibleAura(aura => aura.Spell.HasHarmfulEffects);
                    }
                }
            }
        }
    }
#endregion
}