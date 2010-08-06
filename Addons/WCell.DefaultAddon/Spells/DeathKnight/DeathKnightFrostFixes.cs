using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.RealmServer.Spells.Auras.Misc;
using WCell.RealmServer.Spells.Effects.Custom;

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
			FixDeathKnightFrostHungeringCold();
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
		}

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
		private static void FixDeathKnightFrostHungeringCold()
		{
			SpellLineId.DeathKnightFrostHungeringCold.Apply(spell =>
			{
				var effect = spell.GetEffect(SpellEffectType.Dummy);
				effect.ImplicitTargetA = ImplicitTargetType.AllEnemiesAroundCaster;
				effect.ImplicitTargetB = ImplicitTargetType.None;

				effect.EffectType = SpellEffectType.TriggerSpell;
				effect.TriggerSpellId = SpellId.HungeringColdRank1;
			});
			SpellHandler.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.None;
				spell.AddAuraEffect(() => new HungeringColdDebuffHandler());
			}, SpellId.HungeringColdRank1);
		}

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
				if (action.SpellEffect != null && action.Victim.Auras.Contains(SpellLineId.DeathKnightFrostFeverPassive))
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
			triggerSpell.AddAuraEffect(() => new FrostAcclimationBuffHandler(), ImplicitTargetType.AllParty);
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
				spell.AddEffect((cast, effct) => new RemoveCooldownEffectHandler(cast, effct), ImplicitTargetType.Self);
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
