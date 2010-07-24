using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
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
				spell.AllowedShapeshiftMask = ShapeshiftMask.Bear | ShapeshiftMask.DireBear | ShapeshiftMask.Cat;
			});

			// Primal Fury is triggered by critical hits and only active in "in Bear and Dire Bear Form"
			SpellLineId.DruidFeralCombatPrimalFury.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.MeleeCriticalHit | ProcTriggerFlags.RangedCriticalHit;
				spell.AllowedShapeshiftMask = ShapeshiftMask.Bear | ShapeshiftMask.DireBear;
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
				spell.AllowedShapeshiftMask = ShapeshiftMask.Cat | ShapeshiftMask.Bear | ShapeshiftMask.DireBear;

				// toggle the party aura, whenever the druid shifts into cat bear or dire bear form:
				spell.GetEffect(AuraType.Dummy).AuraEffectHandlerCreator = () => new ToggleAuraHandler(SpellId.LeaderOfThePack);
			});

			// Primal Tenacity has the wrong effect type: "reduces all damage taken while stunned by $s2% while in Cat Form."
			SpellLineId.DruidFeralCombatPrimalTenacity.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.SchoolAbsorb);
				effect.AuraType = AuraType.ModDamageTakenPercent;		// should reduce damage taken
			});

			FixFeralSwiftness(SpellId.DruidFeralCombatFeralSwiftness, SpellId.FeralSwiftnessPassive1a);
			FixFeralSwiftness(SpellId.DruidFeralCombatFeralSwiftness_2, SpellId.FeralSwiftnessPassive2a);
		}

		private static void FixFeralSwiftness(SpellId origSpell, SpellId triggerSpell)
		{
			// Feral Swiftness should only be applied in Cat Form
			SpellHandler.Apply(spell =>
			{
				// only as cat
				// triggers the dodge effect spell
				spell.AllowedShapeshiftMask = ShapeshiftMask.Cat;
				spell.AddTriggerSpellEffect(triggerSpell);
			},
			origSpell);
			SpellHandler.Apply(spell =>
			{
				// this spell applies the dodge bonus for Feral Swiftness
				// "increases your chance to dodge while in Cat Form, Bear Form and Dire Bear Form"
				// must be passive
				spell.Attributes |= SpellAttributes.Passive;
				spell.AllowedShapeshiftMask = ShapeshiftMask.Cat | ShapeshiftMask.Bear | ShapeshiftMask.DireBear;
			},
			triggerSpell);
		}
	}
}
