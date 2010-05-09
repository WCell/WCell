using WCell.Addons.Default.Battlegrounds.ArathiBasin.Bases;
using WCell.Core.Initialization;
using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.Chat;
using WCell.RealmServer.GameObjects;
using WCell.Util.Variables;
using WCell.RealmServer.Entities;

namespace WCell.Addons.Default.Battlegrounds.ArathiBasin
{
	public class ArathiBasin : Battleground
	{
        #region Static Fields
        [Variable("ABMaxScore")]
        public static int MaxScoreDefault = 2000;

        [Variable("ABFlagRespawnTime")]
        public static int FlagRespawnTime = 20;

        [Variable("ABPrepTimeSecs")]
        public static int PreparationTimeSecs = 60;

        [Variable("ABUpdateDelay")]
        public static float BattleUpdateDelay = 1f;

        [Variable("ABPowerUpRespawnTime")]
        public static float PowerUpRespawnTime = 1.5f * 60f;

	    public static float TickDecreaseRate = 3f;

        #endregion

        public readonly ArathiBase[] Bases;
        public int MaxScore;
	    public float AllianceUpdateTickDelay = 12;
	    public float HordeUpdateTickDelay = 12;

	    private uint _hordeScore, _allianceScore;
        private bool _update;
	    private int _hordeBasesCount, _allianceBasesCount;
        #region Props

        public uint HordeScore
        {
            get
            {
                return _hordeScore;
            }
            set
            {
                _hordeScore = value;
                if (_hordeScore >= MaxScore)
                {
                    FinishFight();
                }
            }
        }

        public uint AllianceScore
        {
            get
            {
                return _allianceScore;
            }
            set
            {
                _allianceScore = value;
                if (_allianceScore >= MaxScore)
                {
                    FinishFight();
                }
            }
        }

        public int HordeBasesCount
        {
            get
            {
                return _hordeBasesCount;
            }
            set
            {
                //Bases increased, tick delay decreases.
                if (value > _hordeBasesCount)
                {
                    HordeUpdateTickDelay -= TickDecreaseRate;
                }
                //Bases decreased, tick delay increases
                if (_hordeBasesCount > value)
                {
                    HordeUpdateTickDelay += TickDecreaseRate;
                }
                _hordeBasesCount = value;
            }
        }

        public int AllianceBasesCount
        {
            get
            {
                return _allianceBasesCount;
            }
            set
            {
                //Bases increased, tick delay decreases.
                if(value > _allianceBasesCount)
                {
                    AllianceUpdateTickDelay -= TickDecreaseRate;
                }
                //Bases decreased, tick delay increases
                if(_allianceBasesCount > value)
                {
                    AllianceUpdateTickDelay += TickDecreaseRate;
                }
                _allianceBasesCount = value;
            }
        }

        public override float PreparationTimeSeconds
        {
            get { return PreparationTimeSecs; }
        }
        #endregion

	    public ArathiBasin()
        {
            Bases = new ArathiBase[(int)ArathiBases.End];
        }

        #region Overrides

        protected override void InitRegion()
        {
            base.InitRegion();
            Bases[(int)ArathiBases.Blacksmith] = new Blacksmith(this);
            Bases[(int)ArathiBases.Farm] = new Farm(this);
            Bases[(int)ArathiBases.GoldMine] = new GoldMine(this);
            Bases[(int)ArathiBases.Lumbermill] = new LumberMill(this);
            Bases[(int)ArathiBases.Stables] = new Stables(this);

            MaxScore = MaxScoreDefault;
        }

        protected override BattlegroundStats CreateStats()
        {
            return new ArathiStats();
        }

        /// <summary>
        /// Called when the battle starts (perparation ends now)
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();

            Characters.SendSystemMessage("Let the battle for Arathi Basin begin!");
            _update = true;
            //UpdateHordeScore();
            //UpdateAllianceScore();
        }

        protected override void OnFinish(bool disposing)
        {
            base.OnFinish(disposing);
            foreach (var character in Characters)
            {
                character.SendSystemMessage("The battle has ended!");
            }
        }
        protected override void OnPrepareHalftime()
        {
            base.OnPrepareHalftime();
            var msg = "The battle for Arathi Basin begins in " + PreparationTimeSeconds / 2f + " seconds.";
            Characters.SendSystemMessage(msg);
        }


        protected override void OnPrepare()
        {
            base.OnPrepare();
            var msg = "The battle for Arathi Basin begins in ";
            if ((int)PreparationTimeSeconds / 60 < 1)
            {
                msg += (int)PreparationTimeSeconds + " seconds.";
            }
            else
            {
                msg += PreparationTimeSeconds / 60f + (int)PreparationTimeSeconds / 60f == 1 ? "minute." : "minutes.";
            }

            Characters.SendSystemMessage(msg);
        }

        /// <summary>
        /// Removes and drops the flag and it's aura when a player leaves.
        /// </summary>
        /// <param name="chr"></param>
        protected override void OnLeave(Character chr)
        {
            base.OnLeave(chr);

            Characters.SendSystemMessage("{0} has left the battle!", chr.Name);
        }

        /// <summary>
        /// Messages the players of a new character entering the battle.
        /// </summary>
        /// <param name="chr"></param>
        protected override void OnEnter(Character chr)
        {
            base.OnEnter(chr);

            Characters.SendSystemMessage("{0} has entered the battle!", chr.Name);
        }

        //public override void OnDeath(Character chr)
        //{
        //    base.OnDeath(chr);
        //    foreach (var arathiBase in Bases)
        //    {
        //        if (chr == arathiBase.Capturer)
        //        {
        //            arathiBase.InterruptCapture();
        //        }
        //    }
        //}

        #endregion

        /// <summary>
        /// Begins giving score for each base periodically. Requires _update to be true.
        /// </summary>
        /// <returns>If update enqueued successfully, calls itself and returns true. 
        /// Will not call periodically if _update is false.</returns>
        public bool UpdateHordeScore()
        {
            if (_update)
            {
                CallDelayed(UpdateDelay, () => UpdateHordeScore());
                return true;
            }
            return false;
        }

        public void UpdateAllianceScore()
        {
            
        }

        #region Spell/GO fixes

        [Initialization]
        [DependentInitialization(typeof(GOMgr))]
        public static void FixGOs()
        {
        }

        #endregion
    }
    public delegate void BaseHandler(Character chr);
}
