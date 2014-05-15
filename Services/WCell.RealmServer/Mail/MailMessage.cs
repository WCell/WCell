using System;
using System.Collections.Generic;
using System.Linq;
using WCell.RealmServer.Database.Entities;
using WCell.Util.Logging;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Core;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using System.Threading;

namespace WCell.RealmServer.Mail
{
	//[ActiveRecord(Access = PropertyAccess.Property)]
	public class MailMessage //: ActiveRecordBase<MailMessage>
	{
		private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

		//private static readonly NHIdGenerator s_idGenerator = new NHIdGenerator(typeof(MailMessage), "Guid");
		#region ID Genereator
		private static bool _idGeneratorInitialised;
		private static long _highestId;

		private static void Init()
		{
			//long highestId;
			try
			{
				//_highestId = RealmWorldDBMgr.DatabaseProvider.Query<QuestRecord>().Max(questRecord => questRecord.QuestRecordId);
				MailMessage highestItem = null;
				highestItem = RealmWorldDBMgr.DatabaseProvider.Session.QueryOver<MailMessage>().OrderBy(record => record.Guid).Desc.Take(1).SingleOrDefault();
				_highestId = highestItem != null ? highestItem.Guid : 0;

			}
			catch (Exception e)
			{
				RealmWorldDBMgr.OnDBError(e);
				MailMessage highestItem = null;
				highestItem = RealmWorldDBMgr.DatabaseProvider.Session.QueryOver<MailMessage>().OrderBy(record => record.Guid).Desc.Take(1).SingleOrDefault();
				_highestId = highestItem != null ? highestItem.Guid : 0;
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
				Init();

			return Interlocked.Increment(ref _highestId);
		}

		public static long LastId
		{
			get
			{
				if (!_idGeneratorInitialised)
					Init();
				return Interlocked.Read(ref _highestId);
			}
		}
		#endregion

		//[Field("ReceiverId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private uint _receiverId;

		//[Field("SenderId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _senderId;

		//[Field("IncludedMoney", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private long _includedMoney;

		//[Field("CashOnDelivery", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private long _cashOnDelivery;

		ICollection<ItemRecord> _items;

		/// <summary>
		/// Create an exisiting MailMessage
		/// </summary>
		public MailMessage()
		{
		}

		/// <summary>
		/// Create a new MailMessage
		/// </summary>
		public MailMessage(string subject, string body)
		{
			Guid = NextId();
			TextId = (uint)MailMgr.NextId(); //TODO: Re-implement textidgenerator

			Subject = subject;
			Body = body;
		}

		public CharacterRecord Recipient
		{
			get;
			set;
		}

		//[PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
		public long Guid
		{
			get;
			set;
		}

		//[Version(UnsavedValue = "null")]
		public DateTime? LastModifiedOn
		{
			get;
			set;
		}


		//[Property(NotNull = true)]
		public MailType MessageType
		{
			get;
			set;
		}

		//[Property(NotNull = true)]
		public MailStationary MessageStationary
		{
			get;
			set;
		}

		public uint ReceiverId
		{
			get { return _receiverId; }
			set { _receiverId = value; }
		}

		public uint SenderId
		{
			get { return (uint)_senderId; }
			set { _senderId = (int)value; }
		}

		public EntityId ReceiverEntityId
		{
			get { return EntityId.GetPlayerId((uint)_receiverId); }
			set { _receiverId = value.Low; }
		}

		public EntityId SenderEntityId
		{
			get { return EntityId.GetPlayerId((uint)_senderId); }
			set { _senderId = (int)value.Low; }
		}

		//[Property(NotNull = true, Length = 512)]
		public string Subject
		{
			get;
			set;
		}

		//[Property(NotNull = true, Length = 1024*8)]
		/// <summary>
		/// The body of the message
		/// </summary>
		public string Body
		{
			get;
			set;
		}

		public uint TextId
		{
			get { return (uint)_TextId; }
			set { _TextId = (int)value; }
		}

		//[Property("TextId", NotNull = true)]
		public int _TextId
		{
			get;
			set;
		}

		//[Property(NotNull = true)]
		public DateTime SendTime
		{
			get;
			set;
		}

		//[Property(NotNull = true)]
		public DateTime DeliveryTime
		{
			get;
			set;
		}

		public int RemainingDeliveryMillis
		{
			get { return (int)(DeliveryTime - DateTime.Now).TotalMilliseconds; }
		}

		public bool WasRead
		{
			get { return ReadTime != null; }
		}

		//[Property]
		public DateTime? ReadTime
		{
			get;
			set;
		}

		//[Property(NotNull = true)]
		public DateTime ExpireTime
		{
			get;
			set;
		}

		//[Property]
		public DateTime? DeletedTime
		{
			get;
			set;
		}

		public uint IncludedMoney
		{
			get { return (uint)_includedMoney; }
			set { _includedMoney = value; }
		}

		public uint CashOnDelivery
		{
			get { return (uint)_cashOnDelivery; }
			set { _cashOnDelivery = value; }
		}

		//[Property(NotNull = true)]
		public bool CopiedToItem
		{
			get;
			internal set;
		}

		#region Items
		//[Property]
		public int IncludedItemCount
		{
			get;
			private set;
		}

		/// <summary>
		/// The list of included items. 
		/// Returns null if no items are added.
		/// </summary>
		public ICollection<ItemRecord> IncludedItems
		{
			get
			{
				if (IncludedItemCount == 0)
				{
					return null;
				}

				if (_items == null)
				{
					if (Recipient != null)
					{
						_items = Recipient.GetMailItems(Guid, IncludedItemCount);
					}
					else
					{
						_items = RealmWorldDBMgr.DatabaseProvider.Query<ItemRecord>().Where(itemRecord => itemRecord.MailId == Guid).ToList(); //ItemRecord.FindAllByProperty("MailId", Guid).ToList();
					}
				}
				return _items;
			}
		}

		#region Add / Remove Items
		public ItemRecord AddItem(ItemId item)
		{
			var template = ItemMgr.GetTemplate(item);
			if (template == null)
			{
				return null;
			}
			return AddItem(template);
		}

		public ItemRecord AddItem(ItemTemplate template)
		{
			var record = ItemRecord.CreateRecord(template);
			AddItem(record);
			return record;
		}

		public void AddItem(ItemRecord record)
		{
			if (_items == null)
			{
				_items = new List<ItemRecord>(3);
			}

			record.MailId = Guid;
			_items.Add(record);
			IncludedItemCount++;
		}

		public ItemRecord RemoveItem(uint lowId)
		{
			var items = IncludedItems;
			if (items != null)
			{
				foreach (var item in items)
				{
					if (item.EntityLowId == lowId)
					{
						items.Remove(item);
						IncludedItemCount = items.Count;
						return item;
					}
				}
			}
			return null;
		}
		#endregion

		public void SetItems(ICollection<Item> items)
		{
			if (_items != null)
			{
				throw new InvalidOperationException("Tried to set Items after Items were already set: " + this);
			}

			_items = new List<ItemRecord>(items.Count);
			foreach (var item in items)
			{
				item.Remove(true);
				item.Record.MailId = Guid;
				_items.Add(item.Record);
			}
			IncludedItemCount = _items.Count;
		}

		public void SetItems(ICollection<ItemRecord> items)
		{
			if (_items != null)
			{
				throw new InvalidOperationException("Tried to set Items after Items were already set: " + this);
			}

			_items = new List<ItemRecord>(items.Count);
			foreach (var item in items)
			{
				item.MailId = Guid;
				_items.Add(item);
			}
			IncludedItemCount = _items.Count;
		}

		internal void PutBack(ItemRecord item)
		{
			_items.Add(item);
			IncludedItemCount++;
		}
		#endregion

		public bool IsDeleted
		{
			get
			{
				return (DeletedTime != null && DeletedTime.Value < DateTime.Now);
			}
		}

		//public override void Create()
		//{
		//	if (_items != null)
		//	{
		//		foreach (var item in _items)
		//		{
		//			item.Save();
		//		}
		//	}
		//	base.Create();
		//}

		//public override void Update()
		//{
		//	if (_items != null)
		//	{
		//		foreach (var item in _items)
		//		{
		//			item.Save();
		//		}
		//	}
		//	base.Update();
		//}

		//public override void Save()
		//{
		//	if (_items != null)
		//	{
		//		foreach (var item in _items)
		//		{
		//			item.Save();
		//		}
		//	}
		//	base.Save();
		//}

		/// <summary>
		/// Delete letter and all containing Items
		/// </summary>
		public void Destroy()
		{
			if (IncludedItemCount > 0)
			{
				foreach(var itemRecord in RealmWorldDBMgr.DatabaseProvider.Query<ItemRecord>().Where(itemRecord => itemRecord.MailId == Guid))
				{
					RealmWorldDBMgr.DatabaseProvider.Delete(itemRecord);
				}
				//ItemRecord.DeleteAll("MailId = " + Guid);
				_items = null;
			}
			//Delete();
			RealmWorldDBMgr.DatabaseProvider.Delete(this);
		}

		/// <summary>
		/// Returns to sender or Destroys the mail (if sender doesn't exist)
		/// </summary>
		public void ReturnToSender()
		{
			RealmServer.IOQueue.ExecuteInContext(() =>
			{
				if (!CharacterRecord.Exists(SenderId))
				{
					// can't return
					Destroy();
				}
				else
				{
					// switch sender and receiver and fix Subject
					SenderId = ReceiverId;
					ReceiverId = SenderId;
					Subject += " [Returned]";
					Send();
				}
			});
		}

		public void Send()
		{
			MailMgr.SendMail(this);
		}

		public static IEnumerable<MailMessage> FindAllMessagesFor(uint charLowId)
		{
			return RealmWorldDBMgr.DatabaseProvider.Query<MailMessage>().Where(mailMessage => mailMessage.ReceiverId == charLowId); //FindAllByProperty("_receiverId", (int)charLowId);
		}
	}
}