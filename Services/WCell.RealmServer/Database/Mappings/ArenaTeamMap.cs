using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using WCell.RealmServer.Database.Entities;

namespace WCell.RealmServer.Database.Mappings
{
	public class ArenaTeamMap : ClassMap<ArenaTeam>
	{
		public ArenaTeamMap()
		{
			Not.LazyLoad();
			Id(x => x.Id);
			Map(x => x.Name).Not.Nullable().Unique();
			Map(x => x.LeaderLowId).Not.Nullable();
			Map(x => x.Type).Not.Nullable();

		}
	}
}
