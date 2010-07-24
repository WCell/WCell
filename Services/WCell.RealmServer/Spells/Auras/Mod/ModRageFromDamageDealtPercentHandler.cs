using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Mod
{
	/// <summary>
	/// TODO: Only used for WarriorArmsEndlessRage
	/// </summary>
	public class ModRageFromDamageDealtPercentHandler : AuraEffectHandler
	{
		protected override void Apply()
		{
			var owner = Owner as Character;
			if (owner != null)
			{

			}
		}

		protected override void Remove(bool cancelled)
		{
			var owner = Owner as Character;
			if (owner != null)
			{

			}
		}
	}
}