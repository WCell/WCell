using System;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Instances;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Spells;


///
/// This file was automatically created, using WCell's CodeFileWriter
/// Date: 9/1/2009
///

namespace WCell.Addons.Default.Instances
{
    public class AuchindounManaTombs : DungeonInstance
    {
        #region Setup Content
        private static NPCEntry pandemoniusEntry;

        [Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void InitNPCs()
        {
            //Pandemonius
            pandemoniusEntry = NPCMgr.GetEntry(NPCId.Pandemonius);
            pandemoniusEntry.BrainCreator = pandemonius => new PandemoniusBrain(pandemonius);
            SpellId[] pandemoniusSpells = new SpellId[2] { SpellId.VoidBlast, SpellId.DarkShell };
            pandemoniusEntry.AddSpells(pandemoniusSpells);
            Random spellCooldownVoidBlast = new Random();
            SpellHandler.Apply(spell => spell.CooldownTime = spellCooldownVoidBlast.Next(8000, 23000), pandemoniusSpells[0]);
            SpellHandler.Apply(spell => spell.CooldownTime = 20000, pandemoniusSpells[1]);
        }
        #endregion

        #region Fields

        #endregion

        #region Pandemonius
        public class PandemoniusBrain : MobBrain
        {
            private Random random = new Random();
            private uint voidBlastCounter = 0;

            public PandemoniusBrain(NPC pandemonius)
                : base(pandemonius) { }

            public override void OnEnterCombat()
            {
                switch (random.Next(3))
                {
                    case 0: m_owner.PlayTextAndSoundById(-1557008); break;
                    case 1: m_owner.PlayTextAndSoundById(-1557009); break;
                    case 2: m_owner.PlayTextAndSoundById(-1557010); break;
                }
                base.OnEnterCombat();
            }

            public override void OnDeath()
            {
                m_owner.PlayTextAndSoundById(-1557013);
                base.OnDeath();
            }

            public override void OnKilled(Unit killerUnit, Unit victimUnit)
            {
                switch (random.Next(2))
                {
                    case 0: m_owner.PlayTextAndSoundById(-1557011); break;
                    case 1: m_owner.PlayTextAndSoundById(-1557012); break;
                }
                base.OnKilled(killerUnit, victimUnit);
            }
            public override void OnDamageDealt(RealmServer.Misc.IDamageAction action)
            {
                if (action.Spell == SpellHandler.Get(SpellId.VoidBlast))
                {
                    m_owner.Say(m_owner.Target.Name + " shifts into the void...");
                    ++voidBlastCounter;
                    if (voidBlastCounter == 5)
                    {
                        SpellHandler.Apply(spell => spell.CooldownTime = (random.Next(15000, 25000)), SpellId.VoidBlast);
                        voidBlastCounter = 0;
                    }
                    base.OnDamageDealt(action);
                }
            }
        };
        #endregion
    }

}