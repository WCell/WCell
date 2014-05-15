using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using WCell.RealmServer.Database.Entities;

namespace WCell.RealmServer.Database.Mappings
{
	public class ItemRecordMap : ClassMap<ItemRecord>
	{
		public ItemRecordMap()
		{
			Not.LazyLoad();
			Id(x => x.Guid).GeneratedBy.Assigned();
			Map(x => x.EntryId).Not.Nullable();
			Map(x => x.DisplayId).Not.Nullable();
			Map(x => x.ContainerSlot).Not.Nullable();
			Map(x => x.Flags).Not.Nullable();
			Map(x => x.OwnerId);
			Map(x => x.Slot).Not.Nullable();
			Map(x => x.CreatorEntityId).Not.Nullable();
			Map(x => x.GiftCreatorEntityId).Not.Nullable();
			Map(x => x.Durability).Not.Nullable();
			Map(x => x.Duration).Not.Nullable();
			Map(x => x.ItemTextId).Not.Nullable();
			Map(x => x.ItemText);
			Map(x => x.RandomProperty).Not.Nullable();
			Map(x => x.RandomSuffix).Not.Nullable();
			Map(x => x.Charges).Not.Nullable();
			Map(x => x.Amount).Not.Nullable();
			Map(x => x.ContSlots).Not.Nullable();
			Map(x => x.EnchantIds);
			Map(x => x.EnchantTempTime);
			Map(x => x.IsAuctioned);
			Map(x => x.MailId);
		}
	}
}
