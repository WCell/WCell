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
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.RealmServer.Spells.Auras.Misc;
using WCell.RealmServer.Spells.Effects;
using WCell.Util;

namespace WCell.Addons.Default.Spells.DeathKnight
{
	/// <summary>
	/// Fixes the disease talents and debuffs
	/// </summary>
	public static class DeathKnightDiseases
	{
		[Initialization(InitializationPass.Second)]
		public static void FixDiseases()
		{
			FixDisease(SpellId.EffectBloodPlague);
			FixDisease(SpellId.EffectFrostFever);
		}

		private static void FixDisease(SpellId effectId)
		{

			//  the BP debuff needs to use the disease talent to apply damage
			SpellHandler.Apply(spell =>
			{
				var dmgEffect = spell.GetEffect(AuraType.PeriodicDamage);
				dmgEffect.APValueFactor = 0.055f * 1.15f;
				dmgEffect.RealPointsPerLevel = 0;
			}, effectId);
		}
	}
}
