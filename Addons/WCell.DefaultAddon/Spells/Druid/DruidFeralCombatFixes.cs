using System;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.RealmServer.Spells.Auras.Misc;
using WCell.RealmServer.Spells.Effects;

namespace WCell.Addons.Default.Spells.Druid
{
	public static class DruidFeralCombatFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixIt()
		{
			// triggered Aura of LotP: First effect has invalid radius; 2nd effect is the proc effect for the Improved LotP buff
			SpellHandler.Apply(spell =>
			{
				spell.ForeachEffect(effect => effect.Radius = 45);	// fix radius

				// "heal themselves for $s1% of their total health when they critically hit with a melee or ranged attack"
				spell.SpellAuraOptions.ProcTriggerFlags = ProcTriggerFlags.DoneMeleeSpell | ProcTriggerFlags.DoneMeleeAutoAttack | ProcTriggerFlags.DoneRangedAutoAttack | ProcTriggerFlags.DoneRangedSpell;

				// "The healing effect cannot occur more than once every 6 sec"
				spell.ProcDelay = 6000;

				// make this a special proc
				var dummy = spell.GetEffect(AuraType.Dummy);
				dummy.IsProc = true;
				dummy.AuraEffectHandlerCreator = () => new ImprovedLeaderOfThePackProcHandler();
			},
			SpellId.LeaderOfThePack);

			// Nurturing Instinct simply toggles an aura when in cat form: "and increases healing done to you by $47179s1% while in Cat form."
			SpellHandler.Apply(spell =>
			{
				spell.AddAuraEffect(() => new ToggleAuraHandler(SpellId.NurturingInstinctRank1)).RequiredShapeshiftMask = ShapeshiftMask.Cat;
			},
			SpellId.DruidFeralCombatNurturingInstinctRank1);
			SpellHandler.Apply(spell =>
			{
				spell.AddAuraEffect(() => new ToggleAuraHandler(SpellId.NurturingInstinctRank2)).RequiredShapeshiftMask = ShapeshiftMask.Cat;
			},
			SpellId.DruidFeralCombatNurturingInstinctRank2);

			FixFeralSwiftness(SpellId.DruidFeralCombatFeralSwiftness, SpellId.FeralSwiftnessPassive1a);
			FixFeralSwiftness(SpellId.DruidFeralCombatFeralSwiftness_2, SpellId.FeralSwiftnessPassive2a);

			// Rake has + AP damage
			SpellLineId.DruidRake.Apply(spell =>
			{
				// "Rake the target for ${$AP/100+$m1} bleed damage and an additional ${$m2*3+$AP*0.18} damage over $d"
				var dmgEffect = spell.GetEffect(SpellEffectType.SchoolDamage);
				dmgEffect.APValueFactor = 0.01f;

				var dotEffect = spell.GetEffect(AuraType.PeriodicDamage);
				dotEffect.APValueFactor = 0.18f / dotEffect.GetMaxTicks();
			});

			// Shred: "Effects which increase Bleed damage also increase Shred damage."
			SpellLineId.DruidShred.Apply(spell =>
			{
				spell.GetEffect(SpellEffectType.WeaponDamage).SpellEffectHandlerCreator = (cast, effct) => new AddBleedWeaponDamageHandler(cast, effct);
			});

			// Bash "interrupts non-player spellcasting for $32747d."
			SpellLineId.DruidBash.Apply(spell =>
			{
				spell.AddTriggerSpellEffect(SpellId.InterruptRank1, ImplicitSpellTargetType.SingleEnemy);
			});

			// FR: "Converts up to 10 rage per second into health for $d.  Each point of rage is converted into ${$m2/10}.1% of max health."
            //SpellLineId.DruidFrenziedRegeneration.Apply(spell =>
            //{
            //    var dummy = spell.GetEffect(SpellEffectType.Dummy);
            //    dummy.AuraPeriod = 1000;
            //    dummy.AuraEffectHandlerCreator = () => new FrenziedGenerationHandler();
            //});

