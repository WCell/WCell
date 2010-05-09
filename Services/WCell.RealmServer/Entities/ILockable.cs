using WCell.RealmServer.Looting;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Entities
{
	/// <summary>
	/// Represents any entity that might have a lock
	/// </summary>
	public interface ILockable : ILootable
	{
		/// <summary>
		/// The lock (might be null)
		/// </summary>
		LockEntry Lock
		{
			get;
		}
	}
}
