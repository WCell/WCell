using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Skills;
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

		public PlayerAuraCollection(Character owner)
			: base(owner)
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
				if (aura.Spell.IsModalShapeshiftDependentAura)
				{
					ShapeshiftRestrictedAuras.Add(aura);
				}
			}
		}

		protected internal override void Cancel(Aura aura)
		{
			base.Cancel(aura);
			if (aura.Spell.IsPassive)
			{
				if (aura.Spell.HasItemRequirements)
				{
					ItemRestrictedAuras.Remove(aura);
				}
				if (aura.Spell.AllowedShapeshiftMask != 0)
				{
					ShapeshiftRestrictedAuras.Add(aura);
				}
			}
		}

		internal void OnEquip(Item item)
		{
			if (itemRestrictedAuras != null)
			{
				var plr = (Character)m_owner;
				foreach (var aura in itemRestrictedAuras)
				{
					if (!aura.IsActive)
					{
						aura.IsActive = aura.Spell.CheckItemRestrictions(item, plr.Inventory) == SpellFailedReason.Ok;
					}
				}
			}
		}

		internal void OnUnEquip(Item item)
		{
			if (itemRestrictedAuras != null)
			{
				var plr = (Character)m_owner;
				foreach (var aura in itemRestrictedAuras)
				{
					if (aura.IsActive)
					{
						aura.IsActive = aura.Spell.CheckItemRestrictionsWithout(item, plr.Inventory) == SpellFailedReason.Ok;
					}
				}
			}
		}

		internal void OnShapeshiftFormChanged()
		{
			if (shapeshiftRestrictedAuras != null)
			{
				foreach (var aura in shapeshiftRestrictedAuras)
				{
					if (aura.Spell.AllowedShapeshiftMask != 0)
					{
						// the entire Aura is toggled
						if (!aura.Spell.AllowedShapeshiftMask.HasAnyFlag(m_owner.ShapeshiftMask))
						{
							aura.IsActive = false;	// Aura is off
							continue;
						}
						else
						{
							aura.IsActive = true;	// Aura is running
						}
					}

					if (aura.Spell.HasShapeshiftDependentEffects)
					{
						// TODO: has shapeshift-dependent effects
						var ownerForm = Owner.ShapeshiftMask;
						foreach (var handler in aura.Handlers)
						{
							if (handler.SpellEffect.RequiredShapeshiftMask == 0 ||
							    (handler.SpellEffect.RequiredShapeshiftMask.HasAnyFlag(ownerForm)))
							{
								
							}
						}
					}
				}
			}
		}
	}
}