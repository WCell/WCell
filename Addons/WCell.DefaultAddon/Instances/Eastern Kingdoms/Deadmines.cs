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

namespace WCell.Addons.Default.Instances
{
	public class Deadmines : BaseInstance
	{
		static NPCEntry rhahkzorEntry;
		static NPCEntry sneedEntry;
		static NPCEntry sneedShredderEntry;
		static NPCEntry gilnidEntry;
		static NPCEntry smiteEntry;
		static GOEntry defiasCannonEntry;

		static Spell disarm;
		static Spell terrify, distractingPain, ejectSneed;

		#region Setup Content
		[Initialization(InitializationPass.Second)]
		public static void InitSneedSpells()
		{
			disarm = SpellHandler.Get(SpellId.Disarm_2);  //disarm
			disarm.AISettings.SetCooldown(10000);
			disarm.OverrideAITargetDefinitions(DefaultTargetAdders.AddAreaSource,		// random hostile nearby character
											   DefaultTargetEvaluators.RandomEvaluator,
											   DefaultTargetFilters.IsPlayer, DefaultTargetFilters.IsHostile);


            SpellHandler.Apply(spell => spell.SpellCooldowns = new SpellCooldowns { CooldownTime = 20000 }, SpellId.MoltenMetal);
            SpellHandler.Apply(spell => spell.SpellCooldowns = new SpellCooldowns { CooldownTime = 25000 }, SpellId.MeltOre);


			// Rhakzor's slam has a cooldown of about 12s
			SpellHandler.Apply(spell => { spell.AISettings.SetCooldown(10000, 14000); }, SpellId.RhahkZorSlam);


            SpellHandler.Apply(spell => spell.SpellCooldowns.CooldownTime = 10000, SpellId.SmiteSlam);

			// remember the Spells for later use
			terrify = SpellHandler.Get(SpellId.Terrify);
			terrify.AISettings.SetCooldown(21000);
			terrify.OverrideAITargetDefinitions(DefaultTargetAdders.AddAreaSource,		// random hostile nearby character
											   DefaultTargetEvaluators.RandomEvaluator,
											   DefaultTargetFilters.IsPlayer, DefaultTargetFilters.IsHostile);

			distractingPain = SpellHandler.Get(SpellId.DistractingPain);
			distractingPain.AISettings.SetCooldown(12000);
			distractingPain.OverrideAITargetDefinitions(DefaultTargetAdders.AddAreaSource,		// random hostile nearby character
											   DefaultTargetEvaluators.RandomEvaluator,
											   DefaultTargetFilters.IsPlayer, DefaultTargetFilters.IsHostile);

			ejectSneed = SpellHandler.Get(SpellId.EjectSneed);
		}

		[Initialization]
		[DependentInitialization(typeof(NPCMgr))]
		public static void InitNPCs()
		{
			// Rahkzor
			rhahkzorEntry = NPCMgr.GetEntry(NPCId.RhahkZor);
			rhahkzorEntry.BrainCreator = rhahkzor => new RhahkzorBrain(rhahkzor);

			rhahkzorEntry.AddSpell(SpellId.RhahkZorSlam);		// add Rhakzor's slam


			// Sneed
			sneedShredderEntry = NPCMgr.GetEntry(NPCId.SneedsShredder);
			sneedShredderEntry.Died += sneedShredder =>
			{
				// Cast the sneed ejection spell on the corpse after a short delay
				sneedShredder.CallDelayed(2500, (delayed) => { sneedShredder.SpellCast.TriggerSelf(ejectSneed); });
			};

			sneedEntry = NPCMgr.GetEntry(NPCId.Sneed);
			sneedEntry.AddSpell(disarm);
			sneedEntry.Died += sneed =>
			{
				var instance = sneed.Map as Deadmines;
				if (instance != null)
				{
					GameObject door = instance.sneedDoor;
					if (door != null && door.IsInWorld)
					{
						// Open the door
						door.State = GameObjectState.Disabled;
					}
				}
			};


			// Gilnid
			gilnidEntry = NPCMgr.GetEntry(NPCId.Gilnid);

			gilnidEntry.AddSpell(SpellId.MoltenMetal);
			gilnidEntry.AddSpell(SpellId.MeltOre);

			gilnidEntry.Died += gilnid =>
			{
				var instance = gilnid.Map as Deadmines;
				if (instance != null)
				{
					GameObject door = instance.gilnidDoor;

					if (door != null && door.IsInWorld)
					{
						// Open the door
						door.State = GameObjectState.Disabled;
					}
				}
			};

			gilnidEntry.Activated += gilnid =>
			{
				((BaseBrain)gilnid.Brain).DefaultCombatAction.Strategy = new GilnidAttackAction(gilnid);
			};


			// Mr Smite
			smiteEntry = NPCMgr.GetEntry(NPCId.MrSmite);
			smiteEntry.BrainCreator = smite => new SmiteBrain(smite);
			smiteEntry.Activated += smite =>
			{
				((BaseBrain)smite.Brain).DefaultCombatAction.Strategy = new SmiteAttackAction(smite);
			};
		}

