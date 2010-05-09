// This is addon for "Wcell"
// Developer: FeRus (Rivera team)
// info: http://world-rivera.ru

using System;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Core.Initialization;
using WCell.Constants.Spells;
using WCell.Constants;
using WCell.Constants.GameObjects;
using WCell.RealmServer.AI.Actions.Combat;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Instances;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Entities;
using WCell.Util.Graphics;

namespace WCell.Addons.Default.Instances
{
    public class ShadowfangKeep : RaidInstance
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
            rethilgoreEntry = NPCMgr.GetEntry(3914);
            rethilgoreEntry.AddSpell((SpellId)7295);		// add Rethilgore Spell
            rethilgoreEntry.BrainCreator = rethilgore => new RethilgoreBrain(rethilgore);

            // Rethilgore spell has a cooldown of about 30s
            SpellHandler.Apply(spell => { spell.CooldownTime = 30000; }, (SpellId)7295);


            // (!)Baron Silverlaine
            baronsilverlaineEntry = NPCMgr.GetEntry(3887);
            baronsilverlaineEntry.AddSpell((SpellId)7068);

            SpellHandler.Apply(spell => { spell.CooldownTime = 15000; }, (SpellId)7068);


            // (!)Commander Springvale
            commanderspringvaleEntry = NPCMgr.GetEntry(4278);
            commanderspringvaleEntry.AddSpell((SpellId)5588);
            commanderspringvaleEntry.AddSpell((SpellId)31713);

            SpellHandler.Apply(spell => { spell.CooldownTime = 60000; }, (SpellId)5588);
            SpellHandler.Apply(spell => { spell.CooldownTime = 45000; }, (SpellId)31713);


            // (!)Odo the Blindwatcher
            blindwatcherEntry = NPCMgr.GetEntry(4279);
            blindwatcherEntry.AddSpell((SpellId)7484);

            SpellHandler.Apply(spell => { spell.CooldownTime = 60000; }, (SpellId)7484);


            // (!)Fenrus the Devourer
            fenrusEntry = NPCMgr.GetEntry(4274);
            fenrusEntry.AddSpell((SpellId)7125);
            fenrusEntry.BrainCreator = fenrus => new FenrusBrain(fenrus);

            SpellHandler.Apply(spell => { spell.CooldownTime = 60000; }, (SpellId)7125);


            // (!)Archmage Arugal
            arugalEntry = NPCMgr.GetEntry(4275);
            arugalEntry.AddSpell((SpellId)7803);
            arugalEntry.AddSpell((SpellId)7588);

            SpellHandler.Apply(spell => { spell.CooldownTime = 25000; }, (SpellId)7803);
            SpellHandler.Apply(spell => { spell.CooldownTime = 40000; }, (SpellId)7588);
        }

        [Initialization]
        [DependentInitialization(typeof(GOMgr))]
        public static void InitGOs()
        {

            var rethilgoreDoorEntry = GOMgr.GetEntry((GOEntryId)18895);		// rethilgore door

            if (rethilgoreDoorEntry != null)
            {
                rethilgoreDoorEntry.Activated += go =>
                {
                    var instance = go.Region as ShadowfangKeep;
                    if (instance != null && instance.rethilgoreDoor == null)
                    {
                        // set the instance's Door object after the Door spawned
                        instance.rethilgoreDoor = go;
                    }
                };
            }

            var fenrusDoorEntry = GOMgr.GetEntry((GOEntryId)18972);		// fenrus door

            if (fenrusDoorEntry != null)
            {
                fenrusDoorEntry.Activated += go =>
                {
                    var instance = go.Region as ShadowfangKeep;
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
            var instance = m_owner.Region as ShadowfangKeep;

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
            var instance = m_owner.Region as ShadowfangKeep;

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