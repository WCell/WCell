using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Spells
{
	public struct SpellCooldown
	{
		public DateTime Until;
		public uint SpellId;
		public uint ItemId;
	}

	public struct SpellCategoryCooldown
	{
		public DateTime Until;
		public uint CategoryId;
		public uint ItemId;
	}
}
