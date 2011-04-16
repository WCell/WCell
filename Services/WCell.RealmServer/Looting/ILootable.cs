using WCell.Constants.Looting;
using WCell.Core;

namespace WCell.RealmServer.Looting
{
	public interface ILootable
	{
		EntityId EntityId { get; }

		/// <summary>
		/// The Loot that is currently lootable
		/// </summary>
		Loot Loot
		{
			get;
			set;
		}

		bool UseGroupLoot { get; }

		/// <summary>
		/// The amount of money that can be looted.
		/// </summary>
		uint LootMoney { get; }

		/// <summary>
		/// The LootId for the given <see cref="LootEntryType"/> of this Lootable object.
		/// </summary>
		uint GetLootId(LootEntryType type);

		/// <summary>
		/// Is called after this Lootable has been looted empty or its Loot expired
		/// </summary>
		void OnFinishedLooting();
	}
}