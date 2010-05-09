using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Core.Initialization;
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

namespace WCell.Addons.Default.Instances
{
    public class BlackfathomDeeps : RaidInstance
    {
        #region General (Content)
        private static NPCEntry LadySarevessEntry;
        private static NPCEntry GelihastEntry;
        private static NPCEntry LorgusJettEntry;
        private static NPCEntry LordKelrisEntry;
        private static NPCEntry AkuMaiEntry;

           [Initialization]
           [DependentInitialization(typeof(NPCMgr))]

        public static void InitNPCs()
        {
            LadySarevessEntry = NPCMgr.GetEntry(NPCId.LadySarevess);
            LadySarevessEntry.Activated += LadySarevess =>
            {
                ((BaseBrain)LadySarevess.Brain).DefaultCombatAction.Strategy = new SarevessAttackAction(LadySarevess);
            };

            GelihastEntry = NPCMgr.GetEntry(NPCId.Gelihast);
            GelihastEntry.Activated += Gelihast =>
            {
                ((BaseBrain)Gelihast.Brain).DefaultCombatAction.Strategy = new GelihastAttackAction(Gelihast);
            };

            LorgusJettEntry = NPCMgr.GetEntry(NPCId.LorgusJett);
            LorgusJettEntry.Activated += LorgusJett =>
            {
                ((BaseBrain)LorgusJett.Brain).DefaultCombatAction.Strategy = new LorgusJettAttackAction(LorgusJett);
            };

            LordKelrisEntry = NPCMgr.GetEntry(NPCId.TwilightLordKelris);
            LordKelrisEntry.Activated += LordKelris =>
            {
                ((BaseBrain)LordKelris.Brain).DefaultCombatAction.Strategy = new LordKelrisAttackAction(LordKelris);
            };

            AkuMaiEntry = NPCMgr.GetEntry(NPCId.AkuMai);
            AkuMaiEntry.Activated += AkuMai =>
            {
                ((BaseBrain)AkuMai.Brain).DefaultCombatAction.Strategy = new AkuMaiAttackAction(AkuMai);
            };
        }
        #endregion

        #region LadySarevess
        public class SarevessAttackAction : AIAttackAction
        {
            internal static Spell ForkedLightning, slow, FrostNova;

            private DateTime timeSinceLastInterval;
            private const int interVal = 1;
            private int ForkedLightningTick;
            private int slowTick;
            private int FrostNovaTick;

            [Initialization(InitializationPass.Second)]
            static void InitLady()
            {
                ForkedLightning = SpellHandler.Get(8435);
                slow = SpellHandler.Get(246);
                FrostNova = SpellHandler.Get(865);
            }

            public SarevessAttackAction(NPC LadySarevess)
                : base(LadySarevess)
            {
            }

            public override void Start()
            {
                timeSinceLastInterval = DateTime.Now;
                base.Start();
            }

            public override void Update()
            {
                var timeNow = DateTime.Now;
                var timeBetween = timeNow - timeSinceLastInterval;

                if (timeBetween.TotalSeconds >= interVal)
                {
                    timeSinceLastInterval = timeNow;
                    if (CheckSpellCast())
                    {
                        // idle a little after casting a spell
                        m_owner.Idle(1000);
                        return;
                    }
                }

                base.Update();
            }

            private bool CheckSpellCast()
            {
                ForkedLightningTick++;
                slowTick++;
                FrostNovaTick++;

                if (ForkedLightningTick >= 8)
                {
                    var chr = m_owner.GetNearbyRandomCharacter();
                    if (chr != null)
                    {
                        ForkedLightningTick = 0;
                        m_owner.SpellCast.Start(ForkedLightning, false, chr);
                    }
                    return true;
                }
                if (slowTick >= 13)
                {
                    var chr = m_owner.GetNearbyRandomCharacter();
                    if (chr != null)
                    {
                        slowTick = 0;
                        m_owner.SpellCast.Start(slow, false, chr);
                    }
                    return true;
                }
                if (FrostNovaTick >= 20)
                {
                        ForkedLightningTick = 0;
                        m_owner.SpellCast.Trigger(FrostNova);

                        var Texts = NPCAiTextMgr.GetEntry("You should not be here! Slay them!");
                        var CurrentNpcText = Texts[0];     
                        m_owner.PlaySound((uint)CurrentNpcText.Sound);(m_owner as NPC).Yell(CurrentNpcText.Texts);
                }
                return false;
            }

        }
        #endregion

