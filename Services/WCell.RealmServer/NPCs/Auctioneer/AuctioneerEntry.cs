using WCell.RealmServer.Entities;

namespace WCell.RealmServer.NPCs.Auctioneer
{
	public class AuctioneerEntry
	{
		public AuctionHouse Auctions;
		public AuctionHouseFaction LinkedHouseFaction;
		private readonly NPC npc;

		public AuctioneerEntry(NPC npc)
		{
			this.npc = npc;
			LinkAuctionSetter();
		}

		private void LinkAuctionSetter()
		{
			if (AuctionMgr.AllowInterFactionAuctions)
			{
				Auctions = AuctionMgr.Instance.NeutralAuctions;
				LinkedHouseFaction = AuctionHouseFaction.Neutral;
			}
			else
			{
				if (npc.Faction.IsAlliance)
				{
					Auctions = AuctionMgr.Instance.AllianceAuctions;
					LinkedHouseFaction = AuctionHouseFaction.Alliance;
				}
				else if (npc.Faction.IsHorde)
				{
					Auctions = AuctionMgr.Instance.HordeAuctions;
					LinkedHouseFaction = AuctionHouseFaction.Horde;
				}
				else
				{
					Auctions = AuctionMgr.Instance.NeutralAuctions;
					LinkedHouseFaction = AuctionHouseFaction.Neutral;
				}
			}
		}
	}
}
