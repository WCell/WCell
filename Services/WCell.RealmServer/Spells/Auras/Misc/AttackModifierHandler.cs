using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras.Misc
{
	public abstract class AttackModifierHandler : AuraEffectHandler, IAttackModifier
	{
		protected internal override void Apply()
		{
			Owner.AttackModifiers.Add(this);
		}

		protected internal override void Remove(bool cancelled)
		{
			Owner.AttackModifiers.Remove(this);
		}

		public abstract void ModAttack(DamageAction action);
	}
}
