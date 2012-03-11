using System;
using WCell.Constants;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Core.DBC;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;

namespace WCell.RealmServer.Spells
{
    [Flags]
    public enum ShapeshiftInfoFlags : uint
    {
        /// <summary>
        /// This is used for stances and anything that does not really shift the form of the caster
        /// </summary>
        NotActualShapeshift = 0x0001,

        /// <summary>
        /// Only used in cat form
        /// </summary>
        AgilityBasedAttackPower = 0x20
    }

    public class ShapeshiftEntry
    {
        public ShapeshiftForm Id;
        public uint BarOrder;
        public string Name;
        public ShapeshiftInfoFlags Flags;
        public CreatureType CreatureType;
        /// <summary>
        /// In millis
        /// </summary>
        public int AttackTime;
        public uint ModelIdAlliance;
        public uint ModelIdHorde;
        public SpellId[] DefaultActionBarSpells;

        public UnitModelInfo ModelAlliance
        {
            get { return UnitMgr.GetModelInfo(ModelIdAlliance); }
        }

        public UnitModelInfo ModelHorde
        {
            get { return UnitMgr.GetModelInfo(ModelIdHorde); }
        }

        #region Custom Settings

        public PowerType PowerType = PowerType.End;

        #endregion Custom Settings
    }

    public class ShapeshiftEntryConverter : DBCRecordConverter
    {
        public override void Convert(byte[] rawData)
        {
            var entry = new ShapeshiftEntry();

            int index = 0;
            entry.Id = (ShapeshiftForm)GetInt32(rawData, index++);
            entry.BarOrder = GetUInt32(rawData, index++);
            entry.Name = GetString(rawData, ref index);

            entry.Flags = (ShapeshiftInfoFlags)GetUInt32(rawData, index++);
            entry.CreatureType = (CreatureType)GetInt32(rawData, index++);
            index++;
            entry.AttackTime = GetInt32(rawData, index++);
            entry.ModelIdAlliance = GetUInt32(rawData, index++);
            entry.ModelIdHorde = GetUInt32(rawData, index++);

            index += 2;	// always 0

            entry.DefaultActionBarSpells = new SpellId[8];
            for (int i = 0; i < 8; i++)
            {
                entry.DefaultActionBarSpells[i] = (SpellId)GetInt32(rawData, index++);
            }

            SpellHandler.ShapeshiftEntries[(int)entry.Id] = entry;
        }
    }
}