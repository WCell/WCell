using WCell.RealmServer.Factions;
using WCell.Util.Threading;

namespace WCell.RealmServer.Entities
{
	public interface ITransportInfo : IFactionMember, IWorldLocation, INamedEntity, IContextHandler
	{
		float Orientation
		{
			get;
		}
	}
}