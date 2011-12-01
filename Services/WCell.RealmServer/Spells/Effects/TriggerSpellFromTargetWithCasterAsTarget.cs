using System.Linq;

namespace WCell.RealmServer.Spells.Effects
{
	public class TriggerSpellFromTargetWithCasterAsTargetHandler : SpellEffectHandler
	{
		public TriggerSpellFromTargetWithCasterAsTargetHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
			
		}

		public override void Apply()
		{
			var spellToCast = Effect.TriggerSpell;
			if (spellToCast == null) return;

			foreach (var target in Cast.Targets.Where(target => target != null && target.IsInWorld))
			{
				target.SpellCast.TriggerSelf(spellToCast);
			}
			base.Apply();
		}
	}
}
