using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras
{
	public class PlayerAuraCollection : AuraCollection
	{
		/// <summary>
		/// Set of Auras that are only applied under certain circumstances
		/// </summary>
		public List<Aura> ItemRestrictedAuras = new List<Aura>(3);

		public PlayerAuraCollection(Character owner) : base(owner)
		{
		}

		public override void AddAura(Aura aura, bool update)
		{
			base.AddAura(aura, update);
			if (aura.Spell.IsPassive && aura.Spell.HasItemRequirements)
			{
				ItemRestrictedAuras.Add(aura);
			}
		}

		protected internal override void Cancel(Aura aura)
		{
			base.Cancel(aura);
			if (aura.Spell.IsPassive && aura.Spell.HasItemRequirements)
			{
				ItemRestrictedAuras.Remove(aura);
			}
		}

		internal void OnEquip(Item item)
		{
			foreach (var aura in ItemRestrictedAuras)
			{
				
			}
		}
	}
}
