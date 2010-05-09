using System;
using System.Collections.Generic;
using NLog;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Mail;
using WCell.RealmServer.Network;
using WCell.RealmServer.Items.Enchanting;
using WCell.Constants.GameObjects;

namespace WCell.RealmServer.Handlers
{
	public static class MailHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		#region IN
		[ClientPacketHandler(RealmServerOpCode.CMSG_SEND_MAIL)]
		public static void HandleSendMail(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;

			var mailboxId = packet.ReadEntityId();

			if (!CheckMailBox(chr, chr.Region.GetObject(mailboxId) as GameObject))
			{
				return;
			}

			var recipientName = packet.ReadCString();
			var subject = packet.ReadCString();
			var msg = packet.ReadCString();

			var stationary = (MailStationary)Enum.ToObject(typeof(MailStationary), packet.ReadUInt32());
			var unkowwn1 = packet.ReadUInt32();			// 4 unknown bytes

			var itemCount = packet.ReadByte();
			if (itemCount > MailMgr.MaxStoredItems)
				return;

			var items = new List<Item>(itemCount);
			for (var i = 0; i < itemCount; i++)
			{
				var slot = packet.ReadByte();
				var itemId = packet.ReadEntityId();
				var item = chr.Mail.GetItemToMail(itemId);

				if (item != null)
				{
					items.Add(item);
				}
				else
				{
					// invalid item
					return;
				}
			}

			var money = packet.ReadUInt32();
			var cod = packet.ReadUInt32();

			var unknown2 = packet.ReadUInt32();
			var unknown3 = packet.ReadUInt32();
			var unknown4 = packet.ReadByte();

			chr.Mail.SendMail(recipientName, subject, msg, stationary, items, money, cod);
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_GET_MAIL_LIST)]
		public static void HandleListMail(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var mailboxId = packet.ReadEntityId();

			if (!CheckMailBox(chr, chr.Region.GetObject(mailboxId) as GameObject))
			{
				return;
			}

			chr.Mail.SendMailList();
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_MAIL_TAKE_MONEY)]
		public static void HandleTakeMoney(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var mailboxId = packet.ReadEntityId();
			var mailId = packet.ReadUInt32();

			if (!CheckMailBox(chr, chr.Region.GetObject(mailboxId) as GameObject))
			{
				return;
			}

			chr.Mail.TakeMoney(mailId);
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_MAIL_TAKE_ITEM)]
		public static void HandleTakeItem(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var mailboxId = packet.ReadEntityId();
			var mailId = packet.ReadUInt32();
			var itemId = packet.ReadUInt32();

			if (!CheckMailBox(chr, chr.Region.GetObject(mailboxId) as GameObject))
			{
				return;
			}

			chr.Mail.TakeItem(mailId, itemId);
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_MAIL_MARK_AS_READ)]
		public static void HandleMarkAsRead(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var mailboxId = packet.ReadEntityId();
			var mailId = packet.ReadUInt32();

			if (!CheckMailBox(chr, chr.Region.GetObject(mailboxId) as GameObject))
			{
				return;
			}

			chr.Mail.MarkAsRead(mailId);
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_MAIL_RETURN_TO_SENDER)]
		public static void HandleReturnToSender(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var mailboxId = packet.ReadEntityId();
			var mailId = packet.ReadUInt32();

			if (!CheckMailBox(chr, chr.Region.GetObject(mailboxId) as GameObject))
			{
				return;
			}

			chr.Mail.ReturnToSender(mailId);
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_MAIL_DELETE)]
		public static void HandleDelete(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var mailboxId = packet.ReadEntityId();
			var mailId = packet.ReadUInt32();

			if (!CheckMailBox(chr, chr.Region.GetObject(mailboxId) as GameObject))
			{
				return;
			}

			chr.Mail.DeleteMail(mailId);
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_MAIL_CREATE_TEXT_ITEM)]
		public static void HandleCreateTextItem(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var mailboxId = packet.ReadEntityId();
			var mailId = packet.ReadUInt32();

			if (!CheckMailBox(chr, chr.Region.GetObject(mailboxId) as GameObject))
			{
				return;
			}

			chr.Mail.CreateTextItem(mailId);
		}

		[ClientPacketHandler(RealmServerOpCode.MSG_QUERY_NEXT_MAIL_TIME)]
		public static void HandleNextTime(IRealmClient client, RealmPacketIn packet)
		{
			client.ActiveCharacter.Mail.GetNextMailTime();
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_ITEM_TEXT_QUERY)]
		public static void HandleItemTextQuery(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;

			var itemTextId = packet.ReadUInt32();
			var mailOrItemId = packet.ReadUInt32();
			var unknown = packet.ReadUInt32();

			chr.Mail.SendItemText(itemTextId, mailOrItemId);
		}

		static bool CheckMailBox(Character chr, GameObject mailbox)
		{
			return chr.GodMode ||
				   (mailbox != null && mailbox.GOType == GameObjectType.Mailbox && mailbox.Handler.CanBeUsedBy(chr));
		}
		#endregion

		#region OUT
		/// <summary>
		/// Sends a responce detailing the results of the client's mail request.
		/// </summary>
		public static void SendResult(IPacketReceiver client, uint mailId, MailResult result, MailError err)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SEND_MAIL_RESULT, 12))
			{
				packet.Write(mailId);
				packet.Write((uint)result);
				packet.Write((uint)err);
				client.Send(packet);
			}
		}

		/// <summary>
		/// Sends a responce detailing the results of the client's mail request.
		/// </summary>
		public static void SendResult(IPacketReceiver client, uint mailId, MailResult result, MailError err, InventoryError invErr)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SEND_MAIL_RESULT, 12 + 4))
			{
				packet.Write(mailId);
				packet.Write((uint)result);
				packet.Write((uint)err);
				packet.Write((uint)invErr);
				client.Send(packet);
			}
		}

		/// <summary>
		/// Sends a responce detailing the results of the client's mail request.
		/// </summary>
		public static void SendResult(IPacketReceiver client, uint mailId, MailResult result, MailError err, uint itemId, int itemCount)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SEND_MAIL_RESULT, 12 + 4 + 4))
			{
				packet.Write(mailId);
				packet.Write((uint)result);
				packet.Write((uint)err);
				packet.Write(itemId);
				packet.Write(itemCount);
				client.Send(packet);
			}
		}

		/// <summary>
		/// Sends a list of mail messages to the client.
		/// </summary>
		public static void SendMailList(IPacketReceiver client, IList<MailMessage> messages)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_MAIL_LIST_RESULT, 128 * messages.Count))
			{
				const int EnchantCount = (int)EnchantSlot.Prismatic + 1;

				packet.Write(messages.Count);
				var count = Math.Min(messages.Count, 0xFF);

				packet.Write((byte)count);
				for (var m = 0; m < count; m++)
				{
					var letter = messages[m];

					var sizePos = packet.Position;
					packet.Position = sizePos + 2; // size of message
					packet.Write((uint) letter.Guid);
					packet.Write((byte) letter.MessageType);

					switch (letter.MessageType)
					{
						case MailType.Normal:
							packet.Write(letter.SenderEntityId);
							break;
						case MailType.Creature:
							packet.Write(letter.SenderEntityId.Low);
							break;
						case MailType.GameObject:
							packet.Write(letter.SenderEntityId.Low);
							break;
						case MailType.Auction:
							packet.Write(letter.SenderEntityId.Low);
							break;
						case MailType.Item:
							packet.WriteUInt(0); // ? 3.2.2
							// What should go here?
							break;
						default:
							break;
					}

					packet.Write(letter.CashOnDelivery);
					packet.Write(letter.TextId);

					packet.Write(0u);
					packet.Write((uint) letter.MessageStationary);
					packet.Write(letter.IncludedMoney);

					var flags = letter.ReadTime != null ? MailListFlags.Read : MailListFlags.NotRead;
					switch (letter.MessageType)
					{
						case MailType.Normal:
							flags |= MailListFlags.Delete;
							if (letter.IncludedItemCount > 0)
							{
								flags |= MailListFlags.Return;
							}
							break;
						case MailType.Auction:
							flags |= MailListFlags.Auction;
							break;
					}
					packet.Write((uint) flags);

					packet.Write((float) ((letter.ExpireTime - DateTime.Now).TotalMilliseconds/(24*60*60*1000)));
					packet.Write(0u);
					packet.Write(letter.Subject);

					if (letter.IncludedItemCount == 0)
					{
						// No items
						packet.Write((byte) 0);
					}
					else
					{
						// There are items to display
						var items = letter.IncludedItems;
						packet.Write((byte) items.Count);
						byte i = 0;
						foreach (var record in items)
						{
							packet.Write(i++);
							if (record != null)
							{
								packet.Write(record.EntityLowId);
								packet.Write(record.EntryId);

								if (record.EnchantIds != null)
								{
									for (var j = 0; j < EnchantCount; ++j)
									{
										var enchantId = record.EnchantIds[j];
										if (enchantId != 0)
										{
											var enchant = EnchantMgr.GetEnchantmentEntry((uint) enchantId);
											if (enchant != null)
											{
												packet.Write(0); // charges
												if (j == (int) EnchantSlot.Temporary)
												{
													packet.Write(record.EnchantTempTime);
												}
												else
												{
													packet.Write(0);
												}
												packet.Write(enchantId);
												continue;
											}
										}
										packet.Write(0);
										packet.Write(0);
										packet.Write(0);
									}
								}
								else
								{
									for (var j = 0; j < EnchantCount; ++j)
									{
										packet.Write(0u);
										packet.Write(0);
										packet.Write(0u);
									}
								}

								packet.Write(record.RandomProperty);
								packet.Write(record.RandomSuffix);
								packet.Write(record.Amount);
								packet.Write((uint) record.Charges);
								packet.Write(record.Template.MaxDurability);
								packet.Write(record.Durability);
								packet.Write((byte) 0);
							}
							else
							{
								packet.Write(0u);
								packet.Write(0u);

								for (byte j = 0; j < EnchantCount; ++j)
								{
									packet.Write(0u);
									packet.Write(0u);
									packet.Write(0u);
								}

								packet.Write(0);
								packet.Write(0);
								packet.Write(0);
								packet.Write(0);
								packet.Write(0);
								packet.Write(0);
								packet.Write((byte)0);
							}
						}
					}

					var endPos = packet.Position;
					packet.Position = sizePos;
					packet.Write((ushort) (endPos - sizePos));
					packet.Position = endPos;
				}
				client.Send(packet);
			}
		}

		/// <summary>
		/// Notifies the Client that there is new mail
		/// </summary>
		public static void SendNotify(IRealmClient client)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_RECEIVED_MAIL, 4))
			{
				packet.Write(0);
				client.Send(packet);
			}
		}

		public static void SendNextMailTime(IPacketReceiver client, ICollection<MailMessage> mail)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_QUERY_NEXT_MAIL_TIME, 8))
			{
				if (mail.Count <= 0)
				{
					packet.Write(0xC7A8C000);
					packet.Write(0u);
					client.Send(packet);
					return;
				}

				packet.Write(0u);
				packet.Write((uint)mail.Count);

				foreach (var letter in mail)
				{
					packet.Write(letter.SenderEntityId);
					switch (letter.MessageType)
					{
						case MailType.Auction:
							packet.Write(2u);
							packet.Write(2u);
							packet.Write((uint)letter.MessageStationary);
							break;
						default:
							packet.Write(0u);
							packet.Write(0u);
							packet.Write((uint)letter.MessageStationary);
							break;
					}
					packet.Write(-9f); // what does this represent ??
				}
				client.Send(packet);
			}
		}

		public static void SendItemTextQueryResponce(IPacketReceiver client, uint id, string text)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ITEM_TEXT_QUERY_RESPONSE))
			{
				packet.Write(id);
				packet.Write(text);

				client.Send(packet);
			}
		}
		#endregion
	}
}
