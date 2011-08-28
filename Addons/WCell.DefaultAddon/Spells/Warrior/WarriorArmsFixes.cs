using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;

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
				var effect = spell.GetEffect(AuraType.Dummy);
				if (effect != null)
				{
					effect.AuraEffectHandlerCreator = () => new ProcStrikeAdditionalTargetHandler();
					effect.IsProc = true;
				}
			});

			// Second wind triggers a spell on the carrier if "struck by a Stun or Immobilize effect"
			SpellHandler.Apply(spell =>
			{
				spell.AddProcHandler(new TriggerSpellProcHandlerTemplate(
					SpellHandler.Get(SpellId.ClassSkillSecondWindRank1),
					spell.ProcTriggerFlags,
					ProcHandler.StunValidator
				));
			}, SpellId.WarriorArmsSecondWindRank1);
			SpellHandler.Apply(spell =>
			{
				spell.AddProcHandler(new TriggerSpellProcHandlerTemplate(
					SpellHandler.Get(SpellId.ClassSkillSecondWindRank2),
					spell.ProcTriggerFlags,
					ProcHandler.StunValidator
				));
			}, SpellId.WarriorArmsSecondWindRank2);

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
				spell.GetEffect(SpellEffectType.ScriptEffect).SpellEffectHandlerCreator = (cast, effct) => new ShatteringThrowHandler(cast, effct);
				spell.Visual = 13222;
			});
		}
	}

	public class ShatteringThrowHandler : SpellEffectHandler
	{
		public ShatteringThrowHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}
		protected override void Apply(WorldObject target)
		{
			var unit = ((Unit)target);
			var removedInvul = false;
			unit.Auras.RemoveWhere(aura =>
			{
				if (aura.Spell.Mechanic == SpellMechanic.Invulnerable_2)
				{
					return removedInvul = true;
				}
				return false;
			});

			if (!removedInvul)
			{
				var caster = Cast.CasterChar;
				if (caster != null)
				{
					caster.SpellCast.Trigger(SpellId.ClassSkillShatteringThrow_2);
				}
			}
		}
	}

	public class ProcStrikeAdditionalTargetHandler : AuraEffectHandler
	{
		public override void OnProc(Unit triggerer, IUnitAction action)
		{
			var dmgAction = action as DamageAction;
			if (dmgAction == null) return;
			dmgAction.ReferenceCount++;

			Owner.AddMessage(() =>
			{
				var nextTarget = Owner.GetRandomVisibleUnit(Owner.MaxAttackRange, unit => Owner.MayAttack(unit) && unit != triggerer);
				if (nextTarget != null)
				{
					dmgAction.Victim = nextTarget;
					dmgAction.SpellEffect = m_spellEffect;
					triggerer.DoRawDamage(dmgAction);
				}
				// TODO: To ensure correct pooling, must ensure that reference count gets counted down
				// But object messages don't get executed if the object gets removed before execution
				dmgAction.ReferenceCount--;
			});
		}
	}
}

