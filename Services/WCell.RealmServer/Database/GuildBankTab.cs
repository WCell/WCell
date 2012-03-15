using System.Collections.Generic;
using Castle.ActiveRecord;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Guilds
{
    [ActiveRecord("GuildBankTabs", Access = PropertyAccess.Property)]
    public partial class GuildBankTab : ActiveRecordBase<GuildBankTab>
    {
        [PrimaryKey("TabId")]
        private long _TabId
        {
            get;
            set;
        }

        [Field("GuildId", NotNull = true)]
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

        [Property]
        public string Name
        {
            get;
            set;
        }

        [Property]
        public string Text
        {
            get;
            set;
        }

        [Property]
        public string Icon
        {
            get;
            set;
        }

        /// <summary>
        /// The Slot in the Bank's Tabs that this BankTab belongs in
        /// </summary>
        [Property]
        public int BankSlot
        {
            get;
            set;
        }

        [HasMany(typeof(GuildBankTabItemMapping), Inverse = true, Cascade = ManyRelationCascadeEnum.AllDeleteOrphan)]
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