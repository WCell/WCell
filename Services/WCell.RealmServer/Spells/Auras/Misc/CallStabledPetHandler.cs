using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Spells.Auras
{
	public class CallStabledPetHandler : AuraEffectHandler
	{
		protected internal override void CheckInitialize(SpellCast creatingCast, ObjectReference casterReference, Unit target, ref SpellFailedReason failReason)
		{
			if (target.Class != ClassId.Hunter)
			{
				failReason = SpellFailedReason.BadTargets;
				return;
			}
		}

		protected override void Apply()
		{
			if (m_aura.Owner is Character)
			{
				var chr = m_aura.Owner as Character;
				PetHandler.SendStabledPetsList(chr, chr, (byte)chr.StableSlotCount, chr.StabledPetRecords);
			}
		} 
	}
}
