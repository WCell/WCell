using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Core.Initialization;
using WCell.Core.Timers;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.AI.Groups;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Instances;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Spawns;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Entities;
using WCell.Constants.Spells;
using WCell.Constants.World;
using WCell.Constants.GameObjects;
using WCell.RealmServer.AI.Actions.Combat;
using System;
using WCell.Util;
using WCell.Util.Graphics;


///
/// This file was automatically created, using WCell's CodeFileWriter
/// Date: 9/1/2009
///

namespace WCell.Addons.Default.Instances
{
	public class UtgardeKeep : BaseInstance
	{
		#region Static Settings
		private static NPCEntry princeKelesethEntry;
		public const int PrinceSkeletonCount = 5;
		internal static NPCEntry PrinceSkeletonEntry;
		private static readonly Vector3[] PrinceSkeletonPositions = new[]
        { 
            new Vector3(156.2559f, 259.2093f, 42.8668f),
            new Vector3(156.2559f, 259.2093f, 42.8668f),
            new Vector3(156.2559f, 259.2093f, 42.8668f),
            new Vector3(156.2559f, 259.2093f, 42.8668f),
            new Vector3(156.2559f, 259.2093f, 42.8668f)
        };
		private static readonly NPCSpawnEntry[] PrinceSkeletonSpawnEntries = new NPCSpawnEntry[PrinceSkeletonCount];
		private static NPCEntry dragonflayerIronhelm;
		#endregion

		/// <summary>
		/// Instance of currently active prince.
		/// Will be unset when prince leaves world, so we don't need any IsInWorld checks!
		/// </summary>
		private NPC PrinceKeleseth;
		private int PrinceDeadSkeletonCount = 0;
		private readonly NPC[] PrinceDeadSkeletons = new NPC[PrinceSkeletonCount];

		#region Setup Content
		[Initialization]
		[DependentInitialization(typeof(NPCMgr))]
		public static void InitNPCs()
		{
			// Dragonflayer Ironhelm
			dragonflayerIronhelm = NPCMgr.GetEntry(NPCId.DragonflayerIronhelm);

			dragonflayerIronhelm.AddSpell(SpellId.HeroicStrike_9);
			SpellHandler.Apply(spell => { spell.CooldownTime = 5000; },
				SpellId.HeroicStrike_9);


			//Prince Keleseth
			SetupPrinceKeleseth();


			// Dragonflayer Ironhelm
			dragonflayerIronhelm = NPCMgr.GetEntry(NPCId.DragonflayerIronhelm);

			dragonflayerIronhelm.AddSpell(SpellId.HeroicStrike_9);
			SpellHandler.Apply(spell => { spell.CooldownTime = 5000; },
				SpellId.HeroicStrike_9);
		}

		#endregion


		#region Prince Keleseth
		/// <summary>
		/// Returns the index of the given skeleton for the skeleton arrays
		/// </summary>
		static int GetSkeletonIndex(NPC skel)
		{
			if (skel.SpawnPoint == null)
			{
				// unrelated Skeleton
				return -1;
			}

			for (var i = 0; i < PrinceSkeletonSpawnEntries.Length; i++)
			{
				var spawn = PrinceSkeletonSpawnEntries[i];
				if (spawn == skel.SpawnPoint.SpawnEntry)
				{
					return i;
				}
			}

			// unrelated Skeleton
			return -1;
		}

		/// <summary>
		/// Returns the skeleton with the given index
		/// </summary>
		NPC GetSkeleton(int index)
		{
			// first check for dead skeleton
			var skel = PrinceDeadSkeletons[index];
			if (skel != null)
			{
				return skel;
			}

			// check in Prince' AIGroup
			if (PrinceKeleseth == null) return null;
			var spawn = PrinceSkeletonSpawnEntries[index];

			// return matching Skeleton
			return PrinceKeleseth.Group.FirstOrDefault(npc => npc.SpawnPoint.SpawnEntry == spawn);
		}

		/// <summary>
		/// Maintain array of dead skeletons when something happens to a skeleton.
		/// AIGroup is managed automagically by the pool system
		/// </summary>
		static void UpdateSkeleton(NPC skel)
		{
			var instance = skel.Map as UtgardeKeep;
			if (instance == null) return;

			var index = GetSkeletonIndex(skel);
			if (index >= 0)
			{
				if (skel.IsAlive || skel.IsDeleted)
				{
					// skeleton is resurrected or deleted
					// remove from list of dead skeletons
					--instance.PrinceDeadSkeletonCount;
					instance.PrinceDeadSkeletons[index] = null;
				}
				else
				{
					// Skeleton has been killed
					// add to list of dead skeletons
					++instance.PrinceDeadSkeletonCount;
					instance.PrinceDeadSkeletons[index] = skel;
				}
			}
		}

