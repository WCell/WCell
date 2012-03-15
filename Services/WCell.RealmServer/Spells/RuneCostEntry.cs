using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells
{
    public class RuneCostEntry
    {
        public uint Id;
        public int[] CostPerType = new int[SpellConstants.StandardRuneTypeCount];
        public int RunicPowerGain;

        public int RequiredRuneAmount;

        public int GetCost(RuneType type)
        {
            return CostPerType[(int)type];
        }

        public bool CostsRunes
        {
            get { return RequiredRuneAmount > 0; }
        }

        public override string ToString()
        {
            return string.Format("{0}", Id);
        }
    }
}
