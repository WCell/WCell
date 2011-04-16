using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.RealmServer.Spells.Auras.Misc;
using WCell.RealmServer.Spells.Effects;
using WCell.RealmServer.Spells.Effects.Custom;
using WCell.Util;

namespace WCell.Addons.Default.Spells.DeathKnight
{
	public class DeathKnightFrostFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixIt()
		{
			FixMercilessCombat();

			// Chill of the grave does not have the right set of triggering proc spells
			SpellLineId.DeathKnightFrostChillOfTheGrave.Apply(spell =>
			{
				// "Chains of Ice, Howling Blast, Icy Touch and Obliterate"
				var effect = spell.GetEffect(AuraType.ProcTriggerSpellWithOverride);
				effect.ClearAffectMask();
				effect.AddAffectingSpells(SpellLineId.DeathKnightChainsOfIce, SpellLineId.DeathKnightFrostHowlingBlast,
					SpellLineId.DeathKnightIcyTouch, SpellLineId.DeathKnightObliterate);
			});

			// Improved Icy Touch needs to increase damage of Icy Touch
			SpellLineId.DeathKnightFrostImprovedIcyTouch.Apply(spell =>
			{
				// "Your Icy Touch does an additional $s1% damage"
				var effect = spell.GetEffect(AuraType.Dummy);
				effect.AuraType = AuraType.AddModifierPercent;
				effect.MiscValue = (int)SpellModifierType.SpellPower;
				effect.ClearAffectMask();
				effect.AddAffectingSpells(SpellLineId.DeathKnightIcyTouch);
			});

			FixBladeBarrier();
			FixRime();
			FixDeathKnightFrostAcclimation();
			FixTundraStalker();
			//FixDeathKnightFrostHungeringCold();
			FixGlacierRot();
			FixFrostPresence();

			// Improved Frost Presence also applies the s1% stamina to the other two presences
			SpellLineId.DeathKnightFrostImprovedFrostPresence.Apply(spell =>
			{
				var retainEffect = spell.GetEffect(AuraType.Dummy);
				retainEffect.AuraType = AuraType.ModTotalStatPercent;
				retainEffect.MiscValue = -1;
				retainEffect.AddRequiredActivationAuras(SpellLineId.DeathKnightUnholyPresence, SpellLineId.DeathKnightBloodPresence);
			});

			// Icy Talons applies a buff, when casting FrostFever
			SpellLineId.DeathKnightFrostIcyTalons.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.SpellCast;

