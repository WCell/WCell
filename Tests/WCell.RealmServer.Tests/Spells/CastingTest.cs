using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.RealmServer.Tests.Misc;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Util.Threading;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Tests.Tools;

namespace WCell.RealmServer.Tests.Spells
{
	/// <summary>
	/// Summary description for CastingTest
	/// </summary>
	[TestClass]
	public class CastingTest : PacketUtil
	{
		private static TestCharacter chr;

		public CastingTest()
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
			get { return testContextInstance; }
			set { testContextInstance = value; }
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
			chr = Setup.AllianceCharacterPool.Create();
		}

		#region Auras

		//[TestMethod]
		public void TestSimpleBuff()
		{
			var spell = SpellHandler.Get(SpellId.ClassSkillPowerWordFortitudeRank1);

			ApplyAura(chr, spell);
		}

		//[TestMethod]
		public void TestBattleStance()
		{
			var spell = SpellHandler.Get(SpellId.ClassSkillBattleStance);

			ApplyAura(chr, spell);
		}

		#endregion

		#region Instant Damage

		[TestMethod]
		public void TestInstantDamageSpell()
		{
			var spell = SpellHandler.Get(SpellId.ClassSkillFireBlastRank1);

			Assert.AreEqual(0, spell.ProjectileSpeed); // not delayed

			chr.EnsureInWorld();

			var enemy = chr.Enemies.Create();
			enemy.Regenerates = false;

			var oldHealth = enemy.Health;
			Asser.GreaterThan(enemy.Health, 0);

			// make sure we are in range and facing the enemy
			enemy.EnsureXDistance(chr, spell.Range.MinDist + 0.5f, true);
			chr.EnsureFacing(enemy);

			// triggered spells are instant and cannot miss
			chr.Region.AddMessageAndWait(() => {
				var failReason = chr.SpellCast.Start(spell, true, enemy);
				Assert.AreEqual(SpellFailedReason.Ok, failReason);

				Assert.IsFalse(chr.SpellCast.IsCasting);

				Asser.GreaterThan(oldHealth, enemy.Health);
			});
		}

		#endregion

		#region Battlestance

		#endregion

		#region ApplyAura

		public static void ApplyAura(TestCharacter chr, Spell spell)
		{
			Assert.IsTrue(spell.IsAura || spell.IsAreaAura, "Spell {0} is not an Aura", spell);

			chr.EnsureInWorld();

			chr.ShapeShiftForm = ShapeShiftForm.Normal;
			chr.Auras.Clear();

			Assert.AreEqual(0, chr.Auras.Count);

			// important: Execute this in the Region's thread
			chr.Region.AddMessageAndWait(new Message(() => {
				chr.SpellCast.TriggerSelf(spell);

				var failure =
					chr.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_SPELL_FAILURE);

				Assert.IsNull(failure,
							  failure != null
								? "Spell failed: " + failure["FailReason"].Value
								: "");

				Assert.AreEqual(1, chr.Auras.Count, "Aura was not added.");
				var aura = chr.Auras[spell, !spell.HasHarmfulEffects];

				Assert.IsNotNull(aura);

				Assert.AreEqual(spell.GetDuration(chr), (uint)aura.Duration);
				Assert.AreNotEqual(0, spell.GetDuration(chr));
				Asser.GreaterOrEqual(spell.GetDuration(chr), (uint)aura.TimeLeft);

				aura.Cancel();

				Assert.IsNull(chr.Auras[spell, !spell.HasHarmfulEffects]);
				Assert.AreEqual(0, chr.Auras.Count);
			}));
		}

		#endregion

		#region Pushback

		[TestMethod]
		public void TestPushback()
		{
			Setup.EnsureMinimalSetup();
			SpellHandler.LoadSpells();

			var npc = Setup.NPCPool.CreateDummy();

			npc.EnsureInWorldAndLiving();

			var spell = SpellHandler.Get(SpellId.ClassSkillPrayerOfHealingRank1);
			Assert.IsNotNull(spell);
			Asser.GreaterThan(spell.CastDelay, 0u);

			npc.Region.AddMessageAndWait(new Message(() => {
				var cast = npc.SpellCast;
				var err = cast.Start(spell, false, npc);

				Assert.AreEqual(err, SpellFailedReason.Ok);
				Assert.IsTrue(npc.IsUsingSpell);

				var timeleft = cast.TimeLeft;
				cast.TimeLeft -= 2000;
				Asser.Approx(timeleft - 2000, 40, cast.TimeLeft);
				timeleft = cast.TimeLeft;

				cast.Pushback();
				Asser.Approx(timeleft + 1000, 40, cast.TimeLeft);
				timeleft = cast.TimeLeft;

				cast.Pushback();
				Asser.Approx(timeleft + 800, 40, cast.TimeLeft);
			}));
		}

		#endregion
	}
}