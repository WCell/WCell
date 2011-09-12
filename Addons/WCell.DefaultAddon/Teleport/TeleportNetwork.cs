using System.Collections.Generic;
using WCell.Constants.GameObjects;
using WCell.Constants.World;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Global;
using WCell.Util.Graphics;

namespace WCell.Addons.Default.Teleport
{
	/// <summary>
	/// Defines a set of locations that should be mutually accessible through teleporter portals or npcs (or anything else that you want)
	/// </summary>
	public class TeleportNetwork
	{
		/// <summary>
		/// ID of spawned default GameObject
		/// </summary>
		private const int DefaultPortalGOid = 179944;

		private static bool m_inited;
		public static List<TeleportNetwork> AllNetworks = new List<TeleportNetwork>(5);


		[Initialization(InitializationPass.Last)]
		[DependentInitialization(typeof(GOMgr))]
		public static void Init()
		{
			InitDefaultTeleporters();

			foreach (var network in AllNetworks)
			{
				network.SpawnAll();
			}
			m_inited = true;
		}

		#region Default Teleporters
		/// <summary>
		/// Default locations, that make up for missing zeppelins and boats
		/// </summary>
		private static readonly Dictionary<string, TeleportNode> Locations = new Dictionary<string, TeleportNode>();

		private static void InitDefaultTeleporters()
		{
			InitDefaultLocations();
			AddDefaultPath("Teldrasil", "Auberdin", false);
			AddDefaultPath("Teldrasil", "Theramore Isle", true);
			AddDefaultPath("Theramore Isle", "Mentil Harbor", false);
			AddDefaultPath("Mentil Harbor", "Auberdin", false);
			AddDefaultPath("Exodar", "Auberdin", false);
		}

		private static void AddDefaultPath(string from, string to, bool directed)
		{
			var fromNode = Locations[from];
			var toNode = Locations[to];

			fromNode.Destinations.Add(toNode);
			if (!directed)
			{
				toNode.Destinations.Add(fromNode);
			}
		}

		private static void InitDefaultLocations()
		{
			var defNetwork = new TeleportNetwork();
			Locations["Teldrasil"] = defNetwork.AddNode(new TeleportNode("Teldrasil", MapId.Kalimdor, new Vector3(8566.646f, 1018.332f, 5.861027f)));
			Locations["Auberdin"] = defNetwork.AddNode(new TeleportNode("Auberdin", MapId.Kalimdor, new Vector3(6501.98f, 797.5204f, 7.845122f)));
			Locations["Theramore Isle"] = defNetwork.AddNode(new TeleportNode("Theramore Isle", MapId.Kalimdor, new Vector3(-4009.453f, -4572.461f, 9.4463f)));
			Locations["Mentil Harbor"] = defNetwork.AddNode(new TeleportNode("Mentil Harbor", MapId.EasternKingdoms, new Vector3(-3911.227f, -627.2722f, 4.742308f)));
			Locations["Exodar"] = defNetwork.AddNode(new TeleportNode("Exodar", MapId.Outland, new Vector3(-4268.54f, -11334.3f, 5.654589f)));
		}

		/// <summary>
		/// All nodes in this Network
		/// </summary>
		public readonly List<TeleportNode> Nodes = new List<TeleportNode>();

		public TeleportNetwork()
		{
			AllNetworks.Add(this);
		}

		public TeleportNode AddNode(TeleportNode node)
		{
			Nodes.Add(node);
			if (m_inited)
			{
				node.Spawn();
			}
			return node;
		}

		/// <summary>
		/// Creates a normal-looking portal to represent a node in the network
		/// </summary>
		public static WorldObject CreateDefaultPortal(TeleportNode node, Map map, Vector3 pos)
		{
			var teleportMenu = WorldLocationMgr.CreateTeleMenu(node.Destinations);
			var portalEntry = GOMgr.GetEntry(DefaultPortalGOid);
			var go = portalEntry.Spawn(map, pos);
			go.State = GameObjectState.Enabled;
			go.ScaleX = 1.5f;
			go.GossipMenu = teleportMenu;
			return go;
		}
		#endregion

		private void SpawnAll()
		{
			foreach (var node in Nodes)
			{
				node.Spawn();
			}
		}
	}
}