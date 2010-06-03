using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Effects;

namespace WCell.Addons.Default.Spells.Warrior
{
	public static class WarriorFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixWarrior()
		{

			// Charge doesn't generate rage
			SpellLineId.WarriorCharge.Apply(spell =>
			{
				var effect = spell.GetEffect(SpellEffectType.Dummy);
				if (effect != null)
				{
					effect.EffectType = SpellEffectType.Energize;
					//effect.SpellEffectHandlerCreator =
					//(cast, efct) => new EnergizeEffectHandler(cast, efct);
					effect.MiscValue = (int)PowerType.Rage;
				}
			});

			// Slam has the wrong effect type
			SpellLineId.WarriorSlam.Apply(spell =>
			{
				var effect = spell.GetEffect(SpellEffectType.Dummy);
				if (effect != null)
				{
					effect.EffectType = SpellEffectType.NormalizedWeaponDamagePlus;
				}
			});

			// Intimidating Shout should not have a single enemy target
			SpellLineId.WarriorChallengingShout.Apply(spell => spell.ForeachEffect(
				effect => { effect.ImplicitTargetA = ImplicitTargetType.AllEnemiesAroundCaster; }));

			// Sword Spec should only proc once every 6 sec
			SpellLineId.WarriorArmsSwordSpecialization.Apply(spell => spell.ProcDelay = 6000);

			// Improved Hamstring can only be proc'ed by Hamstring
			SpellLineId.WarriorArmsImprovedHamstring.Apply(spell =>
				spell.AddCasterProcSpells(SpellLineId.WarriorHamstring));

			// Shield Spec is proc'ed when owner dodges, blocks or parries
			SpellLineId.WarriorProtectionShieldSpecialization.Apply(spell =>
			{
				spell.AddCasterProcHandler(new TriggerSpellProcHandler(
					ProcTriggerFlags.MeleeAttack | ProcTriggerFlags.RangedAttack,
					ProcHandler.DodgeBlockOrParryValidator,
					SpellHandler.Get(SpellId.EffectShieldSpecializationRank1),
					spell.ProcChance
					));
			});

			// Gag Order needs a custom proc trigger and the correct auratype
			SpellLineId.WarriorProtectionGagOrder.Apply(spell =>
			{
				spell.AddCasterProcSpells(SpellLineId.WarriorShieldBash, SpellLineId.WarriorHeroicThrow);
				var effect = spell.GetAuraEffect(AuraType.Dummy);
				if (effect != null)
				{
					effect.AuraType = AuraType.ProcTriggerSpell;
				}
			});

			// Enrage and Wrecking Crew proc effects don't stack
			AuraHandler.AddAuraGroup(
				SpellId.EffectEnrageRank1_3, SpellId.EffectEnrageRank2_3, SpellId.EffectEnrageRank3_2, 
				SpellId.EffectEnrageRank4_2, SpellId.EffectEnrageRank5_2,
				SpellId.EffectEnrageRank1, SpellId.EffectEnrageRank2, SpellId.EffectEnrageRank3, 
				SpellId.EffectEnrageRank4, SpellId.EffectEnrageRank5);
		}
	}
}