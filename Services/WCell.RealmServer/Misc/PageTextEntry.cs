using WCell.Constants;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.RealmServer.Items;
using WCell.Util;
using WCell.Util.Data;
using WCell.Util.Variables;

namespace WCell.RealmServer.Misc
{
    [DataHolder]
    public class PageTextEntry : IDataHolder
    {
        [NotVariable]
        public static PageTextEntry[] Entries = new PageTextEntry[10000];

        public static PageTextEntry GetEntry(uint id)
        {
            return Entries.Get(id);
        }

        [Initialization]
        [DependentInitialization(typeof(ItemMgr))]
        public static void InitPageTexts()
        {
            ContentMgr.Load<PageTextEntry>();
            foreach (var entry in Entries)
            {
                if (entry != null && entry.NextPageId != 0)
                {
                    entry.NextPageEntry = GetEntry(entry.NextPageId);
                }
            }
        }

        public uint PageId;
        public uint NextPageId;

        [Persistent((int)ClientLocale.End)]
        public string[] Texts = new string[(int)ClientLocale.End];

        [NotPersistent]
        public PageTextEntry NextPageEntry;

        public void FinalizeDataHolder()
        {
            ArrayUtil.Set(ref Entries, PageId, this);
        }
    }
}