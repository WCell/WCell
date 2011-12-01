using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.AI;
using WCell.RealmServer.AI.Actions.Combat;
using WCell.RealmServer.AI.Actions.States;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Instances;
using WCell.RealmServer.Misc;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Spells;

namespace WCell.Addons.Default.Instances
{
	public class RagefireChasm : BaseInstance
	{
		#region Setup Content
		private static NPCEntry oggleflintEntry;
		private static NPCEntry taragamanEntry;
		private static NPCEntry jergoshEntry;
		private static NPCEntry bazzalanEntry;
//      Oggleflint
        static readonly ProcHandlerTemplate cleave = new TriggerSpellProcHandlerTemplate(SpellHandler.Get(SpellId.Cleave_28), ProcTriggerFlags.ReceivedAnyDamage, ProcHitFlags.None, 10);
//      Taragaman
//      static readonly ProcHandlerTemplate uppercut = new TriggerSpellProcHandlerTemplate(SpellHandler.Get(SpellId.Uppercut_2), ProcTriggerFlags.AnyHit, 10);  //Not working properly
        static readonly ProcHandlerTemplate firenova = new TriggerSpellProcHandlerTemplate(SpellHandler.Get(SpellId.FireNova_2), ProcTriggerFlags.ReceivedAnyDamage, ProcHitFlags.None, 10);
//      Jergosh
        static readonly ProcHandlerTemplate weakness = new TriggerSpellProcHandlerTemplate(SpellHandler.Get(SpellId.CurseOfWeakness_6), ProcTriggerFlags.ReceivedAnyDamage, ProcHitFlags.None, 10);
        static readonly ProcHandlerTemplate immolate = new TriggerSpellProcHandlerTemplate(SpellHandler.Get(SpellId.Immolate_13), ProcTriggerFlags.ReceivedAnyDamage, ProcHitFlags.None, 15);
//      Bazzalan
        static readonly ProcHandlerTemplate poison = new TriggerSpellProcHandlerTemplate(SpellHandler.Get(SpellId.Poison_10), ProcTriggerFlags.ReceivedAnyDamage, ProcHitFlags.None, 5);
        static readonly ProcHandlerTemplate sstrike = new TriggerSpellProcHandlerTemplate(SpellHandler.Get(SpellId.SinisterStrike), ProcTriggerFlags.ReceivedAnyDamage, ProcHitFlags.None, 10);
        
		[Initialization]
		[DependentInitialization(typeof(NPCMgr))]
		public static void InitNPCs()
		{
//          Oggleflint
			oggleflintEntry = NPCMgr.GetEntry(NPCId.Oggleflint);
			oggleflintEntry.AddSpell(SpellId.Cleave);            
			oggleflintEntry.Activated += oggleflint =>
			{
				var brain = (BaseBrain)oggleflint.Brain;
				var combatAction = (AICombatAction)brain.Actions[BrainState.Combat];
				combatAction.Strategy = new OggleflintAttackAction(oggleflint);
                oggleflint.AddProcHandler(cleave);
			};

//          Taragaman the Hungerer
			taragamanEntry = NPCMgr.GetEntry(NPCId.TaragamanTheHungerer);
//          taragamanEntry.AddSpell(SpellId.Uppercut);  //Not working properly
			taragamanEntry.AddSpell(SpellId.FireNova);
			taragamanEntry.Activated += taragaman =>
			{
				var brain = (BaseBrain)taragaman.Brain;
				var combatAction = (AICombatAction)brain.Actions[BrainState.Combat];
				combatAction.Strategy = new TaragamanAttackAction(taragaman);
//              taragaman.AddProcHandler(uppercut);  //Currently not working
                taragaman.AddProcHandler(firenova); 
            };

//          Jergosh the Invoker
			jergoshEntry = NPCMgr.GetEntry(NPCId.JergoshTheInvoker);
			jergoshEntry.AddSpell(SpellId.CurseOfWeakness);
			jergoshEntry.AddSpell(SpellId.Immolate);
			jergoshEntry.Activated += jergosh =>
			{
				var brain = (BaseBrain)jergosh.Brain;
				var combatAction = (AICombatAction)brain.Actions[BrainState.Combat];
				combatAction.Strategy = new JergoshAttackAction(jergosh);
                jergosh.AddProcHandler(weakness);
                jergosh.AddProcHandler(immolate);
			};

//          Bazzalan
			bazzalanEntry = NPCMgr.GetEntry(NPCId.Bazzalan);
			bazzalanEntry.AddSpell(SpellId.Poison);
			bazzalanEntry.AddSpell(SpellId.SinisterStrike);
			bazzalanEntry.Activated += bazzalan =>
			{
				var brain = (BaseBrain)bazzalan.Brain;
				var combatAction = (AICombatAction)brain.Actions[BrainState.Combat];
				combatAction.Strategy = new BazzalanAttackAction(bazzalan);
                bazzalan.AddProcHandler(poison);
                bazzalan.AddProcHandler(sstrike);
			};
		}
		#endregion
	}
	#region Oggleflint
	public class OggleflintAttackAction : AIAttackAction
	{
		public OggleflintAttackAction(NPC oggleflint)
			: base(oggleflint)
		{
		}
	}
	#endregion

	#region Taragaman
	public class TaragamanAttackAction : AIAttackAction
	{
		public TaragamanAttackAction(NPC taragaman)
			: base(taragaman)
		{
  		}
	}
	#endregion

	#region Jergosh
	public class JergoshAttackAction : AIAttackAction
	{
		public JergoshAttackAction(NPC jergosh)
			: base(jergosh)
		{
		}
	}
	#endregion

	#region Bazzalan
	public class BazzalanAttackAction : AIAttackAction
	{
		public BazzalanAttackAction(NPC bazzalan)
			: base(bazzalan)
		{
		}
	}
	#endregion
}