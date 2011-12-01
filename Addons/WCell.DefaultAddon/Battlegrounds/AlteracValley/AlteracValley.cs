using WCell.Constants;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.AI.Groups;
using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Lang;
using WCell.RealmServer.NPCs;

namespace WCell.Addons.Default.Battlegrounds.AlteracValley
{
	public class AlteracValley : Battleground
	{

        private static NPCEntry _vanndarStormpike;
        private static NPCEntry _drekThar;
        private static NPCEntry _cptBalindaStonehearth;
        private static NPCEntry _cptGalvangar;

	    public AIGroup DunBaldarMob;
	    public AIGroup FrostwolfkeepMob;

        public readonly AVFaction[] Factions;
        private AVItem[] AVItems;

	    public AlteracValley()
        {
			_template = BattlegroundMgr.GetTemplate(BattlegroundId.AlteracValley);
            Factions = new AVFaction[(int)BattlegroundSide.End];
	        AVItems = new AVItem[(int)AVBases.End];

        }

        protected override void InitMap()
        {
            base.InitMap();

            //Factions[(int)BattlegroundSide.Alliance] = new StormpikeExpedition(this);
            //Factions[(int)BattlegroundSide.Horde] = new FrostwolfClan(this);

            
            if (!GOMgr.Loaded)
            {
                // can't have fun here
                FinalizeBattleground(true);
            }
        }
        protected override void SpawnNPCs()
        {
            base.SpawnNPCs();
            //create the AI groups (warmasters + general) for both sides
        }

        /// <summary>
        /// Called when the battle starts (perparation ends now)
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();

            //DropGates();

            Characters.SendSystemMessage("Let the battle for Alterac Valley begin!");
        }

        protected override void OnFinish(bool disposing)
        {
        	base.OnFinish(disposing);
        	Characters.SendSystemMessage("The battle has ended!");
        }

		protected override void OnPrepareHalftime()
        {
            base.OnPrepareHalftime();

			var time = RealmLocalizer.FormatTimeSecondsMinutes(PreparationTimeMillis / 2000);
			Characters.SendSystemMessage("The battle for Alterac Valley begins in {0}.", time);
        }


        protected override void OnPrepareBegin()
        {
			base.OnPrepareBegin();

			var time = RealmLocalizer.FormatTimeSecondsMinutes(PreparationTimeMillis / 1000);
			Characters.SendSystemMessage("The battle for Alterac Valley begins in {0}.", time);
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

        protected override BattlegroundStats CreateStats()
        {
            return new AVStats();
        }

        protected override void RewardPlayers()
        {
            var allianceTeam = GetTeam(BattlegroundSide.Alliance);
            if (allianceTeam == Winner)
            {
                foreach (var chr in allianceTeam.GetAllCharacters())
                {
                    chr.SpellCast.TriggerSelf(SpellId.CreateWarsongMarkOfHonorWInner);
                }
            }
            else
            {
                foreach (var chr in GetTeam(BattlegroundSide.Alliance).GetAllCharacters())
                {
                    chr.SpellCast.TriggerSelf(SpellId.CreateWarsongMarkOfHonorLoser);
                }
            }
        }

        public void Win(BattlegroundTeam team)
        {
            Winner = team;
            FinishFight();
        }

        public override void DeleteNow()
        {
            foreach(var avItem in AVItems)
            {
                //avItem.Dispose();
            }

            for (var i = 0; i < Factions.Length; i++)
            {
                Factions[i] = null;
            }

            base.DeleteNow();
        }

        public AVFaction GetFaction(BattlegroundSide side)
        {
            return Factions[(int)side];
        }

        public AVItem GetItem(AVBases item)
        {
            return AVItems[(int) item];
        }

        //public override void OnDeath(Character chr)
        //{
        //    base.OnDeath(chr);
        //    GetFaction(chr.Battlegrounds.Team.Side).Reinforcements -= 1;
        //}

        #region Content setup

        [Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void InitNPCs()
        {
            _vanndarStormpike = NPCMgr.GetEntry(NPCId.VanndarStormpike);
            _drekThar = NPCMgr.GetEntry(NPCId.DrekThar);
            _cptBalindaStonehearth = NPCMgr.GetEntry(NPCId.CaptainBalindaStonehearth);
            _cptGalvangar = NPCMgr.GetEntry(NPCId.CaptainGalvangar);

            
            _vanndarStormpike.Died += (vann) =>
            {
                var instance = vann.Map as AlteracValley;
                if (instance != null)
                {
                    instance.Factions[(int)BattlegroundSide.Horde].Win();
                }
            };

            _drekThar.Died += (drek) =>
            {
                var instance = drek.Map as AlteracValley;
                if (instance != null)
                {
                    instance.Factions[(int)BattlegroundSide.Alliance].Win();
                }
            };

            _cptBalindaStonehearth.Died += (balinda) =>
            {
                var instance = balinda.Map as AlteracValley;
                if (instance != null)
                {
                    instance.Factions[(int)BattlegroundSide.Horde].Reinforcements -=
                        100;
                }
            };

            _cptGalvangar.Died += (galv) =>
            {
                var instance = galv.Map as AlteracValley;
                if (instance != null)
                {
                    instance.Factions[(int)BattlegroundSide.Horde].Reinforcements -=
                        100;
                }
            };
        }

        #endregion
    }
}