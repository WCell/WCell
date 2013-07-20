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
            Id(x => x.MapId);
            Id(x => Reveal.Member<int>("_InstanceId"));
            Map(x => x.DifficultyIndex).Not.Nullable();
            Map(x => x.BindTime).Not.Nullable();


        }
    }
}
