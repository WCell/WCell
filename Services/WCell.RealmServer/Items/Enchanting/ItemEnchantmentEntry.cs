using WCell.Constants.Skills;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Items.Enchanting
{
    /// <summary>
    ///
    /// </summary>
    public class ItemEnchantmentEntry
    {
        public uint Id;
        public uint Charges;
        public ItemEnchantmentEffect[] Effects;
        public string Description;
        public uint Visual;
        public uint Flags; // slot, but m_flags is the official name
        public uint SourceItemId;
        public uint ConditionId;
        public int RequiredSkillAmount;

        public ItemTemplate GemTemplate;

        //public ItemId GemId;
        //public ItemTemplate GemTemplate
        //{
        //    get { return ItemMgr.GetTemplate(GemId); }
        //}

        /// <summary>
        ///
        /// </summary>
        public ItemEnchantmentCondition Condition;

        public SkillId RequiredSkillId;

        public override string ToString()
        {
            return string.Format("{0} (Id: {1})", Description, Id);
        }

        public bool CheckRequirements(Unit enchanter)
        {
            if (enchanter is Character)
            {
                return ((Character)enchanter).Skills.CheckSkill(RequiredSkillId, RequiredSkillAmount);
            }
            return true;
        }
    }
}