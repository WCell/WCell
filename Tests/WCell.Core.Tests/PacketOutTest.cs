using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.Constants;
using WCell.Core.Network;

namespace WCell.Core.Tests
{
	public class TestPacketOut : PacketOut
	{
		public TestPacketOut(PacketId id, int capcity)
			: base(id, capcity)
		{
			Position += 2;
			WriteUShort(id.RawId);
		}

		public override int HeaderSize
		{
			get { return 4; }
		}
	}

	/// <summary>
	/// Summary description for PacketOutTest
	/// </summary>
	[TestClass]
	public class PacketOutTest
	{
		public PacketOutTest()
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
		//public void TestFetchPacketContent()
		//{
		//    using (var packet = new TestPacketOut(RealmServerOpCode.CMSG_1006, 10))
		//    {
		//        var bytes = new byte[] { 1, 2, 3 };
		//        packet.Write(bytes);

		//        var output = packet.Segment;

		//        for (int i = 0; i < bytes.Length; i++)
		//        {
		//            var index = i + packet.HeaderSize;
		//            Assert.AreEqual(bytes[i], output[index]);
		//        }
		//    }
		//}
	}
}