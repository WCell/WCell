using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.Constants.Spells;
using WCell.RealmServer.GameObjects;
using WCell.Constants;
using System.Threading;
using WCell.Core;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Tests.Entities
{
	/// <summary>
	/// Summary description for DuelTest
	/// </summary>
	[TestClass]
	public class DuelTest
	{
		public DuelTest()
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
		public void TestDuel()
		{
			Setup.EnsureBasicSetup();

			var chr1 = Setup.AllianceCharacterPool.Create();
			chr1.EnsureAloneInWorldAndLiving();

			var chr2 = Setup.AllianceCharacterPool.Create();
			chr2.EnsureInWorldAndLiving();
			chr2.EnsureXDistance(chr1, 2, true);

			Assert.IsNotNull(chr1.Region);
			Assert.AreEqual(chr1.Region, chr2.Region);
			Assert.AreEqual(2, chr1.Region.CharacterCount);

			chr1.Region.ForceUpdateCharacters();

			Assert.IsTrue(chr1.KnowsOf(chr2));
			Assert.IsFalse(chr1.CanHarm(chr2));
			Assert.IsFalse(chr2.CanHarm(chr1));

			GOMgr.LoadAll();

			// chr1 requests a duel with chr2
			using (var packet = new RealmPacketOut(RealmServerOpCode.CMSG_CAST_SPELL))
			{
				packet.Write((byte)1);
				packet.Write((uint)SpellId.NotDisplayedDuel);
				packet.Write((byte)0);
				packet.Write((uint)SpellTargetFlags.Unit);
				chr2.EntityId.WritePacked(packet);

				chr1.FakeClient.ReceiveCMSG(packet, false, true);
			}

			chr1.Region.WaitTicks(3);

			Assert.IsFalse(chr1.IsUsingSpell, "Character is still casting Duel Spell.");

			var spellFailure = chr1.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_SPELL_FAILURE);
			Assert.IsNull(spellFailure, "Could not start duel - {0} ({1})",
				spellFailure != null ? spellFailure["Spell"] : null,
				spellFailure != null ? spellFailure["FailReason"] : null);

			// server sent duel request to opposing party
			var request = chr2.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_DUEL_REQUESTED);
			Assert.IsNotNull(request);

		    request = chr1.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_DUEL_REQUESTED);
            Assert.IsNotNull(request);

			var duel = chr1.Duel;
			Assert.IsNotNull(duel);
			Assert.AreEqual(duel, chr2.Duel);

			Assert.AreEqual(chr1.DuelOpponent, chr2);
			Assert.AreEqual(chr2.DuelOpponent, chr1);

			var flag = duel.Flag;
			Assert.IsNotNull(flag);
			Assert.AreEqual(chr1.Region, duel.Flag.Region);

			var region = chr1.Region;

			Assert.IsFalse(duel.IsActive);

			Assert.AreEqual(duel.Flag.EntityId, request["FlagId"].Value);
			Assert.AreEqual(chr1.EntityId, request["ChallengerId"].Value);

			// opponent accepts duel
			using (var packet = new RealmPacketOut(RealmServerOpCode.CMSG_DUEL_ACCEPTED))
			{
				chr2.FakeClient.ReceiveCMSG(packet, false, true);
			}

			Assert.IsFalse(duel.IsActive);
			if (Duel.DefaultStartDelay > 0)
			{
				Asser.InBetween(0.001f, Duel.DefaultStartDelay, duel.StartDelay);
				// lets speed it up
				duel.StartDelay = 0.00001f;
			}

			region.WaitTicks(2);

			// duel started
			Assert.IsTrue(duel.IsActive);
			Assert.IsTrue(chr1.CanHarm(chr2));
			Assert.IsTrue(chr2.CanHarm(chr1));

			region.AddMessage(() => {
				// finish duel by having chr1 knockout chr2
				duel.Finish(DuelWin.Knockout, chr2);

				var winMsg = chr2.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_DUEL_WINNER);
				Assert.AreEqual(DuelWin.Knockout, winMsg["Win"].Value);
				Assert.AreEqual(chr1.Name, winMsg["Winner"].Value);
				Assert.AreEqual(chr2.Name, winMsg["Looser"].Value);

				// duel ended
				Assert.IsNull(flag.Region);

				Assert.IsNull(chr1.DuelOpponent);
				Assert.IsNull(chr1.Duel);
				Assert.IsNull(chr2.DuelOpponent);
				Assert.IsNull(chr2.Duel);

				Assert.IsFalse(chr1.CanHarm(chr2));
				Assert.IsFalse(chr2.CanHarm(chr1));
			});
		}
	}
}
