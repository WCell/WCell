using FluentNHibernate.Mapping;
using WCell.RealmServer.Items;

namespace WCell.RealmServer.Database.Mappings
{
    internal class PetitionRecordMap : ClassMap<PetitionRecord>
    {
        public PetitionRecordMap()
        {
			Not.LazyLoad();
            Id(x => x.OwnerId);
            Map(x => x.Type).Not.Nullable();
            Map(x => x.ItemId).Not.Nullable();
            Map(x => x.Name).Not.Nullable().Unique();
            HasMany(x => x.SignedIds).Element("SignedIds");
        }
    }
}