		private static void SetupPrinceKeleseth()
		{
			princeKelesethEntry = NPCMgr.GetEntry(NPCId.PrinceKeleseth);
			princeKelesethEntry.BrainCreator = princeKeleseth => new PrinceKelesethBrain(princeKeleseth);

			PrinceSkeletonEntry = NPCMgr.GetEntry(NPCId.VrykulSkeleton);

			// add spell to prince
			PrinceSkeletonEntry.AddSpell(SpellId.Decrepify);
			SpellHandler.Apply(spell => { spell.CooldownTime = 5000; }, SpellId.Decrepify);

			var princeSpawnEntry = princeKelesethEntry.SpawnEntries[0];
			var poolTemplate = princeSpawnEntry.PoolTemplate;

			// do not let Skeletons decay
			PrinceSkeletonEntry.DefaultDecayDelayMillis = 0;

			// add skeleton spawn entries to pool
			for (var i = 0; i < PrinceSkeletonCount; i++)
			{
				var skelSpawnEntry = new NPCSpawnEntry(PrinceSkeletonEntry.NPCId, MapId.UtgardeKeep, PrinceSkeletonPositions[i])
				{
					AutoSpawns = false,						// must not respawn automatically when dead
					IsDead = true,							// spawn dead
					PoolId = poolTemplate.PoolId			// share Prince' pool
				};
				skelSpawnEntry.FinalizeDataHolder();		// adds to PoolTemplate automatically

				PrinceSkeletonSpawnEntries[i] = skelSpawnEntry;
			}

			// give the prince his AttackAction
			princeKelesethEntry.Activated += prince =>
			{
				var instance = prince.Map as UtgardeKeep;
				if (instance == null || prince.SpawnPoint == null) return;

				((BaseBrain)prince.Brain).DefaultCombatAction.Strategy = new PrinceKelesethAttackAction(prince);

				instance.SpawnDeadPrinceSkeletons(prince);
			};

			// prince deleted
			princeKelesethEntry.Deleted += prince =>
			{
				var instance = prince.Map as UtgardeKeep;
				if (instance == null) return;

				// add this "if", in case a GM spawns more than one prince
				if (instance.PrinceKeleseth == prince)
				{
					// unset PrinceKeleseth object
					instance.PrinceKeleseth = null;
				}
			};

			// prince dies
			princeKelesethEntry.Died += prince =>
			{
				var instance = prince.Map as UtgardeKeep;
				if (instance == null) return;

				// kill all skeletons
				instance.KillPrinceSkeletons();
			};

			// update Skeleton if it dies/lives or gets deleted
			PrinceSkeletonEntry.Activated += UpdateSkeleton;
			PrinceSkeletonEntry.Died += UpdateSkeleton;
			PrinceSkeletonEntry.Deleted += UpdateSkeleton;

			princeKelesethEntry.AddSpell(SpellId.ShadowBolt_73);
			SpellHandler.Apply(spell => { spell.CooldownTime = 10000; },
				SpellId.ShadowBolt_73);

			//Heroic
			//princeKelesethEntry.AddSpell(SpellId.ShadowBolt_99);
			//SpellHandler.Apply(spell => { spell.CooldownTime = 5000; },
			//    SpellId.ShadowBolt_73);

			//princeKelesethEntry.AddSpell(SpellId.FrostTomb_3);

			//princeKelesethEntry.AddSpell(SpellId.FrostTomb_3);

			//princeKelesethEntry.AddSpell(SpellId.FrostTombSummon);

			//princeKelesethEntry.AddSpell(SpellId.Decrepify);

			//princeKelesethEntry.AddSpell(SpellId.ScourgeResurrection);

		}

		#region Handle Skeleton group
		/// <summary>
		/// Make sure, skeletons are all dead
		/// </summary>
		private void KillPrinceSkeletons()
		{
			if (PrinceKeleseth == null) return;

			// iterate over all living skeletons
			// need ToArray() to create a copy of the collection, because killing the mob will modify it 
			//			(unless we added a message to Map to kill it in the next tick)
			foreach (var mob in PrinceKeleseth.Group.ToArray())
			{
				if (mob != PrinceKeleseth)
				{
					mob.Kill();
				}
			}
		}

