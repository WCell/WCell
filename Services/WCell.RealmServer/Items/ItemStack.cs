namespace WCell.RealmServer.Items
{
	public struct ItemStack
	{
		public ItemTemplate Template;

		public int Amount;

		public override string ToString()
		{
			return Amount + "x " + Template;
		}
	}
}