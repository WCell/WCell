using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras.Misc
{
	/// <summary>
	/// Energizes EffectValue on proc
	/// </summary>
	public class ProcEnergizeHandler : AuraEffectHandler
	{
		public override void OnProc(Unit triggerer, IUnitAction action)
		{
			Owner.Energize(EffectValue, m_aura.CasterUnit, m_spellEffect);
		}
	}
}
