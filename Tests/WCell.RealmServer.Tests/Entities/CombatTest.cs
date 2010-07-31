using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.Core;
using WCell.RealmServer.Items;
using WCell.RealmServer.Tests.Misc;
using WCell.Constants;
using WCell.RealmServer.Modifiers;
using System.Threading;
using WCell.RealmServer.Entities;
using WCell.Constants.Updates;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Tests.Entities
{
	/// <summary>
	/// Summary description for CharacterCombatTest
	/// </summary>
	[TestClass]
	public class CombatTest
	{
		public CombatTest()
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

		[ClassInitialize]
		public static void Initialize(TestContext testContext)
		{
			Setup.EnsureMinimalSetup();
		}

		[TestMethod]
		public void TestSimpleAttack()
		{
			PacketUtil.GetUpdatePackets = true;

			var damage = 30; // do 30 damage
			var damages = GenericWeapon.Fists.Damages;
			damages[0].Minimum = damages[0].Maximum = damage;

			var chr = Setup.AllianceCharacterPool.Create();
			chr.EnsureAloneInWorld();
			var client = chr.FakeClient;
			try
			{
				var npc = Setup.NPCPool.CreateDummy();
				npc.SetBaseResistance(DamageSchool.Physical, 0);

				npc.EnsureInWorldAndLiving();

				chr.EnsureLiving();
				chr.EnsureXDistance(npc, 2f, true);
				chr.EnsureFacing(npc);

				var health = 1000;

				npc.Region.AddMessageAndWait(() => {
					chr.Target = npc;
					chr.IsInCombat = true;
					chr.IsFighting = true;

					npc.BaseHealth = chr.BaseHealth = npc.Health = chr.Health = health;
					npc.Power = (int)npc.MaxPower;
					chr.Regenerates = npc.Regenerates = false;
					// don't regenerate to avoid other update blocks bothering 

					Assert.AreEqual(health, npc.Health);

					Assert.IsTrue(chr.CanHarm(npc));
				});

				AttackHandler dmgHandler = null;
				int amount = 0;
				dmgHandler = (IDamageAction action) => {
					amount = action.ActualDamage;

					// make sure, there are no other update blocks distracting
					client.PurgeUpdatePackets();

					// stop fighting
					action.Attacker.IsInCombat = false;
					action.Victim.IsInCombat = false;

					lock (dmgHandler)
					{
						Monitor.PulseAll(dmgHandler);
					}
				};
				npc.Entry.HitReceived += dmgHandler;

				lock (dmgHandler)
				{
					// wait until damage has been caused
					Monitor.Wait(dmgHandler);
				}

				Assert.AreEqual(damage, amount);

				npc.Entry.HitReceived -= dmgHandler;

				Thread.Sleep(Region.CharacterUpdateEnvironmentTicks * Region.DefaultUpdateDelay);

				// wait 2 ticks
				npc.Region.WaitTicks(3);

				// get the damage packet
				var damagePacket = client.DequeueSMSG(RealmServerOpCode.SMSG_ATTACKERSTATEUPDATE);
				Assert.IsNotNull(damagePacket);
				Assert.AreEqual((uint)damage, damagePacket["TotalDamage"].UIntValue);


				// get the NPC's last update blocks (should only contain the single Health update)
				var valueBlocks = client.GetUpdateBlocks(npc.EntityId, UpdateType.Values);
				Assert.AreEqual(1, valueBlocks.Count);

				var healthUpdate = valueBlocks.First();
				Assert.AreEqual(health - damage, healthUpdate.GetInt(UnitFields.HEALTH));
			}
			finally
			{
				client.PurgeUpdatePackets();
			}
		}
	}
}