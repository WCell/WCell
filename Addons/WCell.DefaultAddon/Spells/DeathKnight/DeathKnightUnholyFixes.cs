using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Auras.Handlers;

namespace WCell.Addons.Default.Spells.DeathKnight
{
	public class DeathKnightUnholyFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixIt()
		{
			// Unholy Virulence has some incorrect restrictions
			SpellLineId.DeathKnightUnholyVirulence.Apply(spell =>
			{
				// Improves all Spells
				spell.GetEffect(AuraType.ModSpellHitChance).MiscValue = 0;
			});

			FixUnholyFever();
		}

		#region Unholy Fever
		private static void FixUnholyFever()
		{
			var cryptFeverRanks = new[]
			{
				SpellId.CryptFeverRank1,
				SpellId.CryptFever,
				SpellId.CryptFever_2,
			};

			// Crypt Fever does not proc correctly
			SpellLineId.DeathKnightUnholyCryptFever.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.SpellCast;

				var effect = spell.GetEffect(AuraType.OverrideClassScripts);
				effect.AuraType = AuraType.ProcTriggerSpell;
				effect.TriggerSpellId = cryptFeverRanks[spell.Rank];
				effect.AuraEffectHandlerCreator = () => new CryptFeverHandler();
			});

			// The Crypt fever effect should increase damage taken %
			SpellHandler.Apply(spell =>
			{
				var effect = spell.Effects[0];
				effect.AuraType = AuraType.ModDamageTakenPercent;
			}, cryptFeverRanks);
		}

		/// <summary>
		/// CF may only proc on diseases
		/// </summary>
		internal class CryptFeverHandler : ProcTriggerSpellHandler
		{
			public override bool CanProcBeTriggeredBy(IUnitAction action)
			{
				return action.Spell != null && action.Spell.DispelType == DispelType.Disease;
			}
		}
		#endregion
	}
}
