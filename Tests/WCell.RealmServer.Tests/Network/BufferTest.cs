using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Cell.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.Core.Network;
using WCell.Constants;
using WCell.RealmServer.UpdateFields;

namespace WCell.RealmServer.Tests.Network
{
	/// <summary>
	/// Summary description for BufferTest
	/// </summary>
	[TestClass]
	public class BufferTest
	{
		public BufferTest()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
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

		[TestMethod]
		public void TestBufferManagerEtc()
		{
			var mgr = new BufferManager(2, 128);

			var segment1 = mgr.CheckOut();	// 1st buffer
			Assert.AreEqual(1, mgr.AvailableSegmentsCount);

			var segment2 = mgr.CheckOut();	// 1st buffer
			Assert.AreEqual(1, mgr.TotalBufferCount);
			Assert.AreEqual(0, mgr.AvailableSegmentsCount);
			//Assert.AreEqual(2, segment2.Buffer.UsedSegmentCount);

			var segment3 = mgr.CheckOut(); // 2nd buffer
			Assert.AreEqual(2, mgr.TotalBufferCount);
			Assert.AreEqual(1, mgr.AvailableSegmentsCount);
			//Assert.AreEqual(1, segment3.Buffer.UsedSegmentCount);

			segment3.DecrementUsage();
			Assert.AreEqual(2, mgr.AvailableSegmentsCount);
			//Assert.AreEqual(0, segment3.Buffer.UsedSegmentCount);

			using (var outPacket = new RealmPacketOut(RealmServerOpCode.CMSG_ACCEPT_TRADE))
			{
				outPacket.Position = outPacket.HeaderSize;
				outPacket.Write(1);
				outPacket.WriteCString("abc");

				var inPacket = DisposableRealmPacketIn.CreateFromOutPacket(segment1, outPacket);
				Assert.AreEqual(1, inPacket.ReadInt32());
				Assert.AreEqual("abc", inPacket.ReadCString());
			}

			using (var outPacket = new RealmPacketOut(RealmServerOpCode.CMSG_ACCEPT_TRADE))
			{
				outPacket.Position = outPacket.HeaderSize;
				outPacket.WriteCString("def");
				outPacket.Write(2);

				var inPacket = DisposableRealmPacketIn.CreateFromOutPacket(segment2, outPacket);
				Assert.AreEqual("def", inPacket.ReadCString());
				Assert.AreEqual(2, inPacket.ReadInt32());
			}
		}


		[TestMethod]
		public void TestUpdatePacket()
		{
			var mgr = new BufferManager(2, 128);

			var segment1 = mgr.CheckOut();	// 1st buffer
			Assert.AreEqual(1, mgr.AvailableSegmentsCount);

			var segment2 = mgr.CheckOut();	// 1st buffer
			Assert.AreEqual(1, mgr.TotalBufferCount);
			Assert.AreEqual(0, mgr.AvailableSegmentsCount);
			//Assert.AreEqual(2, segment2.Buffer.UsedSegmentCount);

			var segment3 = mgr.CheckOut(); // 2nd buffer
			Assert.AreEqual(2, mgr.TotalBufferCount);
			Assert.AreEqual(1, mgr.AvailableSegmentsCount);
			//Assert.AreEqual(1, segment3.Buffer.UsedSegmentCount);

			segment3.DecrementUsage();
			Assert.AreEqual(2, mgr.AvailableSegmentsCount);
			//Assert.AreEqual(0, segment3.Buffer.UsedSegmentCount);

			//using (var outPacket = new UpdatePacket(segment2))
			using (var outPacket = new UpdatePacket(128))
			{
				Assert.AreEqual(outPacket.PacketId, (PacketId)RealmServerOpCode.SMSG_UPDATE_OBJECT);

				outPacket.Write(15);
				outPacket.WriteCString("abc");

				using (var inPacket = DisposableRealmPacketIn.CreateFromOutPacket(segment1, outPacket))
				{
					Assert.AreEqual(0, inPacket.ReadInt32());			//update count

					Assert.AreEqual(15, inPacket.ReadInt32());
					Assert.AreEqual("abc", inPacket.ReadCString());
				}

				outPacket.Reset();
				Assert.AreEqual(outPacket.PacketId, (PacketId)RealmServerOpCode.SMSG_UPDATE_OBJECT);

				outPacket.WriteCString("def");
				outPacket.WriteUShortBE(300);


				using (var inPacket = DisposableRealmPacketIn.CreateFromOutPacket(segment1, outPacket))
				{
					Assert.AreEqual(0, inPacket.ReadInt32());			//update count
					Assert.AreEqual((byte)0, inPacket.ReadByte());			//block count

					Assert.AreEqual("def", inPacket.ReadCString());
					Assert.AreEqual((ushort)300, inPacket.ReadUInt16BE());
				}
			}
		}
	}
}