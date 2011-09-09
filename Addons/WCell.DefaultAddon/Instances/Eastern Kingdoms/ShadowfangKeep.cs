// This is addon for "Wcell"
// Developer: FeRus (Rivera team)
// info: http://world-rivera.ru

using WCell.Constants.GameObjects;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Instances;
using WCell.RealmServer.Misc;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Spells;

namespace WCell.Addons.Default.Instances
{
    public class ShadowfangKeep : BaseInstance
    {

        #region Setup Content

        private static NPCEntry rethilgoreEntry;
        private static NPCEntry commanderspringvaleEntry;
        private static NPCEntry baronsilverlaineEntry;
        private static NPCEntry blindwatcherEntry;
        private static NPCEntry fenrusEntry;
        private static NPCEntry arugalEntry;


        [Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void InitNPCs()
        {
            // (!)Rethilgore
            rethilgoreEntry = NPCMgr.GetEntry(NPCId.Rethilgore);
            rethilgoreEntry.AddSpell(SpellId.SoulDrain);		// add Rethilgore Spell
            rethilgoreEntry.BrainCreator = rethilgore => new RethilgoreBrain(rethilgore);

            // Rethilgore spell has a cooldown of about 30s
            SpellHandler.Apply(spell => { spell.CooldownTime = 30000; }, SpellId.SoulDrain);


            // (!)Baron Silverlaine
            baronsilverlaineEntry = NPCMgr.GetEntry(NPCId.BaronSilverlaine);
            baronsilverlaineEntry.AddSpell(SpellId.VeilOfShadow);
            SpellHandler.Apply(spell => { spell.CooldownTime = 15000; }, SpellId.VeilOfShadow);


            // (!)Commander Springvale
            commanderspringvaleEntry = NPCMgr.GetEntry(NPCId.CommanderSpringvale);
            commanderspringvaleEntry.AddSpell(SpellId.ClassSkillHammerOfJusticeRank2);
            commanderspringvaleEntry.AddSpell(SpellId.HolyLight_7);

            SpellHandler.Apply(spell => { spell.CooldownTime = 60000; }, SpellId.ClassSkillHammerOfJusticeRank2);
            SpellHandler.Apply(spell => { spell.CooldownTime = 45000; }, SpellId.HolyLight_7);


            // (!)Odo the Blindwatcher
            blindwatcherEntry = NPCMgr.GetEntry(NPCId.OdoTheBlindwatcher);
            blindwatcherEntry.AddSpell(SpellId.SkullforgeBrand);

            SpellHandler.Apply(spell => { spell.CooldownTime = 60000; }, SpellId.HowlingRage_3);


            // (!)Fenrus the Devourer
            fenrusEntry = NPCMgr.GetEntry(NPCId.FenrusTheDevourer);
            fenrusEntry.AddSpell(SpellId.ToxicSaliva);
            fenrusEntry.BrainCreator = fenrus => new FenrusBrain(fenrus);

            SpellHandler.Apply(spell => { spell.CooldownTime = 60000; }, SpellId.ToxicSaliva);


            // (!)Archmage Arugal
            arugalEntry = NPCMgr.GetEntry(NPCId.Arugal);
            arugalEntry.AddSpell(SpellId.Thundershock);
            arugalEntry.AddSpell(SpellId.VoidBolt);

            SpellHandler.Apply(spell => { spell.CooldownTime = 25000; }, SpellId.Thundershock);
            SpellHandler.Apply(spell => { spell.CooldownTime = 40000; }, SpellId.VoidBolt);
        }

        [Initialization]
        [DependentInitialization(typeof(GOMgr))]
        public static void InitGOs()
        {

            var rethilgoreDoorEntry = GOMgr.GetEntry(GOEntryId.CourtyardDoor);		// rethilgore door

            if (rethilgoreDoorEntry != null)
            {
                rethilgoreDoorEntry.Activated += go =>
                {
                    var instance = go.Map as ShadowfangKeep;
                    if (instance != null && instance.rethilgoreDoor == null)
                    {
                        // set the instance's Door object after the Door spawned
                        instance.rethilgoreDoor = go;
                    }
                };
            }

            var fenrusDoorEntry = GOMgr.GetEntry(GOEntryId.SorcerersGate);		// fenrus door

            if (fenrusDoorEntry != null)
            {
                fenrusDoorEntry.Activated += go =>
                {
                    var instance = go.Map as ShadowfangKeep;
                    if (instance != null && instance.fenrusDoor == null)
                    {
                        // set the instance's Door object after the Door spawned
                        instance.fenrusDoor = go;
                    }
                };
            }
        }
        // Fixing Spells For NPC
        [Initialization(InitializationPass.Second)]
        public static void FixNPCSpells()
        {
            FixForMob();
        }

        private static void FixForMob()
        {
            SpellHandler.Apply(spell =>
            { 
                spell.CooldownTime = 500000;
                spell.Range = new SimpleRange(0, 5);
            }, SpellId.HauntingSpirits);
        }

        #endregion


        #region Fields
        public GameObject rethilgoreDoor;
        public GameObject fenrusDoor;
        #endregion

    }

    #region Rethilgore

    public class RethilgoreBrain : MobBrain
    {
        private GameObject m_Door;

        public RethilgoreBrain(NPC rethilgore)
            : base(rethilgore)
        {
        }

        public override void OnDeath()
        {
            var instance = m_owner.Map as ShadowfangKeep;

            if (instance != null)
            {
                m_Door = instance.rethilgoreDoor;

                if (m_Door != null)
                {
                    m_Door.State = GameObjectState.Disabled;
                }
            }
            base.OnDeath();
        }
    }
    #endregion

    #region Fenrus the Devourer

    public class FenrusBrain : MobBrain
    {
        private GameObject m_Door;

        public FenrusBrain(NPC fenrus)
            : base(fenrus)
        {
        }

        public override void OnDeath()
        {
            var instance = m_owner.Map as ShadowfangKeep;

            if (instance != null)
            {
                m_Door = instance.fenrusDoor;

                if (m_Door != null)
                {
                    m_Door.State = GameObjectState.Disabled;
                }
            }
            base.OnDeath();
        }
    }

    #endregion
}