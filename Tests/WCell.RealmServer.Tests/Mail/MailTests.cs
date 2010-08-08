using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.Constants.Items;
using WCell.Core;
using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Items;
using WCell.RealmServer.Mail;
using WCell.RealmServer.Debugging;
using WCell.RealmServer.RacesClasses;
using WCell.RealmServer.Tests.Misc;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Tests.Mail
{
	/// <summary>
	/// Summary description for MailTests
	/// </summary>
	[TestClass]
	public class MailTests : PacketUtil
	{
		private static TestCharacter alliance1;
		private static TestCharacter alliance2;
		private static TestCharacter horde1;
		private static TestCharacter horde2;

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

		[TestInitialize()]
		public void MailTestClassInitialize()
		{
			Setup.EnsureItemsLoaded();

			//mailbox = GameObject.Create(GOMgr.Entries[0], GOMgr.Templates[0]);

			EnsureChar(ref alliance1, Setup.AllianceCharacterPool);
			EnsureChar(ref alliance2, Setup.AllianceCharacterPool);

			EnsureChar(ref horde1, Setup.HordeCharacterPool);
			EnsureChar(ref horde2, Setup.HordeCharacterPool);
		}

		void EnsureChar(ref TestCharacter chr, CharacterPool pool)
		{
			//if (chr == null || chr.IsInWorld)
			{
				chr = pool.Create();
				chr.SetMoney(100000);
				chr.GodMode = true;
				chr.EnsureInWorldAndLiving();
			}
		}

		[ClassCleanup()]
		public static void MailTestClassCleanup()
		{
			alliance1.GodMode = false;
			alliance2.GodMode = false;
			horde1.GodMode = false;
			horde2.GodMode = false;
		}

		[TestCleanup]
		public void MailTestCleanup()
		{
			var mailArray = alliance1.MailAccount.AllMail.Values.ToArray();
			for(var i = 0; i < mailArray.Length; ++i)
			{
				mailArray[i].Delete();
			}
			alliance1.MailAccount.AllMail.Clear();

			mailArray = alliance2.MailAccount.AllMail.Values.ToArray();
			for (var i = 0; i < mailArray.Length; ++i)
			{
				mailArray[i].Delete();
			}
			alliance2.MailAccount.AllMail.Clear();

			mailArray = horde1.MailAccount.AllMail.Values.ToArray();
			for (var i = 0; i < mailArray.Length; ++i)
			{
				mailArray[i].Delete();
			}
			horde1.MailAccount.AllMail.Clear();

			mailArray = horde2.MailAccount.AllMail.Values.ToArray();
			for (var i = 0; i < mailArray.Length; ++i)
			{
				mailArray[i].Delete();
			}
			horde2.MailAccount.AllMail.Clear();
		}

		[TestMethod]
		public void TestSendMail()
		{
			SendMail(alliance1, alliance2);

			var mailResult = alliance1.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_SEND_MAIL_RESULT);
			Assert.IsNotNull(mailResult);
			var error = mailResult["Error"].Value;
			Assert.AreEqual(MailError.OK, error);
			
			var mailNotification = alliance2.FakeClient.DequeueSMSGInfo(RealmServerOpCode.SMSG_RECEIVED_MAIL);
			Assert.IsNotNull(mailNotification);
		}

		[TestMethod]
		public void TestGetMail()
		{
			var count = alliance2.MailAccount.GetMail().Count;
			TestSendMail();

			GetMail(alliance2);

			// get the Messages - ListSegment
			var packet = alliance2.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_MAIL_LIST_RESULT);
			var messages = packet["Messages"];
			var messageCount = messages.IntValue;

			Assert.AreEqual(count+1, messageCount);
			Assert.AreEqual("Mail Subject", messages[0]["Subject"].StringValue);
		}

		[TestMethod]
		public void TestSendMailToSelf()
		{
			SendMail(alliance1, alliance1);

			var mailResult = alliance1.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_SEND_MAIL_RESULT);
			Assert.IsNotNull(mailResult);
			var error = mailResult["Error"].Value;
			Assert.AreEqual(error, MailError.CANNOT_SEND_TO_SELF);
		}

		[TestMethod]
		public void TestSendMailToInvalid()
		{
			SendMail(alliance1, "zzddffrrtt", "testes", new List<ItemRecord>(), 0, 0);

			var mailResult = alliance1.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_SEND_MAIL_RESULT);
			Assert.IsNotNull(mailResult);
			var error = mailResult["Error"].Value;
			Assert.AreEqual(error, MailError.RECIPIENT_NOT_FOUND);
		}

		//[TestMethod]
		//public void TestSendMailLackingFunds()
		//{
		//    alliance1.SetMoney(0);

		//    SendMail(alliance1, alliance2);

		//    var mailResult = alliance1.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_SEND_MAIL_RESULT);
		//    Assert.IsNotNull(mailResult);
		//    var error = mailResult["Error"].Value;
		//    Assert.AreEqual(error, MailError.NOT_ENOUGH_MONEY);

		//    alliance1.SetMoney(100000);
		//}

		/// <summary>
		/// Keep in mind: Tests are never executed in a fixed sequence.
		/// Make sure that you have test-specific settings set within the test itself
		/// or using the attributes [TestInitialize]/[TestCleanup]
		/// </summary>
		[TestMethod]
		public void TestSendMailWithMoney()
		{
			var cash = 1u;
			SendMail(alliance1, alliance2, "Money", null, cash);
			VerifyMailSent(alliance1, alliance2);
			
			GetMail(alliance2);
			var mailResult = alliance2.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_MAIL_LIST_RESULT);
			Assert.IsNotNull(mailResult);
			
			var messages = mailResult["Messages"];
			var count = messages.IntValue;
			Assert.IsTrue(count > 0);

			Assert.AreEqual(cash, messages[0]["Money"].UIntValue);
		}

		[TestMethod]
		public void TestTakeMoneyFromMail()
		{
			TestSendMailWithMoney();

			var money = alliance2.Money;

			GetMail(alliance2);
			var mailResult = alliance2.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_MAIL_LIST_RESULT);
			Assert.IsNotNull(mailResult);
			var mailId = mailResult["Messages"][0]["MessageId"].UIntValue;

			var packet = new RealmPacketOut(RealmServerOpCode.CMSG_MAIL_TAKE_MONEY);
			packet.Write(EntityId.Zero);
			packet.Write(mailId);
			
			alliance2.FakeClient.ReceiveCMSG(packet, true);

			mailResult = alliance2.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_SEND_MAIL_RESULT);
			Assert.IsNotNull(mailResult);
			Assert.AreEqual(MailResult.MoneyTaken, mailResult["Result"].Value);
			Assert.AreEqual(MailError.OK, mailResult["Error"].Value);
			Assert.AreEqual(money + 1, alliance2.Money);
		}

		[TestMethod]
		public void TestSendMailWithItems()
		{
			var inv = alliance1.Inventory;
			inv.Purge();	// make sure, inventory is empty

			var amount = 20;
			Assert.AreEqual(InventoryError.OK, inv.TryAdd(ItemId.SilkCloth, ref amount, InventorySlot.BackPack1));

			amount = 20;
			Assert.AreEqual(InventoryError.OK, inv.TryAdd(ItemId.LinenCloth, ref amount, InventorySlot.BackPack2));

			var silkCloth = inv[InventorySlot.BackPack1];
			var linenCloth = inv[InventorySlot.BackPack2];

			Assert.IsNotNull(silkCloth);
			Assert.IsNotNull(linenCloth);

			Assert.AreEqual(silkCloth.Template.ItemId, ItemId.SilkCloth);
			Assert.AreEqual(linenCloth.Template.ItemId, ItemId.LinenCloth);

			var itemList = new List<ItemRecord>(2) { silkCloth.Record, linenCloth.Record };
			SendMail(alliance1, alliance2, "Items", itemList);
			VerifyMailSent(alliance1, alliance2);

            GetMail(alliance2);
			var mailResult = alliance2.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_MAIL_LIST_RESULT);
			Assert.IsNotNull(mailResult);
			
			var messages = mailResult["Messages"];
			var messageCount = messages.IntValue;
			Assert.AreEqual(1, messageCount);

			var items = messages[0]["Items"];
			var itemCount = items.IntValue;
			Assert.AreEqual(2, itemCount);

			var item = items[0];
			Assert.AreEqual(silkCloth.EntityId.Low, item["ItemEntityLowId"].UIntValue);

			item = items[1];
			Assert.AreEqual(linenCloth.EntityId.Low, item["ItemEntityLowId"].UIntValue);
		}

		[TestMethod]
		public void TestTakeItemFromMail()
		{
			var inv = alliance1.Inventory;
			inv.Remove(InventorySlot.BackPack1, true);
			inv.Remove(InventorySlot.BackPack2, true);

			Assert.IsNull(inv[InventorySlot.BackPack1]);
			Assert.IsNull(inv[InventorySlot.BackPack2]);

			var amount = 20;
			Assert.AreEqual(InventoryError.OK, inv.TryAdd(ItemId.SilkCloth, ref amount, InventorySlot.BackPack1));

			amount = 20;
			Assert.AreEqual(InventoryError.OK, inv.TryAdd(ItemId.LinenCloth, ref amount, InventorySlot.BackPack2));

			var silkCloth = inv[InventorySlot.BackPack1];
			var linenCloth = inv[InventorySlot.BackPack2];

			Assert.IsNotNull(silkCloth);
			Assert.IsNotNull(linenCloth);

			Assert.AreEqual(ItemId.SilkCloth, silkCloth.Template.ItemId);
			Assert.AreEqual(ItemId.LinenCloth, linenCloth.Template.ItemId);

			var itemList = new List<ItemRecord>(2) { silkCloth.Record, linenCloth.Record };
			SendMail(alliance1, alliance2, "Items", itemList);
			VerifyMailSent(alliance1, alliance2);

			GetMail(alliance2);
			var mailResult = alliance2.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_MAIL_LIST_RESULT);

			var mailId = mailResult["Messages"][0]["MessageId"].UIntValue;

			var packet = new RealmPacketOut(RealmServerOpCode.CMSG_MAIL_TAKE_ITEM);
			packet.Write(EntityId.Zero);
			packet.Write(mailId);
			packet.Write(silkCloth.EntityId.Low);
			alliance2.FakeClient.ReceiveCMSG(packet, true);

			// Check that the item was taken.
			mailResult = alliance2.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_SEND_MAIL_RESULT);
			Assert.IsNotNull(mailResult);
			var error = mailResult["Error"].Value;
			Assert.AreEqual(error, MailError.OK);

			Assert.IsNotNull(alliance2.Inventory.GetItem(silkCloth.EntityId));
		}

		[TestMethod]
		public void TestDeleteMail()
		{
			TestSendMail();

			GetMail(alliance2);
			var mailResult = alliance2.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_MAIL_LIST_RESULT);
			var mailId = mailResult["Messages"][0]["MessageId"].UIntValue;

			var packet = new RealmPacketOut(RealmServerOpCode.CMSG_MAIL_DELETE);
			packet.Write(EntityId.Zero);
			packet.Write(mailId);
			packet.Write(2u);

			alliance2.FakeClient.ReceiveCMSG(packet, true);

			mailResult = alliance2.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_SEND_MAIL_RESULT);
			Assert.AreEqual(MailResult.Deleted, mailResult["Result"].Value);
			Assert.AreEqual(MailError.OK, mailResult["Error"].Value);
			
            GetMail(alliance2);
			mailResult = alliance2.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_MAIL_LIST_RESULT);
			Assert.AreEqual(0, mailResult["Messages"].IntValue);
		}

		public void GetMail(TestCharacter getter)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.CMSG_GET_MAIL_LIST))
			{
				packet.Write(EntityId.Zero);
				getter.FakeClient.ReceiveCMSG(packet, true);
			}
		}

		public void SendMail(TestCharacter sender, TestCharacter receiver)
		{
			SendMail(sender, receiver, "Mail Subject");
		}

		/// <summary>
		/// Also temporarily sets godmode
		/// </summary>
		public void SendMail(TestCharacter sender, TestCharacter receiver, string subject)
		{
			SendMail(sender, receiver, subject, null);
		}

		/// <summary>
		/// Also temporarily sets godmode
		/// </summary>
		public void SendMail(TestCharacter sender, TestCharacter receiver, string subject, List<ItemRecord> items)
		{
			SendMail(sender, receiver, subject, items, 0, 0);
		}

		public void SendMail(TestCharacter sender, TestCharacter receiver, string subject, List<ItemRecord> items, uint money)
		{
			SendMail(sender, receiver, subject, items, money, 0);
		}

		public void SendMail(TestCharacter sender, TestCharacter receiver, string subject, List<ItemRecord> items, uint money, uint cod)
		{
			SendMail(sender, receiver.Name, subject, items, money, cod);
		}

		public void SendMail(TestCharacter sender, string receiver, string subject, List<ItemRecord> items, uint money, uint cod)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.CMSG_SEND_MAIL))
			{
				packet.Write(EntityId.Zero);
				packet.Write(receiver);
				packet.Write(subject);
				packet.Write("Mail Content");
				packet.Write((uint)MailStationary.Normal);
				//Unknown1
				packet.Write((uint)0);

				if (items != null)
				{
					//Number of Items
					packet.Write((byte) items.Count);

					// TODO: Items + Enchants etc
					for (int i = 0; i < items.Count; i++)
					{
						//Slot Number
						packet.Write((byte) i);
						//Item EntityId
						packet.Write(items[i].EntityId);
					}
				}
				else
				{
					packet.Write((byte) 0);
				}

				//Money
				packet.Write(money);
				//COD
				packet.Write(cod);
				//Unknown2
				packet.Write((uint)0);
				//Unknown3
				packet.Write((uint)0);

				sender.FakeClient.ReceiveCMSG(packet, true);
			}
		}

		private static void VerifyMailSent(TestCharacter sender, TestCharacter receiver)
		{
			var mailResult = sender.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_SEND_MAIL_RESULT);
			Assert.IsNotNull(mailResult);
			var error = mailResult["Error"].Value;
			Assert.AreEqual(MailError.OK, error);

			var mailNotification = receiver.FakeClient.DequeueSMSGInfo(RealmServerOpCode.SMSG_RECEIVED_MAIL);
			Assert.IsNotNull(mailNotification);
		}
        
	}
}