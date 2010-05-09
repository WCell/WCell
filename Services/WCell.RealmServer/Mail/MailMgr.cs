using System;
using System.Collections.Generic;
using Castle.ActiveRecord.Queries;
using NLog;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Core;
using WCell.Core.Database;
using WCell.Core.Initialization;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.Util.Variables;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Mail
{
	public class MailMgr : Manager<MailMgr>
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Max number of items each player can store in their mailbox.
		/// </summary>
		public const int MaxItemsPerMail = 12;

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

		internal static NHIdGenerator TextIdGenerator;

		[Initialization(InitializationPass.Sixth)]	// init after NHIdGenerator init'ed (Fifth)
		public static void Initialize()
		{
			try
			{
				CreateIdGenerators();
			}
			catch (Exception e)
			{
				RealmDBUtil.OnDBError(e);
				CreateIdGenerators();
			}

			Instance.InternalStart();
		}

		private static void CreateIdGenerators()
		{
			var mailIdGenerator = new NHIdGenerator(typeof(MailMessage), "_TextId");
			var itemTextIdGenerator = new NHIdGenerator(typeof(ItemRecord), "m_ItemTextId");
			if (mailIdGenerator.LastId > itemTextIdGenerator.LastId)
			{
				TextIdGenerator = mailIdGenerator;
			}
			else
			{
				TextIdGenerator = itemTextIdGenerator;
			}
		}

		protected MailMgr()
		{
		}

		protected override bool InternalStart()
		{
			return true;
		}

		protected override bool InternalStop()
		{
			return true;
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
			RealmServer.Instance.ExecuteInContext(() =>
			{
				letter.Save();

				// If the recipient is online, send them a new-mail notification
				var recipient = World.GetCharacter(letter.ReceiverId);
				if (recipient != null)
				{
					if (letter.DeliveryTime < DateTime.Now)
					{
						recipient.ExecuteInContext(() =>
						{
							letter.Recipient = recipient.Record;
							recipient.Mail.AllMail.Add((uint)letter.Guid, letter);
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
			var sql = string.Format("UPDATE {0} SET {1} = {2}, {3} = 0 WHERE {4} = {5} AND ({6} > 0 OR {7} > 0 OR {8} > 0)",
				DatabaseUtil.Dialect.QuoteForTableName(typeof(MailMessage).Name),
				DatabaseUtil.Dialect.QuoteForColumnName("ReceiverId"),
				DatabaseUtil.Dialect.QuoteForColumnName("SenderId"),

				DatabaseUtil.Dialect.QuoteForColumnName("CashOnDelivery"),
				DatabaseUtil.Dialect.QuoteForColumnName("SenderId"),
				charId,
				DatabaseUtil.Dialect.QuoteForColumnName("CashOnDelivery"),
				DatabaseUtil.Dialect.QuoteForColumnName("IncludedMoney"),
				DatabaseUtil.Dialect.QuoteForColumnName("IncludedItemCount"));
			var query = new ScalarQuery<long>(typeof(CharacterRecord), QueryLanguage.Sql, sql);
			var result = query.Execute();
		}
	}
}
