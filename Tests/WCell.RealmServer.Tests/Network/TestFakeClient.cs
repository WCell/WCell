using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Updates;
using WCell.RealmServer.Network;
using WCell.Util.Collections;
using WCell.PacketAnalysis.Updates;
using WCell.RealmServer.Tests.Misc;
using WCell.Constants;
using WCell.PacketAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.Core.Network;
using WCell.Core;

namespace WCell.RealmServer.Tests.Network
{
	public class TestFakeClient : FakeRealmClient
	{
		public readonly List<ParsedUpdatePacket> UpdatePackets = new List<ParsedUpdatePacket>();

		public TestFakeClient(TestAccount account)
			: base(account)
		{
		}

		protected override void HandleCMSG(RealmPacketIn inPacket, bool wait)
		{
			AddRealmCMSG(inPacket);
			base.HandleCMSG(inPacket, wait);
		}

		protected override bool HandleSMSG(RealmPacketIn packet)
		{
			AddRealmSMSG(packet);
			return base.HandleSMSG(packet);
		}

		#region Update Packets
		/// <summary>
		/// Returns the last received update packet
		/// </summary>
		public ParsedUpdatePacket GetLastUpdatePacket()
		{
			return UpdatePackets.LastOrDefault();
		}

		/// <summary>
		/// Poped UpdatePackets need to be disposed after usage
		/// </summary>
		public ParsedUpdatePacket PopUpdatePacket()
		{
			var packet = UpdatePackets.LastOrDefault();
			UpdatePackets.RemoveAt(UpdatePackets.Count - 1);
			return packet;
		}

		/// <summary>
		/// Gets the last received update blocks of the given Type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public ICollection<UpdateBlock> GetUpdateBlocks(UpdateType type)
		{
			var blocks = new List<UpdateBlock>();
			foreach (var packet in UpdatePackets)
			{
				packet.GetBlocks(type, blocks);
			}
			return blocks;
		}

		/// <summary>
		/// Gets the last received update blocks of the object with the given id and of the given Type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public ICollection<UpdateBlock> GetUpdateBlocks(EntityId id, UpdateType type)
		{
			var blocks = new List<UpdateBlock>();
			foreach (var packet in UpdatePackets)
			{
				packet.GetBlocks(id, type, blocks);
			}
			return blocks;
		}

		public void PurgeUpdatePackets()
		{
			foreach (var packet in UpdatePackets)
			{
				packet.Dispose();
			}
			UpdatePackets.Clear();
		}

		public void PurgeSMSGs()
		{
			SMSGPackets.Clear();
		}

		public void PurgeCMSGs()
		{
			CMSGPackets.Clear();
		}
		#endregion

		#region Other Packets
		protected IDictionary<RealmServerOpCode, LockfreeQueue<RealmReceiveInfo>> SMSGPackets =
			new SynchronizedDictionary<RealmServerOpCode, LockfreeQueue<RealmReceiveInfo>>();

		protected IDictionary<RealmServerOpCode, LockfreeQueue<RealmReceiveInfo>> CMSGPackets =
			new SynchronizedDictionary<RealmServerOpCode, LockfreeQueue<RealmReceiveInfo>>();

		public RealmReceiveInfo DequeueSMSGInfo(RealmServerOpCode opCode)
		{
			Setup.WriteLine("Fetching packet " + opCode);
			//Assert.IsNotNull(FakePacketMgr.Instance[opCode], "You did not add a PacketHandler for: {0} - " +
			//    "To do so, simply call one of the Add*Handler(...) methods of this class during initialization.", opCode);

			LockfreeQueue<RealmReceiveInfo> queue;
			SMSGPackets.TryGetValue(opCode, out queue);
			if (queue != null)
			{
				var info = queue.TryDequeue();
				if (info == null)
				{
					var def = PacketAnalyzer.GetDefinition(opCode, PacketSender.Server);
					Assert.IsNotNull(def, "You did not define the packet-structure for OpCode {0}!", opCode);
				}
				return info;
			}
			return null;
		}

		public RealmReceiveInfo DequeueCMSGInfo(RealmServerOpCode opCode)
		{
			Setup.WriteLine("Fetching packet " + opCode);
			Assert.IsNotNull(RealmPacketMgr.Instance[opCode], "You did not add a PacketHandler for: {0} - " +
				"To do so, simply call one of the Add*Handler(...) methods of this class during initialization.", opCode);

			LockfreeQueue<RealmReceiveInfo> queue;
			CMSGPackets.TryGetValue(opCode, out queue);
			if (queue != null)
			{
				var info = queue.TryDequeue();
				if (info == null)
				{
					var def = PacketAnalyzer.GetDefinition(opCode, PacketSender.Client);
					Assert.IsNotNull(def, "You did not define the packet-structure for OpCode {0}!", opCode);
				}
				return info;
			}
			throw new InvalidOperationException("Packets are not cached!");
		}

		/// <summary>
		/// Dequeues and returns the last SMSG-Packet that had the given opcode.
		/// </summary>
		public ParsedSegment DequeueSMSG(RealmServerOpCode opCode)
		{
			var info = DequeueSMSGInfo(opCode);
			if (info != null)
			{
				return info.Parser.ParsedPacket;
			}
			return null;
		}

		/// <summary>
		/// Dequeues and returns the last CMSG-Packet that had the given opcode.
		/// </summary>
		public ParsedSegment DequeueCMSG(RealmServerOpCode opCode)
		{
			var info = DequeueCMSGInfo(opCode);
			if (info != null)
			{
				return info.Parser.ParsedPacket;
			}
			return null;
		}


		public void AddRealmSMSG(PacketIn packet)
		{
			if (PacketUtil.GetUpdatePackets &&
				(packet.PacketId == RealmServerOpCode.SMSG_UPDATE_OBJECT || 
				packet.PacketId == RealmServerOpCode.SMSG_COMPRESSED_UPDATE_OBJECT))
			{
				var parsedPacket = new ParsedUpdatePacket(packet);

				// check if all bytes were read - if not, the submitted update-count was too small
				//Assert.AreEqual(0, parsedPacket.RemainingLength, "Count in UpdatePacket does not match with actual packet-size");

				UpdatePackets.Add(parsedPacket);
			}
			else
			{
				AddRealmPacket(SMSGPackets, PacketSender.Server, packet, this);
			}
		}

		public void AddRealmCMSG(PacketIn packet)
		{
			AddRealmPacket(CMSGPackets, PacketSender.Client, packet, this);
		}

		static void AddRealmPacket(IDictionary<RealmServerOpCode, LockfreeQueue<RealmReceiveInfo>> packets,
			PacketSender sender, PacketIn packet, TestFakeClient client)
		{
			var opCode = (RealmServerOpCode)packet.PacketId.RawId;

			LockfreeQueue<RealmReceiveInfo> queue;
			if (!packets.TryGetValue(opCode, out queue))
			{
				packets.Add(opCode, queue = new LockfreeQueue<RealmReceiveInfo>());
			}

			var def = PacketAnalyzer.GetDefinition(packet.PacketId, sender);
			if (def != null)
			{
				if (packet is RealmPacketIn)
				{
					packet = ((DisposableRealmPacketIn) packet).Copy();
				}
				var parser = new PacketParser(packet, sender);
				parser.Parse();
				queue.Enqueue(new RealmReceiveInfo { Parser = parser, Client = client });

				//packet.IncrementUsage();
			}
		}
		#endregion
	}
}
