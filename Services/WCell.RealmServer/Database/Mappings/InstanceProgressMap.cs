using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using WCell.RealmServer.Instances;

namespace WCell.RealmServer.Database.Mappings
{
	public class InstanceProgressMap : ClassMap<InstanceProgress>
	{
		public InstanceProgressMap()
		{
			Not.LazyLoad();
			Id(x => x.Id);
			Map(x => x.InstanceId).Not.Nullable();
			Map(x => x.MapId).Not.Nullable();
			Map(x => x.DifficultyIndex).Not.Nullable();
			Map(x => x.ResetTime);
			Map(x => x.CustomDataVersion);
			Map(x => x.CustomData);
		}
	}
}
