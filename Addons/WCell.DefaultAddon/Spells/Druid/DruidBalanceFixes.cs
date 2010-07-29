using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras.Misc;
using WCell.Util.Graphics;

namespace WCell.Addons.Default.Spells.Druid
{
	public static class DruidBalanceFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixIt()
		{
			// Nature's Grace needs to set the correct set of affecting spells
			SpellLineId.DruidBalanceNaturesGrace.Apply(spell =>
			{
				// copy AffectMask from proc effect, which has it all set correctly
				var effect = spell.GetEffect(AuraType.ProcTriggerSpell);
				var triggerSpellEffect = effect.GetTriggerSpell().GetEffect(AuraType.ModCastingSpeed);
				effect.AffectMask = triggerSpellEffect.AffectMask;
			});

			// Improved Moonkin Form is only active in Moonkin form, and applies an extra AreaAura to everyone
			SpellLineId.DruidBalanceImprovedMoonkinForm.Apply(spell =>
			{
				// only in Moonkin form
				spell.RequiredShapeshiftMask = ShapeshiftMask.Moonkin;

				// apply the extra spell to everyone (it's an AreaAura effect)
				var dummy = spell.GetEffect(AuraType.Dummy);
				dummy.AuraEffectHandlerCreator = () => new ToggleAuraHandler(SpellId.ImprovedMoonkinFormRank1);
			});

			// Force of Nature's summon entry needs to be changed to Friendly, rather than pet
			SpellHandler.GetSummonEntry(SummonType.ForceOfNature).Group = SummonGroup.Friendly;

			// Owlkin Frenzy should proc on any damage spell
			SpellLineId.DruidBalanceOwlkinFrenzy.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.ProcTriggerSpell);
				effect.ClearAffectMask();
			});
		}
	}
}
