using WCell.Core;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Items
{
	public interface IContainer : IEntity
	{
		/// <summary>
		/// The inventory of this Container
		/// </summary>
		BaseInventory BaseInventory { get; }

		/// <summary>
		/// Sets the given EntityId field to the given value
		/// </summary>
		void SetEntityId(int field, EntityId value);
	}
}