using System.Collections.Generic;
using WCell.RealmServer.Guilds;

namespace WCell.RealmServer.Database.Entities
{
	public partial class GuildBankTab
	{
		private long Id
		{
			get;
			set;
		}

		private int _guildId;

		private GuildBank m_Bank;

		public GuildBankTab()
		{
			Items = new GuildBankTabItemMapping[GuildMgr.MAX_BANK_TAB_SLOTS];
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
				_guildId = (int)m_Bank.Guild.Id;
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

		/// <summary>
		/// The Slot in the Bank's Tabs that this BankTab belongs in
		/// </summary>
		public int BankSlot
		{
			get;
			set;
		}

		private IList<GuildBankTabItemMapping> Items
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