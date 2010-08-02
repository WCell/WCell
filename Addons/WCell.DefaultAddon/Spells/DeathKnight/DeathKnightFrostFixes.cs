using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
	public class DeathKnightFrostFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixIt()
		{
			// Merciless Combat needs to be proc'ed and needs an extra check, so it only procs on "targets with less than 35% health"
			SpellLineId.DeathKnightFrostMercilessCombat.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.SpellCast;

				var effect = spell.GetEffect(AuraType.OverrideClassScripts);
				effect.IsProc = true;
				effect.AuraEffectHandlerCreator = () => new MercilessCombatHandler();
			});
		}
	}

	#region Merciless Combat
	public class MercilessCombatHandler : AuraEffectHandler
	{
		public override bool CanProcBeTriggeredBy(IUnitAction action)
		{
			return action.Victim.HealthPct <= 35;
		}

		public override void OnProc(Unit triggerer, IUnitAction action)
		{
			// add extra damage
			if (action is DamageAction)
			{
				((DamageAction)action).IncreaseDamagePercent(EffectValue);
			}
		}
	}
	#endregion
}
