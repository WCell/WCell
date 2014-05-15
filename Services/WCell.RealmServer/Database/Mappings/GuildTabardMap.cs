using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using WCell.RealmServer.Guilds;

namespace WCell.RealmServer.Database.Mappings
{
    public class GuildTabardMap : ClassMap<GuildTabard>
    {
        public GuildTabardMap()
        {
			Not.LazyLoad();
			Id(x => x.Id);
            Map(x => x.EmblemStyle);
            Map(x => x.EmblemColor);
            Map(x => x.BorderStyle);
            Map(x => x.BorderColor);
            Map(x => x.BackgroundColor);
        }
    }
}
