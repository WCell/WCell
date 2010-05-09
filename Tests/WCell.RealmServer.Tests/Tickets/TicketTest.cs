using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.Constants;
using WCell.Constants.Tickets;

namespace WCell.RealmServer.Tests.Tickets
{
	/// <summary>
	/// Summary description for TicketTest
	/// </summary>
	[TestClass]
	public class TicketTest
	{
		public TicketTest()
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
		public void TestTickets()
		{
			var ticketIssuerCount = 10;
			var chars = Setup.AllianceCharacterPool.AddCharacters(ticketIssuerCount);

			foreach (var chr in chars)
			{
				using (var packet = new RealmPacketOut(RealmServerOpCode.CMSG_GMTICKET_CREATE))
				{
					packet.Write((uint)TicketType.Harassment);
					packet.Write((byte)0);
					packet.Write(chr.Position.X);
					packet.Write(chr.Position.Y);
					packet.Write(chr.Position.Z);
					packet.Write(chr.Name + " has a problem!");
					chr.FakeClient.ReceiveCMSG(packet, true);
				}
			}
		}
	}
}
