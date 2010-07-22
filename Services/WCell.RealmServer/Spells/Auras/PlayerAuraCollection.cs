using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras
{
	public class PlayerAuraCollection : AuraCollection
	{
		/// <summary>
		/// Set of Auras that are only applied when certain items are equipped
		/// </summary>
		List<Aura> itemRestrictedAuras;

		/// <summary>
		/// Set of Auras that are only applied in certain shapeshift forms
		/// </summary>
		List<Aura> shapeshiftRestrictedAuras;

		public PlayerAuraCollection(Character owner) : base(owner)
		{
		}

		List<Aura> ItemRestrictedAuras
		{
			get
			{
				if (itemRestrictedAuras == null)
				{
					itemRestrictedAuras = new List<Aura>(3);
				}
				return itemRestrictedAuras;
			}
		}

		List<Aura> ShapeshiftRestrictedAuras
		{
			get
			{
				if (shapeshiftRestrictedAuras == null)
				{
					shapeshiftRestrictedAuras = new List<Aura>(3);
				}
				return shapeshiftRestrictedAuras;
			}
		}

		public override void AddAura(Aura aura, bool update)
		{
			base.AddAura(aura, update);
			if (aura.Spell.IsPassive)
			{
				if (aura.Spell.HasItemRequirements)
				{
					ItemRestrictedAuras.Add(aura);
				}
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
				aura.EvalActive(item, true);
			}
		}

		internal void OnUnEquip(Item item)
		{
			foreach (var aura in ItemRestrictedAuras)
			{
				aura.EvalActive(item, false);
			}
		}
	}
}