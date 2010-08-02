using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Auras.Misc;

namespace WCell.Addons.Default.Spells.DeathKnight
{
	public class DeathKnightBloodFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixIt()
		{
			// Butchery lets you "generate up to 10 runic power" upon kill
			SpellLineId.DeathKnightBloodButchery.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.Dummy);
				effect.IsProc = true;
				effect.ClearAffectMask();
				effect.AuraEffectHandlerCreator = () => new ProcEnergizeHandler();
			});
		}
	}
}
