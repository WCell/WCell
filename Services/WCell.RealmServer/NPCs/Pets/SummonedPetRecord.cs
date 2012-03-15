using System;
using Castle.ActiveRecord;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.NPCs.Pets
{
    /// <summary>
    /// Summoned pets for which we only store ActionBar (and maybe name) settings
    /// </summary>
    [ActiveRecord("Pets_Summoned", Access = PropertyAccess.Property)]
    public class SummonedPetRecord : PetRecordBase<SummonedPetRecord>
    {
        [Field("PetNumber", NotNull = true)]
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
                return FindAllByProperty("_OwnerLowId", (int)ownerId);
            }
            catch (Exception e)
            {
                RealmDBMgr.OnDBError(e);
                return FindAllByProperty("_OwnerLowId", (int)ownerId);
            }
        }
    }
}