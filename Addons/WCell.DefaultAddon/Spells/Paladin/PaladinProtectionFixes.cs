using System;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Effects;

namespace WCell.Addons.Default.Spells.Paladin
{
	public static class PaladinProtectionFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixIt()
		{
			// Devotion Aura: Improved devotion aura needs all auras to have a 2nd effect for healing

            //SpellHandler.Apply(spell =>
            //{
            //    var firstEffect = spell.Effects[0];
            //    for (var i = spell.Effects.Length; i < 3; i++)
            //    {
            //        // add up to two filler effects, so the heal effect is at the 4th place
            //        spell.AddEffect(SpellEffectType.Dummy, ImplicitSpellTargetType.None);
            //    }

            //    if (spell.Effects.Length != 3)
            //    {
            //        throw new Exception("Spell has invalid amount of effects: " + spell);
            //    }

            //    // add heal effect as the 4th effect
            //    var healEffect = spell.AddEffect(SpellEffectType.ApplyRaidAura, ImplicitSpellTargetType.AllPartyInArea);
            //    healEffect.ImplicitTargetA = firstEffect.ImplicitTargetA;
            //    healEffect.Radius = firstEffect.Radius;
            //    healEffect.AuraType = AuraType.ModHealingTakenPercent;
            //}, PaladinFixes.PaladinAuras);


			// Improved devotion needs to apply the healing bonus to the 4th effect
            //SpellLineId.PaladinProtectionImprovedDevotionAura.Apply(spell =>
            //{
            //    var effect = spell.Effects[1];
            //    effect.MiscValue = (int)SpellModifierType.EffectValue4AndBeyond;
            //    effect.AddToAffectMask(PaladinFixes.PaladinAuras);	// applies to all auras
            //});


			// Spiritual Attunement gives mana "equal to $s1% of the amount healed"
            //SpellLineId.PaladinProtectionSpiritualAttunement.Apply(spell =>
            //{
            //    var effect = spell.GetEffect(AuraType.Dummy);
            //    effect.AuraEffectHandlerCreator = () => new SpiritualAttunementHandler();
            //});


			// Combat Expertise increases "chance to critically hit by $s2%"
            //SpellLineId.PaladinProtectionCombatExpertise.Apply(spell =>
            //{
            //    var effect = spell.Effects[1];
            //    effect.AuraType = AuraType.ModCritPercent;
            //});


			// Avenger's Shield should be "dealing ${$m1+0.07*$SPH+0.07*$AP} to ${$M1+0.07*$SPH+0.07*$AP} Holy damage"
            //SpellLineId.PaladinProtectionAvengersShield.Apply(spell =>
            //{
            //    var dmgEffect = spell.GetEffect(SpellEffectType.SchoolDamage);
            //    dmgEffect.APValueFactor = 0.07f;
            //    dmgEffect.SpellPowerValuePct = 7;
            //});

			// Shield of the Templar should proc from Avenger's Shield
            //SpellLineId.PaladinProtectionShieldOfTheTemplar.Apply(spell =>
            //{
            //    spell.SpellAuraOptions.ProcTriggerFlags = ProcTriggerFlags.SpellCast;
            //    spell.GetEffect(AuraType.ProcTriggerSpell).AddToAffectMask(SpellLineId.PaladinProtectionAvengersShield);
            //});

			// Shield of Righteousness "causing Holy damage based on your block value plus an additional $s1"
            //SpellLineId.PaladinShieldOfRighteousness.Apply(spell =>
            //{
            //    var dmgEffect = spell.GetEffect(SpellEffectType.SchoolDamage);
            //    dmgEffect.SpellEffectHandlerCreator = (cast, effct) => new ShieldOfRighteousnessHandler(cast, effct);
            //});

			// Righteous Defense should be "commanding up to 3 enemies attacking the target to attack the Paladin instead."
			SpellLineId.PaladinRighteousDefense.Apply(spell =>
			{
				// trigger uses a server-side spell to taunt 3 guys around the target
				var triggerEffect = spell.GetEffect(SpellEffectType.TriggerSpell);

				// create spell, give it 3 targets
				var triggerSpell = SpellHandler.AddCustomSpell((uint)triggerEffect.TriggerSpellId, "Righteous Defense Trigger");
                triggerSpell.SpellTargetRestrictions = new SpellTargetRestrictions { MaxTargets = 3 };

				// add taunt effect
				var tauntEff = triggerSpell.AddAuraEffect(AuraType.ModTaunt);
				tauntEff.ImplicitTargetA = ImplicitSpellTargetType.AllEnemiesInArea;
				tauntEff.Radius = 5;
			});

			// Hand of reckoning also deals 1 + 0.5 * AP damage
			SpellLineId.PaladinHandOfReckoning.Apply(spell =>
			{
				var dmgEffect = spell.AddEffect(SpellEffectType.SchoolDamage, spell.Effects[0].ImplicitTargetA);
				dmgEffect.APValueFactor = 0.5f;
			});

			// Hammer of Justice also interrupts spell casting
			SpellLineId.PaladinHammerOfJustice.Apply(spell =>
			{
				spell.AddTriggerSpellEffect(SpellId.InterruptRank1, ImplicitSpellTargetType.SingleEnemy);
			});
		}
	}

	public class SpiritualAttunementHandler : AuraEffectHandler
	{
		public override void OnProc(Unit triggerer, IUnitAction action)
		{
			if (action is HealAction)
			{
				var haction = (HealAction)action;
				var value = (haction.Value * EffectValue + 50) / 100;
				Owner.Energize(value, action.Attacker, SpellEffect);
			}
		}
	}

	public class ShieldOfRighteousnessHandler : SchoolDamageEffectHandler
	{
		public ShieldOfRighteousnessHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		protected override void Apply(WorldObject target)
		{
			var dmg = CalcEffectValue();
			if (m_cast.CasterChar != null)
			{
				dmg += (int)m_cast.CasterChar.BlockValue;
			}
			((Unit)target).DealSpellDamage(m_cast.CasterUnit, Effect, dmg);
		}
	}
}
