using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using WCell.RealmServer.Database.Entities;

namespace WCell.RealmServer.Database.Mappings
{
    class AccountDataMap : ClassMap<AccountData>
    {
        public AccountDataMap()
        {
            Id(x => x.AccountId);
            Map(x => x.DataHolder);
            Map(x => x.SizeHolder);
            Map(x => x.TimeStamps);
        }
    }
}
