using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Spells.Effects
{
	public class PlayMusicEffectHandler : SpellEffectHandler
	{
		public PlayMusicEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		protected override void Apply(WorldObject target)
		{
			MiscHandler.SendPlayMusic(target, (uint)Effect.MiscValue, Effect.Radius);
		}

		public override ObjectTypes TargetType
		{
			get
			{
				return ObjectTypes.Player;
			}
		}  
	}
}
