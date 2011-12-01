using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Auras.Misc;
using WCell.RealmServer.Spells.Effects;
using WCell.Util;

namespace WCell.Addons.Default.Spells.DeathKnight
{
	public class DeathKnightBloodFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixIt()
		{
			// Butchery lets you "generate up to 10 runic power" upon kill
			SpellLineId.DeathKnightBloodButchery.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.Dummy);
				effect.IsProc = true;
				effect.ClearAffectMask();
				effect.AuraEffectHandlerCreator = () => new ProcEnergizeHandler();
			});
            
			FixScentOfBlood();

            //// Mark of Blood only has Dummy and None effects
            //SpellLineId.DeathKnightBloodMarkOfBlood.Apply(spell =>
            //{
            //    // "Whenever the marked enemy deals damage to a target"
            //    spell.ProcTriggerFlags = ProcTriggerFlags.MeleeHitOther | ProcTriggerFlags.RangedHitOther | ProcTriggerFlags.SpellCast;
            //    // "that target is healed for $49005s2% of its maximum health"
            //    var effect2 = spell.Effects[1];
            //    effect2.IsProc = true;
            //    effect2.AuraEffectHandlerCreator = () => new MarkOfBloodAuraHandler();
            //});

			// Vendetta has a dummy instead of a heal effect
            //SpellLineId.DeathKnightBloodVendetta.Apply(spell =>
            //{
            //    // "Heals you for up to $s1% of your maximum health whenever you kill a target that yields experience or honor."
            //    spell.GetEffect(AuraType.Dummy).AuraType = AuraType.RegenPercentOfTotalHealth;
            //});

			// Hysteria does not drain life
            //SpellLineId.DeathKnightBloodHysteria.Apply(spell =>
            //{
            //    var effect = spell.GetEffect(AuraType.Dummy2);
            //    effect.AuraType = AuraType.PeriodicDamagePercent;
            //});

			// Sudden Doom: "Your Blood Strikes and Heart Strikes have a $h% chance to launch a free Death Coil at your target."
            //SpellLineId.DeathKnightBloodSuddenDoom.Apply(spell =>
            //{
            //    spell.ProcTriggerFlags = ProcTriggerFlags.SpellCast;
            //    // only one dummy -> Trigger highest level of death coil that the caster has instead
            //    // set correct trigger spells
            //    var effect = spell.GetEffect(AuraType.Dummy);
            //    effect.IsProc = true;
            //    effect.AddToAffectMask(SpellLineId.DeathKnightBloodStrike, SpellLineId.DeathKnightBloodHeartStrike);
            //    effect.AuraEffectHandlerCreator = () => new SuddenDoomAuraHandler();
            //});

            //SpellHandler.Apply(spell =>
            //{
            //    // TODO: Scale bloodworm strength
            //    // TODO: "Caster receives health when the Bloodworm deals damage"
            //    // Amount of bloodworms is wrong
            //    var effect = spell.GetEffect(SpellEffectType.Summon);
            //    effect.BasePoints = 1;
            //    effect.DiceSides = 3;
            //}, SpellId.EffectBloodworm);

			// Spell Deflection only has an Absorb effect, but should have a chance to reduce spell damage
            //SpellLineId.DeathKnightBloodSpellDeflection.Apply(spell =>
            //{
            //    spell.GetEffect(AuraType.SchoolAbsorb).AuraEffectHandlerCreator = () => new SpellDeflectionHandler();
            //});

			// "the Frost and Unholy Runes will become Death Runes when they activate"
            //DeathKnightFixes.MakeRuneConversionProc(SpellLineId.DeathKnightBloodDeathRuneMastery,
            //    SpellLineId.DeathKnightDeathStrike, SpellLineId.DeathKnightObliterate,
            //    RuneType.Death, RuneType.Frost, RuneType.Unholy);

			// Blood Presence is missing 4% life leech
			SpellLineId.DeathKnightBloodPresence.Apply(spell =>
			{
				var effect = spell.AddAuraEffect(() => new LifeLeechPercentAuraHandler());
				effect.BasePoints = 4;
			});

			// Improved Blood Presence also applies the 4% life leech to the other two presences
			SpellLineId.DeathKnightBloodImprovedBloodPresence.Apply(spell =>
			{
				var leechEffect = spell.GetEffect(AuraType.Dummy);
				leechEffect.AuraEffectHandlerCreator = () => new LifeLeechPercentAuraHandler();
				leechEffect.AddRequiredActivationAuras(SpellLineId.DeathKnightUnholyPresence, SpellLineId.DeathKnightFrostPresence);
			});

			FixBloodBoil();
			FixDeathPact();

			// Heart Strike adds damage per disease on target
            //SpellLineId.DeathKnightBloodHeartStrike.Apply(spell =>
            //{
            //    var effect = spell.GetEffect(SpellEffectType.None);
            //    effect.SpellEffectHandlerCreator = (cast, effct) => new WeaponDiseaseDamagePercentHandler(cast, effct);
            //});

			// The HS glyph procs on the wrong spell
            //SpellHandler.Apply(spell => spell.GetEffect(AuraType.ProcTriggerSpell).SetAffectMask(SpellLineId.DeathKnightBloodHeartStrike),
            //    SpellId.GlyphOfHeartStrike);

			// Vampiric blood increases health in %
			SpellLineId.DeathKnightBloodVampiricBlood.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.ModIncreaseHealth);
				effect.AuraType = AuraType.ModIncreaseHealthPercent;
			});

			// Blood Strike: "total damage increased by ${$m3/2}.1% for each of your diseases on the target"
            //SpellLineId.DeathKnightBloodStrike.Apply(spell =>
            //{
            //    spell.GetEffect(SpellEffectType.None).SpellEffectHandlerCreator =
            //        (cast, effct) => new WeaponDiseaseDamageHalfPercentHandler(cast, effct);
            //});

			// "Non-player victim spellcasting is also interrupted for $32747d."
			SpellLineId.DeathKnightStrangulate.Apply(spell => spell.AddTargetTriggerSpells(SpellId.InterruptRank1));
		}

		#region Death Pact
		private static void FixDeathPact()
		{
			SpellLineId.DeathKnightDeathPact.Apply(spell =>
			{
				// Heals in % 
				spell.GetEffect(SpellEffectType.Heal).EffectType = SpellEffectType.RestoreHealthPercent;
			});
		}
		#endregion

		#region Blood Boil
		private static void FixBloodBoil()
		{
			SpellLineId.DeathKnightBloodBoil.Apply(spell =>
			{
				spell.GetEffect(SpellEffectType.SchoolDamage).SpellEffectHandlerCreator =
					(cast, effct) => new BloodBoilDamageHandler(cast, effct);
			});
		}

		internal class BloodBoilDamageHandler : SchoolDamageEffectHandler
		{
			public BloodBoilDamageHandler(SpellCast cast, SpellEffect effect)
				: base(cast, effect)
			{
			}

			protected override void Apply(WorldObject target)
			{
				var unit = ((Unit)target);

				// "Deals additional damage to targets infected with Blood Plague or Frost Fever."
				if (unit.Auras.Contains(SpellId.EffectBloodPlague) ||
					unit.Auras.Contains(SpellId.EffectFrostFever))
				{
					unit.DealSpellDamage(m_cast.CasterUnit, Effect, CalcDamageValue() + 100);
				}
			}
		}
		#endregion

		#region Death Rune Mastery
		#endregion

		#region Scent of Blood
		private static void FixScentOfBlood()
		{
			// Scent of Blood is triggered by "dodging, parrying or taking  direct damage"
			SpellLineId.DeathKnightBloodScentOfBlood.Apply(spell =>
			{
				var triggerSpellEffect = spell.GetEffect(AuraType.ProcTriggerSpell);
				spell.ClearEffects();												// remove all effects

                spell.SpellAuraOptions.ProcHitFlags = ProcHitFlags.Dodge | ProcHitFlags.Parry | ProcHitFlags.Block;
				// create custom handler
				var handler = new TriggerSpellProcHandlerTemplate(
					SpellHandler.Get(triggerSpellEffect.TriggerSpellId),
                    spell.SpellAuraOptions.ProcTriggerFlags,
					(target, action) =>
					{
						var daction = action as DamageAction;
						if (daction == null) return false;
						return daction.ActualDamage > 0 ||
							   daction.VictimState == VictimState.Parry || daction.VictimState == VictimState.Dodge;
					}
					);

				// trigger the spell once per rank
				for (var i = 0; i < spell.Rank; i++)
				{
					spell.AddProcHandler(handler);
				}
			});
			// The proc'ed Aura of SoB triggers a non-existing spell
			SpellHandler.Apply(spell =>
			{
				// "The next successful melee attack will generate 10 runic power"
				var effect = spell.GetEffect(AuraType.ProcTriggerSpell);
				effect.BasePoints = 100;
				effect.IsProc = true;
				effect.AuraEffectHandlerCreator = () => new ProcEnergizeHandler();
			},
			 SpellId.EffectScentOfBlood);
		}
		#endregion
	}

	#region Spell Deflection
	public class SpellDeflectionHandler : AuraEffectHandler
	{
		public override bool CanProcBeTriggeredBy(IUnitAction action)
		{
			// "You have a chance equal to your Parry chance"
			return Utility.Random(0, 100) <= action.Victim.ParryChance;
		}

		public override void OnProc(Unit triggerer, IUnitAction action)
		{
			if (action is DamageAction)
			{
				// "taking $s1% less damage from a direct damage spell"
				((DamageAction)action).ModDamagePercent(-EffectValue);
			}
		}
	}
	#endregion

	#region Sudden Doom
	public class SuddenDoomAuraHandler : AuraEffectHandler
	{
		/// <summary>
		/// "launch a free Death Coil at your target"
		/// </summary>
		public override void OnProc(Unit triggerer, IUnitAction action)
		{
			// get highest level of Death Coil and throw it at the target
			var line = SpellLineId.DeathKnightDeathCoil.GetLine();
			var deathCoil = triggerer.Spells.GetHighestRankOf(line) ?? line.FirstRank;

			action.Attacker.SpellCast.Trigger(deathCoil, action.Victim);
		}
	}
	#endregion

	#region Mark of Blood
	public class MarkOfBloodAuraHandler : AuraEffectHandler
	{
		public override void OnProc(Unit triggerer, IUnitAction action)
		{
			// "target is healed for $49005s2% of its maximum health"
			triggerer.HealPercent(EffectValue, m_aura.CasterUnit, m_spellEffect);	// who is the healer? caster or target?
		}
	}
	#endregion
}