			// Save Roar has two dummy effects
			SpellLineId.DruidSavageRoar.Apply(spell =>
			{
				// "Only useable while in Cat Form"
                spell.SpellShapeshift = new SpellShapeshift { RequiredShapeshiftMask = ShapeshiftMask.Cat };

				// set correct target type
				spell.GetEffect(SpellEffectType.Dummy).ImplicitTargetA = ImplicitSpellTargetType.Self;

				// aura increases dmg done %
				spell.GetEffect(AuraType.Dummy).AuraType = AuraType.ModDamageDonePercent;
			});

			// Ferocious Bite deals damage per AP & CP: "1 point  : ${$m1+$b1*1+0.07*$AP}-${$M1+$b1*1+0.07*$AP} damage"
			//		"and converts each extra point of energy (up to a maximum of $s2 extra energy) into ${$f1+$AP/410}.1 additional damage"
			SpellLineId.DruidFerociousBite.Apply(spell =>
			{
				// damage is increased by AP & CP
				var dmgEffect = spell.GetEffect(SpellEffectType.SchoolDamage);
				dmgEffect.APPerComboPointValueFactor = 0.07f;
				dmgEffect.SpellEffectHandlerCreator = (cast, effct) => new FerociousBiteHandler(cast, effct);
			});

			// Cat Form has a periodic effect that should trigger a passive aura
            //SpellLineId.DruidCatForm.Apply(spell =>
            //{
            //    spell.RemoveEffect(AuraType.PeriodicTriggerSpell);

            //    // "increasing melee attack power by $3025s1 plus Agility"
            //    spell.AddAuraEffect(() => new ToggleAuraHandler(SpellId.ClassSkillCatFormPassivePassive));
            //});
			SpellHandler.Apply(spell =>
			{
				// "increasing melee attack power by $3025s1 plus Agility"
				// => Add 100% of agility to AP
				var apMod = spell.AddAuraEffect(AuraType.ModMeleeAttackPowerByPercentOfStat);
				apMod.MiscValue = (int)StatType.Agility;
				apMod.BasePoints = 100;
			}, SpellId.ClassSkillCatFormPassivePassive);

			// Flight form also toggles a passive Aura while activated
            //SpellLineId.DruidSwiftFlightForm.Apply(spell =>
            //{
            //    spell.AddAuraEffect(() => new ToggleAuraHandler(SpellId.SwiftFlightFormPassivePassive));
            //});

			// Lacerate is only usable in Bear form
			SpellLineId.DruidLacerate.Apply(spell =>
			{
				spell.SpellShapeshift.RequiredShapeshiftMask = ShapeshiftMask.Bear;

				// "Introduced attack power scaling: Every 20 Attack Power adds 1 damage, per stack, over 15 seconds. Mangle further enhances this value."
				spell.GetEffect(AuraType.PeriodicDamage).APValueFactor = 0.05f;
			});

			// Aqua form also toggles a passive Aura while activated
            //SpellLineId.DruidAquaticForm.Apply(spell =>
            //{
            //    spell.AddAuraEffect(() => new ToggleAuraHandler(SpellId.ClassSkillAquaticFormPassivePassive));
            //});

			// Enrage "reduces base armor by 27% in Bear Form and 16% in Dire Bear Form"
			SpellLineId.DruidEnrage.Apply(spell =>
			{
				var bearEffect = spell.AddAuraEffect(AuraType.ModResistancePercent);
				bearEffect.MiscValue = (int)DamageSchool.Physical;
				bearEffect.BasePoints = 27;
				bearEffect.RequiredShapeshiftMask = ShapeshiftMask.Bear;

				var direBearEffect = spell.AddAuraEffect(AuraType.ModResistancePercent);
				direBearEffect.MiscValue = (int)DamageSchool.Physical;
				direBearEffect.BasePoints = 16;
				direBearEffect.RequiredShapeshiftMask = ShapeshiftMask.DireBear;
			});

