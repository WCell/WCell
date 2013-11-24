using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using WCell.RealmServer.Database.Entities;
using WCell.RealmServer.NPCs.Auctioneer;

namespace WCell.RealmServer.Database.Mappings
{
	public class AuctionMap : ClassMap<Auction>
	{
		public AuctionMap()
		{
			Id(x => x.ItemLowId).GeneratedBy.Assigned();
			Map(x => x.TimeEnds).Not.Nullable();
			Map(x => x.AuctionId).Not.Nullable().Access.CamelCaseField();
			Map(x => x.ItemTemplateId).Not.Nullable();
			Map(x => x.OwnerLowId).Not.Nullable();
			Map(x => x.BidderLowId).Not.Nullable();
			Map(x => x.CurrentBid).Not.Nullable();
			Map(x => x.BuyoutPrice).Not.Nullable();
			Map(x => x.Deposit).Not.Nullable();
			Map(x => x.HouseFaction).Not.Nullable();
		}
	}
}
