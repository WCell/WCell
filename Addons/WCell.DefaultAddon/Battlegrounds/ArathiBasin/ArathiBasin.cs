using System.Linq;
using WCell.Addons.Default.Battlegrounds.ArathiBasin.Bases;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Core.Initialization;
using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.Chat;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Lang;
using WCell.Util.Variables;
using WCell.RealmServer.Entities;

namespace WCell.Addons.Default.Battlegrounds.ArathiBasin
{
	public class ArathiBasin : Battleground
	{
        #region Static Fields
	    [Variable("ABMaxScore")]
	    public static int MaxScoreDefault
	    {
            get { return Constants.World.WorldStates.GetState(WorldStateId.ABMaxResources).DefaultValue; }
            set { Constants.World.WorldStates.GetState(WorldStateId.ABMaxResources).DefaultValue = value; }
	    }

        static ArathiBasin()
        {
            MaxScoreDefault = 1600;
        }

		[Variable("ABFlagRespawnTimeMillis")]
        public static int FlagRespawnTimeMillis = 20 * 1000;

        [Variable("ABPrepTimeMillis")]
        public static int ABPreparationTimeMillis = 60 * 1000;

        [Variable("ABUpdateDelay")]
        public static int BattleUpdateDelayMillis = 1000;

        [Variable("ABPowerUpRespawnTime")]
        public static int PowerUpRespawnTimeMillis = 90 * 1000;

        public float DefaultScoreTickDelay = 12;
        #endregion

        public static GOEntry FlagStandNeutral;
        public static GOEntry FlagStandHorde;
        public static GOEntry FlagStandAlliance;

        public readonly ArathiBase[] Bases;
        public int MaxScore;
	    
	    private uint _hordeTicks, _allianceTicks;

	    #region Props

        public int HordeScore
        {
            get { return WorldStates.GetInt32(WorldStateId.ABResourcesAlliance); }
            set
            {
                WorldStates.SetInt32(WorldStateId.ABResourcesAlliance, value);
                if (value >= MaxScore)
                {
                    FinishFight();
                }
            }
        }

        public int AllianceScore
        {
            get { return WorldStates.GetInt32(WorldStateId.ABResourcesHorde); }
            set
            {
                WorldStates.SetInt32(WorldStateId.ABResourcesHorde, value);
                if (value >= MaxScore)
                {
                    FinishFight();
                }
            }
        }

	    public int HordeBaseCount { get; set; }

	    public int AllianceBaseCount { get; set; }

	    public override int PreparationTimeMillis
        {
			get { return ABPreparationTimeMillis; }
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
            CallPeriodically(BattleUpdateDelayMillis, Update);
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
            var msg = string.Format("The battle for Arathi Basin begins in {0} seconds.", PreparationTimeMillis / 2000);
            Characters.SendSystemMessage(msg);
        }


        protected override void OnPrepare()
        {
            base.OnPrepare();

        	var time = RealmLocalizer.FormatTimeSecondsMinutes(PreparationTimeMillis/1000);
            Characters.SendSystemMessage("The battle for Arathi Basin begins in {0}.", time);
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

        #endregion

        private void Update()
        {
            foreach(var team in _teams)
            {
                int scoreTick = 10;
                int bases = 0;

                if(team.Side == BattlegroundSide.Horde)
                {
                    bases = Bases.Count(node => node.BaseOwner == BattlegroundSide.Horde && node.GivesScore);
                    _hordeTicks++;
                }

                else
                {
                    bases = Bases.Count(node => node.BaseOwner == BattlegroundSide.Alliance && node.GivesScore);
                    _allianceTicks++;
                }

                if(bases > 4)
                {
                    scoreTick = 30;
                }

                // See http://www.wowwiki.com/Arathi_Basin#Accumulating_Resources
                var tickLength = (5 - bases) * DefaultScoreTickDelay / 4;

                if(tickLength < 1)
                {
                    tickLength = 1;
                }

                if (team.Side == BattlegroundSide.Horde)
                {
                    if(_hordeTicks == tickLength)
                    {
                        HordeScore += scoreTick;
                        _hordeTicks = 0;
                    }
                }

                else
                {
                    if(_allianceTicks == tickLength)
                    {
                        AllianceScore += scoreTick;
                        _allianceTicks = 0;
                    }
                }
            }
        }

		#region Spell/GO fixes

		[Initialization]
        [DependentInitialization(typeof(GOMgr))]
        public static void FixGOs()
        {
            //Fix the neutral/horde/alliance flags
            //3 entries for each colour
            //5 templates for each base
        }

        #endregion
    }
    public delegate void BaseHandler(Character chr);
}