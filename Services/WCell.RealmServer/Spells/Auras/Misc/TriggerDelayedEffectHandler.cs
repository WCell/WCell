using NLog;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Misc
{
	/// <summary>
	/// Triggers the TriggerSpell of the SpellEffect after a delay of EffectValue on the Owner
	/// </summary>
	public class TriggerDelayedEffectHandler : AuraEffectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		private OneShotUpdateObjectAction timer;

		protected internal override void CheckInitialize(ObjectReference casterReference, Unit target,
			ref SpellFailedReason failReason)
		{
			if (m_spellEffect.TriggerSpell == null)
			{
				failReason = SpellFailedReason.Error;
				log.Warn("Tried to cast Spell \"{0}\" which has invalid TriggerSpellId {1}", m_spellEffect.Spell, m_spellEffect.TriggerSpellId);
			}
		}

		protected override void Apply()
		{
			timer = Owner.CallDelayed(EffectValue, TriggerSpell);
		}

		protected override void Remove(bool cancelled)
		{
			if (timer != null)
			{
				Owner.RemoveUpdateAction(timer);
			}
		}

		private void TriggerSpell(WorldObject owner)
		{
			timer = null;
			SpellCast.ValidateAndTriggerNew(m_spellEffect.TriggerSpell, m_aura.CasterReference, Owner, Owner, m_aura.UsedItem);
		}
	}
}
