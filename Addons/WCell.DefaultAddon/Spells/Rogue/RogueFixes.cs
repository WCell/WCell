using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Effects;
using WCell.RealmServer.Entities;

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
				spell.ProcTriggerFlags = ProcTriggerFlags.SpellCast;
				spell.GetEffect(AuraType.ProcTriggerSpell).SetAffectMask(SpellLineId.RogueKick);
			});

			// RogueDeadlyThrow can proc RogueCombatThrowingSpecialization
			SpellLineId.RogueCombatThrowingSpecialization.Apply(spell => 
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.SpellCast;
				spell.GetEffect(AuraType.ProcTriggerSpell).AddAffectingSpells(SpellLineId.RogueDeadlyThrow);
			});

			// all combo point abilities: 2680070E0000010600000000
			SpellLineId.RogueAssassinationSealFate.Apply(
				spell => spell.Effects[0].AffectMask = new uint[] { 0x2680070E, 0x00000106, 0x00000000 });

			// all finishing moves: RogueKidneyShot, RogueRupture, RogueEviscerate, RogueExposeArmor, RogueDeadlyThrow, RogueEnvenom (003A00000000000900000000)
			SpellLineId.RogueAssassinationRuthlessness.Apply(
				spell => spell.Effects[0].AffectMask = new uint[] { 0x003A0000, 0x00000009, 0x00000000 });

			#region Wrong Facing Requirements
			SpellLineId.RogueAssassinationMutilate.Apply(spell =>
				{
					spell.AttributesExB = SpellAttributesExB.None;
				});
			SpellHandler.Apply(spell =>
				{
					spell.AttributesExB = SpellAttributesExB.None;
				}, SpellId.WeaponProficiencyThrow);
			#endregion

			SpellLineId.RogueAssassinationHungerForBlood.Apply(spell =>
				{
					spell.Effects[0].SpellEffectHandlerCreator =
					(cast, effect) => new HungerForBloodHandler(cast, effect);
				});

		}

		#region HungerForBlood
		public class HungerForBloodHandler : DummyEffectHandler
		{
			public HungerForBloodHandler(SpellCast cast, SpellEffect effect)
				: base(cast, effect)
			{
			}

			public override void Apply()
			{
				var chr = m_cast.CasterUnit as Character;
				if (chr != null)
				{
						chr.SpellCast.Trigger(SpellId.HungerForBlood_3, chr);
				}
			}
		}
		#endregion
	}
}