using WCell.Constants;
using WCell.Core;
using WCell.RealmServer.Global;
using WCell.RealmServer.Network;
using WCell.Util;

namespace WCell.RealmServer.Entities
{
	/// <summary>
	/// Defines an in-game entity.
	/// </summary>
	public interface IEntity
	{
		/// <summary>
		/// The EntityId
		/// </summary>
		EntityId EntityId { get; }
	}

	/// <summary>
	/// Defines an entity that can recieve packets.
	/// </summary>
	public interface IPacketReceivingEntity : IEntity, IPacketReceiver
	{
	}

	/// <summary>
	/// Defines an entity with a name.
	/// </summary>
	public interface INamedEntity : IEntity, INamed
	{
	}

	/// <summary>
	/// A Summoner has an Id, a Name and a WorldZonePosition
	/// </summary>
	public interface ISummoner : INamedEntity, IWorldZoneLocation
	{
		Zone Zone { get; }
	}

	/// <summary>
	/// Defines a living in-game entity.
	/// </summary>
	public interface ILivingEntity : INamedEntity
	{
		GenderType Gender { get; }

		RaceId Race { get; }

		ClassId Class { get; }
	}
}
