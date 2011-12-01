using System.Collections.Generic;
using WCell.Constants.Spells;
using WCell.Constants.Talents;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras.Misc;
using WCell.Util;

namespace WCell.Addons.Default.Spells.Mage
{
	public static class MageFireFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixSpells()
		{
			// Impact needs the right triggers
			SpellLineId.MageFireImpact.Apply(spell =>
			{
				var triggerEffect = spell.GetEffect(AuraType.ProcTriggerSpell);
				// triggerEffect.AddAffectingSpells(...); // TODO: Triggered by all damaging spells
			});
			// Impact's triggered effect also needs some adjustments
			SpellHandler.Apply(spell =>
			{
				var triggerEffect = spell.GetEffect(AuraType.ProcTriggerSpell);
				triggerEffect.ImplicitTargetA = ImplicitSpellTargetType.SingleEnemy;
				triggerEffect.AddAffectingSpells(SpellLineId.MageFireBlast);		// triggered by fire blast only
			}, SpellId.EffectImpactRank1);

			// Combustion should proc
            //SpellLineId.MageFireCombustion.Apply(spell =>
            //{
            //    spell.SpellAuraOptions.ProcTriggerFlags = ProcTriggerFlags.SpellCast;
            //    var modEffect = spell.GetEffect(AuraType.AddModifierPercent);

            //    // the trigger effect is actually supposed to be proc'ed by the same spells that have their crit damage increased
            //    var triggerEffect = spell.GetEffect(SpellEffectType.TriggerSpell);
            //    triggerEffect.EffectType = SpellEffectType.ApplyAura;
            //    triggerEffect.AuraType = AuraType.ProcTriggerSpell;
            //    triggerEffect.AffectMask = modEffect.AffectMask;
            //});

			// Mage Fire Blazing Speed has wrong trigger proc id
			SpellLineId.MageFireBlazingSpeed.Apply(spell =>
			{
				var triggerEffect = spell.GetEffect(AuraType.ProcTriggerSpell);
				triggerEffect.TriggerSpellId = SpellId.ClassSkillBlazingSpeed;
			});

			SpellLineId.MageFireMasterOfElements.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.Dummy);
				effect.AuraEffectHandlerCreator = () => new MasterOfElementsHandler();
			});

			// Molten Fury: "Increases damage of all spells against targets with less than 35% health by $s1%."
			SpellLineId.MageFireMoltenFury.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.OverrideClassScripts);
				effect.AuraEffectHandlerCreator = () => new MoltenFuryHandler();
			});

			// Fire Starter has the wrong trigger spells
			// "Blast Wave and Dragon's Breath spells have a $h% chance to..."
            //SpellLineId.MageFireFirestarter.Apply(spell =>
            //{
            //    var effect = spell.GetEffect(AuraType.ProcTriggerSpell);
            //    effect.SetAffectMask(SpellLineId.MageFireDragonsBreath, SpellLineId.MageFireBlastWave);
            //});

			// "Any time you score 2 non-periodic spell criticals in a row using Fireball, Fire Blast, Scorch, Living Bomb, or Frostfire Bolt, 
			// you have a $m1% chance the next Pyroblast spell cast within $48108d will be instant cast."
			SpellLineId.MageFireHotStreak.Apply(spell => {
				var effect = spell.GetEffect(AuraType.Dummy);
				effect.AuraEffectHandlerCreator = () => new HotStreakHandler();
			});
		}

		#region Hot Streak
		public class HotStreakHandler : AttackEventEffectHandler
		{
			public readonly static HashSet<SpellLineId> TriggerSpells = new HashSet<SpellLineId>()
			{
				SpellLineId.MageFireball,
				SpellLineId.MageFireBlast,
				SpellLineId.MageScorch,
				SpellLineId.MageFireLivingBomb,
				SpellLineId.MageFrostfireBolt
			};

			private int critCount;

			public override void OnAttack(DamageAction action)
			{
				if (action.IsMagic && TriggerSpells.Contains(action.Spell.Line.LineId))
				{
					if (!action.IsCritical)
					{
						critCount = 0;				// reset critCount
					}
					else
					{
						critCount++;
						if (critCount == 2)
						{
							// 2 crits in a row
							critCount = 0;			// reset critCount
							if (Utility.Random(0, 101) < EffectValue)
							{
								// we have a Hot Streak
								Owner.SpellCast.TriggerSelf(SpellId.HotStreak);
							}
						}
					}
				}
			}
		}
		#endregion

		#region Molten Fury
		public class MoltenFuryHandler : AttackEventEffectHandler
		{
			public override void OnAttack(DamageAction action)
			{
				if (action.IsMagic)
				{
					// "Increases damage of all spells against targets with less than 35% health by $s1%."
					if (action.Victim.HealthPct <= 35)
					{
						action.Damage += action.GetDamagePercent(EffectValue);
					}
				}
			}
		}
		#endregion

		#region Master of Elements
		public class MasterOfElementsHandler : AttackEventEffectHandler
		{
			public override void OnAttack(DamageAction action)
			{
				if (action.IsMagic && action.IsCritical)
				{
					var owner = m_aura.CasterUnit;
					if (owner == null) return;

					// refund some of the mana
					owner.Power += (action.Spell.CalcPowerCost(owner, action.UsedSchool) * 100 + 50) / EffectValue;
				}
			}
		}
		#endregion
	}
}