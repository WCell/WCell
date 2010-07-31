using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using WCell.Constants.Misc;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Tests.PacketAnalysis
{
	using PacketAnalyzer = WCell.PacketAnalysis.PacketAnalyzer;
	using WCell.PacketAnalysis.Xml;
	using WCell.PacketAnalysis;
	using WCell.Core;
	using WCell.Constants;
	using WCell.Core.Network;
	using WCell.RealmServer.Debugging;
	using WCell.RealmServer.Spells;
	using WCell.Constants.Spells;

	/// <summary>
	/// Summary description for PacketAnalyzer
	/// </summary>
	[TestClass]
	public class PacketAnalyzerTest
	{
		public PacketAnalyzerTest()
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
			Setup.EnsureMinimalSetup();
			DebugUtil.Init();
		}

		[TestMethod]
		public void TestConditions()
		{
			PacketSegmentStructure spellAttributes, chatType, someNumber;
			SwitchPacketSegmentStructure swtch;
			SwitchCase cond, cond2, cond3, cond4;
			var def = new PacketDefinition(RealmServerOpCode.CMSG_SET_FACTION_INACTIVE, PacketSender.Client,
				spellAttributes = new PacketSegmentStructure(SimpleType.UInt, "SpellAttributes", typeof(SpellAttributes)),
				chatType = new PacketSegmentStructure(SimpleType.UInt, "ChatType", typeof(ChatMsgType)),
				someNumber = new PacketSegmentStructure(SimpleType.Int, "SomeNumber"),

				swtch = new SwitchPacketSegmentStructure("some switch", chatType,
					cond = new SwitchCase(ComparisonType.Equal, ChatMsgType.Say,
						new PacketSegmentStructure(SimpleType.CString, "Message")
					)
				),
				new SwitchPacketSegmentStructure("Mathmatical comparisons", someNumber,
					cond2 = new SwitchCase(ComparisonType.GreaterOrEqual, 300,
						new PacketSegmentStructure(SimpleType.CString, "Message")
					)),
				new SwitchPacketSegmentStructure("Flag Switch", spellAttributes,
					cond3 = new SwitchCase(ComparisonType.And, "Passive | Ranged",
						new PacketSegmentStructure(SimpleType.CString, "Something Else")
					),
					cond4 = new SwitchCase(ComparisonType.AndNot, "OnNextMelee",
						new PacketSegmentStructure(SimpleType.CString, "Meleestuff")
					)
				)
			);

			// basic structure
			Assert.AreEqual(6, ((ComplexPacketSegmentStructure)def.Structure).Segments.Count);

			Assert.AreEqual(1, swtch.Cases.Count);
			Assert.AreEqual(cond, swtch.Cases[0]);

			def.Init();

			// conditions
			Assert.IsFalse(cond.Matches((ChatMsgType)123));
			Assert.AreEqual(cond.Value, ChatMsgType.Say);
			Assert.IsTrue(cond.Matches(ChatMsgType.Say));

			Assert.IsFalse(cond2.Matches(123));
			Assert.IsTrue(cond2.Matches(300));
			Assert.IsTrue(cond2.Matches(3000));

			Assert.IsTrue(cond3.Matches(SpellAttributes.Passive | SpellAttributes.Ranged | SpellAttributes.CannotBeCastInCombat));
			Assert.IsFalse(cond3.Matches(SpellAttributes.StartCooldownAfterEffectFade));

			Assert.IsTrue(cond4.Matches(SpellAttributes.Passive));
			Assert.IsFalse(cond4.Matches(SpellAttributes.Passive | SpellAttributes.OnNextMelee));
			Assert.IsFalse(cond4.Matches(SpellAttributes.OnNextMelee));


		}

		[TestMethod]
		public void TestEmptyStructure()
		{
			var def = new PacketDefinition(RealmServerOpCode.CMSG_SET_CHANNEL_WATCH, PacketSender.Client, new List<PacketSegmentStructure>());
			def.Init();

			using (var packet = new RealmPacketOut(RealmServerOpCode.CMSG_PLAYER_AI_CHEAT))
			{
				packet.WriteUInt((uint)SpellAttributes.CannotRemove);
				packet.WriteByte(0);

				using (var packetIn = DisposableRealmPacketIn.CreateFromOutPacket(packet))
				{
					var parsedPacket = PacketParser.Parse(packetIn, PacketSender.Client, def);
					Assert.AreEqual(0, parsedPacket.SubSegments.Count);
				}
			}
		}

		[TestMethod]
		public void TestSavingAndLoading()
		{
			var def = new PacketDefinition(RealmServerOpCode.CMSG_SET_CHANNEL_WATCH, PacketSender.Client, new List<PacketSegmentStructure>());
			PacketAnalyzer.RegisterDefintion(def);

			var def2 = PacketAnalyzer.GetDefinition(RealmServerOpCode.CMSG_SET_CHANNEL_WATCH, PacketSender.Client);
			Assert.AreEqual(def, def2);


			def = new PacketDefinition(RealmServerOpCode.CMSG_SET_CHANNEL_WATCH, PacketSender.Any, new List<PacketSegmentStructure>());
			PacketAnalyzer.RegisterDefintion(def);

			def2 = PacketAnalyzer.GetDefinition(RealmServerOpCode.CMSG_SET_CHANNEL_WATCH, PacketSender.Server);
			Assert.AreEqual(def, def2);
		}

		[TestMethod]
		public void TestParsedSegment()
		{
			PacketDefinition def = new PacketDefinition(RealmServerOpCode.CMSG_PLAYER_AI_CHEAT, PacketSender.Client,
				new PacketSegmentStructure(SimpleType.UInt, "SpellAttributes", typeof(SpellAttributes)),
				new ListPacketSegmentStructure(SimpleType.Byte, "Members",
					new PacketSegmentStructure(SimpleType.Guid, "MemberId"),
					new PacketSegmentStructure(SimpleType.CString, "Name")
				),

				new SwitchPacketSegmentStructure("Attribute Switch", "SpellAttributes",
					new SwitchCase(ComparisonType.And, SpellAttributes.CannotBeCastInCombat,
						new PacketSegmentStructure(SimpleType.CString, "NoCombatString")
					),
					new SwitchCase(ComparisonType.And, SpellAttributes.CannotBeCastInCombat | SpellAttributes.CastableWhileMounted,
						new PacketSegmentStructure(SimpleType.CString, "NoCombatNoDismountString")
					)
				),

				new SwitchPacketSegmentStructure("List Switch", "Members",
					new SwitchCase(ComparisonType.Equal, (byte)0,
						new PacketSegmentStructure(SimpleType.Byte, "Status", "CharacterStatus"),
						new SwitchPacketSegmentStructure("Nested Switch", "Status",
							new SwitchCase(ComparisonType.AndNot, CharacterStatus.OFFLINE,
								new PacketSegmentStructure(SimpleType.UInt, "NumX")
							)
						)
					),
					new SwitchCase(ComparisonType.GreaterThan, (byte)0,
						new PacketSegmentStructure(SimpleType.CString, "Info")
					)
				),

				new StaticListPacketSegmentStructure(1, "StaticList1",
					new PacketSegmentStructure(SimpleType.Int, "Number1")
				),

				new StaticListPacketSegmentStructure(2, "StaticList2",
					new PacketSegmentStructure(SimpleType.Int, "Number2")
				),

				new PacketSegmentStructure(SimpleType.Byte, "ListLength"),

				new ListPacketSegmentStructure("SomeList3", "ListLength",
					new PacketSegmentStructure(SimpleType.Int, "SomeListEle")
				)
			);

			def.Init();

			using (var packet = new RealmPacketOut(RealmServerOpCode.CMSG_PLAYER_AI_CHEAT))
			{
				packet.WriteUInt((uint)(SpellAttributes.CastableWhileMounted | SpellAttributes.CannotBeCastInCombat));
				packet.WriteByte(2);

				packet.Write(EntityId.Zero);
				packet.WriteCString("Member1Name");

				packet.Write(new EntityId(10));
				packet.WriteCString("Member2Name");

				packet.WriteCString("NoCombat");
				packet.WriteCString("NoCombatNoDismount");

				packet.WriteCString("lotsofinfo");

				// StaticList1
				packet.WriteInt(10);

				// StaticList2
				packet.WriteInt(20);
				packet.WriteInt(30);

				// SomeList3's length
				packet.WriteByte(2);
				// SomeList3
				packet.WriteInt(5);
				packet.WriteInt(7);


				var parsedPacket = PacketParser.Parse(packet, PacketSender.Client, def);

				Assert.AreEqual("NoCombat", parsedPacket["NoCombatString"].StringValue);
				Assert.AreEqual("NoCombatNoDismount", parsedPacket["NoCombatNoDismountString"].StringValue);

				var members = parsedPacket["Members"];
				Assert.AreEqual(EntityId.Zero, members[0]["MemberId"].EntityIdValue);
				Assert.AreEqual("Member1Name", members[0]["Name"].StringValue);
				Assert.AreEqual("Member2Name", members[1]["Name"].StringValue);
				Assert.AreEqual("lotsofinfo", parsedPacket["Info"].StringValue);
				CheckRemainder(parsedPacket);
			}

			using (var packet = new RealmPacketOut(RealmServerOpCode.CMSG_PLAYER_AI_CHEAT))
			{
				packet.WriteUInt((uint)SpellAttributes.None);
				packet.WriteByte(0);

				packet.Write((byte)CharacterStatus.ONLINE);
				packet.Write((uint)1234);

				packet.WriteInt(10);

				packet.WriteInt(20);
				packet.WriteInt(30);

				// SomeList3's length
				packet.WriteByte(2);
				// SomeList3
				packet.WriteInt(5);
				packet.WriteInt(7);


				var parsedPacket = PacketParser.Parse(packet, PacketSender.Client, def);

				Assert.IsNull(parsedPacket.GetByName("Info"));

				Assert.AreEqual(CharacterStatus.ONLINE, parsedPacket["Status"].Value);
				Assert.AreEqual((uint)1234, parsedPacket["NumX"].Value);

				CheckRemainder(parsedPacket);
			}
		}

		void CheckRemainder(ParsedSegment parsedPacket)
		{
			var listEle = parsedPacket["StaticList1"][0];
			Assert.IsNotNull(listEle);
			Assert.AreEqual(10, listEle["Number1"].IntValue);

			listEle = parsedPacket["StaticList2"][0];
			Assert.IsNotNull(listEle);
			Assert.AreEqual(20, listEle["Number2"].IntValue);

			listEle = parsedPacket["StaticList2"][1];
			Assert.IsNotNull(listEle);
			Assert.AreEqual(30, listEle["Number2"].IntValue);

			var listLength = parsedPacket["ListLength"];
			var list = parsedPacket["SomeList3"];
			Assert.AreEqual(2, ((ParsedSegment)list).List.Count);

			Assert.AreEqual(5, list[0]["SomeListEle"].IntValue);
			Assert.AreEqual(7, list[1]["SomeListEle"].IntValue);
		}
	}
}