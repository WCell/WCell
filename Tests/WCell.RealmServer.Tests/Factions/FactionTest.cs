using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Factions;
using WCell.Constants.Factions;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Tests.Factions
{
	/// <summary>
	/// Summary description for FactionTest
	/// </summary>
	[TestClass]
	public class FactionTest
	{
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
		public void TestInitialReputations()
		{
			Setup.EnsureDBSetup();
			FactionMgr.Initialize();

		    var stormWind = FactionMgr.Get(FactionId.Stormwind);
			var bootyBay = FactionMgr.Get(FactionId.BootyBay);

			Assert.IsNotNull(bootyBay);

		    var reputation = new Reputation(new ReputationRecord(), bootyBay, 1000, ReputationFlags.None);

			Asser.FlatNotSet(reputation.Flags, ReputationFlags.AtWar);
		}
	}
}