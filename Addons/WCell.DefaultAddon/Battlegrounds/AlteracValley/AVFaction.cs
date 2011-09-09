using WCell.Constants;
using WCell.Constants.GameObjects;
using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.Entities;

namespace WCell.Addons.Default.Battlegrounds.AlteracValley
{
    public abstract class AVFaction : BattlegroundFaction
    {
        public AlteracValley Instance;
        private GameObject _flag;
        private uint _reinforcements;

        protected AVFaction(AlteracValley instance, GOEntryId flagEntry)
        {
            Instance = instance;

        }

        #region Properties

        public abstract BattlegroundSide Side
        {
            get;
        }

        public abstract string Name
        {
            get;
        }

        public BattlegroundTeam Team
        {
            get { return Instance.GetTeam(Side); }
        }

        public AVFaction Opponent
        {
            get { return Instance.GetFaction(Side.GetOppositeSide()); }
        }

        public GameObject Banner
        {
            get { return _flag; }
            set { _flag = value; }
        }

        public uint Reinforcements
        {
            get { return _reinforcements; }
            set 
            {
                _reinforcements = value;
                if(value == 0)
                {
                    Instance.FinishFight();
                }
            }
        }

        public void Win()
        {
            Instance.Win(Team);
        }

        #endregion

        //public void TriggerCapture(GameObject banner, )
    }
}