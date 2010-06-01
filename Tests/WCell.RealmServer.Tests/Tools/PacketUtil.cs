using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Network;
using WCell.Core.Network;
using WCell.PacketAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.PacketAnalysis.Updates;
using WCell.Util.Collections;
using WCell.RealmServer.Tests.Network;
using WCell.Constants;

namespace WCell.RealmServer.Tests.Misc
{
	public class PacketUtil
	{
		static bool initialized;
		static bool getUpdatePackets;


		static Action<IRealmClient, RealmPacketIn> updatePacketHandler = (client, packet) => {
			var fakeClient = (TestFakeClient)client;
			ParsedUpdatePacket parsedPacket = new ParsedUpdatePacket(packet);

			// check if all bytes were read - if not, the submitted update-count was too small
			Assert.AreEqual(0, parsedPacket.RemainingLength, "Count in UpdatePacket does not match with actual packet-size");

			fakeClient.UpdatePackets.Add(parsedPacket);
		};

		/// <summary>
		/// Whether to fetch and parse UpdatePackets
		/// </summary>
		public static bool GetUpdatePackets
		{
			get
			{
				return getUpdatePackets;
			}
			set
			{
				if (value != getUpdatePackets)
				{
					getUpdatePackets = value;
					//if (value)
					//{
					//    FakePacketMgr.Instance.Register(RealmServerOpCode.SMSG_UPDATE_OBJECT, updatePacketHandler, true);
					//    FakePacketMgr.Instance.Register(RealmServerOpCode.SMSG_COMPRESSED_UPDATE_OBJECT, updatePacketHandler, true);
					//}
					//else
					//{
					//    FakePacketMgr.Instance.Unregister(RealmServerOpCode.SMSG_UPDATE_OBJECT);
					//    FakePacketMgr.Instance.Unregister(RealmServerOpCode.SMSG_COMPRESSED_UPDATE_OBJECT);
					//}
				}
			}
		}

		public PacketUtil()
		{
			if (!initialized)
			{
				initialized = true;
				FakePacketMgr.Instance.Register(GetType());
				RealmPacketMgr.Instance.Register(GetType());
			}
		}
	}

	public class RealmReceiveInfo
	{
		public PacketParser Parser;
		public TestFakeClient Client;
	}
}
