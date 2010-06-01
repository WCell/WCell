using System;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Items
{
	/// <summary>
	/// Handles Item OnHit-procs which are applied to the wearer of the Item
	/// </summary>
	public class ItemHitProcHandler : IProcHandler
	{
		private Item m_Item;
		private readonly Spell m_Spell;

		public ItemHitProcHandler(Item item, Spell spell)
		{
			m_Item = item;
			m_Spell = spell;
		}

		/// <summary>
		/// ItemHitProcs always trigger
		/// </summary>
		public ProcTriggerFlags ProcTriggerFlags
		{
			get { return ProcTriggerFlags.RangedAttackSelf | ProcTriggerFlags.MeleeAttackSelf; }
		}

		/// <summary>
		/// Chance to Proc from 0 to 100
		/// Yet to implement: http://www.wowwiki.com/Procs_per_minute
		/// </summary>
		public uint ProcChance
		{
			get
			{
				return 100;
				// return (m_Spell.ProcChance * m_Item.AttackTime) / 60000
			}
		}

		public Spell ProcSpell
		{
			get { return m_Spell; }
		}

		/// <summary>
		/// ItemHitProcs dont have charges
		/// </summary>
		public int StackCount
		{
			get { return 0; }
			set { throw new NotImplementedException("Items do not have proc charges."); }
		}

		public int MinProcDelay
		{
			get { return 0; }
		}

		public DateTime NextProcTime
		{
			get;
			set;
		}

		public bool CanBeTriggeredBy(Unit triggerer, IUnitAction action, bool active)
		{
			return m_Spell.CanProcBeTriggeredBy(action, active);
		}

		public void TriggerProc(Unit triggerer, IUnitAction action)
		{
			m_Item.Owner.SpellCast.ValidateAndTrigger(m_Spell, triggerer);
		}

		public void Dispose()
		{
			m_Item = null;
		}
	}
}
