using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.Addons.Default.Spells.Paladin
{
	public static class PaladinMiscFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixPaladin()
		{
			FixBlessings();
			FixStrongBuffs();

			// Players may only have one Hand on them per Paladin at any one time
			AuraHandler.AddAuraCasterGroup(
				SpellLineId.PaladinHandOfFreedom,
				SpellLineId.PaladinHandOfProtection,
				SpellLineId.PaladinHandOfReckoning,
				SpellLineId.PaladinHandOfSacrifice,
				SpellLineId.PaladinHandOfSalvation);
		}

		#region Blessings
		private static void FixBlessings()
		{
			// Normal and Greater blessings are mutually exclusive
			AuraHandler.AddAuraGroup(SpellLineId.PaladinBlessingOfKings, SpellLineId.PaladinGreaterBlessingOfKings);
			AuraHandler.AddAuraGroup(SpellLineId.PaladinBlessingOfMight, SpellLineId.PaladinGreaterBlessingOfMight);
			AuraHandler.AddAuraGroup(SpellLineId.PaladinBlessingOfWisdom, SpellLineId.PaladinGreaterBlessingOfWisdom);

			// only one blessing per pala
			AuraHandler.AddAuraCasterGroup(
				SpellLineId.PaladinBlessingOfKings,
				SpellLineId.PaladinBlessingOfMight,
				SpellLineId.PaladinBlessingOfWisdom,
				SpellLineId.PaladinGreaterBlessingOfKings,
				SpellLineId.PaladinGreaterBlessingOfMight,
				SpellLineId.PaladinGreaterBlessingOfWisdom,
				SpellLineId.PaladinGreaterBlessingOfSanctuary);

			// Sanctuary needs a proc: "When the target blocks, parries, or dodges a melee attack the target will gain $57319s1% of maximum displayed mana."
			SpellHandler.Apply(spell =>
			{
				// first effect should mod damage taken
				var firstEffect = spell.Effects[0];
				if (firstEffect.EffectType == SpellEffectType.Dummy)
				{
					firstEffect.EffectType = SpellEffectType.ApplyAura;
					firstEffect.AuraType = AuraType.ModDamageTakenPercent;
				}

				// add custom proc
				spell.AddTargetProcHandler(new TriggerSpellProcHandler(ProcTriggerFlags.MeleeAttack, 
					ProcHandler.DodgeBlockOrParryValidator,
					SpellHandler.Get(SpellId.BlessingOfSanctuary)
					));

				// add str & stam
				var strEff = spell.AddAuraEffect(AuraType.ModStatPercent, ImplicitTargetType.SingleFriend);
				strEff.MiscValue = (int)StatType.Strength;
				strEff.BasePoints = 10;

				var stamEff = spell.AddAuraEffect(AuraType.ModStatPercent, ImplicitTargetType.SingleFriend);
				stamEff.MiscValue = (int)StatType.Stamina;
				stamEff.BasePoints = 10;
			},
			SpellLineId.PaladinProtectionBlessingOfSanctuary,
			SpellLineId.PaladinGreaterBlessingOfSanctuary);
		}
		#endregion

		private static void FixStrongBuffs()
		{
			// used by client only to prevent casting of Invul buffs
			var avengingWrathMarker2 = SpellHandler.AddCustomSpell(61988, "Invul Prevention");
			avengingWrathMarker2.IsPreventionDebuff = true;
			avengingWrathMarker2.AddAuraEffect(AuraType.Dummy);
			avengingWrathMarker2.Attributes |= SpellAttributes.NoVisibleAura | SpellAttributes.UnaffectedByInvulnerability;
			avengingWrathMarker2.AttributesExC |= SpellAttributesExC.PersistsThroughDeath;
			//avengingWrathMarker2.Durations = new Spell.DurationEntry { Min = 180000, Max = 180000 };
			avengingWrathMarker2.Durations = new Spell.DurationEntry { Min = 120000, Max = 120000 };

			SpellHandler.Apply(spell => spell.AddTargetTriggerSpells(SpellId.Forbearance),
								SpellLineId.PaladinHandOfProtection,
				//SpellLineId.PaladinLayOnHands,
								SpellId.ClassSkillDivineShield,
								SpellId.ClassSkillDivineProtection);

			SpellHandler.Apply(spell => spell.AddTargetTriggerSpells(SpellId.AvengingWrathMarker, avengingWrathMarker2.SpellId),
								SpellLineId.PaladinHandOfProtection,
								SpellId.ClassSkillAvengingWrath,
								SpellId.ClassSkillDivineShield,
								SpellId.ClassSkillDivineProtection);

			SpellHandler.Apply(spell => { spell.IsPreventionDebuff = true; },
							   SpellId.AvengingWrathMarker, SpellId.Forbearance);
		}
	}
}