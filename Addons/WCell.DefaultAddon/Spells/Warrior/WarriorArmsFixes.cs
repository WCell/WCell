using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Effects;

namespace WCell.Addons.Default.Spells.Warrior
{
	public static class WarriorArmsFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixIt()
		{
			// wrecking crew has no spell restrictions
			SpellLineId.WarriorArmsWreckingCrew.Apply(spell =>
			{
				spell.ForeachEffect(effect => effect.AffectMask = new uint[3]);
			});

			// Sword Spec should only proc once every 6 sec
			SpellLineId.WarriorArmsSwordSpecialization.Apply(spell => spell.ProcDelay = 6000);

			// Improved Hamstring can only be proc'ed by Hamstring
			SpellLineId.WarriorArmsImprovedHamstring.Apply(spell =>
				spell.AddCasterProcSpells(SpellLineId.WarriorHamstring));

			// Weapon Mastery reduces Disarm effects
			SpellLineId.WarriorArmsWeaponMastery.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.ModMechanicDurationPercent);
				if (effect != null)
				{
					effect.Mechanic = SpellMechanic.Disarmed;
				}
			});

			// Your next 5 melee attacks strike an additional nearby opponent.
			SpellLineId.WarriorArmsSweepingStrikes.Apply(spell =>
			{
				var effect = spell.GetAuraEffect(AuraType.Dummy);
				if (effect != null)
				{
					effect.AuraEffectHandlerCreator = () => new ProcStrikeAdditionalTargetHandler();
					effect.IsProc = true;
				}
			});

			// Second wind triggers a spell on the carrier if "struck by a Stun or Immobilize effect"
			SpellHandler.Apply(spell =>
			{
				spell.AddTargetProcHandler(new TriggerSpellProcHandler(
					ProcTriggerFlags.SpellHit,
					ProcHandler.StunValidator,
					SpellHandler.Get(SpellId.ClassSkillSecondWindRank1)
				));
			}, SpellId.WarriorArmsSecondWindRank1);
			SpellHandler.Apply(spell =>
			{
				spell.AddTargetProcHandler(new TriggerSpellProcHandler(
					ProcTriggerFlags.SpellHit,
					ProcHandler.StunValidator,
					SpellHandler.Get(SpellId.ClassSkillSecondWindRank2)
				));
			}, SpellId.WarriorArmsSecondWindRank2);

			// Trauma should only proc on crit hit
			SpellLineId.WarriorArmsTrauma.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.MeleeCriticalHitOther;
			});

			// Taste for blood only triggers once every 6 seconds
			SpellLineId.WarriorArmsTasteForBlood.Apply(spell =>
			{
				spell.ProcDelay = 6000;
			});

			// Heroic Throw "causing ${$m1+$AP*.50} damage"
			SpellLineId.WarriorHeroicThrow.Apply(spell =>
			{
				spell.Effects[0].APValueFactor = 0.5f;
			});

			// Mocking Blow is against npcs only
			SpellLineId.WarriorMockingBlow.Apply(spell =>
			{
				spell.CanCastOnPlayer = false;
			});

			// Heroic Throw "causing ${$m1+$AP*.50} damage"
			SpellLineId.WarriorHeroicThrow.Apply(spell =>
			{
				spell.Effects[0].APValueFactor = 0.5f;
			});

			// Shattering Throw "causing ${$64382m1+$AP*.50} damage" and 
			// TODO: "reducing the armor on the target by $64382s2% for $64382d or removing any invulnerabilities"
			SpellLineId.WarriorShatteringThrow.Apply(spell =>
			{
				spell.Effects[0].APValueFactor = 0.5f;
			});
		}
	}

	// TODO: substract consumed proc charges
	// TODO: make DoRawDamage aggro NPCs
	public class ProcStrikeAdditionalTargetHandler : AuraEffectHandler
	{
		public override void OnProc(RealmServer.Entities.Unit target, IUnitAction action)
		{
			var dmgAction = action as DamageAction;
			if (dmgAction == null) return;
			dmgAction.MarkInUse();
			Owner.AddMessage(() =>
			{
				var nextTarget = Owner.GetRandomUnit(Owner.MaxAttackRange, unit => Owner.MayAttack(unit) && unit != target);
				if (nextTarget != null)
				{
					dmgAction.Victim = nextTarget;
					dmgAction.SpellEffect = m_spellEffect;
					target.DoRawDamage(dmgAction);
				}
			});
		}
	}
}

