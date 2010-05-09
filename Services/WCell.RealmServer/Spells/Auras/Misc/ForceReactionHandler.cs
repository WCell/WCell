using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Factions;

namespace WCell.RealmServer.Spells.Auras
{
	public class ForceReactionHandler : AuraEffectHandler
	{
		protected internal override void Apply()
		{
			var factionId = (FactionId)SpellEffect.MiscValue;
		}
	}
}
