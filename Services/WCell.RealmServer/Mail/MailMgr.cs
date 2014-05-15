using System;
using System.Collections.Generic;
using WCell.RealmServer.Database.Entities;
using WCell.Util.Logging;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.Util.Variables;
using WCell.RealmServer.Network;
using System.Threading;
using System.Linq;

namespace WCell.RealmServer.Mail
{
	public class MailMgr : Manager<MailMgr>
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Max number of items each player can store in their mailbox.
		/// </summary>
		public const int MaxItemsPerMail = 12;

		public const int MaxMailSubjectLength = 128;	// based on ingame limitations

		public const int MaxMailBodyLength = 512;	// based on ingame limitations

		/// <summary>
		/// Whether or not to delivery mail instantly for any type of mail.
		/// </summary>
		public static bool DeliverMailInstantly;

		/// <summary>
		/// Min delivery delay of mail with items or Gold in seconds (Default: 30 minutes)
		/// </summary>
		public static uint MinPacketDeliveryDelay = 30 * 60;
		/// <summary>
		/// Max delivery delay of mail with items or Gold in seconds (Default: 60 minutes)
		/// </summary>
		public static uint MaxPacketDeliveryDelay = 60 * 60;
		/// <summary>
		/// Max number of messages each player can store in their mailbox.
		/// </summary>
		public static uint MaxMailCount = 100;
		/// <summary>
		/// Max number of items each player can store in their mailbox.
		/// </summary>
		public static uint MaxStoredItems = 256;
		/// <summary>
		/// Max number of days to store regular mail in the mailbox.
		/// </summary>
		public static uint MailExpiryDelay = 30;
		/// <summary>
		/// Max number of days to store Cash-On_Delivery mail in the mailbox.
		/// </summary>
		public static uint MaxCODExpiryDelay = 3;
		/// <summary>
		/// Whether to allow characters to send mail to characters on the opposite Team.
		/// </summary>
		public static bool AllowInterFactionMail;
		/// <summary>
		/// The index of the item template to use for creating permanent mail storage.
		/// </summary>
		public static ItemId MailTextItemTemplate = ItemId.PlainLetter;

		[Variable("ChargeMailPostage")]
		public static bool ChargePostage = true;

		/// <summary>
		/// The amount of postage to charge per message sent, in copper.
		/// </summary>
		public static uint PostagePrice = 30;

		#region Internal
		#region Text Id Generator
		private static bool _idGeneratorInitialised;
		private static long _highestId;

		private static void InitIdGenerator()
		{
			//long highestId;
			try
			{

				MailMessage highestMailMessageItem = null;
				highestMailMessageItem = RealmWorldDBMgr.DatabaseProvider.Session.QueryOver<MailMessage>().OrderBy(record => record.TextId).Desc.Take(1).SingleOrDefault();
				var mailMessagehighestId = highestMailMessageItem != null ? highestMailMessageItem.TextId : 0;


				//var mailMessagehighestId = RealmWorldDBMgr.DatabaseProvider.Query<MailMessage>().Max(mailMessage => mailMessage.TextId);

				ItemRecord highestItemRecordItem = null;
				highestItemRecordItem = RealmWorldDBMgr.DatabaseProvider.Session.QueryOver<ItemRecord>().OrderBy(record => record.ItemTextId).Desc.Take(1).SingleOrDefault();
				var itemRecordHighestId = highestItemRecordItem != null ? highestItemRecordItem.ItemTextId : 0;


				//var itemRecordHighestId = RealmWorldDBMgr.DatabaseProvider.Query<ItemRecord>().Max(itemRecord => itemRecord.ItemTextId);

				_highestId = Math.Max(mailMessagehighestId, itemRecordHighestId);
			}
			catch (Exception e)
			{
				RealmWorldDBMgr.OnDBError(e);


				MailMessage highestMailMessageItem = null;
				highestMailMessageItem = RealmWorldDBMgr.DatabaseProvider.Session.QueryOver<MailMessage>().OrderBy(record => record.TextId).Desc.Take(1).SingleOrDefault();
				var mailMessagehighestId = highestMailMessageItem != null ? highestMailMessageItem.TextId : 0;


				//var mailMessagehighestId = RealmWorldDBMgr.DatabaseProvider.Query<MailMessage>().Max(mailMessage => mailMessage.TextId);


				ItemRecord highestItemRecordItem = null;
				highestItemRecordItem = RealmWorldDBMgr.DatabaseProvider.Session.QueryOver<ItemRecord>().OrderBy(record => record.ItemTextId).Desc.Take(1).SingleOrDefault();
				var itemRecordHighestId = highestItemRecordItem != null ? highestItemRecordItem.ItemTextId : 0;


				//var itemRecordHighestId = RealmWorldDBMgr.DatabaseProvider.Query<ItemRecord>().Max(itemRecord => itemRecord.ItemTextId);

				_highestId = Math.Max(mailMessagehighestId, itemRecordHighestId);
			}

			//_highestId = (long)Convert.ChangeType(highestId, typeof(long));

			_idGeneratorInitialised = true;
		}

