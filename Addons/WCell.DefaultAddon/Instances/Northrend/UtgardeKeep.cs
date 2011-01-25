using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Core.Initialization;
using WCell.Core.Timers;
using WCell.RealmServer.AI.Brains;
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
	public class UtgardeKeep : DungeonInstance
	{
		private static NPCEntry princeKelesethEntry;
		public const int PrinceSkeletonCount = 5;
		internal static NPCEntry PrinceSkeletonEntry;
		private static readonly Vector3[] PrinceSkeletonPositions = new []
        { 
            new Vector3(156.2559f, 259.2093f, 42.8668f),
            new Vector3(156.2559f, 259.2093f, 42.8668f),
            new Vector3(156.2559f, 259.2093f, 42.8668f),
            new Vector3(156.2559f, 259.2093f, 42.8668f),
            new Vector3(156.2559f, 259.2093f, 42.8668f)
        };

		private static NPCEntry dragonflayerIronhelm;


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

		private static void SetupPrinceKeleseth()
		{
			princeKelesethEntry = NPCMgr.GetEntry(NPCId.PrinceKeleseth);
			princeKelesethEntry.BrainCreator = princeKeleseth => new PrinceKelesethBrain(princeKeleseth);

			PrinceSkeletonEntry = NPCMgr.GetEntry(NPCId.VrykulSkeleton);

			// add spell to prince
			PrinceSkeletonEntry.AddSpell(SpellId.Decrepify);
			SpellHandler.Apply(spell => { spell.CooldownTime = 5000; }, SpellId.Decrepify);

			// add the dead skeletons to the prince' SpawnPool
			var princeSpawnEntry = princeKelesethEntry.SpawnEntries[0];

			for (var i = 0; i < PrinceSkeletonCount; i++)
			{
				var skelSpawnEntry = new NPCSpawnEntry(PrinceSkeletonEntry.NPCId, MapId.UtgardeKeep, PrinceSkeletonPositions[i])
				{
					IsDead = true
				};
				skelSpawnEntry.FinalizeDataHolder(false);

				princeSpawnEntry.PoolTemplate.AddEntry(skelSpawnEntry);
			}


			// give the prince his AttackAction
			princeKelesethEntry.Activated += princeKeleseth =>
			{
				((BaseBrain)princeKeleseth.Brain).DefaultCombatAction.Strategy = new PrinceKelesethAttackAction(princeKeleseth);
			};

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


			public override void Start()
			{
				// Revive the Skeletons
				foreach (var mob in Skeletons)
				{
					// resurrect skeleton
					m_owner.SpellCast.Trigger(SpellId.SoulstoneResurrection_7, mob);
				}

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

			/// <summary>
			/// Returns all skeletons that are in the prince' AIGroup
			/// </summary>
			public IEnumerable<NPC> Skeletons
			{
				get { return ((NPC)m_owner).Group.Where(mob => mob.Entry.Id == PrinceSkeletonEntry.Id); }
			}


			void CheckVrykulSekeltonIsDead(WorldObject owner)
			{
				if (Skeletons.Any(mob => mob.IsAlive))
				{
					// at least one skeleton is still alive
					return;
				}

				foreach (var mob in Skeletons)
				{
					// resurrect skeleton
					m_owner.SpellCast.Trigger(SpellId.SoulstoneResurrection_7, mob);
				}
			}
		}

		#region Minion Vrykul Skeleton(23970 - NPCId.VrykulSkeleton)

		public class VrykulSkeletonAttackAction : AIAttackAction
		{
			internal static Spell decrepify;
			private IUpdateObjectAction decreptifyTimer;

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

	}

}