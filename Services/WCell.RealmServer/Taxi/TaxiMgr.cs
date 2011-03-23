using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Util.Logging;
using WCell.Constants;
using WCell.Constants.NPCs;
using WCell.Constants.Pathing;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Core.DBC;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Paths;
using WCell.Util;
using WCell.Util.Graphics;
using WCell.Util.Variables;

namespace WCell.RealmServer.Taxi
{
	/// <summary>
	/// 
	/// TODO: Cancel flight
	/// TODO: Save Character's route to DB
	/// 
	/// Static helper and srcCont class for Taxi-related information (Flight-Paths, Flight-Masters etc) 
	/// </summary>
	public static class TaxiMgr
	{
		/// <summary>
		/// The delay in millis between position updates of Units that are on Taxis.
		/// </summary>
		[Variable("TaxiInterpolationMillis")]
		public static int InterpolationDelayMillis = 800;

		private static int airSpeed = 32;

		/// <summary>
		/// The speed of Units travelling on Taxis in yards/second - Default: 16.
		/// (The average speed on foot is 7 y/s)
		/// </summary>
		[Variable("TaxiAirSpeed")]
		public static int AirSpeed
		{
			get { return airSpeed; }
			set
			{
				airSpeed = value;
				if (init)
				{
					// re-initialize
					Initialize();
				}
			}
		}

		#region Fields

		private static bool init;

		[NotVariable]
		/// <summary>
		/// All TaxiNodes by their id
		/// </summary>
		public static PathNode[] PathNodesById = new PathNode[340];

		[NotVariable]
		/// <summary>
		/// All TaxiPaths by their id
		/// </summary>
		public static TaxiPath[] PathsById = new TaxiPath[1200];

		/*
		/// <summary>
		/// TODO: Change to array
		/// </summary>
		public static readonly Dictionary<uint, TaxiNode> BySpawnId = new Dictionary<uint, TaxiNode>(100);
		*/

		/// <summary>
		/// A TaxiNode Mask with all existing nodes activated.
		/// </summary>
		public static TaxiNodeMask AllActiveMask = new TaxiNodeMask();

		#endregion

		#region Init
		public static MappedDBCReader<PathVertex, DBCTaxiPathNodeConverter> TaxiVertexReader;

		[Initialization(InitializationPass.Fourth, "Initialize Taxi Paths")]
		public static void Initialize()
		{
			init = true;
			var taxiNodeReader = new MappedDBCReader<PathNode, DBCTaxiNodeConverter>(
                RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_TAXINODES));

			var taxiPathReader = new MappedDBCReader<TaxiPath, DBCTaxiPathConverter>(
				RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_TAXIPATHES));

