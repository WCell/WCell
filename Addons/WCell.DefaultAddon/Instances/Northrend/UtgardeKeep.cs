using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Core.Initialization;
using WCell.Core.Timers;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Instances;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Entities;
using WCell.Constants.Spells;
using WCell.Constants;
using WCell.Constants.GameObjects;
using WCell.RealmServer.AI.Actions.Combat;
using System;
using WCell.Util.Graphics;

///
/// This file was automatically created, using WCell's CodeFileWriter
/// Date: 9/1/2009
///

namespace WCell.Addons.Default.Instances
{
	public class UtgardeKeep : DungeonInstance
	{
        #region Setup Content

        private static NPCEntry princeKelesethEntry;
        private static NPCEntry dragonflayerIronhelm;

        [Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void InitNPCs()
        {
            //Prince Keleseth
            princeKelesethEntry = NPCMgr.GetEntry(NPCId.PrinceKeleseth);
            princeKelesethEntry.BrainCreator = princeKeleseth => new PrinceKelesethBrain(princeKeleseth);

            princeKelesethEntry.Activated += princeKeleseth =>
            {
                ((BaseBrain)princeKeleseth.Brain).DefaultCombatAction.Strategy = new PrinceKelesethAttackAction(princeKeleseth);

            };

            princeKelesethEntry.AddSpell(SpellId.ShadowBolt_73);
            SpellHandler.Apply(spell => { spell.CooldownTime = 5000; },
                SpellId.ShadowBolt_73);

            //Heroic
            //princeKelesethEntry.AddSpell(SpellId.ShadowBolt_99);
            //SpellHandler.Apply(spell => { spell.CooldownTime = 5000; },
            //    SpellId.ShadowBolt_73);

            //princeKelesethEntry.AddSpell(SpellId.FrostTomb_3);

            //princeKelesethEntry.AddSpell(SpellId.FrostTomb_3);

            //princeKelesethEntry.AddSpell(SpellId.FrostTombSummon);

            //princeKelesethEntry.AddSpell(SpellId.Decrepify);

            //princeKelesethEntry.AddSpell(SpellId.ScourgeResurrection);



            // Dragonflayer Ironhelm
            dragonflayerIronhelm = NPCMgr.GetEntry(NPCId.DragonflayerIronhelm);

            dragonflayerIronhelm.AddSpell(SpellId.HeroicStrike_9);
            SpellHandler.Apply(spell => { spell.CooldownTime = 5000; },
                SpellId.HeroicStrike_9);
        }


        #endregion

    }

    #region Prince Keleseth

    public class PrinceKelesethBrain : MobBrain
    {

        const string TEXT_AGGRO = "Your blood is mine!";
        const string TEXT_SUMMONING_SKELETOMS = "Aranal, ledel! Their fate shall be yours!";
        const string TEXT_FROSTTOMB = "Not so fast.";
        const string TEXT_DEATH = "I join... the night.";
        const string TEXT_WAIT = "Darkness waits";

        const int SOUND_AGGRO = 13221;
        const int SOUND_FROSTTOMB = 13222;
        const int SOUNG_WAIT = 13223;
        const int SOUND_SUMMONING_SKELETOMS = 13224;
        const int SOUND_DEATH = 13225;

        [Initialization(InitializationPass.Second)]
        public static void InitPrinceKeleseth()
        {

        }

        public PrinceKelesethBrain(NPC princeKeleseth) : base(princeKeleseth) { }

        public override void OnEnterCombat()
        {
            m_owner.Yell(TEXT_AGGRO);
            m_owner.PlaySound((int)SOUND_AGGRO);

            base.OnEnterCombat();
        }

        public override void OnDeath()
        {

            base.OnDeath();
        }

    }

    public class PrinceKelesethAttackAction : AIAttackAction
    {

        private static NPCEntry skeletonEntry;

        private Vector3[] skeletorVectors = new Vector3[5]
        { 
            new Vector3(156.2559f, 259.2093f, 42.8668f),
            new Vector3(156.2559f, 259.2093f, 42.8668f),
            new Vector3(156.2559f, 259.2093f, 42.8668f),
            new Vector3(156.2559f, 259.2093f, 42.8668f),
            new Vector3(156.2559f, 259.2093f, 42.8668f)
        };



        [Initialization(InitializationPass.Second)]
        public static void InitPrinceKeleseth()
        {
        }

        public PrinceKelesethAttackAction(NPC princeKeleseth)
            : base(princeKeleseth)
        {
        }


        public override void Start()
        {
            skeletonEntry = NPCMgr.GetEntry(NPCId.VrykulSkeleton);
            skeletonEntry.Activated += vrykulSkeleton =>
            {
                ((BaseBrain)vrykulSkeleton.Brain).DefaultCombatAction.Strategy = new VrykulSkeletonAttackAction(vrykulSkeleton);

            };

            // m_owner.CallPeriodically(5000, SummonSkeleton);
            NPCEntry sk = NPCMgr.GetEntry(NPCId.VrykulSkeleton);
            var mob = m_owner.SpawnMinion(sk, ref skeletorVectors[0], 50000);

            base.Start();
        }


    }
    //id 23970 npc Vrykul Skeleton NPCId.VrykulSkeleton
    public class VrykulSkeletonAttackAction : AIAttackAction
    {

        public VrykulSkeletonAttackAction(NPC vrykulSkeleton)
            : base(vrykulSkeleton)
        {
        }

        public override void Start()
        {
            base.Start();
        }
    }


    #endregion

}