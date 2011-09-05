using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.RealmServer.Spells.Auras.Misc;
using WCell.RealmServer.Spells.Effects;
using WCell.RealmServer.Entities;

namespace WCell.Addons.Default.Spells.Paladin
{
	public static class PaladinRetributionFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixIt()
		{
			// Eye for an Eye should reflect damage
			SpellLineId.PaladinRetributionEyeForAnEye.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.None;
				spell.GetEffect(AuraType.Dummy).AuraEffectHandlerCreator = () => new ReflectDamagePctHandler();
			});

			// TODO: Repentance should be "removing the effect of Righteous Vengeance"
			SpellLineId.PaladinRetributionRepentance.Apply(spell =>
			{

			});

			// Judgements of The Wise procs spells on allies and self, upon damaging judgements
			SpellLineId.PaladinRetributionJudgementsOfTheWise.Apply(spell =>
			{
				spell.MaxTargets = 10;

				var effect1 = spell.AddAuraEffect(AuraType.ProcTriggerSpell, ImplicitSpellTargetType.PartyAroundCaster);
				effect1.TriggerSpellId = SpellId.EffectReplenishment;
				effect1.AddToAffectMask(SealsAndJudgements.AllJudgements);

				var effect2 = spell.AddAuraEffect(AuraType.ProcTriggerSpell, ImplicitSpellTargetType.Self);
				effect2.TriggerSpellId = SpellId.JudgementsOfTheWise;
				effect2.AddToAffectMask(SealsAndJudgements.AllJudgements);
			});

			// Replenishment effect "Replenishes $s1% of maximum mana per 5 sec for $57669d."
			SpellHandler.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.PeriodicEnergize);
				effect.Amplitude = 5000;
				effect.AuraEffectHandlerCreator = () => new PeriodicEnergizePctHandler();
			},
			SpellId.EffectReplenishment);

			// "Gain $s1% of your base mana."
			SpellHandler.Apply(spell =>
			{
				var effect = spell.GetEffect(SpellEffectType.Energize);
				effect.SpellEffectHandlerCreator = (cast, effct) => new EnergizePctEffectHandler(cast, effct);
			},
			SpellId.JudgementsOfTheWise);


			// Righteous Vengeance procs on crit with Judgement, Crusader Strike and Divine Storm
			SpellLineId.PaladinRetributionRighteousVengeance.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.Dummy);
				effect.AuraType = AuraType.ProcTriggerSpell;
				effect.TriggerSpellId = SpellId.RighteousVengeance;
				effect.AddToAffectMask(SealsAndJudgements.AllJudgements);
				effect.AddToAffectMask(SpellLineId.PaladinRetributionDivineStorm, SpellLineId.PaladinRetributionCrusaderStrike);

				// TODO: Create new aura handler
				//    -> after procing the debuff, get the SpellId.RighteousVengeance aura and set TotalDamage on it's ParameterizedPeriodicDamageHandler
			});
			SpellHandler.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.PeriodicDamage);
				effect.AuraEffectHandlerCreator = () => new ParameterizedPeriodicDamageHandler();
			},
			SpellId.RighteousVengeance);

			// TODO: PaladinRetributionSheathOfLight (similar to PaladinRetributionRighteousVengeance)

			// Hammer of Wrath "strikes an enemy for ${$m1+0.15*$SPH+0.15*$AP} to ${$M1+0.15*$SPH+0.15*$AP} Holy damage"
			SpellLineId.PaladinHammerOfWrath.Apply(spell =>
			{
				var dmgEffect = spell.GetEffect(SpellEffectType.SchoolDamage);
				dmgEffect.APValueFactor = 0.15f;
				dmgEffect.SpellPowerValuePct = 15;
			});

            // Divine Storm Heal Effect
            SpellLineId.PaladinRetributionDivineStorm.Apply(spell =>
            {
                spell.GetEffect(SpellEffectType.WeaponPercentDamage).SpellEffectHandlerCreator =
                    (cast, eff) => new DivineStormHealHandler(cast, eff);
      
            });
		}
	}

    public class DivineStormHealHandler : WeaponDamageEffectHandler
    {
        public DivineStormHealHandler(SpellCast cast, SpellEffect eff)
            : base(cast, eff) { }

        public override void OnHit(DamageAction action)
        {
            var chr = action.Attacker as Character;
            if (chr != null)
            {
                if (chr.Group != null)
                {
                    List<Character> origGroup = new List<Character>(chr.Group.GetAllCharacters());
                    List<Character> scrambledGroup = new List<Character>();

                    Random randNum = new Random();
                    int i = -1;
                    foreach (Character groupMember in origGroup)
                    {
                        i++;
                        if (groupMember == chr)
                            continue;
                        if (randNum.NextDouble() < (3 - scrambledGroup.Count / origGroup.Count - i))
                            scrambledGroup.Add(groupMember);
                    }
                    var effect = SpellHandler.Get(SpellId.DivineStorm_2).GetEffect(SpellEffectType.Heal);
                    foreach (Character target in scrambledGroup)
                        target.Heal(action.GetDamagePercent(25), chr, effect);
                }
            }
            base.OnHit(action);
        }

    }
	/// <summary>
	/// Reflects damage, but caps at 50% of wearer's max health
	/// </summary>
	public class ReflectDamagePctHandler : AttackEventEffectHandler
	{
		public ReflectDamagePctHandler()
		{
		}

		public override void OnBeforeAttack(DamageAction action)
		{
			// do nothing
		}

		public override void OnAttack(DamageAction action)
		{
			// do nothing
		}

		public override void OnDefend(DamageAction action)
		{
			if (!action.IsCritical)
			{
				return;
			}

			var max = action.Victim.MaxHealth / 2;

			// reflect % damage
			var effect = m_spellEffect;
			action.Victim.AddMessage(() =>
			{
				if (action.Victim.MayAttack(action.Attacker))
				{
					var dmg = (EffectValue * action.Damage + 50) / 100;
					if (dmg > max)
					{
						dmg = max;
					}
					action.Attacker.DealSpellDamage(action.Victim, effect, dmg);
				}
			});
		}
	}
}
