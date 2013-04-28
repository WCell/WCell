using FluentNHibernate.Mapping;
using WCell.RealmServer.Database.Entities;

namespace WCell.RealmServer.Database.Mappings
{
    public class SpellRecordMap : ClassMap<SpellRecord>
    {
        public SpellRecordMap()
        {
            //SpellRecordId
            Id(x => x.RecordId);
            Map(x => x.OwnerId).Not.Nullable();
            Map(x => x.SpecIndex);
            Map(x => x.Spell);
            Map(x => x.SpellId).Not.Nullable();
        }
    }
}
