using System;
using System.Linq;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.NPCs.Pets
{
	/// <summary>
	/// Summoned pets for which we only store ActionBar (and maybe name) settings
	/// </summary>
	//[ActiveRecord("Pets_Summoned", Access = PropertyAccess.Property)] TODO: Map this
	public class SummonedPetRecord : PetRecordBase<SummonedPetRecord>
	{
		//[Field("PetNumber", NotNull = true)]
		private int m_PetNumber;

		public override uint PetNumber
		{
			get { return (uint)m_PetNumber; }
			set { m_PetNumber = (int)value; }
		}

		public static SummonedPetRecord[] LoadSummonedPetRecords(uint ownerId)
		{
			try
			{
				return RealmWorldDBMgr.DatabaseProvider.Query<SummonedPetRecord>().Where(summonedPetRecord => summonedPetRecord.OwnerId == ownerId).ToArray();//FindAllByProperty("_OwnerLowId", (int)ownerId);
			}
			catch (Exception e)
			{
				RealmWorldDBMgr.OnDBError(e);
				return RealmWorldDBMgr.DatabaseProvider.Query<SummonedPetRecord>().Where(summonedPetRecord => summonedPetRecord.OwnerId == ownerId).ToArray();//FindAllByProperty("_OwnerLowId", (int)ownerId);
			}
		}
	}
}