using FluentNHibernate;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Instances;

namespace WCell.RealmServer.Database.Mappings
{
    public class InstanceBindingMap : ClassMap<InstanceBinding>
    {
        public InstanceBindingMap()
        {
			Not.LazyLoad();
			CompositeId().KeyProperty(x => x.MapId).KeyProperty(Reveal.Member<InstanceBinding>("_InstanceId"));
            Map(x => x.DifficultyIndex).Not.Nullable();
            Map(x => x.BindTime).Not.Nullable();
        }
    }
}