			// Mangle (Cat) triggers a spell to add a CP
            //SpellLineId.DruidMangleCat.Apply(spell =>
            //{
            //    spell.AddTriggerSpellEffect(SpellId.ComboPoint);
            //});

            //// Dire Bear is missing it's passive Aura
            //SpellLineId.DruidDireBearForm.Apply(spell =>
            //{
            //    spell.AddAuraEffect(() => new ToggleAuraHandler(SpellId.ClassSkillDireBearFormPassivePassive));
            //});

            //// Travel form also requires it's passive Aura
            //SpellLineId.DruidTravelForm.Apply(spell =>
            //{
            //    spell.AddAuraEffect(() => new ToggleAuraHandler(SpellId.TravelFormPassivePassive));
            //});

            //// Savage Defense has wrong trigger flags & it's buff has wrong effect type 
            //SpellLineId.DruidSavageDefense.Apply(spell =>
            //{
            //    spell.ProcTriggerFlags = ProcTriggerFlags.MeleeCriticalHitOther;
            //    spell.RequiredShapeshiftMask = ShapeshiftMask.Bear | ShapeshiftMask.DireBear;
            //});
			SpellHandler.Apply(spell =>
			{
				spell.GetEffect(AuraType.SchoolAbsorb).AuraType = AuraType.ModDamageTakenPercent;
			}, SpellId.EffectSavageDefense);

			// Flight Form is also missing it's passive Aura
            //SpellLineId.DruidFlightForm.Apply(spell =>
            //{
            //    spell.AddAuraEffect(() => new ToggleAuraHandler(SpellId.FlightFormPassivePassive));
            //});

			//Wrong Facing Requirement
			SpellLineId.DruidPounce.Apply(spell =>
				{
					spell.AttributesExB = SpellAttributesExB.None;
				});

            // Druid Faerie Fire / Druid Faerie Fire (Feral)
            // Should add stealth/invis immunity to the target.
            SpellHandler.Apply(spell =>
            {
                var stealth = spell.AddAuraEffect(AuraType.DispelImmunity, ImplicitSpellTargetType.SingleEnemy);
                var invis = spell.AddAuraEffect(AuraType.DispelImmunity, ImplicitSpellTargetType.SingleEnemy);

                stealth.MiscValue = (int)DispelType.Stealth;
                invis.MiscValue = (int)DispelType.Invisibility;
            }, SpellLineId.DruidFaerieFire, SpellLineId.DruidFaerieFireFeral);

			FixBloodFrenzy();

            // Dash should only be usable while in cat form
            SpellLineId.DruidDash.Apply(spell =>
            {
                if(spell.SpellShapeshift != null)
                    spell.SpellShapeshift.RequiredShapeshiftMask = ShapeshiftMask.Cat;
                else
                {
                    spell.SpellShapeshift = new SpellShapeshift {RequiredShapeshiftMask = ShapeshiftMask.Cat};
                }
            });
		}

		#region Blood Frenzy
		private static void FixBloodFrenzy()
		{
            //SpellLineId.DruidBloodFrenzy.Apply(spell =>
            //{
            //    spell.ProcTriggerFlags = ProcTriggerFlags.MeleeCriticalHitOther | ProcTriggerFlags.RangedCriticalHit |
            //                             ProcTriggerFlags.SpellCastCritical;
            //    spell.RequiredShapeshiftMask = ShapeshiftMask.Cat;

            //    var effect = spell.GetEffect(AuraType.ProcTriggerSpell);
            //    effect.AuraEffectHandlerCreator = () => new BloodFrenzyHandler();
            //});
		}

		internal class BloodFrenzyHandler : ProcTriggerSpellHandler
		{
			public override bool CanProcBeTriggeredBy(IUnitAction action)
			{
				return action.Spell != null && action.Spell.GeneratesComboPoints;
			}
		}
		#endregion

