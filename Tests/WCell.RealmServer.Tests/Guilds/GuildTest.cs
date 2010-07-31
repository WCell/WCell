using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.Constants.Guilds;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Guilds;
using WCell.RealmServer.Network;
using WCell.RealmServer.Tests.Misc;
using WCell.Core.Network;
using WCell.Core;
using WCell.Constants;
using WCell.PacketAnalysis;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Debugging;
using WCell.Util;

namespace WCell.RealmServer.Tests.Guilds
{
    [TestClass]
    public class GuildTest : PacketUtil
    {
    	private static TestCharacter leader;

        public GuildTest()
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
        	leader = Setup.AllianceCharacterPool.Create();
        }

        #region Creation
		[TestMethod]
		public void TestGuildCreation()
		{
            DebugUtil.Dumps = true;
		    const int count = 10;

            var guild = CreateGuild(count);
			var fifth = guild.GetCharacters().ToArray()[5];

			guild.RemoveMember(guild[fifth.EntityId.Low], true);

			Assert.IsNull(guild[fifth.EntityId.Low]);

            guild.UpdateAndFlush();
            guild.Disband();

            Assert.AreEqual(0, guild.MemberCount);
        }

        public static Guild CreateGuild(int count)
        {
            var pool = Setup.AllianceCharacterPool;
            pool.EnsureLiving = true;

            Asser.GreaterThan(count, 1);

            // create
			leader.EnsureInWorldAndLiving();
			leader.EnsureNoGuild();

			new Guild(leader.Record, "TestGuild " + Utility.Random(1, 1000));

			var guild = leader.Guild;

        	Assert.IsNotNull(guild);

            // invite
        	var members = new TestCharacter[count - 1];
            for (uint i = 1; i < count; i++)
			{
				var member = members[i-1] = pool.Create();
				member.EnsureInWorldAndLiving();
				member.EnsureNoGuild();
				Invite(leader, member);
            }

            // accept 
            for (uint i = 1; i < count; i++)
			{
				var member = members[i - 1];

				Accept(leader, member);
				var character = member;
                var guildMember = guild[character.Name];
                Assert.IsNotNull(guildMember);
            }

            Assert.AreEqual(count, guild.MemberCount);

            return guild;
        }

        public static void SendInvitePacket(TestCharacter inviter, string inviteeName)
        {
            var packet = new RealmPacketOut(RealmServerOpCode.CMSG_GUILD_INVITE);
            packet.WriteCString(inviteeName);

            inviter.FakeClient.ReceiveCMSG(packet, true);
        }

        static void Invite(TestCharacter inviter, TestCharacter invitee)
        {
            SendInvitePacket(inviter, invitee.Name);

            // invited notification
            var parsedPacket = inviter.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_GUILD_COMMAND_RESULT);
            var inviteeName = parsedPacket["Name"].StringValue;
            Assert.AreEqual(invitee.Name, inviteeName);
            Assert.AreEqual(GuildResult.SUCCESS, parsedPacket["ResultCode"].Value, inviter + " failed to invite " + invitee);
        }

        static void Accept(TestCharacter leader, TestCharacter accepter)
        {
            var packet = new RealmPacketOut(RealmServerOpCode.CMSG_GUILD_ACCEPT);
            accepter.FakeClient.ReceiveCMSG(packet, true);

            // adding the new member:
            Assert.IsNotNull(leader.Guild);
            Assert.AreEqual(leader.Guild, accepter.Guild);
        }
        #endregion
    }
}