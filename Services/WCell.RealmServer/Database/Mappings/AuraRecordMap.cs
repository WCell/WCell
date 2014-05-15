using FluentNHibernate.Mapping;
using WCell.RealmServer.Database.Entities;

namespace WCell.RealmServer.Database.Mappings
{
    public class AuraRecordMap : ClassMap<AuraRecord>
    {
        public AuraRecordMap()
        {
			Not.LazyLoad();
			Id(x => x.RecordId);
            Map(x => x.OwnerId).Not.Nullable();
            Map(x => x.CasterId);
            Map(x => x.Level);
            Map(x => x.SpellId);
            Map(x => x.MillisLeft);
            Map(x => x.StackCount);
            Map(x => x.IsBeneficial);
        }
    }
}
