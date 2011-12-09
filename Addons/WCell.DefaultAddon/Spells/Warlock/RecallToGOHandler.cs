using WCell.Constants.GameObjects;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;

namespace WCell.Addons.Default.Spells.Warlock
{
    public class RecallToGOHandler : SpellEffectHandler
    {
    	private GameObject go;
		
        public RecallToGOHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

		public override SpellFailedReason Initialize()
		{
			var goId = (GOEntryId)Effect.MiscValue;
			var caster = m_cast.CasterUnit as Character;
            if (caster != null)
            {
            	go = caster.GetOwnedGO(goId);
				if (go == null || !caster.IsInSpellRange(Effect.Spell, go))
				{
					return SpellFailedReason.OutOfRange;
				}
            }
			else
            {
            	return base.Initialize();
            }

			return SpellFailedReason.Ok;
		}

        public override void Apply()
        {
            var caster = m_cast.CasterUnit as Character;

            if (caster != null)
            {
                if (go != null && go.IsInWorld)
                {
                    caster.TeleportTo(go);
                }
            }
        }
    }
}