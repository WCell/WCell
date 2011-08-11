using WCell.Constants.GameObjects;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
	// TODO : Change the state of the GO according to the MiscValue (0 = Intact, 1 = Damaged, 2 = Destroyed, 3 = Rebuild)
	public class WMOChange : SpellEffectHandler
	{
		public WMOChange(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override SpellFailedReason InitializeTarget(WorldObject target)
		{
			if (!(target is GameObject))
			{
				return SpellFailedReason.NoValidTargets;
			}

			if (((GameObject)target).GOType != GameObjectType.DestructibleBuilding)
			{
				return SpellFailedReason.BadTargets;
			}

			return SpellFailedReason.Ok;
		}

		protected override void Apply(WorldObject target)
		{
		}

		public override ObjectTypes TargetType
		{
			get
			{
				return ObjectTypes.GameObject; // Not sure (perhaps DynamicObject)
			}
		}
	}
}
