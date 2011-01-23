using WCell.Constants.NPCs;
using WCell.Core.Initialization;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Instances;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Entities;
using WCell.Constants.Spells;
using WCell.Constants;
using WCell.Util;
using WCell.RealmServer.AI;
using WCell.RealmServer.AI.Actions.States;
using WCell.RealmServer.AI.Actions.Combat;
using System.Collections.Generic;
using System;


namespace WCell.Addons.Default.Instances
{
	public class RagefireChasm : DungeonInstance
	{
		#region Setup Content
		private static NPCEntry oggleflintEntry;
		private static NPCEntry taragamanEntry;
		private static NPCEntry jergoshEntry;
		private static NPCEntry bazzalanEntry;

		[Initialization]
		[DependentInitialization(typeof(NPCMgr))]
		public static void InitNPCs()
		{
			// Oggleflint
			oggleflintEntry = NPCMgr.GetEntry(NPCId.Oggleflint);

			oggleflintEntry.AddSpell(SpellId.Cleave);

			SpellHandler.Apply(spell => { spell.CooldownTime = 5000; },
							   SpellId.Cleave);

			oggleflintEntry.Activated += oggleflint =>
			{
				var brain = (BaseBrain)oggleflint.Brain;
				var combatAction = (AICombatAction)brain.Actions[BrainState.Combat];
				combatAction.Strategy = new OggleflintAttackAction(oggleflint);
			};

			// Taragaman the Hungerer
			taragamanEntry = NPCMgr.GetEntry(NPCId.TaragamanTheHungerer);

			taragamanEntry.AddSpell(SpellId.Uppercut);
			taragamanEntry.AddSpell(SpellId.FireNova);

			SpellHandler.Apply(spell => { spell.CooldownTime = 5000; },
							   SpellId.Uppercut);
			SpellHandler.Apply(spell => { spell.CooldownTime = 10000; },
							   SpellId.FireNova);

			taragamanEntry.Activated += taragaman =>
			{
				var brain = (BaseBrain)taragaman.Brain;
				var combatAction = (AICombatAction)brain.Actions[BrainState.Combat];
				combatAction.Strategy = new TaragamanAttackAction(taragaman);
			};

			// Jergosh the Invoker
			jergoshEntry = NPCMgr.GetEntry(NPCId.JergoshTheInvoker);

			jergoshEntry.AddSpell(SpellId.CurseOfWeakness);
			jergoshEntry.AddSpell(SpellId.Immolate);

			SpellHandler.Apply(spell => { spell.CooldownTime = 12000; },
							   SpellId.CurseOfWeakness);
			SpellHandler.Apply(spell => { spell.CooldownTime = 5000; },
							   SpellId.Immolate);

			jergoshEntry.Activated += jergosh =>
			{
				var brain = (BaseBrain)jergosh.Brain;
				var combatAction = (AICombatAction)brain.Actions[BrainState.Combat];
				combatAction.Strategy = new JergoshAttackAction(jergosh);
			};

			// Bazzalan
			bazzalanEntry = NPCMgr.GetEntry(NPCId.Bazzalan);

			bazzalanEntry.AddSpell(SpellId.Poison);
			bazzalanEntry.AddSpell(SpellId.SinisterStrike);

			SpellHandler.Apply(spell => { spell.CooldownTime = 10000; },
							   SpellId.Poison);
			SpellHandler.Apply(spell => { spell.CooldownTime = 12000; },
							   SpellId.SinisterStrike);

			bazzalanEntry.Activated += bazzalan =>
			{
				var brain = (BaseBrain)bazzalan.Brain;
				var combatAction = (AICombatAction)brain.Actions[BrainState.Combat];
				combatAction.Strategy = new BazzalanAttackAction(bazzalan);
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