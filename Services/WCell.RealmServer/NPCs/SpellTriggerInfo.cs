using WCell.Constants.Spells;
using WCell.RealmServer.Spells;
using WCell.Util.Data;

namespace WCell.RealmServer.NPCs
{
    public class SpellTriggerInfo
    {
        [NotPersistent]
        public Spell Spell;

        private SpellId m_SpellId;

        public SpellId SpellId
        {
            get { return m_SpellId; }
            set
            {
                m_SpellId = value;
                if (value != 0)
                {
                    Spell = SpellHandler.Get(value);
                }
            }
        }

        public uint QuestId;
    }
}