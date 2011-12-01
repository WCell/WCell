using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;

namespace WCell.Addons.Default.Spells.DeathKnight
{
	public static class DeathKnightFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixDeathKnight()
		{
			//Forceful Reflection is missing stat values
			SpellHandler.Apply(spell =>
			{
				spell.Effects[0].MiscValue = (int)CombatRating.Parry;
				spell.Effects[0].MiscValueB = (int)StatType.Strength;
			},
			SpellId.ClassSkillForcefulDeflectionPassive);

			// Only one Presence may be active at a time
			AuraHandler.AddAuraGroup(SpellLineId.DeathKnightBloodPresence, SpellLineId.DeathKnightFrostPresence, SpellLineId.DeathKnightUnholyPresence);
		}


		#region Rune Conversion
		public static void MakeRuneConversionProc(SpellLineId line, SpellLineId trigger1, SpellLineId trigger2, RuneType to, params RuneType[] from)
		{
			line.Apply(spell =>
			{
				spell.SpellAuraOptions.ProcTriggerFlags = ProcTriggerFlags.DoneHarmfulMagicSpell | ProcTriggerFlags.DoneMeleeSpell;

				var effect = spell.GetEffect(AuraType.Dummy2);
				// should not have an amplitude 
				// (although it's probably the timeout for when the death rune is converted back to its original but it's not mentioned in the tooltip)
				effect.AuraPeriod = 0;

				effect.ClearAffectMask();
				effect.AddAffectingSpells(trigger1, trigger2);
				effect.IsProc = true;
				effect.AuraEffectHandlerCreator = () => new ProcRuneConversionHandler(to, from);
			});
		}

		public class ProcRuneConversionHandler : AuraEffectHandler
		{
			public RuneType To { get; set; }
			public RuneType[] From { get; set; }

			public ProcRuneConversionHandler(RuneType to, RuneType[] @from)
			{
				To = to;
				From = from;
			}

			public override void OnProc(Unit triggerer, IUnitAction action)
			{
				var chr = action.Attacker as Character;
				if (chr != null)
				{
					var runes = chr.PlayerSpells.Runes;
					if (runes != null)
					{
						// convert one of each "From rune" to "To rune" (if not on cooldown)
						foreach (var rune in From)
						{
							runes.Convert(rune, To);
						}
					}
				}
			}
		}
		#endregion
	}
}