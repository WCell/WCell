using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Database.Entities.Pets;

namespace WCell.RealmServer.Database.Mappings
{
    public class PetSpellMap : ClassMap<PetSpell>
    {
        public PetSpellMap()
        {
			Not.LazyLoad();
			Id(x => x.Guid).GeneratedBy.GuidComb(); //TODO: Work out what to do about the Guid -> Long type change & how it affects WoW (seems unused)
            Map(x => x.SpellId).Not.Nullable();
            Map(x => x.State).Not.Nullable();
        }
    }
}