		/// <summary>
		/// Spawn the dead skeletons (if any are missing or happen to be alive)
		/// </summary>
		void SpawnDeadPrinceSkeletons(NPC prince)
		{
			// set this prince to be the one prince of the instance
			PrinceKeleseth = prince;

			// make Prince leader of group
			PrinceKeleseth.Group.Leader = PrinceKeleseth;

			// spawn missing skeletons and add them to array of dead skeletons
			for (var i = 0; i < PrinceSkeletonSpawnEntries.Length; i++)
			{
				// check if skeleton still exists, else, respawn it
				var skel = GetSkeleton(i);
				if (skel == null)
				{
					// Skeleton did not exist -> get SpawnPoint and spawn a new one
					var spawn = prince.SpawnPoint.Pool.SpawnPoints[i + 1];
					spawn.SpawnNow();
				}
				else if (skel.IsAlive)
				{
					// make sure Skeleton is dead
					skel.Kill();
				}
			}
		}
		#endregion

		public class PrinceKelesethBrain : MobBrain
		{
			const string TEXT_AGGRO = "Your blood is mine!";
			const string TEXT_SUMMONING_SKELETOMS = "Aranal, ledel! Their fate shall be yours!";
			const string TEXT_FROSTTOMB = "Not so fast.";
			const string TEXT_DEATH = "I join... the night.";
			const string TEXT_WAIT = "Darkness waits";

			const int SOUND_AGGRO = 13221;
			const int SOUND_FROSTTOMB = 13222;
			const int SOUNG_WAIT = 13223;
			const int SOUND_SUMMONING_SKELETOMS = 13224;
			const int SOUND_DEATH = 13225;

			[Initialization(InitializationPass.Second)]
			public static void InitPrinceKeleseth()
			{

			}

			public PrinceKelesethBrain(NPC princeKeleseth)
				: base(princeKeleseth)
			{
			}

			public override void OnEnterCombat()
			{
				m_owner.Yell(TEXT_AGGRO);
				m_owner.PlaySound((int)SOUND_AGGRO);

				base.OnEnterCombat();
			}

			public override void OnDeath()
			{

				base.OnDeath();
			}

		}

		public class PrinceKelesethAttackAction : AIAttackAction
		{
			[Initialization(InitializationPass.Second)]
			public static void InitPrinceKeleseth()
			{
			}

			public PrinceKelesethAttackAction(NPC princeKeleseth)
				: base(princeKeleseth)
			{
			}

			/// <summary>
			/// The instance in which this prince was spawned.
			/// It is always set because this AIAction is only created for the prince if spawned in an instance
			/// </summary>
			public UtgardeKeep Instance { get { return (UtgardeKeep)m_owner.Map; } }

			public override void Start()
			{
				// Revive the Skeletons
				ResurrectSkeletons();

				// Cheking if Vrykul Skeleton is death
				// if all Skeleton is death ress all
				//m_owner.SpellCast.Start(SpellId.ScourgeResurrection, false, SkeletonsMinions);
				m_owner.CallPeriodically(10000, CheckVrykulSekeltonIsDead);

				base.Start();
			}

			public override void Update()
			{

				base.Update();
			}

			public override void Stop()
			{

				base.Stop();
			}


			void CheckVrykulSekeltonIsDead(WorldObject owner)
			{
				if (!m_owner.IsAlive || m_owner.IsDeleted) return;

				if (Instance.PrinceDeadSkeletons.Any(mob => mob == null))
				{
					// at least one skeleton is still alive in the prince' group
					return;
				}

				ResurrectSkeletons();
			}

			void ResurrectSkeletons()
			{
				foreach (var mob in Instance.PrinceDeadSkeletons)
				{
					// if Skeleton is alive, it is not in the PrinceDeadSkeletons array
					if (mob != null)
					{
						// resurrect skeleton
						m_owner.SpellCast.Trigger(SpellId.ScourgeResurrection, mob);
						mob.HealthPct = 100;
					}
				}
			}
		}

		#region Minion Vrykul Skeleton(23970 - NPCId.VrykulSkeleton)

		public class VrykulSkeletonAttackAction : AIAttackAction
		{
			internal static Spell decrepify;
			private ObjectUpdateTimer decreptifyTimer;

			[Initialization(InitializationPass.Second)]
			public static void InitVrykulSkeleton()
			{
				decrepify = SpellHandler.Get(SpellId.Decrepify);
			}

			public VrykulSkeletonAttackAction(NPC vrykulSkeleton)
				: base(vrykulSkeleton)
			{
			}

			public override void Start()
			{
				decreptifyTimer = m_owner.CallPeriodically(Utility.Random(5000, 10000), CastDecrepify);
				base.Start();
			}

			public override void Stop()
			{
				base.Stop();
			}


			void CastDecrepify(WorldObject owner)
			{

				owner.Yell("Hi");

				//Character chr = owner.GetNearbyRandomHostileCharacter();

				//if (chr != null)
				//{
				//    m_owner.SpellCast.Start(decrepify, false, chr);
				//}

			}
		}

		#endregion

		#endregion

		#region Overrides
		protected override void SpawnNPCs()
		{
			base.SpawnNPCs();
		}
		#endregion

	}

}