		/// <summary>
		/// Returns the next unique Id for a new Item
		/// </summary>
		public static long NextId()
		{
			if (!_idGeneratorInitialised)
				InitIdGenerator();

			return Interlocked.Increment(ref _highestId);
		}

		public static long LastId
		{
			get
			{
				if (!_idGeneratorInitialised)
					InitIdGenerator();
				return Interlocked.Read(ref _highestId);
			}
		}
		#endregion
		//internal static NHIdGenerator TextIdGenerator;

		[Initialization(InitializationPass.Sixth)]	// init after NHIdGenerator init'ed (Fifth)
		public static void Initialize()
		{
			try
			{
				CreateIdGenerators();
			}
			catch (Exception e)
			{
				RealmWorldDBMgr.OnDBError(e);
				CreateIdGenerators();
			}
		}

		private static void CreateIdGenerators()
		{
			//var mailIdGenerator = new NHIdGenerator(typeof(MailMessage), "_TextId");
			//var itemTextIdGenerator = new NHIdGenerator(typeof(ItemRecord), "m_ItemTextId");
			//if (LastId > LastItemRecordId)
			//{
			//	//TextIdGenerator = mailIdGenerator;
			//}
			//else
			//{
			//	//TextIdGenerator = itemTextIdGenerator;
			//}
		}

		protected MailMgr()
		{
		}
		#endregion

		#region SendMail
		public static bool SendMail(string recipientName, string subject, string body)
		{
			var id = CharacterRecord.GetIdByName(recipientName);
			if (id <= 0)
			{
				return false;
			}

			SendMail(id, subject, body);
			return true;
		}

		public static void SendMail(uint recipientLowId, string subject, string body)
		{
			var now = DateTime.Now;
			var letter = new MailMessage(subject, body)
			{
				LastModifiedOn = null,
				SenderId = 0,
				ReceiverId = recipientLowId,
				MessageStationary = MailStationary.Normal,
				MessageType = MailType.Normal,
				SendTime = now,
				DeliveryTime = now
			};

			letter.Send();
		}

		public static MailError SendMail(string recipientName,
										string subject,
		                                 string body,
		                                 MailStationary stationary,
		                                 ICollection<ItemRecord> items,
		                                 uint money,
		                                 uint cod,
		                                 IPacketReceiver sender)
		{
			var id = CharacterRecord.GetIdByName(recipientName);
			if (id <= 0)
			{
				return MailError.RECIPIENT_NOT_FOUND;
			}
			return SendMail(id, subject, body, stationary, items, money, cod, sender);
		}

