using WCell.RealmServer.Entities;

namespace WCell.RealmServer.GameObjects.Handlers
{
	/// <summary>
	/// Use this handler to attach a Gossip menu to a GO
	/// </summary>
	public class GossipGOHandler : GameObjectHandler
	{
		public override bool Use(Character user)
		{
			return true;
		}
	}
}