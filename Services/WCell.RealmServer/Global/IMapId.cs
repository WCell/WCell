using WCell.Constants.World;

namespace WCell.RealmServer.Global
{
	public interface IMapId
	{
		MapId MapId { get; }

		uint InstanceId { get; }
	}
}