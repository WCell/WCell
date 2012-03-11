namespace WCell.RealmServer.Items.Enchanting
{
    /// <summary>
    /// See SpellItemEnchantmentCondition.dbc
    ///
    /// TODO:
    /// </summary>
    public class ItemEnchantmentCondition
    {
        public uint Id;
        public uint[] LTOperandType; // 5
        public uint[] LTOperand; // 5
        public uint[] Operator;// 5
        public uint[] RTOperandType; // 5
        public uint[] RTOperand; // 5
        public uint[] Logic;// 5
    }
}