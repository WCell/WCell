using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Instances;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Spells;
using WCell.Util;

namespace WCell.Addons.Default.Instances
{
    public class AuchindounManaTombs : BaseInstance
    {
        #region Setup Content
		private static NPCEntry pandemoniusEntry;
		static readonly SpellId[] pandemoniusSpells = new[] { SpellId.VoidBlast, SpellId.DarkShell };

        [Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void InitNPCs()
        {
            //Pandemonius
            pandemoniusEntry = NPCMgr.GetEntry(NPCId.Pandemonius);
            pandemoniusEntry.BrainCreator = pandemonius => new PandemoniusBrain(pandemonius);
            pandemoniusEntry.AddSpells(pandemoniusSpells);

			SpellHandler.Apply(spell => spell.AISettings.SetCooldown(15000, 25000), SpellId.VoidBlast);
            SpellHandler.Apply(spell =>
                                   {
                                       if (spell.SpellCooldowns != null)
                                           spell.SpellCooldowns.CooldownTime = 20000;
                                       else
                                       {
                                           spell.SpellCooldowns = new SpellCooldowns {CooldownTime = 20000};
                                       }
                                   }, SpellId.DarkShell);
        }
        #endregion

        #region Fields

        #endregion

        #region Pandemonius
        public class PandemoniusBrain : MobBrain
        {
            public PandemoniusBrain(NPC pandemonius)
                : base(pandemonius) { }

            public override void OnEnterCombat()
            {
				switch (Utility.Random(3))
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
                switch (Utility.Random(2))
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
                    base.OnDamageDealt(action);
                }
            }
        };
        #endregion

    	protected override void PerformSave()
    	{
    	}
    }

}