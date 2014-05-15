using FluentNHibernate.Mapping;
using WCell.RealmServer.Instances;

namespace WCell.RealmServer.Database.Mappings
{
    class GlobalInstanceTimerMap : ClassMap<GlobalInstanceTimer>
    {
        public GlobalInstanceTimerMap()
		{
			Not.LazyLoad();
			Id(x => x.MapId);
			Map(x => x.LastResets).Not.Nullable();
		}
    }
}
