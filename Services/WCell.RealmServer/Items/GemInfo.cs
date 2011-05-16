using WCell.Constants.Items;
using WCell.Core.ClientDB;
using WCell.RealmServer.Items.Enchanting;

namespace WCell.RealmServer.Items
{
	public struct SocketInfo
	{
		public SocketColor Color;
		public int Content;// condition id maybe?
	}

	public class GemProperties
	{
		public uint Id;
		public ItemEnchantmentEntry Enchantment;
		public SocketColor Color;

		public override string ToString()
		{
			return string.Format("{0} (Color: {1}, Enchantment: {2})", Id, Color, Enchantment);
		}
	}

	public class GemPropertiesConverter : AdvancedClientDBRecordConverter<GemProperties>
	{
		public override GemProperties ConvertTo(byte[] rawData, ref int id)
		{
			var props = new GemProperties();

			props.Id = (uint)(id = GetInt32(rawData, 0));
			var enchantmentId = GetUInt32(rawData, 1);
			props.Enchantment = EnchantMgr.GetEnchantmentEntry(enchantmentId);
			props.Color = (SocketColor)GetUInt32(rawData, 4);

			return props;
		}
	}
}