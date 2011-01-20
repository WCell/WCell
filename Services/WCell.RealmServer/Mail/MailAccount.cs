using System;
using System.Collections.Generic;
using NLog;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Items;
using WCell.Core;
using WCell.Util.Threading;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Interaction;

namespace WCell.RealmServer.Mail
{
	/// <summary>
	/// Represents the ingame Mail-Account of this Character
	/// </summary>
	// TODO: Store MailAccounts somewhere seperate for offline chars (or query them from DB)
	public class MailAccount
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		private Character m_chr;
		private GameObject m_mailBox;
		private bool firstCheckSinceLogin = true;

		/// <summary>
		/// All Mail that is associated with this character (undelivered, read or unread)
		/// </summary>
		public Dictionary<uint, MailMessage> AllMail;

		public MailAccount(Character chr)
		{
			m_chr = chr;

			AllMail = new Dictionary<uint, MailMessage>((int)MailMgr.MaxMailCount);
		}

		public Character Owner
		{
			get { return m_chr; }
			internal set { m_chr = value; }
		}

		/// <summary>
		/// Loads stored mail from DB
		/// </summary>
		internal void Load()
		{
			foreach (var letter in MailMessage.FindAllMessagesFor(m_chr.EntityId.Low))
			{
				if (AllMail.ContainsKey((uint)letter.Guid))
				{
					continue;
				}
				AllMail.Add((uint)letter.Guid, letter);
			}
		}

		/// <summary>
		/// The currently used MailBox (or null)
		/// </summary>
		public GameObject MailBox
		{
			get { return m_mailBox; }
			set
			{
				if (m_mailBox != value)
				{
					m_mailBox = value;
				}
			}
		}

		#region SendMail
		public MailError SendMail(string recipientName,
			string subject,
			string body,
			MailStationary stationary,
			ICollection<Item> items,
			uint money,
			uint cod)
		{
			FactionGroup recipientFaction;
			int recipientMailCount;

			// Find and verify the recipient
			var normalizedName = recipientName;
			var recipient = World.GetCharacter(normalizedName, false);
			CharacterRecord recipientRecord;
			if (recipient != null)
			{
				// receiving character is online, get info from the character object
				recipientFaction = recipient.Faction.Group;
				recipientMailCount = recipient.MailAccount.AllMail.Count;
				recipientRecord = recipient.Record;
			}
			else
			{
				// no character online with that name, get info from the CharacterRecord
				var charRecord = CharacterRecord.GetRecordByName(normalizedName);
				if (charRecord == null)
				{
					MailHandler.SendResult(m_chr.Client, 0, MailResult.MailSent, MailError.RECIPIENT_NOT_FOUND);
					return MailError.RECIPIENT_NOT_FOUND;
				}

				recipientFaction = FactionMgr.GetFactionGroup(charRecord.Race);
				recipientMailCount = charRecord.MailCount;
				recipientRecord = charRecord;
			}

			if (!m_chr.GodMode)
			{
				// Can't send to people who ignore you
				if (RelationMgr.IsIgnoring(recipientRecord.EntityLowId, m_chr.EntityId.Low))
				{
					MailHandler.SendResult(m_chr.Client, 0, MailResult.MailSent, MailError.RECIPIENT_NOT_FOUND);
					return MailError.RECIPIENT_NOT_FOUND;
				}

				// Can't send mail to people on the other team
				if (!MailMgr.AllowInterFactionMail && !m_chr.GodMode)
				{
					if (recipientFaction != m_chr.Faction.Group)
					{
						MailHandler.SendResult(m_chr, 0u, MailResult.MailSent, MailError.NOT_YOUR_ALLIANCE);
						return MailError.NOT_YOUR_ALLIANCE;
					}
				}

				// Check that the recipient can recieve more mail.
				if (recipientMailCount > MailMgr.MaxMailCount)
				{
					MailHandler.SendResult(m_chr, 0u, MailResult.MailSent, MailError.RECIPIENT_CAP_REACHED);
					return MailError.RECIPIENT_CAP_REACHED;
				}
			}

			return SendMail(recipientRecord, subject, body, stationary, items, money, cod);
		}

