using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Cell.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.Core.Network;
using WCell.Core.Cryptography;
using WCell.RealmServer.Network;
using WCell.Core.Initialization;
using WCell.RealmServer.Tests.Misc;
using WCell.Constants;

namespace WCell.RealmServer.Tests.Network
{
	/// <summary>
	/// Test for PacketIn.
	/// </summary>
	[TestClass]
	public class FakeClientTest
	{
		static TestCharacter m_chr;

		public FakeClientTest()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		private TestContext testContextInstance;
		static int receivedPackets;
		static int sentPackets;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get { return testContextInstance; }
			set { testContextInstance = value; }
		}

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[ClassInitialize()]
		public static void Initialize(TestContext testContext)
		{
			// register the test packet handlers
			FakePacketMgr.Instance.Register(typeof(FakeClientTest));
			RealmPacketMgr.Instance.Register(typeof(FakeClientTest));
			
			Setup.EnsureBasicSetup();

			m_chr = Setup.DefaultCharacter;
			m_chr.EnsureInWorld();
		}

		[TestMethod]
		public void TestPacketHandling()
		{
			Assert.IsNotNull(m_chr);
			Assert.IsNotNull(RealmPacketMgr.Instance[RealmServerOpCode.SMSG_DBLOOKUP]);
			RealmPacketMgr.Instance[RealmServerOpCode.SMSG_DBLOOKUP].RequiresLogIn = false;

			var packet = new RealmPacketOut(RealmServerOpCode.SMSG_DBLOOKUP);
			packet.Write(true);
			packet.Write("abc");
			packet.WriteUInt(345);
			packet.WriteByte(0xFF);

			var packets = receivedPackets;
			m_chr.FakeClient.ReceiveCMSG(packet, false, true);
			m_chr.FakeClient.ReceiveCMSG(packet, false, true);
			m_chr.FakeClient.ReceiveCMSG(packet, false, true);
			m_chr.FakeClient.ReceiveCMSG(packet, false, true);
			packet.Close();
			Assert.AreEqual(packets + 4, receivedPackets);
		}

		[TestMethod]
		public void TestPacketSending()
		{
			Assert.IsNotNull(m_chr);
			Assert.IsNotNull(FakePacketMgr.Instance[RealmServerOpCode.CMSG_BOOTME]);
			FakePacketMgr.Instance[RealmServerOpCode.CMSG_BOOTME].RequiresLogIn = false;

			var packet = new RealmPacketOut(RealmServerOpCode.CMSG_BOOTME);
			packet.Write(true);
			packet.Write("abc");
			packet.WriteUInt(345);
			packet.WriteByte(0xFF);

			var packets = sentPackets;
			m_chr.FakeClient.SendAndWait(packet, false);
			m_chr.FakeClient.SendAndWait(packet, false);
			m_chr.FakeClient.SendAndWait(packet, false);
			m_chr.FakeClient.SendAndWait(packet, false);
			packet.Close();
			Assert.AreEqual(packets + 4, sentPackets);
		}

		#region Server PacketHandlers
		// override some packet handler for testing purposes
		[PacketHandler(RealmServerOpCode.SMSG_DBLOOKUP)]
		public static void Handle1042(IRealmClient client, RealmPacketIn packet)
		{
			Assert.AreEqual(packet.ReadBoolean(), true);
			Assert.AreEqual(packet.ReadCString(), "abc");
			Assert.AreEqual(packet.ReadUInt32(), (uint)345);
			Assert.AreEqual(packet.ReadByte(), 0xFF);

			receivedPackets++;
		}
		#endregion

		#region Client PacketHandlers
		/// <summary>
		/// The client received the given packet
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_BOOTME, IsGamePacket = false)]
		public static void HandleAddFriend(IRealmClient client, RealmPacketIn packet)
		{
			Assert.AreEqual(packet.ReadBoolean(), true);
			Assert.AreEqual(packet.ReadCString(), "abc");
			Assert.AreEqual(packet.ReadUInt32(), (uint)345);
			Assert.AreEqual(packet.ReadByte(), 0xFF);

			sentPackets++;
		}
		#endregion
	}
}