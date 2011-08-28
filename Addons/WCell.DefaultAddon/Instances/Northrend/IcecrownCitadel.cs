using System;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.AI.Actions.Combat;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Instances;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Spells;
using WCell.Util;

namespace WCell.Addons.Default.Instances
{

    public class IcecrownCitadel : BaseInstance
    {
        #region Setup Content
        private static NPCEntry MarrowgarEntry;

        [Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void InitNPCs()
        {
            MarrowgarEntry = NPCMgr.GetEntry(NPCId.LordMarrowgar);
            MarrowgarEntry.BrainCreator = marrowgar => new MarrowgarBrain(marrowgar);
            MarrowgarEntry.Activated += marrowgar =>
            {
                ((BaseBrain)marrowgar.Brain).DefaultCombatAction.Strategy = new MarrowgarAIAttackAction(marrowgar);
            };
        }
        #endregion
    }

    #region 10-Man Lord Marrowgar
    public class MarrowgarBrain : MobBrain
    {
        public MarrowgarBrain(NPC marrowgar) : base(marrowgar) { }
    }

    public class MarrowgarAIAttackAction : AIAttackAction
    {
        public MarrowgarAIAttackAction(NPC marrowgar)
            : base(marrowgar)
        {
            fbasespeed = m_owner.RunSpeed;
            IntroDone = false;
        }

        // Spells
        private static Spell BoneSlice, BoneStorm, ColdFlame, ColdFlameBone;
        private int BoneSliceTick, BoneStormTick, BoneStormStopTick, BoneStormWarnTick, ColdFlameTick, ColdFlameBoneTick, BoneStormMove;

        private DateTime timeSinceLastInterval;

        private static float fbasespeed;
        private static bool IntroDone;
        private static int interval = 1;

        private bool isBoneStorm;
        private static int boneLength;
        private bool bBoneSlice;
        private bool bBoneStormWarn;
        private bool BoneStormWarned;

        [Initialization(InitializationPass.Second)]
        public static void InitMarrowgar()
        {
            BoneSlice = SpellHandler.Get(SpellId.BoneSlice);
            BoneStorm = SpellHandler.Get(SpellId.BoneStorm);
            ColdFlame = SpellHandler.Get(SpellId.Coldflame_3);
            ColdFlameBone = SpellHandler.Get(SpellId.Coldflame_13);
            boneLength = Utility.Random(20, 30);
        }


        public override void Start()
        {
            m_owner.WalkSpeed = m_owner.RunSpeed;
            timeSinceLastInterval = DateTime.Now;

            BoneSliceTick = 0;
            BoneStormTick = 0;
            BoneStormStopTick = 0;
            BoneStormWarnTick = 0;
            ColdFlameTick = 0;
            ColdFlameBoneTick = 0;
            BoneStormMove = 0;

            isBoneStorm = false;
            bBoneStormWarn = false;
            BoneStormWarned = false;

            base.Start();
        }

        public override void Update()
        {
            var timeNow = DateTime.Now;
            var timeBetween = timeNow - timeSinceLastInterval;

            if (timeBetween.TotalSeconds >= interval)
            {
                timeSinceLastInterval = timeNow;
                CheckSpellCast();
            }
            base.Update();
        }

        public void CheckSpellCast()
        {
            if (isBoneStorm)
            {
                ColdFlameBoneTick++;
                BoneStormStopTick++;
                if (ColdFlameTick >= Utility.Random(10, 15))
                {
                    m_owner.SpellCast.Start(ColdFlameBone, false);
                    ColdFlameTick = 0;
                }

                if (BoneStormMove <= boneLength / 3)
                {
                    var target = m_owner.GetNearbyRandomHostileCharacter();
                    if (target != null)
                        m_owner.Movement.MoveTo(target.Position);
                }

                if (BoneStormStopTick >= boneLength + 0.001)
                {
                    isBoneStorm = false;
                    m_owner.RunSpeed = m_owner.RunSpeed / 3.0f;
                    m_owner.Movement.Stop();
                    m_owner.Movement.MoveTo(m_target.Position);
                    bBoneSlice = false;
                    bBoneStormWarn = true;
                    BoneStormWarned = false;
                }
            }
            else
            {
                if (bBoneStormWarn)
                    BoneStormWarnTick++;

                if (BoneStormWarnTick >= Utility.Random(35, 50))
                {
                    BoneStormWarned = true;
                    m_owner.SpellCast.Start(BoneStorm, false);
                }

                if (BoneStormWarned)
                    BoneStormTick++;

                if (BoneStormTick >= 3)
                {
                    var aura = m_owner.Auras[BoneStorm];

                    if (aura != null)
                        aura.TimeLeft = boneLength * 1000;

                    m_owner.RunSpeed = m_owner.RunSpeed * 3.0f;
                    isBoneStorm = true;
                    BoneStormTick = 0;
                }

                if (!bBoneSlice)
                    BoneSliceTick++;

                if (BoneSliceTick >= 10)
                {
                    BoneSliceTick = 0;
                    bBoneSlice = true;
                }
            }

            if (bBoneSlice && !BoneStormWarned)
                m_owner.SpellCast.Start(BoneSlice, false);
        }


    }
    #endregion
}