using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.World;
using WCell.RealmServer.RacesClasses;
using WCell.RealmServer.Factions;
using WCell.PacketAnalysis;
using WCell.Core;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Tests.Misc
{
	/// <summary>
	/// Summary description for WhoListTest
	/// </summary>
	[TestClass]
	public class WhoListTest : PacketUtil
	{
		private static TestCharacter _allianceChar1, _allianceChar2, _allianceChar3;
		private static TestCharacter _hordeChar1, _hordeChar2, _hordeChar3;

		public WhoListTest()
		{
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
		[ClassInitialize]
		public static void Init(TestContext testContext)
		{
			Setup.EnsureBasicSetup();
			FactionMgr.Initialize();
			ArchetypeMgr.EnsureInitialize();
		}

		[TestInitialize()]
		public void TestClassInitialize()
		{
			CharacterPool.RemoveAllChars();

			foreach (var chr in World.GetAllCharacters())
			{
				World.RemoveCharacter(chr);
			}

			_allianceChar1 = Setup.AllianceCharacterPool.Create();
			_allianceChar1.Class = WCell.Constants.ClassId.Priest;
			_allianceChar1.Race = WCell.Constants.RaceId.Draenei;
			_allianceChar1.SetName("AllianceChar1");
			_allianceChar1.Level = 1;
			_allianceChar1.EnsureInWorld();

			_allianceChar2 = Setup.AllianceCharacterPool.Create();
			_allianceChar2.Class = WCell.Constants.ClassId.Druid;
			_allianceChar2.Race = WCell.Constants.RaceId.NightElf;
			_allianceChar2.SetName("AllianceChar2");
			_allianceChar2.Level = 2;
			_allianceChar2.EnsureInWorld();

			_allianceChar3 = Setup.AllianceCharacterPool.Create();
			_allianceChar3.Class = WCell.Constants.ClassId.Paladin;
			_allianceChar3.Race = WCell.Constants.RaceId.Human;
			_allianceChar3.SetName("AllianceChar3");
			_allianceChar3.Level = 3;
			_allianceChar3.EnsureInWorld();

			_hordeChar1 = Setup.HordeCharacterPool.Create();
			_hordeChar1.Class = WCell.Constants.ClassId.Druid;
			_hordeChar1.Race = WCell.Constants.RaceId.Tauren;
			_hordeChar1.SetName("HordeChar1");
			_hordeChar1.Level = 1;
			_hordeChar1.EnsureInWorld();

			_hordeChar2 = Setup.HordeCharacterPool.Create();
			_hordeChar2.Class = WCell.Constants.ClassId.Paladin;
			_hordeChar2.Race = WCell.Constants.RaceId.BloodElf;
			_hordeChar2.SetName("HordeChar2");
			_hordeChar2.Level = 2;
			_hordeChar2.EnsureInWorld();

			_hordeChar3 = Setup.HordeCharacterPool.Create();
			_hordeChar3.Class = WCell.Constants.ClassId.Warlock;
			_hordeChar3.Race = WCell.Constants.RaceId.Undead;
			_hordeChar3.SetName("HordeChar3");
			_hordeChar3.Level = 3;
			_hordeChar3.EnsureInWorld();
		}

		[Owner("Nosferatus"), TestMethod]
		public void TestSameFaction()
		{
			ParsedSegment whoList = null;

			SendRequest(_allianceChar1, 0, 100, string.Empty, string.Empty, 
				RaceMask2.All, ClassMask2.All, new List<ZoneId>(), new List<string>());

			whoList = _allianceChar1.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_WHO);

			Assert.IsNotNull(whoList);
			Assert.AreEqual(3, whoList.SubSegments["Characters"].List.Count);
			AssertSameFaction(whoList, _allianceChar1.Faction.Group);

			SendRequest(_hordeChar1, 0, 100, string.Empty, string.Empty,
				RaceMask2.All, ClassMask2.All, new List<ZoneId>(), new List<string>());

			whoList = _hordeChar1.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_WHO);

			Assert.IsNotNull(whoList);
			Assert.AreEqual(3, whoList.SubSegments["Characters"].List.Count);
			AssertSameFaction(whoList, _hordeChar1.Faction.Group);
		}

		[Owner("Nosferatus"), TestMethod]
		public void TestRaceClass()
		{
			ParsedSegment whoList = null;
			RaceId raceId;
			ClassId classId;

			SendRequest(_allianceChar1, 0, 100, string.Empty, string.Empty,
				_allianceChar1.RaceMask2, _allianceChar1.ClassMask2, new List<ZoneId>(), new List<string>());

			whoList = _allianceChar1.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_WHO);

			Assert.IsNotNull(whoList);
			Assert.AreEqual(1, whoList.SubSegments["Characters"].List.Count);

			raceId = (RaceId)Enum.Parse(typeof(RaceId), whoList.SubSegments["Characters"].List[0]["Race"].Value.ToString(), true);
			Assert.AreEqual(raceId, _allianceChar1.Race);

			classId = (ClassId)Enum.Parse(typeof(ClassId), whoList.SubSegments["Characters"].List[0]["Class"].Value.ToString(), true);
			Assert.AreEqual(classId, _allianceChar1.Class);

			SendRequest(_hordeChar1, 0, 100, string.Empty, string.Empty,
				_hordeChar1.RaceMask2, _hordeChar1.ClassMask2, new List<ZoneId>(), new List<string>());

			whoList = _hordeChar1.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_WHO);

			Assert.IsNotNull(whoList);
			Assert.AreEqual(1, whoList.SubSegments["Characters"].List.Count);

			raceId = (RaceId)Enum.Parse(typeof(RaceId), whoList.SubSegments["Characters"].List[0]["Race"].Value.ToString(), true);
			Assert.AreEqual(raceId, _hordeChar1.Race);

			classId = (ClassId)Enum.Parse(typeof(ClassId), whoList.SubSegments["Characters"].List[0]["Class"].Value.ToString(), true);
			Assert.AreEqual(classId, _hordeChar1.Class);
		}

		[Owner("Nosferatus"), TestMethod]
		public void TestLevelQuery()
		{
			ParsedSegment whoList = null;

			SendRequest(_allianceChar1, 10, 20, string.Empty, string.Empty,
				RaceMask2.All, ClassMask2.All, new List<ZoneId>(), new List<string>());

			whoList = _allianceChar1.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_WHO);
			Assert.IsNotNull(whoList);
			Assert.AreEqual(0, whoList.SubSegments["Characters"].List.Count);

			SendRequest(_hordeChar1, 10, 20, string.Empty, string.Empty,
				RaceMask2.All, ClassMask2.All, new List<ZoneId>(), new List<string>());

			whoList = _hordeChar1.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_WHO);
			Assert.IsNotNull(whoList);
			Assert.AreEqual(0, whoList.SubSegments["Characters"].List.Count);

			SendRequest(_allianceChar1, 1, 1, string.Empty, string.Empty,
				RaceMask2.All, ClassMask2.All, new List<ZoneId>(), new List<string>());

			whoList = _allianceChar1.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_WHO);
			Assert.IsNotNull(whoList);
			Assert.AreEqual(1, whoList.SubSegments["Characters"].List.Count);
			Assert.AreEqual(1, Convert.ToInt32(whoList.SubSegments["Characters"].List[0]["Level"].Value.ToString()));

			SendRequest(_hordeChar1, 1, 1, string.Empty, string.Empty,
				RaceMask2.All, ClassMask2.All, new List<ZoneId>(), new List<string>());

			whoList = _hordeChar1.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_WHO);
			Assert.IsNotNull(whoList);
			Assert.AreEqual(1, whoList.SubSegments["Characters"].List.Count);
			Assert.AreEqual(1, Convert.ToInt32(whoList.SubSegments["Characters"].List[0]["Level"].Value.ToString()));
		}

		[Owner("Nosferatus"), TestMethod]
		public void TestNameQuery()
		{
			ParsedSegment whoList = null;

			SendRequest(_allianceChar1, 1, 100, "Horde", string.Empty,
				RaceMask2.All, ClassMask2.All, new List<ZoneId>(), new List<string>());

			whoList = _allianceChar1.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_WHO);
			Assert.IsNotNull(whoList);
			Assert.AreEqual(0, whoList.SubSegments["Characters"].List.Count);

			SendRequest(_hordeChar1, 1, 100, "Horde", string.Empty,
				RaceMask2.All, ClassMask2.All, new List<ZoneId>(), new List<string>());

			whoList = _hordeChar1.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_WHO);
			Assert.IsNotNull(whoList);
			Assert.AreEqual(3, whoList.SubSegments["Characters"].List.Count);

			SendRequest(_hordeChar1, 1, 100, "horde", string.Empty,
				RaceMask2.All, ClassMask2.All, new List<ZoneId>(), new List<string>());

			whoList = _hordeChar1.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_WHO);
			Assert.IsNotNull(whoList);
			Assert.AreEqual(3, whoList.SubSegments["Characters"].List.Count);

			SendRequest(_hordeChar1, 1, 100, "ord", string.Empty,
				RaceMask2.All, ClassMask2.All, new List<ZoneId>(), new List<string>());

			whoList = _hordeChar1.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_WHO);
			Assert.IsNotNull(whoList);
			Assert.AreEqual(3, whoList.SubSegments["Characters"].List.Count);

			SendRequest(_hordeChar1, 1, 100, "char1", string.Empty,
				RaceMask2.All, ClassMask2.All, new List<ZoneId>(), new List<string>());

			whoList = _hordeChar1.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_WHO);
			Assert.IsNotNull(whoList);
			Assert.AreEqual(1, whoList.SubSegments["Characters"].List.Count);
		}

		private static void AssertSameFaction(ParsedSegment whoList, FactionGroup factionGroup)
		{
			RaceId raceId;
			for (int i = 0; i < whoList.SubSegments["Characters"].List.Count; i++)
			{
				raceId = (RaceId)Enum.Parse(typeof(RaceId), whoList.SubSegments["Characters"].List[i]["Race"].Value.ToString(), true);
				Assert.AreEqual(factionGroup, ArchetypeMgr.GetRace(raceId).Faction.Group);
			}
		}

		public void SendRequest(TestCharacter sender, uint minLevel, uint maxLevel, string playerName, 
			string guildName, RaceMask2 raceMask, ClassMask2 classMask, List<ZoneId> zones, List<string> names)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.CMSG_WHO))
			{
				packet.WriteUInt(minLevel);
				packet.WriteUInt(maxLevel);
				packet.WriteCString(playerName);
				packet.WriteCString(guildName);
				packet.WriteUInt((uint)raceMask);
				packet.WriteUInt((uint)classMask);

				packet.WriteUInt(zones.Count);
				foreach (ZoneId zone in zones)
					packet.WriteUInt((uint)zone);

				packet.WriteUInt(names.Count);
				foreach (string name in names)
					packet.WriteCString(name);

				sender.FakeClient.ReceiveCMSG(packet, true);
			}
		}
	}
}