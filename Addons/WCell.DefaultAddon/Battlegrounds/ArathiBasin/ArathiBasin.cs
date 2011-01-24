using System.Linq;
using WCell.Addons.Default.Battlegrounds.ArathiBasin.Bases;
using WCell.Addons.Default.Lang;
using WCell.Constants;
using WCell.Constants.AreaTriggers;
using WCell.Constants.Battlegrounds;
using WCell.Constants.Factions;
using WCell.Constants.GameObjects;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.Constants.World;
using WCell.Core.Initialization;
using WCell.Core.Timers;
using WCell.RealmServer.AreaTriggers;
using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Handlers;
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

        [Variable("ABNearVictoryScore")]
        public static int NearVictoryScoreDefault
        {
            get { return Constants.World.WorldStates.GetState(WorldStateId.ABNearVictoryWarning).DefaultValue; }
            set { Constants.World.WorldStates.GetState(WorldStateId.ABNearVictoryWarning).DefaultValue = value; }
        }

        static ArathiBasin()
        {
            MaxScoreDefault = 1600;
            NearVictoryScoreDefault = 1400;
        }

        [Variable("ABPrepTimeMillis")]
        public static int ABPreparationTimeMillis = 60 * 1000;

        [Variable("ABUpdateDelay")]
        public static int BattleUpdateDelayMillis = 1000;

        [Variable("ABPowerUpRespawnTime")]
        public static int PowerUpRespawnTimeMillis = 90 * 1000;

        [Variable("ABReputationScoreTicks")]
        public static int ReputationScoreTicks = 200;

        [Variable("ABHonorScoreTicks")]
        public static int HonorScoreTicks = 330;

        [Variable("ABReputationScore")]
        public static int ReputationScore = 10;

        public float DefaultScoreTickDelay = 12;
        #endregion

        #region Fields
        private GameObject _allianceDoor;
        private GameObject _hordeDoor;

        public ArathiBase[] Bases;
        public int MaxScore;
	    public int NearVictoryScore;

        public int[] scoreTicks, tickLengths;
	    private uint _allianceScoreLastTick, _hordeScoreLastTick;
        private uint _allianceHonorLastTick, _hordeHonorLastTick;
        private uint _allianceReputationLastTick, _hordeReputationlastTick;

        public bool isInformatedNearVictory;
        public TimerEntry timerUpdate;
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
                    Winner = HordeTeam;
                    FinishFight();
                }
                if (value >= NearVictoryScore && !isInformatedNearVictory)
                {
                    InformNearVictory(HordeTeam, value);
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
                    Winner = AllianceTeam;
                    FinishFight(); 
                }
                if (value >= NearVictoryScore && !isInformatedNearVictory)
                {
                    InformNearVictory(AllianceTeam, value);
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
            scoreTicks = new int[6]{ 0, 10, 10 ,10 ,10, 30};
            tickLengths = new int[6]{ 0, 12000, 9000, 6000, 3000, 1000};
        }

        #region Overrides

        protected override void InitMap()
        {
            base.InitMap();
            Bases[(int)ArathiBases.Blacksmith] = new Blacksmith(this);
            Bases[(int)ArathiBases.Farm] = new Farm(this);
            Bases[(int)ArathiBases.GoldMine] = new GoldMine(this);
            Bases[(int)ArathiBases.Lumbermill] = new LumberMill(this);
            Bases[(int)ArathiBases.Stables] = new Stables(this);

            MaxScore = MaxScoreDefault;
            NearVictoryScore = NearVictoryScoreDefault;
            RegisterEvents();
        }

        protected override void RewardPlayers()
        {
            /* Obsolete since patch 3.3.3. See http://www.wowwiki.com/Patch_3.3.3
             * BattlegroundTeam allianceTeam = GetTeam(BattlegroundSide.Alliance);
            if (allianceTeam == Winner)
            {
                foreach (Character chr in allianceTeam.GetCharacters())
                {
                    chr.SpellCast.TriggerSelf(SpellId.ArathiBasinMarkOfHonorWinner);
                }
            }
            else
            {
                foreach (Character chr in GetTeam(BattlegroundSide.Alliance).GetCharacters())
                {
                    chr.SpellCast.TriggerSelf(SpellId.ArathiBasinMarkOfHonorLoser);
                }
            }*/
        }

        public override void DeleteNow()
        {
            foreach (ArathiBase arathiBase in Bases)
            {
                arathiBase.Destroy();
            }

            for (int i = 0; i < (int)ArathiBases.End; i++)
            {
                Bases[i] = null;
            }

            base.DeleteNow();
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
            Characters.SendMultiStringSystemMessage(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ABOnStart));
            _allianceDoor.State = GameObjectState.Disabled;
            _hordeDoor.State = GameObjectState.Disabled;

            timerUpdate = CallPeriodically(BattleUpdateDelayMillis, Update);
        }

        protected override void OnFinish(bool disposing)
        {
            base.OnFinish(disposing);
            /*Characters.SendMultiStringSystemMessage(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ABOnFinish), 
                                                                                                        Winner.Side.ToString());*/
        }

        protected override void OnPrepareHalftime()
        {
            base.OnPrepareHalftime();
            Characters.SendMultiStringSystemMessage(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ABOnPrepareHalfTime), PreparationTimeMillis / 2000);
        }

        protected override void OnPrepare()
        {
            base.OnPrepare();
        	var time = RealmLocalizer.FormatTimeSecondsMinutes(PreparationTimeMillis/1000);
            Characters.SendMultiStringSystemMessage(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ABOnPrepare), time);
        }

        /// <summary>
        /// Removes and drops the flag and it's aura when a player leaves.
        /// </summary>
        /// <param name="chr"></param>
        protected override void OnLeave(Character chr)
        {
            base.OnLeave(chr);
            Characters.SendMultiStringSystemMessage(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ABOnLeave), chr.Name);
        }

        /// <summary>
        /// Messages the players of a new character entering the battle.
        /// </summary>
        /// <param name="chr"></param>
        protected override void OnEnter(Character chr)
        {
            base.OnEnter(chr);
            Characters.SendMultiStringSystemMessage(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ABOnEnter), chr.Name);
        }

        protected override void SpawnGOs()
        {
            base.SpawnGOs();
            GOEntry allianceDoorEntry = GOMgr.GetEntry(GOEntryId.ALLIANCEDOOR);
            GOEntry hordeDoorEntry = GOMgr.GetEntry(GOEntryId.HORDEDOOR);
            
            _allianceDoor = allianceDoorEntry.FirstSpawnEntry.Spawn(this);
            _hordeDoor = hordeDoorEntry.FirstSpawnEntry.Spawn(this);
        }

        public override void OnPlayerClickedOnflag(GameObject go, Character chr)
        {
            Bases.First(arathiBase => arathiBase.FlagStand == go).PlayerClickedOnFlagstand(go, chr);
        }

        public override void FinishFight()
        {
            timerUpdate.Stop();
            base.FinishFight();
        }
        #endregion

        private void Update()
        {
            foreach(var team in _teams)
            {
                int bases = 0;

                if(team.Side == BattlegroundSide.Horde)
                {
                    bases = Bases.Count(node => node.BaseOwner == BattlegroundSide.Horde && node.GivesScore);
                    HordeBaseCount = Bases.Count(node => node.BaseOwner == BattlegroundSide.Horde);
                    _hordeScoreLastTick += (uint)BattleUpdateDelayMillis;
                    _hordeHonorLastTick += (uint)scoreTicks[bases];
                    _hordeReputationlastTick += (uint)scoreTicks[bases];
                }

                else
                {
                    bases = Bases.Count(node => node.BaseOwner == BattlegroundSide.Alliance && node.GivesScore);
                    AllianceBaseCount = Bases.Count(node => node.BaseOwner == BattlegroundSide.Alliance);
                    _allianceScoreLastTick += (uint)BattleUpdateDelayMillis;
                    _allianceHonorLastTick += (uint)scoreTicks[bases];
                    _allianceReputationLastTick += (uint)scoreTicks[bases];

                }


                // See http://www.wowwiki.com/Arathi_Basin#Accumulating_Resources
                int tickLength = tickLengths[bases];
                
                if (team.Side == BattlegroundSide.Horde && _hordeScoreLastTick >= tickLength)
                {
                     HordeScore += scoreTicks[bases];
                     _hordeScoreLastTick = 0;

                    if (_hordeReputationlastTick >= ReputationScoreTicks)
                    {
                        CallOnAllCharacters(chr => chr.Reputations.GainReputation(FactionId.TheDefilers, ReputationScore));
                        _hordeReputationlastTick = 0;
                    }

                    if (_hordeHonorLastTick >= HonorScoreTicks)
                    {
                        // TODO : Honor Formula
                        _hordeHonorLastTick = 0;
                    }
                }
                else if (_allianceScoreLastTick >= tickLength)
                {
                    AllianceScore += scoreTicks[bases];
                    _allianceScoreLastTick = 0;

                    if (_allianceHonorLastTick >= ReputationScoreTicks)
                    {
                        CallOnAllCharacters(chr => chr.Reputations.GainReputation(FactionId.TheLeagueOfArathor, ReputationScore));
                        _allianceHonorLastTick = 0;
                    }
                    if (_allianceReputationLastTick >= HonorScoreTicks)
                    {
                        // TODO : Honor Formula
                        _allianceReputationLastTick = 0;
                    }
                }
            }
        }

        private void InformNearVictory(BattlegroundTeam team, int score)
        {
            Characters.SendMultiStringSystemMessage(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ABNearVictory), 
                                                                                                            team.Side.ToString(), score);
            MiscHandler.SendPlaySoundToMap(this, (uint)ABSounds.NearVictory);
            isInformatedNearVictory = true;
        }

		#region Spell/GO fixes and event registration

		[Initialization]
        [DependentInitialization(typeof(GOMgr))]
        public static void FixGOs()
        {
            GOEntry allianceDoorEntry = GOMgr.GetEntry(GOEntryId.ALLIANCEDOOR);
            GOEntry hordeDoorEntry = GOMgr.GetEntry(GOEntryId.HORDEDOOR);
            
            allianceDoorEntry.FirstSpawnEntry.State = GameObjectState.Enabled;
            allianceDoorEntry.Flags |= GameObjectFlags.DoesNotDespawn | GameObjectFlags.InUse;
            allianceDoorEntry.FirstSpawnEntry.AutoSpawn = false;

            hordeDoorEntry.FirstSpawnEntry.State = GameObjectState.Enabled;
            hordeDoorEntry.Flags |= GameObjectFlags.DoesNotDespawn | GameObjectFlags.InUse;
            hordeDoorEntry.FirstSpawnEntry.AutoSpawn = false;

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

            farmBannerEntry.FirstSpawnEntry.AutoSpawn = false;
            mineBannerEntry.FirstSpawnEntry.AutoSpawn = false;
            lumbermillBannerEntry.FirstSpawnEntry.AutoSpawn = false;
            stablesBannerEntry.FirstSpawnEntry.AutoSpawn = false;
            blacksmithBannerEntry.FirstSpawnEntry.AutoSpawn = false;

            allianceAttackFlagEntry.Templates.ForEach(spawn => spawn.AutoSpawn = false);
            allianceControlledFlagEntry.Templates.ForEach(spawn => spawn.AutoSpawn = false);

            hordeAttackFlagEntry.Templates.ForEach(spawn => spawn.AutoSpawn = false);
            hordeControlledFlagEntry.Templates.ForEach(spawn => spawn.AutoSpawn = false);

            neutralBannerAuraEntry.Templates.ForEach(spawn => spawn.AutoSpawn = false);
            hordeBannerAuraEntry.Templates.ForEach(spawn => spawn.AutoSpawn = false);
            allianceBannerAuraEntry.Templates.ForEach(spawn => spawn.AutoSpawn = false);
        }

        private void RegisterEvents()
        {
            AreaTrigger blacksmithAT = AreaTriggerMgr.GetTrigger(AreaTriggerId.ArathiBasinBlackSmith);
            AreaTrigger stablesAT = AreaTriggerMgr.GetTrigger(AreaTriggerId.ArathiBasinStables);
            AreaTrigger farmAT = AreaTriggerMgr.GetTrigger(AreaTriggerId.ArathiBasinFarm);
            AreaTrigger lumberMillAT = AreaTriggerMgr.GetTrigger(AreaTriggerId.ArathiBasinLumberMill);
            AreaTrigger goldMineAT = AreaTriggerMgr.GetTrigger(AreaTriggerId.ArathiBasinGoldMine);
        }
        #endregion
    }
    public delegate void BaseHandler(Character chr);
}