using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace WCell.RealmServer.Instances
{
    class GlobalInstanceTimerMap : ClassMap<GlobalInstanceTimer>
    {
        public GlobalInstanceTimerMap()
		{
			Id(x => x.MapId);
			Map(x => x.LastResets).Not.Nullable();
		}
    }
}
