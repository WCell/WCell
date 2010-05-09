using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Cell.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.Core.Network;
using System.Net;

namespace WCell.RealmServer.Tests.Network
{
	/// <summary>
	/// Summary description for PacketInTest
	/// </summary>
	[TestClass]
	public class PacketInTest
	{

		public PacketInTest()
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


		//[TestMethod]
		//public void TestBigEndian()
		//{
		//    var segment = SmallBuffers.CheckOut();
		//    var bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
		//    using (var packet = new DisposableRealmPacketIn(segment))
		//    {
		//        packet.Initialize();

		//        var pos = packet.Position;

		//        var origShort1 = packet.ReadInt16();
		//        var short1 = IPAddress.NetworkToHostOrder(origShort1);

		//        packet.Position = pos;

		//        var short2 = (short)packet.ReadUInt16BE();

		//        Assert.AreNotEqual(0, short1);
		//        Assert.AreEqual(short1, short2);
		//    }
		//}
	}
}
