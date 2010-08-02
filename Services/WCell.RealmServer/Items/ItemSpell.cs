using System.Collections.Generic;
using WCell.Constants.Items;
using WCell.Constants.Spells;
using WCell.RealmServer.Spells;
using WCell.Util;
using WCell.Util.Data;

namespace WCell.RealmServer.Items
{
	/// <summary>
	/// Item-bound spell, contains information on charges, cooldown, trigger etc
	/// </summary>
	public class ItemSpell
	{
		public static readonly ItemSpell[] EmptyArray = new ItemSpell[0];

		[NotPersistent]
		/// <summary>
		/// The actual Spell
		/// </summary>
		public Spell Spell;

		public SpellId Id;

		/// <summary>
		/// The index of this spell within the Template
		/// </summary>
		[NotPersistent]
		public uint Index;

		public ItemSpellTrigger Trigger;

		public uint Charges;

		/// <summary>
		/// SpellCategory.dbc
		/// </summary>
		public uint CategoryId;

		public int Cooldown;

		public int CategoryCooldown;

		[NotPersistent]
		public bool HasCharges;


		public void FinalizeAfterLoad()
		{
			Spell = SpellHandler.Get(Id);
			HasCharges = (int)Charges > 0;
		}

		public override string ToString()
		{
			return ToString(true);
		}

		public string ToString(bool inclTrigger)
		{
			var list = new List<string>(5);
			//if (Index != 0) {
			//    str.Add("Index: " + Index);
			//}
			if (Charges > 0)
			{
				list.Add("Charges: " + Charges);
			}
			if (Cooldown > 0)
			{
				list.Add("Cooldown: " + Cooldown);
			}
			if (CategoryId > 0)
			{
				list.Add("CategoryId: " + CategoryId);
			}
			if (CategoryCooldown > 0)
			{
				list.Add("CategoryCooldown: " + CategoryCooldown);
			}

			var str = string.Format(Id + " (" + (uint)Id + ")" + (list.Count > 0 ? (" - " + list.ToString(", ")) : ""));
			if (inclTrigger)
			{
				str += "[" + Trigger + "]";
			}
			return str;
		}
	}
}