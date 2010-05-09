using WCell.Constants.Items;
using WCell.Util;

namespace WCell.RealmServer.Items.Enchanting
{
	public class ItemEnchantmentEffect
	{
		public ItemEnchantmentType Type;
		public int MinAmount, MaxAmount;

		public int RandomAmount
		{
			get { return Utility.Random(MinAmount, MaxAmount); }
		}

		/// <summary>
		/// Depending on the <see cref="Type"/>:
		/// SpellId
		/// DamageSchool
		/// other
		/// </summary>
		public uint Misc;

		public override string ToString()
		{
			return string.Format("EnchantEffect - Type: {0}, Amount: {1}, Misc: {2}", Type, MinAmount, Misc);
		}
	}
}