		[Initialization]
		[DependentInitialization(typeof(GOMgr))]
		public static void InitGOs()
		{
			GOEntry rhahkzorDoorEntry = GOMgr.GetEntry(GOEntryId.FactoryDoor);		// 13965

			if (rhahkzorDoorEntry != null)
			{
				rhahkzorDoorEntry.Activated += go =>
				{
					var instance = go.Map as Deadmines;
					if (instance != null && instance.rhahkzorDoor == null)
					{
						// set the instance's Door object after the Door spawned
						instance.rhahkzorDoor = go;
					}
				};
			}

			GOEntry sneedDoorEntry = GOMgr.GetEntry(GOEntryId.HeavyDoor);
			var sneedDoorCord = new Vector3(-290.294f, -536.96f, 49.4353f);
			if (sneedDoorEntry != null)
			{
				sneedDoorEntry.Activated += go =>
				{
					var instance = go.Map as Deadmines;
					if (instance != null)
					{
						// set the instance's Door object after the Door spawned
						instance.sneedDoor = go.Map.GetNearestGameObject(sneedDoorCord, (GOEntryId)sneedDoorEntry.Id);
					}
				};
			}

			GOEntry gilnidDoorEntry = GOMgr.GetEntry(GOEntryId.HeavyDoor);
			var gilnidDoorCord = new Vector3(-168.514f, -579.861f, 19.3159f);
			if (gilnidDoorEntry != null)
			{
				gilnidDoorEntry.Activated += go =>
				{
					var instance = go.Map as Deadmines;
					if (instance != null)
					{
						// set the instance's Door object after the Door spawned
						instance.gilnidDoor = go.Map.GetNearestGameObject(gilnidDoorCord, (GOEntryId)gilnidEntry.Id);
					}
				};
			}

			// Cannon
			defiasCannonEntry = GOMgr.GetEntry(GOEntryId.DefiasCannon);

			defiasCannonEntry.Used += (cannon, user) =>
			{
				GameObject door = cannon.GetNearbyGO(GOEntryId.IronCladDoor);
				if (door != null && door.IsInWorld)
				{
					// the cannon destroys the door
					door.State = GameObjectState.Destroyed;
					door.CallDelayed(5000, obj => obj.Delete());	// delete the actual object a little later
					return true;
				}
				return false;
			};
		}
		#endregion

		public GameObject rhahkzorDoor;
		public GameObject sneedDoor; //16400
		public GameObject gilnidDoor; //16399
	}

	#region Rahkzor

	public class RhahkzorBrain : MobBrain
	{
		private GameObject m_Door;

		public RhahkzorBrain(NPC rhahkzor)
			: base(rhahkzor)
		{
		}

