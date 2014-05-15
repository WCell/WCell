using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using WCell.RealmServer.NPCs.Pets;

namespace WCell.RealmServer.Database.Mappings
{
	public class PermanentPetRecordMap : ClassMap<PermanentPetRecord>
	{
		public PermanentPetRecordMap()
		{
			Not.LazyLoad();
			#region Base Fields
			Id(x => x.EntryId).GeneratedBy.Assigned();
			Map(x => x.NameTimeStamp);
			Map(x => x.PetState).Not.Nullable();
			Map(x => x.AttackMode).Not.Nullable();
			Map(x => x.Flags).Not.Nullable();
			Map(x => x.OwnerId).Not.Nullable();
			Map(x => x.IsActivePet);
			Map(x => x.Name);
			Map(x => x.ActionButtons).Not.Nullable();
			#endregion

			Map(x => x.PetNumber).Not.Nullable();
			Map(x => x.Experience);
			Map(x => x.Level);
			Map(x => x.StabledSince);
			Map(x => x.LastTalentResetTime);
			Map(x => x.TalentResetPriceTier);
			Map(x => x.FreeTalentPoints).Not.Nullable();
		}
	}
}
