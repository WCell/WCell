using WCell.Constants.Looting;
using WCell.Constants.Spells;
using WCell.RealmServer.Looting;

namespace WCell.RealmServer.Spells.Effects
{
	public abstract class ItemConvertEffectHandler : SpellEffectHandler
	{
		public ItemConvertEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override void Initialize(ref SpellFailedReason failReason)
		{
			if (m_cast.TargetItem.Amount < Effect.MinValue)
			{
				failReason = SpellFailedReason.NeedMoreItems;
			}
		}

		public override void Apply()
		{
			var caster = m_cast.CasterChar;
			var item = m_cast.TargetItem;
			var loot = LootMgr.CreateAndSendObjectLoot(item, caster, LootEntryType, false);
			if (loot != null)
			{
				loot.OnLootFinish = () => {
					if (item.IsInWorld)
					{
						item.Amount -= Effect.MinValue;		// effect has always a constant value
					}
				};
			}
		}

		public abstract LootEntryType LootEntryType
		{
			get;
		}

		public override bool HasOwnTargets
		{
			get
			{
				return false;
			}
		}
	}
}