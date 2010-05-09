using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.RealmServer.Network;
using WCell.RealmServer.Tests.Misc;
using WCell.Core.Network;
using WCell.Core;
using WCell.Constants;
using WCell.PacketAnalysis;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Debugging;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Interaction;

namespace WCell.RealmServer.Tests.Groups
{
	/// <summary>
	/// GroupTest
	/// </summary>
	[TestClass]
	public class GroupTest : PacketUtil
	{
		public GroupTest()
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
		}

		#region Creation
		[TestMethod]
		public void TestGroupCreation()
		{
			//Setup.AllianceCharacterPool.EnsureSameRegion = true;
			DebugUtil.Dumps = true;
			var pool = Setup.AllianceCharacterPool;

			int count = PartyGroup.MaxMemberCount;
			var group = CreateGroup(count);

			var leader = (TestCharacter)group.Leader.Character;

			// no MasterLooter
			Assert.IsNull(group.MasterLooter);

			var noGroupChar = pool.Create();
			noGroupChar.EnsureInWorldAndLiving();

			// invite too many and expect failure message
			SendInvitePacket(leader, noGroupChar.Name);

			var parsedPacket = leader.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_PARTY_COMMAND_RESULT);
			Assert.IsNotNull(parsedPacket, "No response when inviting member to full party");
			Assert.AreEqual(GroupResult.GroupIsFull, parsedPacket["Result"].Value);

			var leaderMember = leader.GroupMember;
			var nextMember = leaderMember.Next;

			// let the leader leave the group
			leaderMember.LeaveGroup();

			Assert.IsNull(leader.GroupMember);
			Assert.IsNull(leaderMember.Next);
			Assert.AreEqual(count - 1, group.Count);
			Assert.AreEqual(nextMember, group.Leader);

			// new leader
			leader = (TestCharacter)nextMember.Character;
			leaderMember = nextMember;
			nextMember = leaderMember.Next;

			// one member logs out
			var client = ((TestCharacter)nextMember.Character).FakeClient;
			leader.FakeClient.PurgeSMSGs();

			nextMember.Character.Logout(false, 0);
			nextMember.Region.WaitOneTick();
			Assert.IsNull(nextMember.Character);
			Assert.IsNull(client.ActiveCharacter);

			var logoutResponse = client.DequeueSMSG(RealmServerOpCode.SMSG_LOGOUT_COMPLETE);
			Assert.IsNotNull(logoutResponse);

			var groupUpdate = leader.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_GROUP_LIST);
			Assert.IsNotNull(groupUpdate);

			// look for the member in the GROUP_LIST packet and assert that everyone is offline but him/her
			for (int i = 0; i < groupUpdate["Members"].IntValue; i++)
			{
				var member = groupUpdate["Members"][i];

				if (nextMember.Name == member["MemberName"].StringValue)
				{
					Assert.AreEqual(CharacterStatus.OFFLINE, member["Status"].Value);
				}
				else
				{
					Assert.AreEqual(CharacterStatus.ONLINE, member["Status"].Value);
				}
			}


			group.Disband();

			Assert.AreEqual(0, group.Count);
		}

		[TestMethod]
		public void TestGroupChat()
		{
			var group = CreateGroup(3);

			var leader = group.Leader;
			Assert.IsNotNull(leader, "Group was not created properly");

			var leaderChr = leader.Character;
			Assert.IsNotNull(leaderChr, "Group was not created properly");

			leaderChr.Region.WaitOneTick();
			foreach (TestCharacter chr in group.GetCharacters())
			{
				// purge all pending packets - we don't care for them
				chr.FakeClient.PurgeSMSGs();
			}

			var text = "Hello test";
			leaderChr.SayGroup(text);
			leaderChr.Region.WaitOneTick();

			foreach (TestCharacter chr in group.GetCharacters())
			{
				var chatPacket = chr.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_MESSAGECHAT);
				Assert.IsNotNull(chatPacket, "Character did not receive Group-Chat: " + chr);
				Assert.AreEqual(text, chatPacket["Message"].StringValue);
			}
		}

		public static Group CreateGroup(int count)
		{
			var pool = Setup.AllianceCharacterPool;
			pool.EnsureLiving = true;

			var members = new TestCharacter[count];
			var leader = members[0] = pool.Create();

			Asser.GreaterThan(count, 1);

			// invite
			leader.EnsureInWorldAndLiving();
			for (uint i = 1; i < count; i++)
			{
				members[i] = pool.Create();
				members[i].EnsureInWorldAndLiving();
				Invite(leader, members[i]);
			}

			// accept 
			for (uint i = 1; i < count; i++)
			{
				Accept(leader, members[i]);
				Assert.AreEqual(members[i].GroupMember, members[i - 1].GroupMember.Next);
			}

			var group = leader.Group;
			Assert.AreEqual(count, group.Count);

			return group;
		}

		public static void SendInvitePacket(TestCharacter inviter, string inviteeName)
		{
			var packet = new RealmPacketOut(RealmServerOpCode.CMSG_GROUP_INVITE);
			packet.WriteCString(inviteeName);

			inviter.FakeClient.ReceiveCMSG(packet, true);
		}

		static void Invite(TestCharacter inviter, TestCharacter invitee)
		{
			SendInvitePacket(inviter, invitee.Name);
			var relation = inviter.GetRelationTo(invitee, CharacterRelationType.GroupInvite);
			Assert.IsNotNull(relation, "Invitation ({0} -> {1}) was not issued", inviter, invitee);

			// invited notification
			var parsedPacket = inviter.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_PARTY_COMMAND_RESULT);
			Assert.IsNotNull(parsedPacket, "SMSG_PARTY_COMMAND_RESULT was not sent");
			var inviteeName = parsedPacket["Name"].StringValue;
			Assert.AreEqual(invitee.Name, inviteeName);
			Assert.AreEqual(GroupResult.NoError, parsedPacket["Result"].Value);
		}

		static void Accept(TestCharacter leader, TestCharacter accepter)
		{
			// adding the new member:
			var newGroup = leader.Group == null;
			var packet = new RealmPacketOut(RealmServerOpCode.CMSG_GROUP_ACCEPT);
			accepter.FakeClient.ReceiveCMSG(packet, true);

			Assert.IsNotNull(leader.Group, "Leader was not in a group after invitation was accepted: " + leader);
			Assert.IsNotNull(accepter.Group, "New member is not in a group after accepting invitation: " + accepter);
			Assert.AreEqual(leader.Group, accepter.Group,
				"Leader and invited member are not in the same Group: {0} != {1}", leader.Group, accepter.Group);

			if (newGroup)
			{
				var packetInfo1 = leader.FakeClient.DequeueSMSGInfo(RealmServerOpCode.SMSG_GROUP_LIST);
				// New Group: First list is sent to the creator
				Assert.AreEqual(leader.FakeClient, packetInfo1.Client);
				var response1 = packetInfo1.Parser.ParsedPacket;

				// should only contain the new member
				var member1 = response1["Members"][0]["MemberName"].StringValue;
				Assert.AreEqual(accepter.Name, member1);

				Assert.AreEqual(leader.EntityId, response1["Leader"].EntityIdValue);
			}

			var packetInfo = accepter.FakeClient.DequeueSMSGInfo(RealmServerOpCode.SMSG_GROUP_LIST);
			var response = packetInfo.Parser.ParsedPacket;

			// leader comes first (usually)
			var member = response["Members"][0]["MemberName"].StringValue;
			Assert.AreEqual(leader.Name, member);
		}
		#endregion
	}
}