				var effect = spell.GetEffect(AuraType.ProcTriggerSpellWithOverride);
				effect.ClearAffectMask();
				effect.AddAffectingSpells(SpellId.EffectFrostFever);
			});

			// Killing Machine: Proc chance scales with rank (10% per rank)
			SpellLineId.DeathKnightFrostKillingMachine.Apply(spell => spell.ProcChance = (uint)(10 * spell.Rank));

			FixObliterate();

			// Blood of the north converts runes when using "Blood Strike or Pestilence"
			DeathKnightFixes.MakeRuneConversionProc(SpellLineId.DeathKnightFrostBloodOfTheNorth,
													SpellLineId.DeathKnightBloodStrike, SpellLineId.DeathKnightPestilence,
													RuneType.Death, RuneType.Blood);

			// Improved Icy Talons only increases melee haste (not also ranged haste)
			SpellLineId.DeathKnightFrostImprovedIcyTalons.Apply(spell =>
			{
				spell.GetEffect(AuraType.ModHaste).AuraType = AuraType.ModMeleeHastePercent;
			});

			FixThreatOfThassarian();
			FixChainsOfIce();
		}

		#region Chains of Ice
		private static void FixChainsOfIce()
		{
			// "The target regains $s2% of their movement each second for $d."
			SpellLineId.DeathKnightChainsOfIce.Apply(spell =>
			{
				spell.GetEffect(AuraType.Dummy2).AuraEffectHandlerCreator = () => new ChainsOfIceThawEffect();
			});
		}

		internal class ChainsOfIceThawEffect : AuraEffectHandler
		{
			protected override void Apply()
			{
				var decreaseSpeedHandler = m_aura.GetHandler(AuraType.ModDecreaseSpeed) as ModDecreaseSpeedHandler;
				if (decreaseSpeedHandler != null)
				{
					// need to reduce, else upon removal, owner would be faster than before
					var thawValue = EffectValue/100f;
					decreaseSpeedHandler.Value += thawValue;
					Owner.SpeedFactor += thawValue;
				}
			}
		}
		#endregion

		#region Threat of Thassarian
		private static void FixThreatOfThassarian()
		{
			// Threat of Thassarian
			SpellLineId.DeathKnightFrostThreatOfThassarian.Apply(spell =>
			{
				// "When dual-wielding"
				spell.EquipmentSlot = EquipmentSlot.OffHand;

				var effect = spell.GetEffect(AuraType.Dummy);

				// "your Death Strikes, Obliterates, Plague Strikes, Rune Strikes, Blood Strikes and Frost Strikes"
				effect.AddAffectingSpells(SpellLineId.DeathKnightDeathStrike, SpellLineId.DeathKnightObliterate, SpellLineId.DeathKnightPlagueStrike,
					SpellLineId.DeathKnightRuneStrike, SpellLineId.DeathKnightFrostFrostStrike);

				effect.AuraEffectHandlerCreator = () => new ThreatOfThassarianHandler();
			});
		}

		internal class ThreatOfThassarianHandler : AttackEventEffectHandler
		{
			public override void OnAttack(DamageAction action)
			{
				var owner = action.Attacker; // also equal to the Owner property
				if (action.Weapon == owner.MainWeapon && owner.OffHandWeapon != null &&
					Utility.Random(0, 100) <= m_spellEffect.CalcEffectValue(owner))	// must recalculate probability every time
				{
					// "have a $s1% chance to also deal damage with your offhand weapon"
					// TODO: Auto attack or cast same spell with offhand weapon?
					var victim = action.Victim;
					var weapon = owner.OffHandWeapon;
					owner.AddMessage(() => owner.Strike(weapon, victim));
				}
			}
		}
		#endregion

		#region Obliterate
		private static void FixObliterate()
		{
			// Obliterate adds damage per disease and consumes diseases
			SpellLineId.DeathKnightObliterate.Apply(spell =>
			{
				var consumeEffect = spell.GetEffect(SpellEffectType.Dummy);
				consumeEffect.SpellEffectHandlerCreator = (cast, effct) => new ObliterateStrikeHandler(cast, effct);
			});
		}

		class ObliterateStrikeHandler : WeaponDamageEffectHandler
		{
			public ObliterateStrikeHandler(SpellCast cast, SpellEffect effect)
				: base(cast, effect)
			{
			}

			public override void OnHit(DamageAction action)
			{
				var doubleBonus = CalcEffectValue() * action.Victim.Auras.GetVisibleAuraCount(action.Attacker.SharedReference, DispelType.Disease);
				action.Damage += (action.Damage * doubleBonus + 100) / 200;	// + <1/2 of effect value> percent per disease

				// consume diseases if the Annihilation talent does not save them
				var annihilation = action.Attacker.Auras[SpellLineId.DeathKnightFrostAnnihilation];
				if (annihilation != null)
				{
					var dummy = annihilation.GetHandler(AuraType.Dummy);
					if (dummy != null)
					{
						if (Utility.Random(0, 101) < dummy.EffectValue)
						{
							// diseases remain
							return;
						}
					}
				}

				// consume diseases
				action.Victim.Auras.RemoveWhere(aura => aura.Spell.DispelType == DispelType.Disease);
			}
		}
		#endregion

		private static void FixFrostPresence()
		{
			// Frost Presence toggles a second aura
			SpellLineId.DeathKnightFrostPresence.Apply(spell =>
			{
				spell.AddAuraEffect(() => new ToggleAuraHandler(SpellId.FrostPresence));
			});
		}

		#region Glacier Rot
		private static void FixGlacierRot()
		{
			// "Diseased enemies take $s1% more damage from your Icy Touch, Howling Blast and Frost Strike"
			SpellLineId.DeathKnightFrostGlacierRot.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.Dummy);
				effect.MakeProc(() => new GlacierRotProcHandler(), SpellLineId.DeathKnightIcyTouch,
								SpellLineId.DeathKnightFrostHowlingBlast, SpellLineId.DeathKnightFrostFrostStrike);
			});
		}

		internal class GlacierRotProcHandler : ProcOnDiseaseTriggerSpellHandler
		{
			public override void OnProc(Unit triggerer, IUnitAction action)
			{
				if (action is DamageAction)
				{
					// "enemies take $s1% more damage"
					((DamageAction)action).ModDamagePercent(EffectValue);
				}
			}
		}
		#endregion

		#region Hungering Cold
        //private static void FixDeathKnightFrostHungeringCold()
        //{
        //    SpellLineId.DeathKnightFrostHungeringCold.Apply(spell =>
        //    {
        //        var effect = spell.GetEffect(SpellEffectType.Dummy);
        //        effect.ImplicitTargetA = ImplicitSpellTargetType.AllEnemiesAroundCaster;
        //        effect.ImplicitTargetB = ImplicitSpellTargetType.None;

        //        effect.EffectType = SpellEffectType.TriggerSpell;
        //        effect.TriggerSpellId = SpellId.HungeringColdRank1;
        //    });
        //    SpellHandler.Apply(spell =>
        //    {
        //        spell.ProcTriggerFlags = ProcTriggerFlags.None;
        //        spell.AddAuraEffect(() => new HungeringColdDebuffHandler());
        //    }, SpellId.HungeringColdRank1);
        //}

		internal class HungeringColdDebuffHandler : AttackEventEffectHandler
		{
			public override void OnBeforeAttack(DamageAction action)
			{ }

			public override void OnAttack(DamageAction action)
			{ }

			public override void OnDefend(DamageAction action)
			{
				// "Enemies are considered Frozen, but any damage other than diseases will break the ice"
				if (action.SpellEffect == null || action.Spell.DispelType != DispelType.Disease)
				{
					// break aura
					m_aura.Cancel();
				}
			}
		}
		#endregion

		#region Tundra Stalker
		private static void FixTundraStalker()
		{
			// Tundra Stalker needs a custom Attack event aura handler
			SpellLineId.DeathKnightFrostTundraStalker.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.OverrideClassScripts);
				effect.AuraEffectHandlerCreator = () => new TundraStalkerHandler();
			});
		}

		public class TundraStalkerHandler : AttackEventEffectHandler
		{
			public override void OnBeforeAttack(DamageAction action)
			{ }

			public override void OnAttack(DamageAction action)
			{
				// "spells and abilities deal $s1% more damage to targets infected with Frost Fever"
				if (action.SpellEffect != null && action.Victim.Auras.Contains(SpellId.EffectFrostFever))
				{
					action.ModDamagePercent(EffectValue);
				}
			}

			public override void OnDefend(DamageAction action)
			{
			}
		}
		#endregion

		#region Frost Acclimation
		private static void FixDeathKnightFrostAcclimation()
		{
			// Frost Acclimation trigger a server-side Aura on the entire party to improve resistences
			// http://www.wowhead.com/spell=50152#comments
			var triggerSpellId = (uint)SpellLineId.DeathKnightFrostAcclimation.GetLine().FirstRank.GetEffect(AuraType.ProcTriggerSpell).TriggerSpellId;
			var triggerSpell = SpellHandler.AddCustomSpell(triggerSpellId, "DeathKnightFrostAcclimation Buff");
			triggerSpell.AddAuraEffect(() => new FrostAcclimationBuffHandler(), ImplicitSpellTargetType.AllParty);
			triggerSpell.SetDuration(18000);	// "for 18 sec"

			SpellLineId.DeathKnightFrostAcclimation.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.ProcTriggerSpell);

				// not restricted by any particular spell
				effect.ClearAffectMask();
			});
		}

		/// <summary>
		/// "you have a $h% chance to boost your resistance to that type of magic for 18 sec"
		/// </summary>
		internal class FrostAcclimationBuffHandler : AuraEffectHandler
		{
			private const int ResistanceAmount = 50;
			private DamageSchool school;

			protected override void CheckInitialize(SpellCast creatingCast, ObjectReference casterReference, Unit target, ref SpellFailedReason failReason)
			{
				if (creatingCast != null && creatingCast.TriggerAction is DamageAction)
				{
					school = ((DamageAction)creatingCast.TriggerAction).UsedSchool;
				}
				else
				{
					failReason = SpellFailedReason.Error;
				}
			}

			protected override void Apply()
			{
				m_aura.Auras.Owner.AddResistanceBuff(school, ResistanceAmount);
			}

			protected override void Remove(bool cancelled)
			{
				m_aura.Auras.Owner.RemoveResistanceBuff(school, ResistanceAmount);
			}
		}
		#endregion

		#region Rime
		static void FixRime()
		{
			// Rime should only proc on Obliterate
			SpellLineId.DeathKnightFrostRime.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.SpellCast;

				var effect = spell.GetEffect(AuraType.ProcTriggerSpell);
				effect.ClearAffectMask();
				effect.AddAffectingSpells(SpellLineId.DeathKnightObliterate);
			});
			SpellHandler.Apply(spell =>
			{
				spell.AddEffect((cast, effct) => new RemoveCooldownEffectHandler(cast, effct), ImplicitSpellTargetType.Self);
			}, SpellId.EffectFreezingFog);
		}
		#endregion

		#region Blade Barrier
		private static void FixBladeBarrier()
		{
			// Blade Barrier only procs under special circumstances
			SpellLineId.DeathKnightBloodBladeBarrier.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.AuraStarted | ProcTriggerFlags.SpellCast;

				var effect = spell.GetEffect(AuraType.ProcTriggerSpell);
				effect.AuraEffectHandlerCreator = () => new BladeBarrierHandler();
			});
		}

		public class BladeBarrierHandler : ProcTriggerSpellHandler
		{
			public override bool CanProcBeTriggeredBy(IUnitAction action)
			{
				// only proc if it is a spell that costs Blood runes and if the caster is not already cooling down blood runes
				return
						action.Spell != null &&
						action.Spell.RuneCostEntry != null &&
						action.Spell.RuneCostEntry.GetCost(RuneType.Blood) > 0 &&							// must cost Blood runes
						action.Attacker is Character &&														// only player Characters have runes
						((Character)action.Attacker).PlayerSpells.Runes != null &&							// need runes
						((Character)action.Attacker).PlayerSpells.Runes.GetReadyRunes(RuneType.Blood) == SpellConstants.MaxRuneCountPerType && 	// no blood rune cooling down yet
						!((Character)action.Attacker).GodMode;												// won't cooldown in godmode
			}
		}
		#endregion

		#region Merciless Combat
		private static void FixMercilessCombat()
		{
			// Merciless Combat needs to be proc'ed and needs an extra check, so it only procs on "targets with less than 35% health"
			SpellLineId.DeathKnightFrostMercilessCombat.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.SpellCast;

				var effect = spell.GetEffect(AuraType.OverrideClassScripts);
				effect.IsProc = true;
				effect.AuraEffectHandlerCreator = () => new MercilessCombatHandler();
			});
		}

		public class MercilessCombatHandler : AuraEffectHandler
		{
			public override bool CanProcBeTriggeredBy(IUnitAction action)
			{
				return action.Victim.AuraState.HasAnyFlag(AuraStateMask.Health35Percent);
			}

			public override void OnProc(Unit triggerer, IUnitAction action)
			{
				// add extra damage
				if (action is DamageAction)
				{
					((DamageAction)action).ModDamagePercent(EffectValue);
				}
			}
		}
		#endregion
	}
}
