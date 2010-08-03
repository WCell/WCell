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
using WCell.RealmServer.Spells.Effects.Custom;

namespace WCell.Addons.Default.Spells.DeathKnight
{
	public class DeathKnightFrostFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixIt()
		{
			// Merciless Combat needs to be proc'ed and needs an extra check, so it only procs on "targets with less than 35% health"
			SpellLineId.DeathKnightFrostMercilessCombat.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.SpellCast;

				var effect = spell.GetEffect(AuraType.OverrideClassScripts);
				effect.IsProc = true;
				effect.AuraEffectHandlerCreator = () => new MercilessCombatHandler();
			});

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

			// Blade Barrier only procs under special circumstances
			SpellLineId.DeathKnightBloodBladeBarrier.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.AuraStarted | ProcTriggerFlags.SpellCast;

				var effect = spell.GetEffect(AuraType.ProcTriggerSpell);
				effect.AuraEffectHandlerCreator = () => new BladeBarrierHandler();
			});

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
	}

	#region Blade Barrier
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
