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
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.RealmServer.Spells.Auras.Misc;
using WCell.RealmServer.Spells.Effects;
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

			// Starfall triggers a lot of spells in a chain - The first trigger spell effect is a dummy that limits the accumulated amount of triggers to 20
			SpellLineId.DruidBalanceStarfall.Apply(spell =>
			{
				// cancels on move
				spell.AuraInterruptFlags |= AuraInterruptFlags.OnMovement;

				// all triggered Spells are instant casts - so we need to use the Aura effect handler to remember the total amount of stars

				// use Aura handler for accumulating
				var periodicTriggerEffect = spell.GetEffect(AuraType.PeriodicTriggerSpell);
				periodicTriggerEffect.AuraEffectHandlerCreator = () => new StarfallAuraHandler();

				// use triggered spell handler for updating the accumulator in the Aura
				var triggeredDummySpell = periodicTriggerEffect.GetTriggerSpell();
				var triggeredDummyEffect = triggeredDummySpell.GetEffect(SpellEffectType.Dummy);
				triggeredDummyEffect.TriggerSpellId = (SpellId)triggeredDummyEffect.CalcEffectValue();		// trigger spell is set as effect value
				triggeredDummyEffect.SpellEffectHandlerCreator = (cast, effct) => new StarfallCountTriggerHandler(cast, effct, spell);
			});
		}
	}

	#region Starfall
	/// <summary>
	/// The aura needs to keep track of the star count
	/// </summary>
	public class StarfallAuraHandler : PeriodicTriggerSpellHandler
	{
		public int FallenStars;
	}

	/// <summary>
	/// This effect needs to count the amount of stars casted
	/// </summary>
	public class StarfallCountTriggerHandler : TriggerSpellEffectHandler
	{
		public static int MaxStars = 20;
		public Spell StarFallAuraSpell { get; private set; }

		public StarfallCountTriggerHandler(SpellCast cast, SpellEffect effect, Spell starFallAuraSpell)
			: base(cast, effect)
		{
			StarFallAuraSpell = starFallAuraSpell;
		}

		public override void Apply()
		{
			if (Targets == null) return;					// must have targets

			var caster = m_cast.CasterUnit;
			if (caster == null) return;

			var aura = caster.Auras[StarFallAuraSpell];
			if (aura == null) return;

			var handler = aura.Handlers.First(handlr => handlr is StarfallAuraHandler) as StarfallAuraHandler;
			if (handler == null) return;					// we need the handler for counting

			if (handler.FallenStars + Targets.Count >= MaxStars)
			{
				// reached the max amount of stars
				var amount = MaxStars - handler.FallenStars;
				if (amount > 0)
				{
					var spellTargets = new WorldObject[amount];
					Targets.CopyTo(0, spellTargets, 0, amount);

					m_cast.Trigger(Effect.TriggerSpell, spellTargets);
				}
				aura.Remove(false);			// remove Aura
			}
			else
			{
				// we can still keep going
				handler.FallenStars += Targets.Count;
				m_cast.Trigger(Effect.TriggerSpell, Targets.ToArray());
			}
		}
	}
	#endregion
}
