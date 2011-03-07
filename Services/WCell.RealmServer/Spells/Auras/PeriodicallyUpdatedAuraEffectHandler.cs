using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Spells.Auras
{
	/// <summary>
	/// A kind of AuraEffectHandler that gets updated on every tick.
	/// Can be used to re-evaluate effect values of depending Auras
	/// </summary>
	public abstract class PeriodicallyUpdatedAuraEffectHandler : AuraEffectHandler
	{
		public abstract void Update();
	}
}
