using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace WCell.Addons.Default.Spells.Druid
{
	public static class DruidFeralCombatFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixIt()
		{
			// Thick Hide needs to be restricted to "cloth and leather items"
			SpellLineId.DruidFeralCombatThickHide.Apply(spell =>
			{
				spell.RequiredItemClass = ItemClass.Armor;
				spell.RequiredItemSubClassMask = ItemSubClassMask.ArmorCloth | ItemSubClassMask.ArmorLeather;
			});

			// Sharpened Claws only works "while in Bear, Dire Bear or Cat Form"
			SpellLineId.DruidFeralCombatSharpenedClaws.Apply(spell =>
			{
				spell.RequiredShapeshiftMask = ShapeshiftMask.Bear | ShapeshiftMask.DireBear | ShapeshiftMask.Cat;
			});

			// Primal Fury is triggered by critical hits and only active in "in Bear and Dire Bear Form"
			SpellLineId.DruidFeralCombatPrimalFury.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.MeleeCriticalHit | ProcTriggerFlags.RangedCriticalHit;
				spell.RequiredShapeshiftMask = ShapeshiftMask.Bear | ShapeshiftMask.DireBear;
			});

			// Heart of the wild: "while in Bear or Dire Bear Form your Stamina is increased by $s3% and while in Cat Form your attack power is increased by $s2%."
			SpellLineId.DruidFeralCombatHeartOfTheWild.Apply(spell =>
			{
				var dummy = spell.GetEffect(SpellEffectType.Dummy);

				// increase 10% of something, depending on the form
				var bearEffect = spell.AddAuraEffect(AuraType.ModTotalStatPercent);
				bearEffect.RequiredShapeshiftMask = ShapeshiftMask.Bear | ShapeshiftMask.DireBear;
				bearEffect.MiscValue = (int)StatType.Stamina;										// increases stamina
				bearEffect.BasePoints = dummy.BasePoints;
				bearEffect.DiceSides = dummy.DiceSides;

				var catEffect = spell.AddAuraEffect(AuraType.ModAttackPowerPercent);
				catEffect.RequiredShapeshiftMask = ShapeshiftMask.Cat;
				catEffect.BasePoints = dummy.BasePoints;
				catEffect.DiceSides = dummy.DiceSides;
			});

			// Leader of the Pack toggles an Aura "While in Cat, Bear or Dire Bear Form"
			SpellLineId.DruidFeralCombatLeaderOfThePack.Apply(spell =>
			{
				spell.RequiredShapeshiftMask = ShapeshiftMask.Cat | ShapeshiftMask.Bear | ShapeshiftMask.DireBear;

				// toggle the party aura, whenever the druid shifts into cat bear or dire bear form:
				spell.GetEffect(AuraType.Dummy).AuraEffectHandlerCreator = () => new ToggleAuraHandler(SpellId.LeaderOfThePack);
			});
			// triggered Aura of LotP: First effect has invalid radius; 2nd effect is the proc effect for the Improved LotP buff
			SpellHandler.Apply(spell =>
			{
				spell.ForeachEffect(effect => effect.Radius = 45);	// fix radius

				// "heal themselves for $s1% of their total health when they critically hit with a melee or ranged attack"
				spell.ProcTriggerFlags = ProcTriggerFlags.MeleeCriticalHitOther | ProcTriggerFlags.RangedCriticalHit;

				// "The healing effect cannot occur more than once every 6 sec"
				spell.ProcDelay = 6000;

				// make this a special proc
				var dummy = spell.GetEffect(AuraType.Dummy);
				dummy.IsProc = true;
				dummy.AuraEffectHandlerCreator = () => new ImprovedLeaderOfThePackProcHandler();
			},
			SpellId.LeaderOfThePack);

			// Primal Tenacity has the wrong effect type: "reduces all damage taken while stunned by $s2% while in Cat Form."
			SpellLineId.DruidFeralCombatPrimalTenacity.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.SchoolAbsorb);
				effect.AuraType = AuraType.ModDamageTakenPercent;		// should reduce damage taken
			});

			// Survival of the Fittest "increases your armor contribution from cloth and leather items in Bear Form and Dire Bear Form by $s3%"
			SpellLineId.DruidFeralCombatSurvivalOfTheFittest.Apply(spell =>
			{
				var effect = spell.GetEffect(SpellEffectType.Dummy);
				effect.RequiredShapeshiftMask = ShapeshiftMask.Bear | ShapeshiftMask.DireBear;
				effect.AuraEffectHandlerCreator = () => new SurvivalOfTheFittestHandler();
			});

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

			// Infected Wounds: "Your Shred, Maul, and Mangle attacks cause an Infected Wound in the target"
			SpellLineId.DruidFeralCombatInfectedWounds.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.ProcTriggerSpell);

				// can only be proc'ed by a certain set of spells:
				effect.AddToAffectMask(SpellLineId.DruidShred, SpellLineId.DruidMaul, SpellLineId.DruidFeralCombatMangleBear, SpellLineId.DruidMangleCat);
			});

			// King of the Jungle has 2 dummies for shapeshift-restricted aura effects
			SpellLineId.DruidFeralCombatKingOfTheJungle.Apply(spell =>
			{
				// "While using your Enrage ability in Bear Form or Dire Bear Form, your damage is increased by $s1%"
				var effect1 = spell.Effects[0];
				effect1.EffectType = SpellEffectType.ApplyAura;
				effect1.AuraType = AuraType.ModDamageDonePercent;
				effect1.AddToAffectMask(SpellLineId.DruidEnrage);
				effect1.RequiredShapeshiftMask = ShapeshiftMask.Bear | ShapeshiftMask.DireBear;

				// "your Tiger's Fury ability also instantly restores $s2 energy"
				var effect2 = spell.Effects[1];
				effect2.EffectType = SpellEffectType.Energize;
				effect2.AddToAffectMask(SpellLineId.DruidTigersFury);
			});
			
			FixFeralSwiftness(SpellId.DruidFeralCombatFeralSwiftness, SpellId.FeralSwiftnessPassive1a);
			FixFeralSwiftness(SpellId.DruidFeralCombatFeralSwiftness_2, SpellId.FeralSwiftnessPassive2a);
		}

		#region FixFeralSwiftness
		private static void FixFeralSwiftness(SpellId origSpell, SpellId triggerSpell)
		{
			// Feral Swiftness should only be applied in Cat Form
			SpellHandler.Apply(spell =>
			{
				// only as cat
				// triggers the dodge effect spell
				spell.RequiredShapeshiftMask = ShapeshiftMask.Cat;
				spell.AddTriggerSpellEffect(triggerSpell);
			},
			origSpell);
			SpellHandler.Apply(spell =>
			{
				// this spell applies the dodge bonus for Feral Swiftness
				// "increases your chance to dodge while in Cat Form, Bear Form and Dire Bear Form"
				// must be passive
				spell.Attributes |= SpellAttributes.Passive;
				spell.RequiredShapeshiftMask = ShapeshiftMask.Cat | ShapeshiftMask.Bear | ShapeshiftMask.DireBear;
			},
			triggerSpell);
		}
		#endregion
	}

	#region ImprovedLeaderOfThePackProcHandler
	public class ImprovedLeaderOfThePackProcHandler : AuraEffectHandler
	{
		public override bool CanProcBeTriggeredBy(IUnitAction action)
		{
			// Check whether Improved LotP has been activated yet
			return EffectValue > 0;
		}

		public override void OnProc(Unit target, IUnitAction action)
		{
			// "causes affected targets to heal themselves for $s1% of their total health when they critically hit with a melee or ranged attack."
			action.Attacker.HealPercent(EffectValue, m_aura.Caster, m_spellEffect);

			if (action.Attacker == m_aura.Caster)
			{
				// "In addition, you gain $s2% of your maximum mana when you benefit from this heal."
				action.Attacker.EnergizePercent(EffectValue * 2, m_aura.Caster, m_spellEffect);
			}
		}
	}
	#endregion

	#region SurvivalOfTheFittestHandler
	public class SurvivalOfTheFittestHandler : ItemEquipmentEventAuraHandler
	{
		public override void OnEquip(Item item)
		{
			var templ = item.Template;
			if (templ.Class == ItemClass.Armor &&	// only works on leather and cloth armor
				(templ.SubClass == ItemSubClass.ArmorCloth || templ.SubClass == ItemSubClass.ArmorLeather))
			{
				var bonus = (templ.GetResistance(DamageSchool.Physical) * EffectValue + 50) / 100;
				Owner.ModBaseResistance(DamageSchool.Physical, bonus);
			}
		}

		public override void OnBeforeUnEquip(Item item)
		{
			var templ = item.Template;
			if (templ.Class == ItemClass.Armor &&	// only works on leather and cloth armor
				(templ.SubClass == ItemSubClass.ArmorCloth || templ.SubClass == ItemSubClass.ArmorLeather))
			{
				var bonus = (templ.GetResistance(DamageSchool.Physical) * EffectValue + 50) / 100;
				Owner.ModBaseResistance(DamageSchool.Physical, -bonus);
			}
		}
	}
	#endregion
}
