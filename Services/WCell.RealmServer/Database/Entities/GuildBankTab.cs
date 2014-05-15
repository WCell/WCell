using System.Collections.Generic;
using WCell.RealmServer.Database;
using WCell.RealmServer.Database.Entities;

namespace WCell.RealmServer.Guilds
{
	public partial class GuildBankTab
	{
		private long Id
		{
			get;
			set;
		}

		//private Guild _guildId; TODO: Work out need for existance

		private GuildBank m_Bank;

		public GuildBankTab()
		{
			Items = new GuildBankTabItem[GuildMgr.MAX_BANK_TAB_SLOTS];
			ItemRecords = new ItemRecord[GuildMgr.MAX_BANK_TAB_SLOTS];
		}

		public GuildBankTab(GuildBank bank)
			: this()
		{
			Bank = bank;
		}

		public GuildBank Bank
		{
			get { return m_Bank; }
			internal set
			{
				m_Bank = value;
				Guild = m_Bank.Guild;
			}
		}

		public string Name
		{
			get;
			set;
		}

		public string Text
		{
			get;
			set;
		}

		public string Icon
		{
			get;
			set;
		}

		public Guild Guild //TODO: Map this & Setup as reference
		{
			get;
			set;
		}

		/// <summary>
		/// The Slot in the Bank's Tabs that this BankTab belongs in
		/// </summary>
		public int BankSlot
		{
			get;
			set;
		}

		private IList<GuildBankTabItem> Items
		{
			get;
			set;
		}

		private IList<ItemRecord> _itemRecords;

		public IList<ItemRecord> ItemRecords
		{
			set { _itemRecords = value; }
			get
			{
				if (_itemRecords == null)
				{
					_itemRecords = new ItemRecord[GuildMgr.MAX_BANK_TAB_SLOTS];
					foreach (var mapping in Items)
					{
						_itemRecords[mapping.TabSlot] = ItemRecord.GetRecordByID(mapping.Guid);
					}
				}
				return _itemRecords;
			}
		}
	}
}