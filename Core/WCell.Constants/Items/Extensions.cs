
namespace WCell.Constants.Items
{
	public static class Extensions
	{
		#region HasAnyFlag
		public static bool HasAnyFlag(this ItemSubClassMask flags, ItemSubClassMask otherFlags)
		{
			return (flags & otherFlags) != 0;
		}
		public static bool HasAnyFlag(this InventorySlotTypeMask flags, InventorySlotTypeMask otherFlags)
		{
			return (flags & otherFlags) != 0;
		}
		public static bool HasAnyFlag(this InventorySlotTypeMask flags, InventorySlotType type)
		{
			return (flags & type.ToMask()) != 0;
		}
		public static bool HasAnyFlag(this SocketColor flags, SocketColor otherFlags)
		{
			return (flags & otherFlags) != 0;
		}
		public static bool HasAnyFlag(this ItemBagFamilyMask flags, ItemBagFamilyMask otherFlags)
		{
			return (flags & otherFlags) != 0;
		}
		public static bool HasAnyFlag(this ItemFlags flags, ItemFlags otherFlags)
		{
			return (flags & otherFlags) != 0;
		}
		public static bool HasAnyFlag(this ItemFlags2 flags, ItemFlags2 otherFlags)
		{
			return (flags & otherFlags) != 0;
		}
		#endregion

		public static InventorySlotTypeMask ToMask(this InventorySlotType type)
		{
			return (InventorySlotTypeMask) (1 << (int) type);
		}
	}
}