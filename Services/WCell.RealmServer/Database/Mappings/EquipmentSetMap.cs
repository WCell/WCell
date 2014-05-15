using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using WCell.RealmServer.Database.Entities;

namespace WCell.RealmServer.Database.Mappings
{
	public class EquipmentSetMap : ClassMap<EquipmentSet>
	{
		public EquipmentSetMap() //TODO: This looks as though it needs to have Items mapped as well
		{
			Not.LazyLoad();
			Id(x => x.Id);
			Map(x => x.Name);
			Map(x => x.Icon);
		}
	}
}
