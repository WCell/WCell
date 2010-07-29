using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
	public class CreateManaGemEffectHandler : CreateItemEffectHandler
	{
		public CreateManaGemEffectHandler(SpellCast cast, SpellEffect effect) : base(cast, effect)
		{
		}

		public override void Initialize(ref SpellFailedReason failReason)
		{
			if(Effect.BasePoints < 0)
			{
				Effect.BasePoints = 0;
			}
			base.Initialize(ref failReason);
		}

		public override SpellFailedReason InitializeTarget(WorldObject target)
		{
			var itemId = Effect.ItemId;
			if(((Character)target).Inventory.Contains(itemId))
			{
				return SpellFailedReason.TooManyOfItem;
			}
			return base.InitializeTarget(target);
		}
	}
}