        #region Gelihast
        public class GelihastAttackAction : AIAttackAction
        {
            internal static Spell Net;

            private DateTime timeSinceLastInterval;
            private const int interVal = 1;
            private int NetTick;

            [Initialization(InitializationPass.Second)]
            static void InitGelihast()
            {
                Net = SpellHandler.Get(6533);
            }

            public GelihastAttackAction(NPC Gelihast)
                : base(Gelihast)
            {
            }

            public override void Start()
            {
                timeSinceLastInterval = DateTime.Now;
                base.Start();
            }

            public override void Update()
            {
                var timeNow = DateTime.Now;
                var timeBetween = timeNow - timeSinceLastInterval;

                if (timeBetween.TotalSeconds >= interVal)
                {
                    timeSinceLastInterval = timeNow;
                    if (CheckSpellCast())
                    {
                        // idle a little after casting a spell
                        m_owner.Idle(1000);
                        return;
                    }
                }

                base.Update();
            }

            private bool CheckSpellCast()
            {
                NetTick++;

                if (NetTick >= 3)
                {
                    var chr = m_owner.GetNearbyRandomCharacter();
                    if (chr != null)
                    {
                        NetTick = 0;
                        m_owner.SpellCast.Start(Net, false, chr);
                    }
                    return true;
                }
                return false;
            }

        }
        #endregion

        #region LorgusJett
        public class LorgusJettAttackAction : AIAttackAction
        {
            internal static Spell LightningBolt, LightningShield;

            private DateTime timeSinceLastInterval;
            private const int interVal = 1;
            private int LightningBoltTick;
            private int LightningShieldTick;

            [Initialization(InitializationPass.Second)]
            static void InitLorgusJett()
            {
                LightningBolt = SpellHandler.Get(12167);
                LightningShield = SpellHandler.Get(12550);
            }

            public LorgusJettAttackAction(NPC LorgusJett)
                : base(LorgusJett)
            {
            }
            /*public override void OnEnterCombat()
            {
                m_owner.SpellCast.Start(LightningShield, false, m_owner);
            }*/

            public override void Start()
            {
                timeSinceLastInterval = DateTime.Now;
                base.Start();
            }

            public override void Update()
            {
                var timeNow = DateTime.Now;
                var timeBetween = timeNow - timeSinceLastInterval;

                if (timeBetween.TotalSeconds >= interVal)
                {
                    timeSinceLastInterval = timeNow;
                    if (CheckSpellCast())
                    {
                        // idle a little after casting a spell
                        m_owner.Idle(1000);
                        return;
                    }
                }

                base.Update();
            }

            private bool CheckSpellCast()
            {
                LightningBoltTick++;
                LightningShieldTick++;

                if (LightningBoltTick >= 4)
                {
                    var chr = m_owner.GetNearbyRandomCharacter();
                    if (chr != null)
                    {
                        LightningBoltTick = 0;
                        m_owner.SpellCast.Start(LightningBolt, false, chr);
                    }
                    return true;
                }
               /* if (LightningShieldTick >= 2)
                {
                    if (m_owner!= null)
                    {
                        LightningShieldTick = 0;
                        m_owner.SpellCast.Start(LightningShield, false, m_owner);
                    }
                    return true;
                }*/
                return false;
            }

        }
        #endregion

        #region LordKelris
        public class LordKelrisAttackAction : AIAttackAction
        {
            internal static Spell MindBlast, Sleep;

