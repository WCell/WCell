using System;
using WCell.Core.DBC;

namespace WCell.RealmServer.Spells
{

    [Flags]
    public enum ShapeshiftInfoFlags : uint
    {

        /// <summary>
        /// Only used in cat form
        /// </summary>
        AgilityBasedAttackPower = 0x20,
    }

    public class ShapeshiftInfo
    {
        public int Id;
        public uint BarOrder;
        public string Name;
        public ShapeshiftInfoFlags Flags;
        public int CreatureFamily;
        public int AttackSpeed;
        public int[] DefaultActionBarSpells;
    }

    public class ShapeshiftInfoConverter : AdvancedDBCRecordConverter<ShapeshiftInfo>
    {
        public override ShapeshiftInfo ConvertTo(byte[] rawData, ref int id)
        {
            var ssInfo = new ShapeshiftInfo();

            uint index = 0;
            ssInfo.Id = id = GetInt32(rawData, index++);
            ssInfo.BarOrder = GetUInt32(rawData, index++);
            ssInfo.Name = GetString(rawData, ref index);
            ssInfo.Flags = (ShapeshiftInfoFlags) GetUInt32(rawData, index++);
            ssInfo.CreatureFamily = GetInt32(rawData, index++);
            ssInfo.AttackSpeed = GetInt32(rawData, index++);

            ssInfo.DefaultActionBarSpells = new int[12];
            for (int i = 0; i < 12;i++)
            {
                ssInfo.DefaultActionBarSpells[i] = GetInt32(rawData, index++);
            }

            return base.ConvertTo(rawData, ref id);
        }
    }
}
