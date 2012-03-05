using System;
using WCell.Constants;
using WCell.Constants.GameObjects;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.AI.Actions.Combat;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Instances;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Targeting;
using WCell.Util.Graphics;

///
/// This file was automatically created, using WCell's CodeFileWriter
/// Date: 6/11/2009
///
/* TODO: 
		*Finish upp Mograine and Whitemane encounter
		*Add Headless Horseman for Hallow's End Event */

namespace WCell.Addons.Default.Instances
{
    #region Setup Content
    public class ScarletMonastery : BaseInstance
	{
        private static NPCEntry vishasEntry;
        private static NPCEntry thalnosEntry;
        private static NPCEntry scornEntry;
        private static NPCEntry houndmasterlokseyEntry;
        private static NPCEntry arcanistdoanEntry;
        private static NPCEntry herodEntry;
        private static NPCEntry fairbanksEntry;
        private static NPCEntry mograineEntry;
        private static NPCEntry whitemaneEntry;

        public GameObject cathedralDoor;

        #region NPCs Initialization
        [Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void InitNPCs()
        {
            // Graveyard
            // Interrogator Vishas
            vishasEntry = NPCMgr.GetEntry(NPCId.InterrogatorVishas);
            vishasEntry.AddSpell(SpellId.ClassSkillShadowWordPainRank5);
            vishasEntry.BrainCreator = interrogatorvishas => new InterrogatorVishasBrain(interrogatorvishas);
            vishasEntry.Activated += interrogatorvishas =>
            {
                ((BaseBrain)interrogatorvishas.Brain).DefaultCombatAction.Strategy = new InterrogatorVishasAttackAction(interrogatorvishas);
            };

            SpellHandler.Apply(spell => { spell.CooldownTime = 5000; }, SpellId.ClassSkillShadowWordPainRank5);

            // Bloodmage Thalnos
            thalnosEntry = NPCMgr.GetEntry(NPCId.BloodmageThalnos);
            thalnosEntry.AddSpell(SpellId.ClassSkillFlameShockRank3);
            thalnosEntry.AddSpell(SpellId.ClassSkillShadowBoltRank5);
            thalnosEntry.AddSpell(SpellId.FlameSpike_2);
            thalnosEntry.AddSpell(SpellId.FireNova_4);
            thalnosEntry.BrainCreator = bloodmagethalnos => new BloodmageThalnosBrain(bloodmagethalnos);
			thalnosEntry.Activated += bloodmagethalnos =>
            {
                ((BaseBrain)bloodmagethalnos.Brain).DefaultCombatAction.Strategy = new BloodmageThalnosAttackAction(bloodmagethalnos);
            };

            SpellHandler.Apply(spell => { spell.CooldownTime = 10000; }, SpellId.ClassSkillFlameShockRank3);
            SpellHandler.Apply(spell => { spell.CooldownTime = 2000; }, SpellId.ClassSkillShadowBoltRank5);
            SpellHandler.Apply(spell => { spell.CooldownTime = 8000; }, SpellId.FlameSpike_2);
            SpellHandler.Apply(spell => { spell.CooldownTime = 40000; }, SpellId.FireNova_4);

            // Scorn (Scourge Invasions Event)
            scornEntry = NPCMgr.GetEntry(NPCId.Scorn);
            scornEntry.AddSpell(SpellId.LichSlap);
            scornEntry.AddSpell(SpellId.FrostboltVolley);
            scornEntry.AddSpell(SpellId.ClassSkillMindFlayRank4);
            scornEntry.AddSpell(SpellId.FrostNova_6);
            scornEntry.BrainCreator = scorn => new ScornBrain(scorn);

            SpellHandler.Apply(spell => { spell.CooldownTime = 45000; }, SpellId.LichSlap);
            SpellHandler.Apply(spell => { spell.CooldownTime = 30000; }, SpellId.FrostboltVolley);
            SpellHandler.Apply(spell => { spell.CooldownTime = 30000; }, SpellId.ClassSkillMindFlayRank4);
            SpellHandler.Apply(spell => { spell.CooldownTime = 30000; }, SpellId.FrostNova_6);

            // Library
            // Houndmaster Loksey
            houndmasterlokseyEntry = NPCMgr.GetEntry(NPCId.HoundmasterLoksey);
			houndmasterlokseyEntry.AddSpell(SpellId.SummonScarletHound);
			houndmasterlokseyEntry.AddSpell(SpellId.Bloodlust);
            houndmasterlokseyEntry.BrainCreator = houndmasterloksey => new HoundmasterLokseyBrain(houndmasterloksey);
			
			SpellHandler.Apply(spell => { spell.CooldownTime = 20000; }, SpellId.Bloodlust);

            // Arcanist Doan
			arcanistdoanEntry = NPCMgr.GetEntry(NPCId.ArcanistDoan);
            arcanistdoanEntry.AddSpell(SpellId.SilenceRank1);
			arcanistdoanEntry.AddSpell(SpellId.ArcaneExplosion_25);
			arcanistdoanEntry.AddSpell(SpellId.Polymorph);
            arcanistdoanEntry.BrainCreator = arcanistdoan => new ArcanistDoanBrain(arcanistdoan);
			arcanistdoanEntry.Activated += arcanistdoan =>
            {
                ((BaseBrain)arcanistdoan.Brain).DefaultCombatAction.Strategy = new ArcanistDoanAttackAction(arcanistdoan);
            };

            SpellHandler.Apply(spell => { spell.CooldownTime = 15000; }, SpellId.SilenceRank1);
			SpellHandler.Apply(spell => { spell.CooldownTime = 3000; }, SpellId.ArcaneExplosion_25);
			SpellHandler.Apply(spell => { spell.CooldownTime = 20000; }, SpellId.Polymorph);

            // Armory
            // Herod
            herodEntry = NPCMgr.GetEntry(NPCId.Herod);
			herodEntry.AddSpell(SpellId.Cleave_2);
            herodEntry.BrainCreator = herod => new HerodBrain(herod);
            herodEntry.Activated += herod =>
            {
                ((BaseBrain)herod.Brain).DefaultCombatAction.Strategy = new HerodAttackAction(herod);
            };
			
			SpellHandler.Apply(spell => { spell.CooldownTime = 12000; }, SpellId.Cleave_2);

            // Cathedral
            // High Inquisitor Fairbanks
            fairbanksEntry = NPCMgr.GetEntry(NPCId.HighInquisitorFairbanks);
            fairbanksEntry.AddSpell(SpellId.CurseOfBlood);
            fairbanksEntry.AddSpell(SpellId.DispelMagic);
            fairbanksEntry.AddSpell(SpellId.Fear);
            fairbanksEntry.AddSpell(SpellId.PowerWordShield);
            fairbanksEntry.AddSpell(SpellId.Sleep);
            fairbanksEntry.BrainCreator = highinquisitorfairbanks => new HighInquisitorFairbanksBrain(highinquisitorfairbanks);
            fairbanksEntry.Activated += highinquisitorfairbanks =>
            {
                ((BaseBrain)highinquisitorfairbanks.Brain).DefaultCombatAction.Strategy = new HighInquisitorFairbanksAttackAction(highinquisitorfairbanks);
            };

            SpellHandler.Apply(spell => { spell.CooldownTime = 10000; }, SpellId.CurseOfBlood);
            SpellHandler.Apply(spell => { spell.CooldownTime = 30000; }, SpellId.DispelMagic);
            SpellHandler.Apply(spell => { spell.CooldownTime = 40000; }, SpellId.Fear);
            SpellHandler.Apply(spell => { spell.CooldownTime = 30000; }, SpellId.Sleep);

            // Scarlet Commander Mograine
            mograineEntry = NPCMgr.GetEntry(NPCId.ScarletCommanderMograine);
            mograineEntry.AddSpell(SpellId.CrusaderStrike_2);
            mograineEntry.AddSpell(SpellId.ClassSkillHammerOfJusticeRank3);
            mograineEntry.AddSpell(SpellId.LayOnHandsRank2);
            mograineEntry.AddSpell(SpellId.RetributionAuraRank1);
            mograineEntry.BrainCreator = scarletcommandermograine => new ScarletCommanderMograineBrain(scarletcommandermograine);

            SpellHandler.Apply(spell => { spell.CooldownTime = 10000; }, SpellId.CrusaderStrike_2);
            SpellHandler.Apply(spell => { spell.CooldownTime = 60000; }, SpellId.ClassSkillHammerOfJusticeRank3);


            // Arcanist Doan
			whitemaneEntry = NPCMgr.GetEntry(NPCId.HighInquisitorWhitemane);
            whitemaneEntry.AddSpell(SpellId.SilenceRank1);
			whitemaneEntry.AddSpell(SpellId.ArcaneExplosion_25);
			whitemaneEntry.AddSpell(SpellId.Polymorph);
            whitemaneEntry.BrainCreator = highinquisitorwhitemane => new HighInquisitorWhitemaneBrain(highinquisitorwhitemane);
			whitemaneEntry.Activated += highinquisitorwhitemane =>
            {
                ((BaseBrain)highinquisitorwhitemane.Brain).DefaultCombatAction.Strategy = new HighInquisitorWhitemaneAttackAction(highinquisitorwhitemane);
            };

            SpellHandler.Apply(spell => { spell.CooldownTime = 15000; }, SpellId.SilenceRank1);
			SpellHandler.Apply(spell => { spell.CooldownTime = 3000; }, SpellId.ArcaneExplosion_25);
			SpellHandler.Apply(spell => { spell.CooldownTime = 20000; }, SpellId.Polymorph);
		}
        #endregion

        #region Gameobjects Initialization
        [Initialization]
        [DependentInitialization(typeof(GOMgr))]
        public static void InitGOs()
        {
            var cathedralDoorEntry = GOMgr.GetEntry(GOEntryId.HighInquisitorsDoor);

            if (cathedralDoorEntry != null)
            {
                cathedralDoorEntry.Activated += go =>
                {
                    var instance = go.Map as ScarletMonastery;
                    if (instance != null && instance.cathedralDoor == null)
                    {
                        // set the instance's Door object after the Door spawned
                        instance.cathedralDoor = go;
                    }
                };
            }
        }
        #endregion
	}
    #endregion

