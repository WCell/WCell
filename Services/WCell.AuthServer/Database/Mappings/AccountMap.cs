using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using WCell.AuthServer.Database.Entities;
using WCell.Constants;

namespace WCell.AuthServer.Database.Mappings
{
	public class AccountMap : ClassMap<Account>
	{
		public AccountMap()
		{
			Id(x => x.AccountId);
			Map(x => x.Created).Not.Nullable();
			Map(x => x.Name).Not.Nullable().Unique().Length(16);
			Map(x => x.Password).Not.Nullable().Length(20); //TODO: Find out if it specifying the column type is needed
			Map(x => x.EmailAddress);
			Map(x => x.ClientId).CustomType<ClientId>();
			Map(x => x.ClientVersion);
			Map(x => x.RoleGroupName).Not.Nullable().Length(16);
			Map(x => x.IsActive).Not.Nullable();
			Map(x => x.StatusUntil);
			Map(x => x.LastChanged);
			Map(x => x.LastLogin);
			Map(x => x.LastIP);
			Map(x => x.HighestCharLevel);
			Map(x => x.Locale);
		}
	}
}
