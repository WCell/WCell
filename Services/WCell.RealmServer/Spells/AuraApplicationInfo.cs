using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.Spells
{
	public struct AuraApplicationInfo
	{
		public readonly Unit Target;

		public readonly List<AuraEffectHandler> Handlers;

		public AuraApplicationInfo(Unit target)
		{
			Target = target;
			Handlers = new List<AuraEffectHandler>(3);
		}

		public AuraApplicationInfo(Unit target, AuraEffectHandler firstHandler)
		{
			Target = target;
			Handlers = new List<AuraEffectHandler>(3) {firstHandler};
		}
	}

	public struct SingleAuraApplicationInfo
	{
		public readonly Unit Target;

		public readonly AuraEffectHandler Handler;

		public SingleAuraApplicationInfo(Unit target, AuraEffectHandler handler)
		{
			Target = target;
			Handler = handler;
		}
	}
}