    #region Graveyard
    #region Interrogator Vishas
    public class InterrogatorVishasAttackAction : AIAttackAction
    {
        private int phase = 1;

        public InterrogatorVishasAttackAction(NPC InterrogatorVishas)
            : base(InterrogatorVishas)
        {
        }

        public override void Update()
        {
            int hpPct = m_owner.HealthPct;
            
            if (hpPct <= 25 && phase == 2)
            {
                m_owner.Yell("I'll rip the secrets from your flesh!");
                m_owner.PlaySound(5850);
                phase = 3;
                return;
            }
            else if (hpPct <= 75 && phase == 1)
            {
                m_owner.Yell("Naughty secrets!");
                m_owner.PlaySound(5849);
                phase = 2;
                return;
            }
            base.Update();
        }
    }
    public class InterrogatorVishasBrain : MobBrain
    {
        public InterrogatorVishasBrain(NPC InterrogatorVishas)
            : base(InterrogatorVishas)
        {
        }

        public override void OnEnterCombat()
        {
            m_owner.Yell("Tell me... tell me everything!");
            m_owner.PlaySound(5847);
            base.OnEnterCombat();
        }

        public override void OnKilled(Unit killerUnit, Unit victimUnit)
        {
            m_owner.Yell("Purged by pain!");
            m_owner.PlaySound(5848);
            base.OnKilled(killerUnit, victimUnit);
        }
    }
    #endregion