		#region FixFeralSwiftness
		private static void FixFeralSwiftness(SpellId origSpell, SpellId triggerSpell)
		{
			// Feral Swiftness should only be applied in Cat Form
			SpellHandler.Apply(spell =>
			{
				// only as cat
				// triggers the dodge effect spell
				spell.SpellShapeshift.RequiredShapeshiftMask = ShapeshiftMask.Cat;
				spell.AddTriggerSpellEffect(triggerSpell);
			},
			origSpell);
			SpellHandler.Apply(spell =>
			{
				// this spell applies the dodge bonus for Feral Swiftness
				// "increases your chance to dodge while in Cat Form, Bear Form and Dire Bear Form"
				// must be passive
				spell.Attributes |= SpellAttributes.Passive;
				spell.SpellShapeshift.RequiredShapeshiftMask = ShapeshiftMask.Cat | ShapeshiftMask.Bear | ShapeshiftMask.DireBear;
			},
			triggerSpell);
		}
		#endregion
	}

	#region FerociousBite
	public class FerociousBiteHandler : SchoolDamageEffectHandler
	{
		public FerociousBiteHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		protected override void Apply(WorldObject target)
		{
			var unit = (Unit)target;
			var otherEffect = Effect.Spell.GetEffect(SpellEffectType.None);

			// "up to a maximum of $s2 extra energy"
			var maxEnergy = otherEffect.CalcEffectValue(m_cast.CasterUnit);
			var energy = unit.Power;
			var toConsume = Math.Min(energy, maxEnergy);
			unit.Power = energy - toConsume;

			// "each extra point (...) into ${$f1+$AP/410}.1 additional damage"
            var bonusDmg = toConsume * (int)(Effect.ChainAmplitude + ((unit.TotalMeleeAP + 210) / 410f));

			((Unit)target).DealSpellDamage(m_cast.CasterUnit, Effect, CalcDamageValue() + bonusDmg);
		}
	}
	#endregion

	#region FrenziedGenerationHandler
	public class FrenziedGenerationHandler : AuraEffectHandler
	{
		protected override void Apply()
		{
			// one point of rage is 10 actual rage

			// "Converts up to 10 rage per second"
			var rage = Owner.Power;
			var toConsume = Math.Min(rage, 100);
			Owner.Power = rage - toConsume;

			// "Each point of rage is converted into ${$m2/10}.1% of max health"
			var health = (rage * EffectValue * Owner.MaxHealth + 50) / 10000;
			Owner.Heal(health, m_aura.CasterUnit, m_spellEffect);
		}
	}
	#endregion

	#region Maul & Shred
	public class AddBleedWeaponDamageHandler : WeaponDamageEffectHandler
	{
		public AddBleedWeaponDamageHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override void OnHit(DamageAction action)
		{
			// "Effects which increase Bleed damage also increase Maul damage."
			var bleedBonusPct = action.Attacker.Auras.GetBleedBonusPercent();
			action.ModDamagePercent(bleedBonusPct);
		}
	}
	#endregion

	#region ImprovedLeaderOfThePackProcHandler
	public class ImprovedLeaderOfThePackProcHandler : AuraEffectHandler
	{
		public override bool CanProcBeTriggeredBy(IUnitAction action)
		{
			// Check whether Improved LotP has been activated yet
			return EffectValue > 0;
		}

		public override void OnProc(Unit triggerer, IUnitAction action)
		{
			// "causes affected targets to heal themselves for $s1% of their total health when they critically hit with a melee or ranged attack."
			action.Attacker.HealPercent(EffectValue, m_aura.CasterUnit, m_spellEffect);

			if (action.Attacker == m_aura.CasterUnit)
			{
				// "In addition, you gain $s2% of your maximum mana when you benefit from this heal."
				action.Attacker.EnergizePercent(EffectValue * 2, m_aura.CasterUnit, m_spellEffect);
			}
		}
	}
	#endregion
}