		public static MailError SendMail(uint recipientLowId,
		   string subject,
		   string body,
		   MailStationary stationary,
		   ICollection<ItemRecord> items,
			uint money,
		   uint cod,
			IPacketReceiver sender)
		{
			// All good, send an ok message
			if (sender != null)
			{
				MailHandler.SendResult(sender, 0u, MailResult.MailSent, MailError.OK);
			}

			var deliveryDelay = 0u;

			// Create a new letter object
			var letter = new MailMessage(subject, body)
			{
				LastModifiedOn = null,
				SenderId = sender is IEntity ? ((IEntity)sender).EntityId.Low : 0,
				ReceiverId = recipientLowId,
				MessageStationary = stationary,
				MessageType = MailType.Normal,
				CashOnDelivery = cod,
				IncludedMoney = money,
				SendTime = DateTime.Now,
				DeliveryTime = DateTime.Now.AddSeconds(deliveryDelay)
			};

			// Attach items to the new letter
			if (items != null && items.Count > 0)
			{
				// remove the item from the sender's inventory and add them to the list
				letter.SetItems(items);
			}

			SendMail(letter);
			return MailError.OK;
		}

		public static void SendMail(MailMessage letter)
		{
			// Make a new TextItem that contains the body of the message

			letter.ExpireTime = letter.DeliveryTime.AddDays(letter.CashOnDelivery > 0 ? MaxCODExpiryDelay : MailExpiryDelay);
			RealmServer.IOQueue.ExecuteInContext(() =>
			{
				RealmWorldDBMgr.DatabaseProvider.SaveOrUpdate(letter);

				// If the recipient is online, send them a new-mail notification
				var recipient = World.GetCharacter((uint)letter.ReceiverId);
				if (recipient != null)
				{
					if (letter.DeliveryTime < DateTime.Now)
					{
						recipient.ExecuteInContext(() =>
						{
							letter.Recipient = recipient.Record;
							recipient.MailAccount.AllMail.Add((uint)letter.Guid, letter);
							MailHandler.SendNotify(recipient.Client);
						});
					}
					else
					{
						recipient.CallDelayed(letter.RemainingDeliveryMillis, chr => MailHandler.SendNotify(recipient.Client));
					}
				}
			});
		}
		#endregion

		/// <summary>
		/// Returns all value mail that was sent to the Character with the given Id to their original sender
		/// </summary>
		public static void ReturnValueMailFor(uint charId)
		{
			//var dlct = RealmWorldDBMgr.DatabaseProvider.CurrentDialect;
			//var sql = string.Format("UPDATE {0} SET {1} = {2}, {3} = 0 WHERE {4} = {5} AND ({6} > 0 OR {7} > 0 OR {8} > 0)",
			//	dlct.QuoteForTableName(typeof(MailMessage).Name),
			//	dlct.QuoteForColumnName("ReceiverId"),
			//	dlct.QuoteForColumnName("SenderId"),

			//	dlct.QuoteForColumnName("CashOnDelivery"),
			//	dlct.QuoteForColumnName("SenderId"),
			//	charId,
			//	dlct.QuoteForColumnName("CashOnDelivery"),
			//	dlct.QuoteForColumnName("IncludedMoney"),
			//	dlct.QuoteForColumnName("IncludedItemCount"));
			
			//var query = new ScalarQuery<long>(typeof(CharacterRecord), QueryLanguage.Sql, sql);
			//var result = query.Execute();

			foreach (var mailMessage in RealmWorldDBMgr.DatabaseProvider.Query<MailMessage>().Where(mailMessage => 
				mailMessage.SenderId == charId && 
					(mailMessage.CashOnDelivery > 0 ||
						mailMessage.IncludedMoney > 0 ||
						mailMessage.IncludedItemCount > 0)))
			{
				mailMessage.ReceiverId = mailMessage.SenderId;
				RealmWorldDBMgr.DatabaseProvider.SaveOrUpdate(mailMessage);
			}
		}
	}
}