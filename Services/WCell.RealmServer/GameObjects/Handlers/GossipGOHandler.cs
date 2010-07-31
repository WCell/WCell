using WCell.RealmServer.Entities;
using WCell.RealmServer.Gossips;

namespace WCell.RealmServer.GameObjects.Handlers
{
	/// <summary>
	/// Use this handler to attach a Gossip menu to a GO
	/// </summary>
	public class GossipGOHandler : GameObjectHandler
	{
		public GossipMenu Menu { get; set; }

		public GossipGOHandler(GossipMenu menu)
		{
			Menu = menu;
		}

		public override bool Use(Character user)
		{
			user.StartGossip(Menu);
			return true;
		}
	}
}