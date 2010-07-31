using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Items
{
	/// <summary>
	/// Represents a unique slot-descriptor per PlayerInventory (the slot number and its containing BaseInventory)
	/// </summary>
	public struct SimpleSlotId
	{
		public static readonly SimpleSlotId Default = new SimpleSlotId { Slot = BaseInventory.INVALID_SLOT };

		public BaseInventory Container;
		public int Slot;

		/// <summary>
		/// The item at this slot
		/// </summary>
		public Item Item
		{
			get
			{
				return Container[Slot];
			}
		}

		public override string ToString()
		{
			return string.Format("Container: {0}, Slot: {1}, Item: {2}", Container, Slot, Item);
		}
	}

	/// <summary>
	/// Represents a unique slot-descriptor per PlayerInventory (the slot number and its containing BaseInventory),
	/// also keeps track of where it was looking through last, so search can be commenced
	/// </summary>
	public struct SearchSlotId
	{
		public BaseInventory Container;
		public int Slot;

		/// <summary>
		/// Whether to also look in the bank
		/// </summary>
		public readonly bool InclBank;
		/// <summary>
		/// The position in SetterSlots
		/// </summary>
		public int SearchPos;

		public SearchSlotId(bool inclBank)
		{
			Container = null;
			Slot = 0;
			InclBank = inclBank;
			SearchPos = 0;
		}

		/// <summary>
		/// The item at this slot
		/// </summary>
		public Item Item
		{
			get
			{
				return Container[Slot];
			}
		}

		public override string ToString()
		{
			return string.Format("Container: {0}, Slot: {1}, Item: {2}", Container, Slot, Item);
		}
	}
}