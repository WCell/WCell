using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Constants.Talents;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;

namespace WCell.Addons.Default.Spells.Mage
{
	public static class MageFireFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixSpells()
		{
			// Burnout: "your non-periodic spell criticals cost an additional $s2% of the spell's cost"
			SpellEffect burnoutEffect = null;
			SpellHandler.Apply(spell =>
			{
				// consume mana for the given spells
				burnoutEffect = spell.GetEffect(SpellEffectType.Dummy);
				burnoutEffect.ImplicitTargetA = ImplicitTargetType.None;	// no target selection (we find the target in the Apply method)
				burnoutEffect.SpellEffectHandlerCreator = (cast, effect) => new BurnoutHandler(cast, effect);
			}, SpellId.ClassSkillBurnout);
			SpellLineId.MageFireBurnout.Apply(spell =>
			{
				// always procs on critical spells of the given AffectMask
				var effect = spell.GetEffect(AuraType.Dummy);
				spell.ProcChance = 100;
				spell.ProcTriggerFlags = ProcTriggerFlags.SpellCastCritical;

				effect.AuraType = AuraType.ProcTriggerSpell;		// make it a ProcTriggerSpell
				effect.AffectMask = burnoutEffect.AffectMask;		// set the correct affect mask
				effect.TriggerSpellId = SpellId.ClassSkillBurnout;	// trigger the mana consumption spell
			});
		}
	}

	#region Burnout
	public class BurnoutHandler : SpellEffectHandler
	{
		public BurnoutHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override void Apply()
		{
			var triggerAction = m_cast.TriggerAction;	// the triggerAction is the one that proc'ed this spell
			if (triggerAction != null)					// should always be != null, but let us be safe
			{
				// get the current rank of the burnout talent to determine the power cost
				var caster = m_cast.CasterUnit as Character;
				if (caster == null) return;

				var burnoutTalent = caster.Talents.GetTalent(TalentId.MageFireBurnout);
				if (burnoutTalent == null) return;

				var burnoutEffect = burnoutTalent.Spell.GetEffect(AuraType.ProcTriggerSpell);

				// get the spell base cost from the triggerAction and the costPct modifier from the talent
				var spell = triggerAction.Spell;
				var cost = spell.CalcBasePowerCost(caster);
				var costPct = Effect.CalcEffectValue(caster, burnoutEffect.CalcEffectValue());

				// deduct extra power
				var power = (cost * costPct + 50) / 100;
				caster.Power -= power;
			}
		}
	}
	#endregion
}
