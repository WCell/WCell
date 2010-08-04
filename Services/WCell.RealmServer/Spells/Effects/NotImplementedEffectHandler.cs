using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
	public class NotImplementedEffectHandler : SpellEffectHandler
	{
		public NotImplementedEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
			if (cast.CasterObject is Character)
			{
				(cast.CasterObject as Character).SendSystemMessage(
					"Spell {0} ({1}) has not implemented Effect {2}. Please report this to the developers",
					cast.Spell.Name, cast.Spell.Id, effect.EffectType);
			}
		}

		public override bool HasOwnTargets
		{
			get
			{
				return false;
			}
		}

    	public override void Apply()
		{
		}
	}

	public class VoidEffectHandler : SpellEffectHandler
	{
		public VoidEffectHandler(SpellCast cast, SpellEffect effect) : base(cast, effect)
		{
		}

		public override bool HasOwnTargets
		{
			get
			{
				return false;
			}
		}

		public override void Apply()
		{
		}
	}
}