			TaxiVertexReader = new MappedDBCReader<PathVertex, DBCTaxiPathNodeConverter>(
				RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_TAXIPATHNODES));

			foreach (var node in taxiNodeReader.Entries.Values)
			{
				// skip inactive nodes
				if (node.Position == Vector3.Zero)
					continue;

				ArrayUtil.Set(ref PathNodesById, node.Id, node);
				AllActiveMask.Activate(node.Id);
			}

			var nodeLists = new Dictionary<uint, SortedList<uint, PathVertex>>();
			var toRemove = new List<int>(5);
			foreach (var path in taxiPathReader.Entries.Values)
			{
				if (taxiNodeReader.Entries.TryGetValue((int)path.StartNodeId, out path.From))
				{
					path.From.AddPath(path);
					taxiNodeReader.Entries.TryGetValue((int)path.EndNodeId, out path.To);
					nodeLists[path.Id] = new SortedList<uint, PathVertex>();
					ArrayUtil.Set(ref PathsById, path.Id, path);
				}
				else
				{
					toRemove.Add((int) path.StartNodeId);
					//LogManager.GetCurrentClassLogger().Warn("Taxi-Path has no valid starting-point: " + path.StartNodeId);
				}
			}

			// remove invalid taxi paths
			foreach (var path in toRemove)
			{
				taxiPathReader.Entries.Remove(path);
			}

			// add all vertices to their path
			foreach (var vertex in TaxiVertexReader.Entries.Values)
			{
				SortedList<uint, PathVertex> vertices;
				if (nodeLists.TryGetValue(vertex.PathId, out vertices))
				{
					vertices.Add(vertex.NodeIndex, vertex);
				}
			}

			foreach (var nodeList in nodeLists)
			{
				TaxiPath path;
				if (!taxiPathReader.Entries.TryGetValue((int)nodeList.Key, out path))
				{
					continue;
				}

				LinkedListNode<PathVertex> current = null;
				float totalLength = 0;

				foreach (var vertex in nodeList.Value.Values)
				{
					if (current == null)
					{
						// This is the first PathNode in the TaxiPath
						current = path.Nodes.AddFirst(vertex);
						current.Value.DistFromStart = 0;
						current.Value.TimeFromStart = 0;

						current.Value.DistFromPrevious = 0;
						current.Value.TimeFromPrevious = 0;
					}
					else
					{
                        var isTeleport = current.Value.HasMapChange = current.Previous != null && (current.Value.MapId != current.Previous.Value.MapId ||
                            current.Value.Flags.HasFlag(TaxiPathNodeFlags.IsTeleport));

						if (isTeleport)
						{
							// Since we teleported, there is no distance from previous, and we reset the dist from start
							current.Value.DistFromPrevious = 0;
							current.Value.DistFromStart = 0;
							current.Value.TimeFromPrevious = 0;
							current.Value.TimeFromStart = 0;
						}
						else
						{
							// Get the distance from the current to the next
							if (current.Previous != null)
							{
								var last = current.Previous.Value;
								current.Value.DistFromPrevious = current.Value.Pos.GetDistance(ref last.Pos);
								(current.Value.FromLastNode = (current.Value.Pos - last.Pos)).Normalize();
								current.Value.TimeFromPrevious = (int)((current.Value.DistFromPrevious * 1000) / AirSpeed);

								totalLength += current.Value.DistFromPrevious;
							}

							current.Value.DistFromStart = totalLength;
							current.Value.TimeFromStart = (int)((totalLength * 1000) / AirSpeed);
						}

						current = path.Nodes.AddAfter(current, vertex);
					}
					vertex.ListEntry = current;
					vertex.Path = path;
				}

				path.PathLength = totalLength;
				path.PathTime = (uint)((totalLength * 1000) / AirSpeed);
				var map = path.From.Map;
				if (map != null && map.FirstTaxiNode == null)
				{
					map.FirstTaxiNode = path.From;
				}
			}
		}
		#endregion

		public static PathNode GetNode(uint id)
		{
			return PathNodesById.Get(id);
		}

		/// <summary>
		/// Returns the TaxiNode closest to the given position (within 10 yards)
		/// </summary>
		/// <param name="pos">A position given in world coordinates</param>
		/// <returns>The closest TaxiNode within 10 yards, or null.</returns>
		public static PathNode GetNearestTaxiNode(Vector3 pos)
		{
			PathNode closest = null;
			var distSq = Single.MaxValue;

			foreach (var node in PathNodesById)
			{
				if (node == null)
					continue;

				var temp = node.Position.DistanceSquared(ref pos);
				if (temp < distSq)
				{
					distSq = temp;
					closest = node;
				}
			}

			return closest;
		}

		public static PathVertex GetVertex(int id)
		{
			PathVertex v;
			TaxiVertexReader.Entries.TryGetValue(id, out v);
			return v;
		}

		/// <summary>
		/// Sends the given Character on the given Path.
		/// </summary>
		/// <param name="chr">The Character to fly around.</param>
		/// <param name="destinations">An array of destination TaxiNodes.</param>
		/// <returns>Whether the client was sent on its way.</returns>
		internal static bool TryFly(Character chr, NPC vendor, PathNode[] destinations)
		{
			var client = chr.Client;

			if (vendor == null && chr.Role.IsStaff)
			{
				var dest = destinations.LastOrDefault();
				if (dest != null)
				{
					chr.TeleportTo(dest);
					return true;
				}
				return false;
			}

			if (vendor == null || !vendor.CheckVendorInteraction(chr))
			{
				TaxiHandler.SendActivateTaxiReply(client, TaxiActivateResponse.NotAvailable);
			}
			else if (PreFlightCheatChecks(client, destinations) &&
				PreFlightValidPathCheck(client, destinations) &&
				(client.ActiveCharacter.GodMode || PreFlightMoneyCheck(client)))
			{
				// All good, send an "All Good" reply to the client.
				TaxiHandler.SendActivateTaxiReply(client, TaxiActivateResponse.Ok);

				// PvP flag is auto-cleared when starting a taxi-flight
				chr.UpdatePvPState(false, true);

				FlyUnit(chr, true);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Check various character states that disallow flights.
		/// </summary>
		/// <param name="client">The IRealmClient requesting the flight.</param>
		/// <param name="destinations">An array of destination TaxiNodes.</param>
		/// <returns>True if flight allowed.</returns>
		private static bool PreFlightCheatChecks(IRealmClient client, PathNode[] destinations)
		{
			var curChar = client.ActiveCharacter;
			var sourceNode = destinations[0]; //[startNode, destination, destination, ...]

			if (destinations.Length < 2)
			{
				return false;
			}

			// Cheat Checks
			// Cheat check -- can't fly while mounted
			if (curChar.IsMounted)
			{
				TaxiHandler.SendActivateTaxiReply(client, TaxiActivateResponse.PlayerAlreadyMounted);
				return false;
			}

			// Cheat check -- can't fly shape-shifted
			if (!(curChar.ShapeshiftForm == ShapeshiftForm.Normal ||
				   curChar.ShapeshiftForm == ShapeshiftForm.BattleStance ||
				   curChar.ShapeshiftForm == ShapeshiftForm.BerserkerStance ||
				   curChar.ShapeshiftForm == ShapeshiftForm.DefensiveStance ||
				   curChar.ShapeshiftForm == ShapeshiftForm.Shadow))
			{
				TaxiHandler.SendActivateTaxiReply(client, TaxiActivateResponse.PlayerShapeShifted);
				return false;
			}

			// Cheat check -- can't fly while logging out
			if (curChar.IsLoggingOut)
			{
				TaxiHandler.SendActivateTaxiReply(client, TaxiActivateResponse.PlayerShapeShifted);
				return false;
			}


			// Cheat check -- can't fly immobilized
			if (!curChar.CanMove)
			{
				TaxiHandler.SendActivateTaxiReply(client, TaxiActivateResponse.PlayerMoving);
				return false;
			}


			// Cheat check -- can't fly while casting
			if (curChar.IsUsingSpell)
			{
				TaxiHandler.SendActivateTaxiReply(client, TaxiActivateResponse.PlayerBusy);
				return false;
			}

			// Cheat check -- can't fly from a node across the continent
			if (sourceNode.MapId != curChar.Map.Id)
			{
				TaxiHandler.SendActivateTaxiReply(client, TaxiActivateResponse.NoPathNearby);
				return false;
			}

			// Cheat check -- can't fly while trading
			if (curChar.TradeWindow != null)
			{
				TaxiHandler.SendActivateTaxiReply(client, TaxiActivateResponse.PlayerBusy);
				return false;
			}
			return true;
		}

		/// <summary>
		/// Check that a valid path exists between the destinations.
		/// Also sets the characters TaxiPaths queue with the sequence of valid 
		/// paths to the final destination.
		/// </summary>
		/// <param name="client">The IRealmClient requesting the flight.</param>
		/// <param name="destinations">An array of destination TaxiNodes.</param>
		/// <returns>True if a valid path exists.</returns>
		private static bool PreFlightValidPathCheck(IRealmClient client, PathNode[] destinations)
		{
			var curChar = client.ActiveCharacter;

			curChar.TaxiPaths.Clear();

			for (uint i = 0; i < (destinations.Length - 1); ++i)
			{
				TaxiPath path = destinations[i].GetPathTo(destinations[i + 1]);
				if (path == null)
				{
					curChar.TaxiPaths.Clear();
					TaxiHandler.SendActivateTaxiReply(client, TaxiActivateResponse.InvalidChoice);
					return false;
				}
				curChar.TaxiPaths.Enqueue(path);
			}
			return true;
		}

		/// <summary>
		/// Check that the character has enough money to cover the cost of the flight(s).
		/// Also deducts the cost of the flight from the character.
		/// </summary>
		/// <param name="client">The IRealmClient requesting the flight.</param>
		/// <returns>An array of destination TaxiNodes.</returns>
		private static bool PreFlightMoneyCheck(IRealmClient client)
		{
			Character curChar = client.ActiveCharacter;

			uint totalCost = 0;
			foreach (TaxiPath tempPath in curChar.TaxiPaths)
			{
				if (tempPath != null)
				{
					totalCost += tempPath.Price;
				}
				else
				{
					curChar.TaxiPaths.Clear();
					TaxiHandler.SendActivateTaxiReply(client, TaxiActivateResponse.InvalidChoice);
					return false;
				}
			}

			// Do we have enough cash-money?
			if (curChar.Money < totalCost)
			{
				curChar.TaxiPaths.Clear();
				TaxiHandler.SendActivateTaxiReply(client, TaxiActivateResponse.InsufficientFunds);
				return false;
			}
			// Charge for the flight
			client.ActiveCharacter.Money -= totalCost;
			return true;
		}

		/// <summary>
		/// Client-side taxi interpolation gets fishy when exceeding certain speed limits
		/// </summary>
		internal static bool IsNormalSpeed
		{
			get { return AirSpeed <= 32; }
		}

		/// <summary>
		/// Send character down the next leg of a multi-hop trip.
		/// </summary>
		internal static void ContinueFlight(Unit unit)
		{
			if (unit.LatestTaxiPathNode == null)
			{
				//throw new InvalidOperationException("Tried to continue Taxi-flight of Unit which did not start flying.");
				return;
			}

			var latestVertex = unit.LatestTaxiPathNode.Value;
			var current = latestVertex.Path.To;

			if (unit.m_TaxiMovementTimer.IsRunning &&
				!unit.IsInRadius(current.Position, AirSpeed))
			{
				return;
			}

			var done = false;

			// Are we at the end of the line?
			if (unit.TaxiPaths.Count < 2)
			{
				done = true;
			}
			else
			{
				var arrivalPath = unit.TaxiPaths.Dequeue();
				if (arrivalPath.To != current)
				{
					unit.CancelTaxiFlight();
					return;
				}

			    var destPath = unit.TaxiPaths.Peek();
			    if (current != destPath.From)
			    {
			        unit.CancelTaxiFlight();
			        return;
			    }
			}

			if (!done)
			{
				// One stop on a multi-stop ride
				FlyUnit(unit, false);
			}
			else
			{
				if (IsNormalSpeed)
				{
					unit.Map.MoveObject(unit, latestVertex.Pos);
				}
				else
				{
					// Client doesn't seem to display the movement fast enough
					// so we have to speed it up by teleporting
					unit.TeleportTo(latestVertex.Pos);
				}
				unit.CancelTaxiFlight();
			}
		}

		public static void FlyUnit(Unit chr, bool startFlight)
		{
			FlyUnit(chr, startFlight, null);
		}

		public static void FlyUnit(Unit unit, bool startFlight, LinkedListNode<PathVertex> startNode)
		{
			if (unit.TaxiPaths.Count < 1)
			{
				throw new InvalidOperationException("Tried to fly Unit without Path given.");
			}

			var path = unit.TaxiPaths.Peek();

			// Stop combat
			unit.IsInCombat = false;

			// Cannot be invisible when flying
			unit.Stealthed = 0;

			if (startFlight)
			{
				// Regulators, Mount Up!
				var mount = NPCMgr.GetEntry(unit.Faction.IsAlliance ? path.From.AllianceMountId : path.From.HordeMountId);
				if (mount != null)
				{
					var mountId = mount.GetRandomModel().DisplayId;
					unit.Mount(mountId);
					if (unit is Character)
					{
						unit.PushFieldUpdateToPlayer((Character)unit, UnitFields.MOUNTDISPLAYID, mountId);
					}
				}

				unit.OnTaxiStart();
			}

			unit.LatestTaxiPathNode = startNode ?? path.Nodes.First;
			//var next = unit.LatestPathNode.Next.Value;
			if (unit.LatestTaxiPathNode == path.Nodes.First)
			{
				// new flight
				unit.taxiTime = 0;
				MovementHandler.SendMoveToPacket(unit, path.PathTime, MonsterMoveFlags.Fly, path.Nodes);
			}
			else
			{
				// continue:
				// set time to the amount that it takes to get this far along the path
				// ReSharper disable PossibleNullReferenceException
				unit.taxiTime = startNode.Previous.Value.TimeFromStart +
					(int)((1000 * startNode.Value.Pos.GetDistance(unit.Position)) / AirSpeed);
				// ReSharper restore PossibleNullReferenceException
				MovementHandler.SendMoveToPacket(unit, AirSpeed, MonsterMoveFlags.Fly, startNode);
			}
		}

		/// <summary>
		/// Interpolates the position of the given Unit along the Path given the elapsed flight time.
		/// </summary>
		/// <param name="elapsedTime">Time that elapsed since the given unit passed by the last PathVertex</param>
		internal static void InterpolatePosition(Unit unit, int elapsedTime)
		{
			var latestNode = unit.LatestTaxiPathNode;
			unit.taxiTime += elapsedTime;

			if (latestNode.Next == null)
			{
				// must not happen
				unit.CancelTaxiFlight();
				return;
			}

			while (latestNode.Next.Value.TimeFromStart <= unit.taxiTime)
			{
				// arrived at a node

				//if (unit is Character)
				//{
				//    var chr = (Character) unit;
				//    chr.SendSystemMessage("[{0}] Node: {1} yards ({2} millis)", 
				//        (int)RealmServer.RunTime.TotalMilliseconds, 
				//        latestNode.Next.Value.DistFromStart,
				//        latestNode.Next.Value.TimeFromStart);
				//}
				latestNode = latestNode.Next;
				unit.LatestTaxiPathNode = latestNode;

				if (latestNode.Next == null)
				{
					// Finished this Path
					if (IsNormalSpeed)
					{
						unit.m_TaxiMovementTimer.Stop();
					}
					else
					{
						ContinueFlight(unit);
					}
					return;
				}
			}

			var prevPathNode = latestNode.Value;
			var nextPathNode = latestNode.Next.Value;

			// time in millis since we passed the last node
			var timeDelta = unit.taxiTime - latestNode.Value.TimeFromStart;

			var pos = prevPathNode.Pos +
					  (((nextPathNode.Pos - prevPathNode.Pos) * timeDelta) / nextPathNode.TimeFromPrevious);
			unit.Map.MoveObject(unit, ref pos);
		}
	}
}