	#region Bloodmage Thalnos
    public class BloodmageThalnosAttackAction : AIAttackAction
    {
        private int phase = 1;

        public BloodmageThalnosAttackAction(NPC BloodmageThalnos)
            : base(BloodmageThalnos)
        {
        }

        public override void Update()
        {
            int hpPct = m_owner.HealthPct;
            if (hpPct <= 50 && phase == 1)
            {
                m_owner.Yell("No rest, for the angry dead.");
                m_owner.PlaySound(5846);
                phase = 2;
                return;
            }
            base.Update();
        }
    }
    public class BloodmageThalnosBrain : MobBrain
    {
        public BloodmageThalnosBrain(NPC BloodmageThalnos)
            : base(BloodmageThalnos)
        {
        }

        public override void OnEnterCombat()
        {
            m_owner.Yell("We hunger for vengeance.");
            m_owner.PlaySound(5844);
            base.OnEnterCombat();
        }

        public override void OnKilled(Unit killerUnit, Unit victimUnit)
        {
            m_owner.Yell("More... More souls.");
            m_owner.PlaySound(5845);
            base.OnKilled(killerUnit, victimUnit);
        }
    }
    #endregion

    #region Scorn (Scourge Invasions Event)
    public class ScornBrain : MobBrain
    {
        public ScornBrain(NPC Scorn)
            : base(Scorn)
        {
        }
		
		public override void OnEnterCombat()
		{
			base.OnEnterCombat();
		}
    }
    #endregion
	#endregion

    #region Library
    #region Houndmaster Loksey
    public class HoundmasterLokseyBrain : MobBrain
    {
        public HoundmasterLokseyBrain(NPC HoundmasterLoksey)
            : base(HoundmasterLoksey)
        {
        }
		
