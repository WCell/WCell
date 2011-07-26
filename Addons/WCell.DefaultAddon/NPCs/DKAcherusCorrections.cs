using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.GameObjects;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.AI;
using WCell.RealmServer.AI.Actions;
using WCell.RealmServer.AI.Actions.Combat;
using WCell.RealmServer.AI.Actions.Movement;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Items;
using WCell.RealmServer.Misc;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Targeting;

namespace WCell.Addons.Default.NPCs
{
	public static class DKAcherusCorrections
	{

		[Initialization]
		[DependentInitialization(typeof (NPCMgr))]
		public static void FixThem()
		{
			var entry = NPCMgr.GetEntry(NPCId.UnworthyInitiate);
			entry.Activated += UnworthyInitiateActivated;
			entry.Activated += initiate =>
			                   	{
			                   		((BaseBrain) initiate.Brain).DefaultCombatAction.Strategy =
			                   			new UnworthyInitiateAttackAction(initiate);
			                   	};
			entry = NPCMgr.GetEntry(NPCId.UnworthyInitiate_2);
			entry.Activated += UnworthyInitiateActivated;
			entry.Activated += initiate =>
			                   	{
			                   		((BaseBrain) initiate.Brain).DefaultCombatAction.Strategy =
			                   			new UnworthyInitiateAttackAction(initiate);
			                   	};
			entry = NPCMgr.GetEntry(NPCId.UnworthyInitiate_3);
			entry.Activated += UnworthyInitiateActivated;
			entry.Activated += initiate =>
			                   	{
			                   		((BaseBrain) initiate.Brain).DefaultCombatAction.Strategy =
			                   			new UnworthyInitiateAttackAction(initiate);
			                   	};
			entry = NPCMgr.GetEntry(NPCId.UnworthyInitiate_4);
			entry.Activated += UnworthyInitiateActivated;
			entry.Activated += initiate =>
			                   	{
			                   		((BaseBrain) initiate.Brain).DefaultCombatAction.Strategy =
			                   			new UnworthyInitiateAttackAction(initiate);
			                   	};
			entry = NPCMgr.GetEntry(NPCId.UnworthyInitiate_5);
			entry.Activated += UnworthyInitiateActivated;
			entry.Activated += initiate =>
			                   	{
			                   		((BaseBrain) initiate.Brain).DefaultCombatAction.Strategy =
			                   			new UnworthyInitiateAttackAction(initiate);
			                   	};
		}

		private static void UnworthyInitiateActivated(NPC npc)
		{
			npc.StandState = StandState.Kneeling;
			npc.AddMessage(
				() =>
					{
						var nearest = npc.GetNearbyNPC(NPCId.UnworthyInitiateAnchor, 7);
						if (nearest == null) return;
						nearest.SpellCast.Trigger(SpellId.ChainedPeasantChest, npc);
					});
		}
	}

	public class UnworthyInitiateAttackAction : AIAttackAction
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();

		public UnworthyInitiateAttackAction(NPC unworthyInitiate)
			: base(unworthyInitiate)
		{
		}

		/// <summary>
		/// Called when starting to attack a new Target
		/// </summary>
		public override void Start()
		{
			m_owner.IsFighting = true;
			if (UsesSpells)
			{
				((NPC)m_owner).NPCSpells.ShuffleReadySpells();
			}

			m_target = m_owner.Target;
			if (m_target != null)
			{
				maxDist = m_owner.GetAttackRange(m_owner.MainWeapon, m_target) - 1;
				if (maxDist < 0.5f)
				{
					maxDist = 0.5f;
				}
				desiredDist = maxDist / 2;
			}
			if (m_owner.CanMelee)
			{
				if (m_target == null)
				{
					if (m_owner.Target == null)
					{
						Log.Error("Started " + GetType().Name + " without Target set: " + m_owner);
						m_owner.Brain.EnterDefaultState();
						return;
					}
					
					m_target = m_owner.Target;
				}
				Update();
			}
		}
	}
}
