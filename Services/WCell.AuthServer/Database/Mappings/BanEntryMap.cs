using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using WCell.AuthServer.Database.Entities;

namespace WCell.AuthServer.Database.Mappings
{
	public class BanEntryMap : ClassMap<BanEntry>
	{
		public BanEntryMap()
		{
			Id(x => x.BanId);
			Map(x => x.Created).Not.Nullable();
			Map(x => x.Expires);
			Map(x => x.Reason);
			Map(x => x.BanMask).Not.Nullable();
		}
	}
}