		public override void OnEnterCombat()
		{
			m_owner.Yell("Release the hounds!");
            m_owner.PlaySound(5841);
			base.OnEnterCombat();
		}
    }
    #endregion

    #region Arcanist Doan
    public class ArcanistDoanAttackAction : AIAttackAction
    {
        private static Spell arcanistdoanProtection;
        private static Spell arcanistdoanAoE;
        private int phase = 1;

        [Initialization(InitializationPass.Second)]
        public static void InitArcanistDoan()
        {
            arcanistdoanProtection = SpellHandler.Get(SpellId.ArcaneBubble);
            arcanistdoanAoE = SpellHandler.Get(SpellId.Detonation_2);
        }

        public ArcanistDoanAttackAction(NPC ArcanistDoan)
            : base(ArcanistDoan)
        {
        }

        public override void Update()
        {
            if (m_owner.HealthPct <= 50 && phase == 1)
            {
                m_owner.Auras.CreateSelf(arcanistdoanProtection);		// apply Arcane Bubble after 50% to self
                m_owner.Yell("Burn in righteous fire!");
                m_owner.PlaySound(5843);
                m_owner.SpellCast.Start(arcanistdoanAoE);		// aoe spell finds targets automatically
                phase = 2;
                return;
            }
            base.Update();
        }
    }

    public class ArcanistDoanBrain : MobBrain
    {
        public ArcanistDoanBrain(NPC ArcanistDoan)
            : base(ArcanistDoan)
        {
        }

        public override void OnEnterCombat()
        {
            m_owner.Yell("You will not defile these mysteries!");
            m_owner.PlaySound(5842);
            base.OnEnterCombat(); 
        }
    }
    #endregion
    #endregion

    #region Armory
    #region Herod
    public class HerodAttackAction : AIAttackAction
    {   
        private static Spell herodFrenzy;
        private static Spell herodWhirlWind;

        private const int Interval = 1;
        private DateTime timeSinceLastInterval;
        private int herodWhirlWindTick;

        private int phase = 1;

        [Initialization(InitializationPass.Second)]
        public static void InitHerod()
        {
            herodFrenzy = SpellHandler.Get(SpellId.Frenzy_2);
            herodWhirlWind = SpellHandler.Get(SpellId.WhirlwindRank1);
        }

        public HerodAttackAction(NPC Herod)
            : base(Herod)
        {
        }

        public override void Start()
        {
            herodWhirlWindTick = 0;
            timeSinceLastInterval = DateTime.Now;
            base.Start();
        }

        public override void Update()
        {
            var timeNow = DateTime.Now;
            var timeBetween = timeNow - timeSinceLastInterval;

            if (timeBetween.TotalSeconds >= Interval)
            {
                timeSinceLastInterval = timeNow;
                if (CheckSpellCast())
                {
                    // idle a little after casting a spell
                    m_owner.Idle(1000);
                    return;
                }
            }
            else if (m_owner.HealthPct <= 50 && phase == 1)
            {
                m_owner.Auras.CreateSelf(herodFrenzy);		// apply Frenzy after 50% to self
                m_owner.Yell("Light, give me strength!");
                m_owner.PlaySound(5833);
                phase = 2;
                return;
            }
            base.Update();
        }

        private bool CheckSpellCast()
        {
            herodWhirlWindTick++;

            if (herodWhirlWindTick >= 50)
            {
                var chr = m_owner.GetNearbyRandomHostileCharacter();
                if (chr != null)
                {
                    herodWhirlWindTick = 0;
                    m_owner.Yell("Blades of Light!");
                    m_owner.PlaySound(5832);
                    m_owner.SpellCast.Start(herodWhirlWind, false, chr);
                }
                return true;
            }
            return false;
        }
    }

    public class HerodBrain : MobBrain
    {
        public HerodBrain(NPC Herod)
            : base(Herod)
        {
        }

        public override void OnEnterCombat()
        {
            m_owner.Yell("Ah I've been waiting for a real challenge.");
            m_owner.PlaySound(5830);
            m_owner.SpellCast.Start(SpellHandler.Get(SpellId.RushingChargeRank1_2));
            base.OnEnterCombat();
        }

        public override void OnKilled(Unit killerUnit, Unit victimUnit)
        {
            m_owner.Yell("Ha, is that all?");
            m_owner.PlaySound(5831);
            base.OnKilled(killerUnit, victimUnit);
        }
		
		public override void OnDamageDealt(RealmServer.Misc.IDamageAction action)
		{
            if (action.Spell == SpellHandler.Get(SpellId.WhirlwindRank1))
			{
                m_owner.Yell("Blades of Light!");
                m_owner.PlaySound(5832);
				base.OnDamageDealt(action);
			}
		}
    }
    #endregion
    #endregion

