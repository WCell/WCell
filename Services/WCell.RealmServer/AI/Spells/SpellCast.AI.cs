using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras;


/**
 * AI SpellCast behavior
 */


namespace WCell.RealmServer.Spells
{
	public partial class SpellCast
	{
		#region Prepare
		/// <summary>
		/// Find valid targets for AI cast during preparation
		/// </summary>
		SpellFailedReason PrepareAI(SpellCast cast)
		{
			var caster = cast.CasterUnit;
			cast.SourceLoc = caster.Position;

			if (caster.Target != null)
			{
				caster.SpellCast.TargetLoc = caster.Target.Position;
			}


			// TODO: Init handlers
			//var targets = FindValidTargetsForCaster(caster);
			//if (targets == null)
			//{
			//    return SpellFailedReason.NoValidTargets;
			//}

			return SpellFailedReason.NoValidTargets;
		}
		#endregion
	}
}
