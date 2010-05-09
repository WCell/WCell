using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Tests.Misc;
using WCell.RealmServer.Network;
using WCell.Core.Network;
using WCell.RealmServer.Tests.Network;
using WCell.PacketAnalysis;
using WCell.RealmServer.Database;
using WCell.Constants;

namespace WCell.RealmServer.Tests.Login
{
	/// <summary>
	/// Summary description for LoginSequence
	/// </summary>
	[TestClass]
	public class LoginSequenceTest : PacketUtil
	{
		public LoginSequenceTest()
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

		[ClassInitialize()]
		public static void Initialize(TestContext testContext)
		{
			Setup.EnsureDBSetup();
		}

		[TestMethod]
		public void TestCharEnum()
		{
			var account = Setup.AccountPool.CreateAccount();
			var client = new TestFakeClient(account);

			var record1 = account.AddRecord();
			var record2 = account.AddRecord();

			CharacterHandler.SendCharEnum(client);

			var parser = client.DequeueSMSGInfo(RealmServerOpCode.SMSG_CHAR_ENUM).Parser;

			CheckCharEnumSize(parser, account.Characters);

			var charEnum = parser.ParsedPacket;
			Assert.IsNotNull(charEnum);

			var chars = charEnum["Characters"];

			var char1 = chars[0];
			var char2 = chars[1];

			Assert.AreEqual(char1["Id"].EntityIdValue, record1.EntityId);
			Assert.AreEqual(char2["Id"].EntityIdValue, record2.EntityId);

		}

		/// <summary>
		/// Outdated, since strucutre changes too often - need to use PA to check actual size
		/// </summary>
		/// <param name="charEnumParser"></param>
		/// <param name="chars"></param>
		public static void CheckCharEnumSize(PacketParser charEnumParser, IEnumerable<CharacterRecord> chars)
		{
			//var totalLength = 1;			// char count
			//foreach (var record in chars)
			//{
			//    totalLength += record.Name.Length + 239;
			//}
			//Assert.AreEqual(totalLength, charEnumParser.Packet.ContentLength);
		}
	}
}
