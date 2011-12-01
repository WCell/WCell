using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Passive
{
	public class ArenaPreparationHandler : AuraEffectHandler
	{
		protected override void Apply()
		{
			var chr = m_aura.Auras.Owner as Character;
			if (chr != null)
			{
				chr.UnitFlags |= UnitFlags.Preparation;
			}
		}

		protected override void Remove(bool cancelled)
		{
			var chr = m_aura.Auras.Owner as Character;
			if (chr != null)
			{
				chr.UnitFlags &= ~UnitFlags.Preparation;
			}
		}
	}
}
