using NLog;
using WCell.Constants;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Network;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Taxi;
using WCell.Util;

namespace WCell.RealmServer.Handlers
{
	public static class TaxiHandler
	{
		private static readonly Logger sLog = LogManager.GetCurrentClassLogger();

		#region Packets IN

		/// <summary>
		/// Talk to a Taxi Vendor
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_ENABLETAXI)]
		public static void HandleEnable(IRealmClient client, RealmPacketIn packet)
		{
			var vendorId = packet.ReadEntityId();
			var vendor = client.ActiveCharacter.Region.GetObject(vendorId) as NPC;

			if (vendor != null)
			{
				vendor.TalkToFM(client.ActiveCharacter);
			}
		}

		/// <summary>
		/// Talk to a Taxi Vendor
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_TAXIQUERYAVAILABLENODES)]
		public static void HandleAvailableNodesQuery(IRealmClient client, RealmPacketIn packet)
		{
			var vendorId = packet.ReadEntityId();
			var vendor = client.ActiveCharacter.Region.GetObject(vendorId) as NPC;

			if (vendor != null)
			{
				vendor.TalkToFM(client.ActiveCharacter);
			}
		}

		/// <summary>
		/// Fly away
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_ACTIVATETAXI)]
		public static void HandleTaxiActivate(IRealmClient client, RealmPacketIn packet)
		{
			var vendorId = packet.ReadEntityId();
			var from = packet.ReadUInt32();
			var to = packet.ReadUInt32();

			var vendor = client.ActiveCharacter.Region.GetObject(vendorId) as NPC;

			var destinations = new PathNode[2];
			destinations[0] = TaxiMgr.PathNodesById.Get(from);
			destinations[1] = TaxiMgr.PathNodesById.Get(to);
			TaxiMgr.TryFly(client.ActiveCharacter, vendor, destinations);
		}

		/// <summary>
		/// Fly far away. (For taxi paths that include more than one stop.)
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_ACTIVATETAXIEXPRESS)]
		public static void HandleTaxiActivateFar(IRealmClient client, RealmPacketIn packet)
		{
			var vendorId = packet.ReadEntityId();
			//var totalCost = packet.ReadUInt32(); // removed, 3.2.2
			var numNodes = packet.ReadUInt32();


			var destinations = new PathNode[numNodes];
			for (int i = 0; i < numNodes; ++i)
			{
				destinations[i] = TaxiMgr.PathNodesById.Get(packet.ReadUInt32());
			}

			var vendor = client.ActiveCharacter.Region.GetObject(vendorId) as NPC;
			TaxiMgr.TryFly(client.ActiveCharacter, vendor, destinations);
		}

		/// <summary>
		/// Client asks "Is this TaxiNode activated yet?"
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_TAXINODE_STATUS_QUERY)]
		public static void HandleTaxiStatusQuery(IRealmClient client, RealmPacketIn packet)
		{
			var vendorId = packet.ReadEntityId();
			var chr = client.ActiveCharacter;
			var vendor = chr.Region.GetObject(vendorId) as NPC;
			if (vendor == null)
			{
				return;
			}

			var isActive = chr.GodMode || chr.TaxiNodes.IsActive(vendor.VendorTaxiNode);
			SendTaxiNodeStatus(client, vendor.VendorTaxiNode, vendorId, isActive);
		}

		/// <summary>
		/// Asked by the client upon landing at each stop in a multinode trip
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_MOVE_SPLINE_DONE)]
		public static void HandleNextTaxiDestination(IRealmClient client, RealmPacketIn packet)
		{
			TaxiMgr.ContinueFlight(client.ActiveCharacter);
		}

		#endregion

		#region Packets OUT
		public static void SendTaxiPathActivated(IRealmClient client)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_NEW_TAXI_PATH, 0))
			{
				client.Send(packet);
			}
		}

		public static void SendTaxiPathUpdate(IPacketReceiver client, EntityId vendorId, bool activated)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_TAXINODE_STATUS, 9))
			{
				packet.Write(vendorId.Full);
				packet.Write(activated);
				client.Send(packet);
			}
		}

		public static void ShowTaxiList(Character chr, PathNode node)
		{
			if (node != null)
			{
				ShowTaxiList(chr, chr, node);
			}
		}

		public static void ShowTaxiList(Character chr, IEntity vendor, PathNode curNode)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SHOWTAXINODES, (4 + 8 + 4 + 8 * 4)))
			{
				packet.Write(1);
				if (vendor != null)
				{
					packet.Write(vendor.EntityId.Full);
				}
				else
				{
					packet.Write(EntityId.Zero);
				}
				packet.Write(curNode.Id);

				for (var i = 0; i < chr.TaxiNodes.Mask.Length; ++i)
				{
					packet.Write(chr.TaxiNodes.Mask[i]);
				}
				chr.Send(packet);
			}
		}

		public static void SendTaxiNodeStatus(IPacketReceiver client, PathNode curNode, EntityId vendorId, bool isActiveNode)
		{
			if (curNode != null)
			{
				SendTaxiPathUpdate(client, vendorId, isActiveNode);
			}
			else
			{
				sLog.Warn("Vendor: {0} not associated with a TaxiNode", vendorId.Full);
			}
		}

		public static void SendActivateTaxiReply(IPacketReceiver client, TaxiActivateResponse response)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ACTIVATETAXIREPLY, 4))
			{
				packet.Write((uint)response);
				client.Send(packet);
			}
		}

		#endregion
	}
}