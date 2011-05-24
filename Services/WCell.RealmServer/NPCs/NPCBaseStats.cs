using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Util.Data;

namespace WCell.RealmServer.NPCs
{
    [DataHolder]
    public class NPCBaseStats : IDataHolder
    {
        public byte Level;
        public byte Class;
        [Persistent(4)]
        public int[] BaseHpByExpansion = new int[(int)ClientId.Cataclysm + 1];
        public short BaseMana;
        public short BaseArmor;

        public int GenerateHealth(short expansion, float healthModifier)
        {
            return (int)((BaseHpByExpansion[expansion] * healthModifier) + 0.5f);
        }
        
        public int GenerateMana(float manaModifier)
        {
            return BaseMana == 0 ? 0 : (int) ((BaseMana*manaModifier) + 0.5f);
        }

        public int GenerateArmor(float armorModifier)
        {
            return (int)((BaseArmor * armorModifier) + 0.5f);
        }

        public void FinalizeDataHolder()
        {
            if(1==1)
            {
                this.ToString();
            }
            NPCMgr.NPCBaseStats.Add(new KeyValuePair<int, int>(Level, Class), this);
        }
    }
}
