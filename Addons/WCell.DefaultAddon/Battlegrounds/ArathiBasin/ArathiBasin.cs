using System.Linq;
using WCell.Addons.Default.Battlegrounds.ArathiBasin.Bases;
using WCell.Addons.Default.Lang;
using WCell.Constants;
using WCell.Constants.GameObjects;
using WCell.Constants.World;
using WCell.Core.Initialization;
using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Lang;
using WCell.Util.Variables;

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
            MaxScoreDefault = 2000;
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

        #region Fields
        /*public static GOEntry FlagStandHorde;
        public static GOEntry FlagStandAlliance;*/

        private GameObject _allianceDoor;
        private GameObject _hordeDoor;

        public readonly ArathiBase[] Bases;
        public int MaxScore;
	    
	    private uint _hordeTicks, _allianceTicks;
        #endregion
	    #region Props

        public int HordeScore
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

        public int AllianceScore
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

	    public int HordeBaseCount 
        { 
            get { return WorldStates.GetInt32(WorldStateId.ABOccupiedBasesHorde); }
            set
            { 
                WorldStates.SetInt32(WorldStateId.ABOccupiedBasesHorde, value);
            }
        }

	    public int AllianceBaseCount
        {
            get { return WorldStates.GetInt32(WorldStateId.ABOccupiedBasesAlliance); }
            set
            {
                WorldStates.SetInt32(WorldStateId.ABOccupiedBasesAlliance, value);
            }
        }

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

            Characters.SendSystemMessage(DefaultAddonLocalizer.Instance.Translate(AddonMsgKey.ABOnStart));

            _allianceDoor.State = GameObjectState.Disabled;
            _hordeDoor.State = GameObjectState.Disabled;

            CallPeriodically(BattleUpdateDelayMillis, Update);
        }

        protected override void OnFinish(bool disposing)
        {
            base.OnFinish(disposing);
            foreach (var character in Characters)
            {
                character.SendSystemMessage(DefaultAddonLocalizer.Instance.Translate(AddonMsgKey.ABOnFinish));
            }
        }
        protected override void OnPrepareHalftime()
        {
            base.OnPrepareHalftime();
            var msg = DefaultAddonLocalizer.Instance.Translate(AddonMsgKey.ABOnPrepareHalfTime, PreparationTimeMillis / 2000);
            Characters.SendSystemMessage(msg);
        }


        protected override void OnPrepare()
        {
            base.OnPrepare();

        	var time = RealmLocalizer.FormatTimeSecondsMinutes(PreparationTimeMillis/1000);
            Characters.SendSystemMessage(DefaultAddonLocalizer.Instance.Translate(AddonMsgKey.ABOnPrepare, time));
        }

        /// <summary>
        /// Removes and drops the flag and it's aura when a player leaves.
        /// </summary>
        /// <param name="chr"></param>
        protected override void OnLeave(Character chr)
        {
            base.OnLeave(chr);

            Characters.SendSystemMessage(DefaultAddonLocalizer.Instance.Translate(AddonMsgKey.ABOnLeave, chr.Name));
        }

        /// <summary>
        /// Messages the players of a new character entering the battle.
        /// </summary>
        /// <param name="chr"></param>
        protected override void OnEnter(Character chr)
        {
            base.OnEnter(chr);

            Characters.SendSystemMessage(DefaultAddonLocalizer.Instance.Translate(AddonMsgKey.ABOnEnter, chr.Name));
        }

        protected override void SpawnGOs()
        {
            base.SpawnGOs();
            GOEntry allianceDoorEntry = GOMgr.GetEntry(GOEntryId.ALLIANCEDOOR);
            GOEntry hordeDoorEntry = GOMgr.GetEntry(GOEntryId.HORDEDOOR);
            
            _allianceDoor = allianceDoorEntry.FirstSpawn.Spawn(this);
            _hordeDoor = hordeDoorEntry.FirstSpawn.Spawn(this);
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

		#region Spell/GO fixes and event registration

		[Initialization]
        [DependentInitialization(typeof(GOMgr))]
        public static void FixGOs()
        {
            GOEntry allianceDoorEntry = GOMgr.GetEntry(GOEntryId.ALLIANCEDOOR);
            GOEntry hordeDoorEntry = GOMgr.GetEntry(GOEntryId.HORDEDOOR);
            
            allianceDoorEntry.FirstSpawn.State = GameObjectState.Enabled;
            allianceDoorEntry.Flags |= GameObjectFlags.DoesNotDespawn | GameObjectFlags.InUse;
            allianceDoorEntry.FirstSpawn.AutoSpawn = false;

            hordeDoorEntry.FirstSpawn.State = GameObjectState.Enabled;
            hordeDoorEntry.Flags |= GameObjectFlags.DoesNotDespawn | GameObjectFlags.InUse;
            hordeDoorEntry.FirstSpawn.AutoSpawn = false;

            GOEntry bersekerBuffEntry = GOMgr.GetEntry(GOEntryId.BerserkBuff_6);
            GOEntry foodBuffEntry = GOMgr.GetEntry(GOEntryId.FoodBuff_5);
            GOEntry speedBuffEntry = GOMgr.GetEntry(GOEntryId.SpeedBuff_3);

            GOEntry allianceAttackFlagEntry = GOMgr.GetEntry(GOEntryId.ContestedBanner_26);
            GOEntry hordeControlledFlagEntry = GOMgr.GetEntry(GOEntryId.HordeBanner_10);

            GOEntry hordeAttackFlagEntry = GOMgr.GetEntry(GOEntryId.ContestedBanner_25);
            GOEntry allianceControlledFlagEntry = GOMgr.GetEntry(GOEntryId.AllianceBanner_10);

            GOEntry farmBannerEntry = GOMgr.GetEntry(GOEntryId.FarmBanner_2);
            GOEntry mineBannerEntry = GOMgr.GetEntry(GOEntryId.MineBanner_2);
            GOEntry lumbermillBannerEntry = GOMgr.GetEntry(GOEntryId.LumberMillBanner_2);
            GOEntry stablesBannerEntry = GOMgr.GetEntry(GOEntryId.StableBanner_2);
            GOEntry blacksmithBannerEntry = GOMgr.GetEntry(GOEntryId.BlacksmithBanner_2);

            GOEntry neutralBannerAuraEntry = GOMgr.GetEntry(GOEntryId.NeutralBannerAura);
            GOEntry hordeBannerAuraEntry = GOMgr.GetEntry(GOEntryId.HordeBannerAura);
            GOEntry allianceBannerAuraEntry = GOMgr.GetEntry(GOEntryId.AllianceBannerAura);

            farmBannerEntry.FirstSpawn.AutoSpawn = false;
            mineBannerEntry.FirstSpawn.AutoSpawn = false;
            lumbermillBannerEntry.FirstSpawn.AutoSpawn = false;
            stablesBannerEntry.FirstSpawn.AutoSpawn = false;
            blacksmithBannerEntry.FirstSpawn.AutoSpawn = false;

            allianceAttackFlagEntry.Templates.ForEach(spawn => spawn.AutoSpawn = false);
            allianceControlledFlagEntry.Templates.ForEach(spawn => spawn.AutoSpawn = false);
            hordeAttackFlagEntry.Templates.ForEach(spawn => spawn.AutoSpawn = false);
            hordeControlledFlagEntry.Templates.ForEach(spawn => spawn.AutoSpawn = false);

            neutralBannerAuraEntry.Templates.ForEach(spawn => spawn.AutoSpawn = false);
            hordeBannerAuraEntry.Templates.ForEach(spawn => spawn.AutoSpawn = false);
            allianceBannerAuraEntry.Templates.ForEach(spawn => spawn.AutoSpawn = false);
            //Fix the neutral/horde/alliance flags
            //3 entries for each colour
            //5 templates for each base
        }

        [Initialization]
        [DependentInitialization(typeof(GOMgr))]
        public static void RegisterEvent()
        {
        }
        #endregion
    }
    public delegate void BaseHandler(Character chr);
}