    #region Cathedral
    #region High Inquisitor Fairbanks
    public class HighInquisitorFairbanksAttackAction : AIAttackAction
    {
        private static Spell fairbanksHeal;
        private int phase = 1;

        [Initialization(InitializationPass.Second)]
        public static void InitHerod()
        {
            fairbanksHeal = SpellHandler.Get(SpellId.Heal_3);
        }

        public HighInquisitorFairbanksAttackAction(NPC HighInquisitorFairbanks)
            : base(HighInquisitorFairbanks)
        {
        }

        public override void Update()
        {
            if (m_owner.HealthPct <= 25 && phase == 1)
            {
                m_owner.Auras.CreateSelf(fairbanksHeal);		// heals him self when below 25% once
                phase = 2;
                return;
            }
            base.Update();
        }
    }

    public class HighInquisitorFairbanksBrain : MobBrain
    {
        public HighInquisitorFairbanksBrain(NPC HighInquisitorFairbanks)
            : base(HighInquisitorFairbanks)
        {
        }

        public override void OnActivate()
		{
            m_owner.StandState = StandState.Dead;
            base.OnActivate();
		}

        public override void OnEnterCombat()
        {
            base.OnEnterCombat();
        }
    }
    #endregion

    #region Scarlet Commander Mograine
    public class ScarletCommanderMograineBrain : MobBrain
    {
        private GameObject m_Door;

        public ScarletCommanderMograineBrain(NPC ScarletCommanderMograine)
            : base(ScarletCommanderMograine)
        {
        }

        public override void OnEnterCombat()
        {
            m_owner.Yell("Infidels. They must be purified!");
            m_owner.PlaySound(5835);
            base.OnEnterCombat();
        }

        public override void OnKilled(Unit killerUnit, Unit victimUnit)
        {
            m_owner.Yell("Unworthy.");
            m_owner.PlaySound(5836);
            base.OnKilled(killerUnit, victimUnit);
        }

        public override void OnActivate()
		{
            m_owner.Yell("At your side, milady.");
            m_owner.PlaySound(5837);
            OnEnterCombat();
        }		

        public override void OnDeath()
        {
            var instance = m_owner.Map as ScarletMonastery;

            if (instance != null)
            {
                m_Door = instance.cathedralDoor;

                if (m_Door != null)
                {
                    m_Door.State = GameObjectState.Disabled;
                }
            }
            base.OnDeath();
        }
    }
    #endregion

    #region High Inquisitor Whitemane
    public class HighInquisitorWhitemaneAttackAction : AIAttackAction
    {
        private static Vector3 AltarLocation = new Vector3(1163.113370f, 1398.856812f, 32.527786f);

        private static Spell whitemaneMassSleep;
        private static Spell whitemaneResurrection;

        private int phase = 1;

        [Initialization(InitializationPass.Second)]
        public static void InitHighInquisitorWhitemane()
        {
            whitemaneMassSleep = SpellHandler.Get(SpellId.DeepSleep);
            whitemaneResurrection = SpellHandler.Get(SpellId.ScarletResurrection);
        }

        public HighInquisitorWhitemaneAttackAction(NPC HighInquisitorWhitemane)
            : base(HighInquisitorWhitemane)
        {
        }

        public override void Update()
        {
            int hpPct = m_owner.HealthPct;
            
            if (hpPct <= 50 && phase == 2)
            {
                m_owner.SpellCast.Start(whitemaneMassSleep);
                m_owner.Yell("Arise, my champion!");
                m_owner.PlaySound(5840);
                m_owner.SpellCast.Start(whitemaneResurrection, false, m_owner.GetNearbyNPC(NPCId.ScarletCommanderMograine));
                phase = 3;
                return;
            }
            else if (phase == 1)
            {
                m_owner.Yell("Mograine has fallen? You shall pay for this treachery!");
                m_owner.PlaySound(5838);
                m_owner.MoveToThenEnter(ref AltarLocation, RealmServer.AI.BrainState.Combat);
                phase = 2;
                return;
            }
            base.Update();
        }
    }
    public class HighInquisitorWhitemaneBrain : MobBrain
    {
        public HighInquisitorWhitemaneBrain(NPC HighInquisitorWhitemane)
            : base(HighInquisitorWhitemane)
        {
        }

        public override void OnEnterCombat()
        {
            base.OnEnterCombat();
        }
    }   
    #endregion
    #endregion
}