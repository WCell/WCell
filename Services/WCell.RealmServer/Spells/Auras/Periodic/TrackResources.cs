using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	public class TrackResourcesHandler : AuraEffectHandler
	{
		protected internal override void CheckInitialize(CasterInfo casterInfo, Unit target, ref SpellFailedReason failReason)
		{
			if (!(target is Character))
			{
				failReason = SpellFailedReason.TargetNotPlayer;
			}
			else if (((Character)target).CurrentTracker != null)
			{
				((Character) target).CurrentTracker.Remove(true);
			}
		}

		protected internal override void Apply()
		{
			var chr = ((Character)m_aura.Auras.Owner);
			chr.CurrentTracker = m_aura;

			// masked value in diguise
			chr.ResourceTracking = (LockMask)(1 << (m_spellEffect.MiscValue - 1));
		}

		protected internal override void Remove(bool cancelled)
		{
			var chr = ((Character)m_aura.Auras.Owner);
			chr.CurrentTracker = null;

			// masked value in diguise
			chr.ResourceTracking = 0;
		}

	}
};
