using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras.Misc
{
	public abstract class AttackEventEffectHandler : AuraEffectHandler, IAttackEventHandler
	{
		protected override void Apply()
		{
			Owner.AttackEventHandlers.Add(this);
		}

		protected override void Remove(bool cancelled)
		{
			Owner.AttackEventHandlers.Remove(this);
		}

		public abstract void OnBeforeAttack(DamageAction action);

		public abstract void OnAttack(DamageAction action);
		
		public abstract void OnDefend(DamageAction action);
	}
}