using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Items;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.NPCs.Auctioneer 
{
    public class AuctionSearch 
    {
        private uint m_maxResultCount = 0;

        public uint MaxResultCount {
            get { return m_maxResultCount; }
            set { m_maxResultCount = value; }
        }

        public uint StartIndex {
            get;
            set;
        }

        public string Name {
            get;
            set;
        }

        public uint LevelRange1 { get; set; }

        public uint LevelRange2 { get; set; }

        public InventorySlotType InventoryType { get; set; }

        public ItemClass ItemClass { get; set; }

        public ItemSubClass ItemSubClass { get; set; }

        public int Quality { get; set; }

        public bool IsUsable { get; set; }



        public ICollection<Auction> RetrieveMatchedAuctions(AuctionHouse auctionHouse) 
        {
            var matchedAuctions = new List<Auction>();
            var auctions = auctionHouse.Auctions;
            foreach (var auction in auctions.Values) 
            {
                var item = AuctionMgr.Instance.AuctionItems[auction.ItemLowId];
                if (item == null)
                    continue;

                if (!string.IsNullOrEmpty(Name))  //name check
                {
                    if (!item.Template.DefaultName.ToLower().Contains(Name.ToLower()))
                        continue;
                }

                //if (LevelRange1 > 0 && item.Template.Level < LevelRange1)
                //    continue;               

                //if (LevelRange2 > 0 && item.Template.Level > LevelRange2)
                //    continue;

                //if (InventoryType != InventorySlotType.None && item.Template.InventorySlotType != InventoryType)
                //    continue;

                //if (ItemClass != ItemClass.None && item.Template.Class != ItemClass)
                //    continue;

                //if (ItemSubClass != ItemSubClass.None && item.Template.SubClass != ItemSubClass)
                //    continue;

                //if (Quality != -1 && item.Template.Quality != (ItemQuality)Quality)
                //    continue;

                matchedAuctions.Add(auction);
                MaxResultCount++;
            }
            return matchedAuctions;
        }
    }
}