            private DateTime timeSinceLastInterval;
            private const int interVal = 1;
            private int MindBlastTick;
            private int SleepTick;

            [Initialization(InitializationPass.Second)]
            static void InitLord()
            {
                MindBlast = SpellHandler.Get(15587);
                Sleep = SpellHandler.Get(8399);
            }

            public LordKelrisAttackAction(NPC LordKelris)
                : base(LordKelris)
            {
            }

            /*public override void OnEnterCombat()
            {
                var Texts = NPCAiTextMgr.GetEntry("Who dares disturb my meditation?!");
                var CurrentNpcText = Texts[0];
                m_owner.PlaySound(CurrentNpcText.Sound); (m_owner as NPC).Yell(CurrentNpcText.Texts); 
            }*/

            public override void Start()
            {
                timeSinceLastInterval = DateTime.Now;
                base.Start();
            }

            public override void Update()
            {
                var timeNow = DateTime.Now;
                var timeBetween = timeNow - timeSinceLastInterval;

                if (timeBetween.TotalSeconds >= interVal)
                {
                    timeSinceLastInterval = timeNow;
                    if (CheckSpellCast())
                    {
                        // idle a little after casting a spell
                        m_owner.Idle(1000);
                        return;
                    }
                }

                base.Update();
            }

            private bool CheckSpellCast()
            {
                MindBlastTick++;
                SleepTick++;

                if (MindBlastTick >= 3)
                {
                    var chr = m_owner.GetNearbyRandomCharacter();
                    if (chr != null)
                    {
                        MindBlastTick = 0;
                        m_owner.SpellCast.Start(MindBlast, false, chr);
                    }
                    return true;
                }
                if (SleepTick >= 7)
                {
                    var chr = m_owner.GetNearbyRandomCharacter();
                    if (chr != null)
                    {
                        SleepTick = 0;
                        m_owner.SpellCast.Start(Sleep, false, chr);

                        var Texts = NPCAiTextMgr.GetEntry("Sleep...");
                        var CurrentNpcText = Texts[0];
                        m_owner.PlaySound((uint)CurrentNpcText.Sound); (m_owner as NPC).Yell(CurrentNpcText.Texts);
                    }
                    return true;
                }
                return false;
            }

        }
        #endregion

        #region Aku'mai
        public class AkuMaiAttackAction : AIAttackAction
        {
            internal static Spell FrenziedRage, PoisonCloud;

            private DateTime timeSinceLastInterval;
            private const int interVal = 1;
            private int PoisonCloudTick;

            [Initialization(InitializationPass.Second)]
            static void InitAkuMai()
            {
                FrenziedRage = SpellHandler.Get(3490);
                PoisonCloud = SpellHandler.Get(3815);
            }

            public AkuMaiAttackAction(NPC AkuMai)
                : base(AkuMai)
            {
            }

            public override void Start()
            {
                timeSinceLastInterval = DateTime.Now;
                base.Start();
            }

            public override void Update()
            {
                var timeNow = DateTime.Now;
                var timeBetween = timeNow - timeSinceLastInterval;

                if (timeBetween.TotalSeconds >= interVal)
                {
                    timeSinceLastInterval = timeNow;
                    if (CheckSpellCast())
                    {
                        // idle a little after casting a spell
                        m_owner.Idle(1000);
                        return;
                    }
                }

                base.Update();
            }

            private bool CheckSpellCast()
            {
                PoisonCloudTick++;

                var hpPct = m_owner.HealthPct;
                if (hpPct <= 55)
                {
                    if (m_owner != null)
                    {
                        m_owner.SpellCast.Start(FrenziedRage, false, m_owner);
                    }
                    return true;
                }
                if (PoisonCloudTick >= 3)
                {
                    var chr = m_owner.GetNearbyRandomCharacter();
                    if (chr != null)
                    {
                        PoisonCloudTick = 0;
                        m_owner.SpellCast.Start(PoisonCloud, false, chr);
                    }
                    return true;
                }
                return false;
            }

        }
        #endregion

    }
}

