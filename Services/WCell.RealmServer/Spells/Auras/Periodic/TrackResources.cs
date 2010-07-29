using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	public class TrackResourcesHandler : AuraEffectHandler
	{
		protected internal override void CheckInitialize(SpellCast creatingCast, ObjectReference casterReference, Unit target, ref SpellFailedReason failReason)
		{
			if (!(target is Character))
			{
				failReason = SpellFailedReason.TargetNotPlayer;
			}
		}

		protected override void Apply()
		{
			var chr = ((Character)m_aura.Auras.Owner);

			// masked value in diguise
			chr.ResourceTracking = (LockMask)(1 << (m_spellEffect.MiscValue - 1));
		}

		protected override void Remove(bool cancelled)
		{
			var chr = ((Character)m_aura.Auras.Owner);

			// masked value in diguise
			chr.ResourceTracking = 0;
		}

	}
};