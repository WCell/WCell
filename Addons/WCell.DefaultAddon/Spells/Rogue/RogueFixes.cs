using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Effects;

namespace WCell.Addons.Default.Spells.Rogue
{
	public static class RogueFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixRogue()
		{
			// RogueKick can proc RogueCombatImprovedKick
			SpellLineId.RogueCombatImprovedKick.Apply(spell =>
			{
				spell.SpellAuraOptions.ProcTriggerFlags = ProcTriggerFlags.DoneMeleeSpell;
				spell.GetEffect(AuraType.ProcTriggerSpell).SetAffectMask(SpellLineId.RogueKick);
			});

			// RogueDeadlyThrow can proc RogueCombatThrowingSpecialization
			SpellLineId.RogueCombatThrowingSpecialization.Apply(spell =>
			{
				spell.SpellAuraOptions.ProcTriggerFlags = ProcTriggerFlags.DoneRangedSpell;
				spell.GetEffect(AuraType.ProcTriggerSpell).AddAffectingSpells(SpellLineId.RogueDeadlyThrow);
			});

			// All combo moves which can critical hit
			SpellLineId.RogueAssassinationSealFate.Apply(
				spell =>
				{
					var effect = spell.GetEffect(SpellEffectType.ApplyAura);
					effect.ClearAffectMask();
					effect.AddAffectingSpells(SpellLineId.RogueSinisterStrike, SpellLineId.RogueBackstab, SpellLineId.RogueAmbush,
						SpellLineId.RogueMutilate, SpellLineId.RogueSubtletyHemorrhage, SpellLineId.RogueGouge, SpellLineId.RogueShiv);
				});

			// all finishing moves: RogueKidneyShot, RogueRupture, RogueEviscerate, RogueExposeArmor, RogueDeadlyThrow, RogueEnvenom (003A00000000000900000000)
			SpellLineId.RogueAssassinationRuthlessness.Apply(
				spell => spell.Effects[0].AffectMask = new uint[] { 0x003A0000, 0x00000009, 0x00000000 });

		}
	}
}