		public override void OnEnterCombat()
		{
			//m_owner.PlayTextAndSoundByEnglishPrefix("Van");	// Van Cleef pay big...
			m_owner.PlayTextAndSoundById(-22);
			base.OnEnterCombat();
		}

		public override void OnDeath()
		{
			// open door on death
			var instance = m_owner.Map as Deadmines;

			if (instance != null)
			{
				m_Door = instance.rhahkzorDoor;

				if (m_Door != null)
				{
					// Open the door
					m_Door.State = GameObjectState.Disabled;
				}
			}
			base.OnDeath();
		}
	}
	#endregion

	#region Gilnid
	public class GilnidAttackAction : AIAttackAction
	{
		public GilnidAttackAction(NPC gilnid)
			: base(gilnid)
		{
		}
		/*add his quotes somewhere dont know right now when he says it
		 * "Anyone want to take a break? Well too bad! Get to work you oafs!" 
		 * "Get those parts moving down to the ship!" 
		 * "The cannons must be finished soon." 
		 */
	}
	#endregion

	#region Mr Smite
	public class SmiteAttackAction : AIAttackAction
	{
		public static Vector3 ChestLocation = new Vector3(1.100060f, -780.026367f, 9.811194f);
		private static Spell smiteStomp;
		private static Spell smiteBuff;
		private static Spell smiteSlam;

		[Initialization(InitializationPass.Second)]
		public static void InitSmite()
		{
			smiteStomp = SpellHandler.Get(SpellId.SmiteStomp);
			smiteBuff = SpellHandler.Get(SpellId.SmitesHammer);
			smiteSlam = SpellHandler.Get(SpellId.SmiteSlam);

		}

		private int phase;

		public SmiteAttackAction(NPC smite)
			: base(smite)
		{
		}

		public override void Update()
		{
			if (phase < 2)
			{
				int hpPct = m_owner.HealthPct;
				if (hpPct <= 33 && phase == 1)
				{
					// when below 33% health, do second special action
					m_owner.PlayTextAndSoundById(-176);
					m_owner.SpellCast.Trigger(smiteStomp);		// aoe spell finds targets automatically
					m_owner.Auras.CreateSelf(smiteBuff, true);		// apply buff to self
					MoveToChest();
					phase = 2;
					return;
				}
				else if (hpPct <= 66 && phase == 0)
				{
					// when below 66% health, do first special action
					m_owner.PlayTextAndSoundById(-175);
					m_owner.SpellCast.Trigger(smiteStomp);
					MoveToChest();
					phase = 1;
					return;
				}
			}

			base.Update();
		}

		/// <summary>
		/// Approach the Chest and start using it
		/// </summary>
		public void MoveToChest()
		{
			m_owner.IsInCombat = false;
			m_owner.Target = null;				// deselect target so client wont display npc facing the last guy
			m_owner.MoveToThenExecute(ChestLocation, unit => StartUsingChest());
		}

		/// <summary>
		/// Use Chest and act busy
		/// </summary>
		private void StartUsingChest()
		{
			m_owner.Brain.StopCurrentAction();

			// look at chest and kneel in front of it
			m_owner.Face(ChestLocation);
			m_owner.StandState = StandState.Kneeling;
			m_owner.Emote(EmoteType.SimpleApplaud);

			m_owner.IdleThenExecute(2000, LeaveChest);	// wait 2 seconds then leave and continue doing what we did before
		}

		/// <summary>
		/// Go back to fighting
		/// </summary>
		private void LeaveChest()
		{
			m_owner.StandState = StandState.Stand;
			// from here on, Mr Smite will simply go back to doing what he did before (fighting)
		}
	}

	public class SmiteBrain : MobBrain
	{
		public SmiteBrain(NPC smite)
			: base(smite)
		{
		}

		public override void OnEnterCombat()
		{
			m_owner.PlayTextAndSoundById(-174); // "We're under attack! Avast ya swabs! Repel the invaders!"
			base.OnEnterCombat();
		}
	}
	#endregion
}