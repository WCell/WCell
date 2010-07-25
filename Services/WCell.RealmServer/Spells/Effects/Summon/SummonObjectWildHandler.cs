using WCell.Constants.GameObjects;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Handlers
{
	/// <summary>
	/// Summons an object without owner
	/// </summary>
    public class SummonObjectWildEffectHandler : SpellEffectHandler
    {
        private GameObject go;

        public SummonObjectWildEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override void Apply()
        {
            var goId = (GOEntryId)Effect.MiscValue;
            var goEntry = GOMgr.GetEntry(goId);
            var caster = m_cast.Caster;
            if (goEntry != null)
            {
                if (Cast.TargetLoc.X != 0)
                {
                    var worldLocation = new WorldLocation(caster.Region, Cast.TargetLoc);
					go = goEntry.Spawn(worldLocation);
                }
                else
                {
                    go = goEntry.Spawn(caster, null);
                }

                go.State = GameObjectState.Enabled;
                go.Orientation = caster.Orientation;
                go.ScaleX = 1;
            }
        }
    }
}