		/// <summary>
		/// Creates and sends a new Mail with the given parameters
		/// </summary>
		public MailError SendMail(CharacterRecord recipient,
			string subject,
			string body,
			MailStationary stationary,
			ICollection<Item> items,
			uint money,
			uint cod)
		{
			if (subject.Length > MailMgr.MaxMailSubjectLength ||
				body.Length > MailMgr.MaxMailBodyLength)
			{
				// Player cannot send mails this long through the mail dialog
				return MailError.INTERNAL_ERROR;
			}

			// Can't send mail to yourself.
			if (recipient.EntityLowId == m_chr.EntityId.Low)
			{
				MailHandler.SendResult(m_chr.Client, 0, MailResult.MailSent, MailError.CANNOT_SEND_TO_SELF);
				return MailError.CANNOT_SEND_TO_SELF;
			}

			// Check that sender is good for the money.
			if (MailMgr.ChargePostage && !m_chr.GodMode)
			{
				var requiredCash = money + MailMgr.PostagePrice;

				var count = (items == null) ? 0u : (uint)items.Count;
				if (count > 0)
				{
					requiredCash += ((count - 1) * MailMgr.PostagePrice);
				}

				if (requiredCash > m_chr.Money)
				{
					MailHandler.SendResult(m_chr.Client, 0u, MailResult.MailSent, MailError.NOT_ENOUGH_MONEY);
					return MailError.NOT_ENOUGH_MONEY;
				}

				// Charge for the letter (already checked, Character has enough)
				m_chr.Money -= requiredCash;
			}

			// All good, send an ok message
			MailHandler.SendResult(m_chr.Client, 0u, MailResult.MailSent, MailError.OK);

			var deliveryDelay = 0u;
			if (!m_chr.GodMode)
			{
				if ((money > 0 || (items != null && items.Count > 0)) && (m_chr.Account.AccountId != recipient.AccountId))
				{
					deliveryDelay = MailMgr.MaxPacketDeliveryDelay;
				}
			}

			// Create a new letter object
			var letter = new MailMessage(subject, body)
			{
				LastModifiedOn = null,
				SenderId = m_chr.EntityId.Low,
				ReceiverId = recipient.EntityLowId,
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

			MailMgr.SendMail(letter);
			return MailError.OK;
		}
		#endregion

		/// <summary>
		/// Returns the corresponding Item, if it can be mailed, else will send error message
		/// </summary>
		/// <param name="itemId"></param>
		/// <returns></returns>
		public Item GetItemToMail(EntityId itemId)
		{
			// Does the player have this item in their inventory?
			var item = m_chr.Inventory.GetItem(itemId);
			if (item == null)
			{
				MailHandler.SendResult(m_chr, 0u, MailResult.MailSent, MailError.INTERNAL_ERROR);
				return null;
			}

			// If the item is a container, is it empty?
			if (item.IsContainer && !((Container)item).BaseInventory.IsEmpty)
			{
				MailHandler.SendResult(m_chr, 0u, MailResult.MailSent, MailError.BAG_FULL);
				return null;
			}

			// Can't send conjured items or non-permanent items.
			if (item.IsConjured || item.Duration > 0)
			{
				MailHandler.SendResult(m_chr, 0u, MailResult.MailSent, MailError.INTERNAL_ERROR);
				return null;
			}

			return item;
		}

		public void MarkAsRead(uint messageId)
		{
			MailMessage letter;
			if (AllMail.TryGetValue(messageId, out letter))
			{
				letter.ReadTime = DateTime.Now;
			}
		}

		public void SendMailList()
		{
			if (firstCheckSinceLogin)
			{
				// enqueue Task to load from DB
				// then enqueue another task to do the actual sending from the Map thread
				RealmServer.Instance.AddMessage(new Message(() =>
				{
					Load();
					var context = m_chr.ContextHandler;
					if (context != null)
					{
						context.AddMessage(() =>
						{
							if (m_chr != null && m_chr.IsInWorld)
							{
								SendMailList();
							}
						});
					}
				}));
				firstCheckSinceLogin = false;
			}
			else
			{
				var list = CollectMail();
				MailHandler.SendMailList(m_chr.Client, list);
			}
		}

		/// <summary>
		/// IMPORTANT: Required IO-Queue Context
		/// </summary>
		/// <returns></returns>
		public List<MailMessage> GetMail()
		{
			if (firstCheckSinceLogin)
			{
				//RealmServer.Instance.EnsureContext();

				// enqueue Task to load from DB
				Load();
				firstCheckSinceLogin = false;
			}
			return CollectMail();
		}

		List<MailMessage> CollectMail()
		{
			var letterList = new List<MailMessage>((int)MailMgr.MaxMailCount);
			var letterCount = 0u;
			var itemCount = 0u;

			var removeMe = new List<MailMessage>(10);
			foreach (var letter in AllMail.Values)
			{
				if (letter.DeliveryTime >= DateTime.Now)
					continue;
				if (letter.ExpireTime <= DateTime.Now)
				{
					removeMe.Add(letter);
					continue;
				}
				if (letter.IsDeleted)
					continue;

				if (letterCount > MailMgr.MaxMailCount)
					break;

				if (itemCount > MailMgr.MaxStoredItems)
					break;

				letterCount++;
				itemCount += (uint)letter.IncludedItemCount;

				letterList.Add(letter);
			}

			foreach (var letter in removeMe)
			{
				DeleteOrReturn(letter);
			}
			return letterList;
		}

		public void TakeMoney(uint messageId)
		{
			MailMessage letter;
			if (!AllMail.TryGetValue(messageId, out letter) ||
				letter.IsDeleted || letter.DeliveryTime > DateTime.Now)
			{
				MailHandler.SendResult(m_chr.Client, messageId, MailResult.MoneyTaken, MailError.INTERNAL_ERROR);
				return;
			}

			m_chr.Money += letter.IncludedMoney;
			letter.IncludedMoney = 0;

			MailHandler.SendResult(m_chr.Client, messageId, MailResult.MoneyTaken, MailError.OK);
		}

		public MailError TakeItem(uint messageId, uint itemId)
		{
			MailMessage letter;
			if (!AllMail.TryGetValue(messageId, out letter) ||
				letter.IsDeleted || letter.DeliveryTime > DateTime.Now)
			{
				MailHandler.SendResult(m_chr.Client, messageId, MailResult.ItemTaken, MailError.INTERNAL_ERROR);
				return MailError.INTERNAL_ERROR;
			}

			if (m_chr.Money < letter.CashOnDelivery)
			{
				MailHandler.SendResult(m_chr.Client, messageId, MailResult.ItemTaken, MailError.NOT_ENOUGH_MONEY);
				return MailError.NOT_ENOUGH_MONEY;
			}

			if (letter.IncludedItemCount == 0)
			{
				MailHandler.SendResult(m_chr.Client, messageId, MailResult.ItemTaken, MailError.INTERNAL_ERROR);
				return MailError.INTERNAL_ERROR;
			}

			// Add the item to the character's inventory and remove it from the letter
			var itemRecord = letter.RemoveItem(itemId);
			if (itemRecord == null)
			{
				MailHandler.SendResult(m_chr.Client, messageId, MailResult.ItemTaken, MailError.INTERNAL_ERROR);
				return MailError.INTERNAL_ERROR;
			}

			var item = Item.CreateItem(itemRecord, m_chr);
			if (item == null)
			{
				// add Item back
				letter.PutBack(itemRecord);
				MailHandler.SendResult(m_chr.Client, messageId, MailResult.ItemTaken, MailError.INTERNAL_ERROR);
				return MailError.INTERNAL_ERROR;
			}

			var count = item.Amount;
			var msg = m_chr.Inventory.TryAdd(item, true);
			if (msg != InventoryError.OK)
			{
				// add Item back
				letter.PutBack(itemRecord);
				MailHandler.SendResult(m_chr.Client, messageId, MailResult.ItemTaken, MailError.BAG_FULL, msg);
				return MailError.BAG_FULL;
			}

			if (letter.CashOnDelivery > 0)
			{
				m_chr.Money -= letter.CashOnDelivery;
				letter.CashOnDelivery = 0;

				// item was sent COD. Send the payment back to the sender in a new mail.
				var charRecord = CharacterRecord.GetRecord(letter.SenderEntityId.Low);
				if (charRecord != null)
				{
					SendMail(charRecord.Name, letter.Subject, "", MailStationary.Normal, null, letter.CashOnDelivery, 0);
				}
			}

			RealmServer.Instance.AddMessage(new Message(() =>
			{
				letter.Update();
				MailHandler.SendResult(m_chr.Client, (uint)letter.Guid, MailResult.ItemTaken, MailError.OK, itemId, count);
			}));

			return MailError.OK;
		}

		public MailError ReturnToSender(uint messageId)
		{
			MailMessage letter;
			if (!AllMail.TryGetValue(messageId, out letter) ||
				letter.IsDeleted || letter.DeliveryTime > DateTime.Now)
			{
				MailHandler.SendResult(m_chr.Client, messageId, MailResult.ReturnedToSender, MailError.INTERNAL_ERROR);
				return MailError.INTERNAL_ERROR;
			}

			letter.ReturnToSender();

			MailHandler.SendResult(m_chr.Client, messageId, MailResult.ReturnedToSender, MailError.OK);
			return MailError.OK;
		}

		public MailError DeleteMail(uint messageId)
		{
			MailMessage letter;
			if (!AllMail.TryGetValue(messageId, out letter) ||
				letter.IsDeleted || letter.DeliveryTime > DateTime.Now)
			{
				MailHandler.SendResult(m_chr.Client, messageId, MailResult.Deleted, MailError.INTERNAL_ERROR);
				return MailError.INTERNAL_ERROR;
			}

			DeleteOrReturn(letter);
			MailHandler.SendResult(m_chr.Client, messageId, MailResult.Deleted, MailError.OK);
			return MailError.OK;
		}

		public MailError CreateTextItem(uint messageId)
		{
			MailMessage mail;
			if (!AllMail.TryGetValue(messageId, out mail) ||
				mail.IsDeleted || mail.DeliveryTime > DateTime.Now)
			{
				MailHandler.SendResult(m_chr.Client, messageId, MailResult.MadePermanent, MailError.INTERNAL_ERROR);
				return MailError.INTERNAL_ERROR;
			}

			var template = ItemMgr.Templates[(uint)MailMgr.MailTextItemTemplate];

			var letter = Item.CreateItem(template, m_chr, 1);
			letter.ItemText = mail.Body;
			letter.Record.ItemTextId = mail.TextId;

			var msg = m_chr.Inventory.TryAdd(letter, false);
			if (msg != InventoryError.OK)
			{
				ItemHandler.SendInventoryError(m_chr, msg);
				//MailHandler.SendResult(m_chr.Client, messageId, MailResult.MadePermanent, MailError.BAG_FULL, msg);

				letter.Destroy();
				return MailError.BAG_FULL;
			}

			mail.CopiedToItem = true;

			RealmServer.Instance.AddMessage(new Message(() =>
			{
				mail.Save();
				MailHandler.SendResult(m_chr, messageId, MailResult.MadePermanent, MailError.OK);
			}));

			return MailError.OK;
		}

		public void GetNextMailTime()
		{
			uint count = 0;
			var mailList = new List<MailMessage>(2);
			foreach (var letter in AllMail.Values)
			{
				if (letter.WasRead || letter.DeliveryTime > DateTime.Now)
					continue;

				++count;
				if (count > 2)
					break;

				mailList.Add(letter);
			}

			MailHandler.SendNextMailTime(m_chr.Client, mailList);
		}

		public void SendItemText(uint itemTextId, uint mailOrItemId)
		{
			MailMessage letter;
			if (AllMail.TryGetValue(mailOrItemId, out letter))
			{
				//if (letter.TextId == itemTextId)
				MailHandler.SendItemTextQueryResponce(m_chr.Client, itemTextId, letter.Body);
			}
			else
			{
				var item = m_chr.Inventory.GetItemByLowId(mailOrItemId);
				if (item != null)
				{
					//if (item.Record.ItemTextId == itemTextId)
					MailHandler.SendItemTextQueryResponce(m_chr.Client, itemTextId, item.Record.ItemText);
				}
			}
		}

		private void DeleteOrReturn(MailMessage letter)
		{
			AllMail.Remove((uint)letter.Guid);

			if (!letter.IsDeleted && (letter.IncludedItemCount > 0 || letter.IncludedMoney > 0))
			{
				letter.ReturnToSender();
			}
			else
			{
				letter.DeletedTime = DateTime.Now;
				RealmServer.Instance.AddMessage(new Message(letter.Destroy));
			}
		}
	}
}