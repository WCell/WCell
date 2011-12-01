using System;
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

namespace WCell.Addons.Default.Instances
{
	public class TheStockades : BaseInstance
	{
		#region Setup Content
		private static NPCEntry kamdeepfuryEntry;
		private static NPCEntry hamhockEntry;
		private static NPCEntry bazilThreddEntry;
		private static NPCEntry dextrenEntry;
		private static NPCEntry inmateEntry;
		private static NPCEntry insurgentEntry;
		private static NPCEntry prisonerEntry;
		private static NPCEntry convictEntry;

		[Initialization]
		[DependentInitialization(typeof(NPCMgr))]
		public static void InitNPCs()
		{
			// KamDeepfury
			kamdeepfuryEntry = NPCMgr.GetEntry(NPCId.KamDeepfury);
			kamdeepfuryEntry.Activated += kamdeepfury =>
			{
				((BaseBrain)kamdeepfury.Brain).DefaultCombatAction.Strategy = new KamdeepfuryAttackAction(kamdeepfury);
			};

			// Hamhock
			hamhockEntry = NPCMgr.GetEntry(NPCId.Hamhock);
			hamhockEntry.Activated += hamhock =>
			{
				((BaseBrain)hamhock.Brain).DefaultCombatAction.Strategy = new HamhockAttackAction(hamhock);
			};

			// Bazil
			bazilThreddEntry = NPCMgr.GetEntry(NPCId.BazilThredd);
			bazilThreddEntry.Activated += bazilthredd =>
			{
				((BaseBrain)bazilthredd.Brain).DefaultCombatAction.Strategy = new BazilThreddAttackAction(bazilthredd);
			};

			// Dextren
			dextrenEntry = NPCMgr.GetEntry(NPCId.DextrenWard);
			dextrenEntry.Activated += dextren =>
			{
				((BaseBrain)dextren.Brain).DefaultCombatAction.Strategy = new DextrenAttackAction(dextren);
			};

			// Defias Inmate
			inmateEntry = NPCMgr.GetEntry(NPCId.DefiasInmate);
			inmateEntry.Activated += inmate =>
			{
				((BaseBrain)inmate.Brain).DefaultCombatAction.Strategy = new InmateAttackAction(inmate);
			};

			// Defias Insurgent
			insurgentEntry = NPCMgr.GetEntry(NPCId.DefiasInsurgent);
			insurgentEntry.Activated += insurgent =>
			{
				((BaseBrain)insurgent.Brain).DefaultCombatAction.Strategy = new InsurgentAttackAction(insurgent);
			};

			// Defias Prisoner
			prisonerEntry = NPCMgr.GetEntry(NPCId.DefiasPrisoner);
			prisonerEntry.Activated += prisoner =>
			{
				((BaseBrain)prisoner.Brain).DefaultCombatAction.Strategy = new PrisonerAttackAction(prisoner);
			};

			// Defias Convict
			convictEntry = NPCMgr.GetEntry(NPCId.DefiasConvict);
			convictEntry.Activated += convict =>
			{
				((BaseBrain)convict.Brain).DefaultCombatAction.Strategy = new ConvictAttackAction(convict);
			};
		}

		[Initialization]
		[DependentInitialization(typeof(GOMgr))]
		public static void InitGOs()
		{

		}
		#endregion

	}

	#region Kamdeepfury
	public class KamdeepfuryAttackAction : AIAttackAction
	{
		internal static Spell shieldSlam;

		private DateTime timeSinceLastInterval;
		private const int interVal = 1;
		private int shieldSlamTick;

		[Initialization(InitializationPass.Second)]
		static void InitKamdeepfury()
		{
			shieldSlam = SpellHandler.Get(SpellId.ShieldSlam);
		}

		public KamdeepfuryAttackAction(NPC kamdeepfury)
			: base(kamdeepfury)
		{
		}

		public override void Start()
		{
			shieldSlamTick = 0;
			timeSinceLastInterval = DateTime.Now;
			base.Start();
		}

		public override void Update()
		{
			var timeNow = DateTime.Now;
			var timeBetween = timeNow - timeSinceLastInterval;

			if (timeBetween.TotalSeconds >= interVal)
			{
				timeSinceLastInterval = timeNow;
				if (CheckSpellCast())
				{
					// idle a little after casting a spell
					m_owner.Idle(1000);
					return;
				}
			}

			base.Update();
		}

		private bool CheckSpellCast()
		{
			shieldSlamTick++;

			if (shieldSlamTick >= 12)
			{
				var chr = m_owner.GetNearbyRandomHostileCharacter();
				if (chr != null)
				{
					shieldSlamTick = 0;
					m_owner.SpellCast.Start(shieldSlam, false, chr);
				}
				return true;
			}

			return false;
		}
	}
	#endregion

	#region Hamhock
	public class HamhockAttackAction : AIAttackAction
	{
		internal static Spell chainLight, demoralize;

		private DateTime timeSinceLastInterval;
		private const int interVal = 1;
		private int chainLightTick;
		private int demoralizeTick;

		[Initialization(InitializationPass.Second)]
		static void InitHamhock()
		{
			chainLight = SpellHandler.Get(SpellId.ChainLightning);
			demoralize = SpellHandler.Get(SpellId.Demoralize);
		}

		public HamhockAttackAction(NPC hamhock)
			: base(hamhock)
		{
		}

		public override void Start()
		{
			timeSinceLastInterval = DateTime.Now;
			base.Start();
		}

		public override void Update()
		{
			var timeNow = DateTime.Now;
			var timeBetween = timeNow - timeSinceLastInterval;

			if (timeBetween.TotalSeconds >= interVal)
			{
				timeSinceLastInterval = timeNow;
				if (CheckSpellCast())
				{
					// idle a little after casting a spell
					m_owner.Idle(1000);
					return;
				}
			}

			base.Update();
		}

		private bool CheckSpellCast()
		{
			chainLightTick++;
			demoralizeTick++;

			if (chainLightTick >= 12)
			{
				var chr = m_owner.GetNearbyRandomHostileCharacter();
				if (chr != null)
				{
					chainLightTick = 0;
					m_owner.Yell("Casting chainlight spell"); // TODO: Remove this line
					m_owner.SpellCast.Start(chainLight, false, chr);
				}
				return true;
			}
			if (demoralizeTick >= 21)
			{
				var chr = m_owner.GetNearbyRandomHostileCharacter();
				if (chr != null)
				{
					demoralizeTick = 0;
					m_owner.Yell("Casting demoralize spell"); // TODO: Remove this line
					m_owner.SpellCast.Start(demoralize, false, chr);
				}
				return true;
			}
			return false;
		}
	}
	#endregion

	#region BazilThredd
	public class BazilThreddAttackAction : AIAttackAction
	{
		internal static Spell bazilBomb;

		private DateTime timeSinceLastInterval;
		private const int interVal = 1;
		private int bazilBombTick;

		[Initialization(InitializationPass.Second)]
		static void InitBazilThredd()
		{
			bazilBomb = SpellHandler.Get(SpellId.Bomb);
		}

		public BazilThreddAttackAction(NPC bazilthredd)
			: base(bazilthredd)
		{
		}

		public override void Start()
		{
			timeSinceLastInterval = DateTime.Now;
			base.Start();
		}

		public override void Update()
		{
			var timeNow = DateTime.Now;
			var timeBetween = timeNow - timeSinceLastInterval;

			if (timeBetween.TotalSeconds >= interVal)
			{
				timeSinceLastInterval = timeNow;
				if (CheckSpellCast())
				{
					// idle a little after casting a spell
					m_owner.Idle(1000);
					return;
				}
			}

			base.Update();
		}

		private bool CheckSpellCast()
		{
			bazilBombTick++;

			if (bazilBombTick >= 12)
			{
				var chr = m_owner.GetNearbyRandomHostileCharacter();
				if (chr != null)
				{
					bazilBombTick = 0;
					m_owner.SpellCast.Start(bazilBomb, false, chr);
				}
				return true;
			}

			return false;
		}
	}
	#endregion

	#region Dextren
	public class DextrenAttackAction : AIAttackAction
	{
		internal static Spell intimidation;

		private DateTime timeSinceLastInterval;
		private const int interVal = 1;
		private int intimidationTick;

		[Initialization(InitializationPass.Second)]
		static void InitDextren()
		{
			intimidation = SpellHandler.Get(SpellId.Intimidation);
		}

		public DextrenAttackAction(NPC dextren)
			: base(dextren)
		{
		}

		public override void Start()
		{
			timeSinceLastInterval = DateTime.Now;
			base.Start();
		}

		public override void Update()
		{
			var timeNow = DateTime.Now;
			var timeBetween = timeNow - timeSinceLastInterval;

			if (timeBetween.TotalSeconds >= interVal)
			{
				timeSinceLastInterval = timeNow;
				if (CheckSpellCast())
				{
					// idle a little after casting a spell
					m_owner.Idle(1000);
					return;
				}
			}

			base.Update();
		}

		private bool CheckSpellCast()
		{
			intimidationTick++;

			if (intimidationTick >= 12)
			{
				var chr = m_owner.GetNearbyRandomHostileCharacter();
				if (chr != null)
				{
					intimidationTick = 0;
					m_owner.SpellCast.Start(intimidation, false, chr);
				}
				return true;
			}

			return false;
		}
	}
	#endregion

	#region Defias Inmate
	public class InmateAttackAction : AIAttackAction
	{
		internal static Spell convictrend;

		private DateTime timeSinceLastInterval;
		private const int interVal = 1;
		private int convictrendTick;

		[Initialization(InitializationPass.Second)]
		static void InitInmate()
		{
			convictrend = SpellHandler.Get(SpellId.Rend);
		}

		public InmateAttackAction(NPC dextren)
			: base(dextren)
		{
		}

		public override void Start()
		{
			timeSinceLastInterval = DateTime.Now;
			base.Start();
		}

		public override void Update()
		{
			var timeNow = DateTime.Now;
			var timeBetween = timeNow - timeSinceLastInterval;

			if (timeBetween.TotalSeconds >= interVal)
			{
				timeSinceLastInterval = timeNow;
				if (CheckSpellCast())
				{
					// idle a little after casting a spell
					m_owner.Idle(1000);
					return;
				}
			}

			base.Update();
		}

		private bool CheckSpellCast()
		{
			convictrendTick++;

			if (convictrendTick >= 12)
			{
				var chr = m_owner.GetNearbyRandomHostileCharacter();
				if (chr != null)
				{
					convictrendTick = 0;
					m_owner.SpellCast.Start(convictrend, false, chr);
				}
				return true;
			}

			return false;
		}
	}
	#endregion

	#region Defias Insurgent
	public class InsurgentAttackAction : AIAttackAction
	{
		internal static Spell demoralize;

		private DateTime timeSinceLastInterval;
		private const int interVal = 1;
		private int demoralizeTick;

		[Initialization(InitializationPass.Second)]
		static void InitInsurgent()
		{
			demoralize = SpellHandler.Get(SpellId.DemoralizingShout);
		}

		public InsurgentAttackAction(NPC insurgent)
			: base(insurgent)
		{
		}

		public override void Start()
		{
			timeSinceLastInterval = DateTime.Now;
			base.Start();
		}

		public override void Update()
		{
			var timeNow = DateTime.Now;
			var timeBetween = timeNow - timeSinceLastInterval;

			if (timeBetween.TotalSeconds >= interVal)
			{
				timeSinceLastInterval = timeNow;
				if (CheckSpellCast())
				{
					// idle a little after casting a spell
					m_owner.Idle(1000);
					return;
				}
			}

			base.Update();
		}

		private bool CheckSpellCast()
		{
			demoralizeTick++;

			if (demoralizeTick >= 12)
			{
				var chr = m_owner.GetNearbyRandomHostileCharacter();
				if (chr != null)
				{
					demoralizeTick = 0;
					m_owner.SpellCast.Start(demoralize, false, chr);
				}
				return true;
			}

			return false;
		}
	}
	#endregion

	#region Defias Prisoner
	public class PrisonerAttackAction : AIAttackAction
	{
		internal static Spell prisonKick, prisonDisarm;

		private DateTime timeSinceLastInterval;
		private const int interVal = 1;
		private int prisonKickTick;
		private int prisonDisarmTick;

		[Initialization(InitializationPass.Second)]
		static void InitPrison()
		{
			prisonKick = SpellHandler.Get(SpellId.ClassSkillKick);
			prisonDisarm = SpellHandler.Get(SpellId.Disarm_2);
		}

		public PrisonerAttackAction(NPC prisoner)
			: base(prisoner)
		{
		}

		public override void Start()
		{
			timeSinceLastInterval = DateTime.Now;
			base.Start();
		}

		public override void Update()
		{
			var timeNow = DateTime.Now;
			var timeBetween = timeNow - timeSinceLastInterval;

			if (timeBetween.TotalSeconds >= interVal)
			{
				timeSinceLastInterval = timeNow;
				if (CheckSpellCast())
				{
					// idle a little after casting a spell
					m_owner.Idle(1000);
					return;
				}
			}

			base.Update();
		}

		private bool CheckSpellCast()
		{
			prisonKickTick++;
			prisonDisarmTick++;

			if (prisonKickTick >= 12)
			{
				var chr = m_owner.GetNearbyRandomHostileCharacter();
				if (chr != null)
				{
					prisonKickTick = 0;
					m_owner.SpellCast.Start(prisonKick, false, chr);
				}
				return true;
			}
			if (prisonKickTick >= 24)
			{
				var chr = m_owner.GetNearbyRandomHostileCharacter();
				if (chr != null)
				{
					prisonDisarmTick = 0;
					m_owner.SpellCast.Start(prisonDisarm, false, chr);
				}
				return true;
			}

			return false;
		}
	}
	#endregion

	#region Defias Convict
	public class ConvictAttackAction : AIAttackAction
	{
		internal static Spell backhand;

		private DateTime timeSinceLastInterval;
		private const int interVal = 1;
		private int backhandTick;

		[Initialization(InitializationPass.Second)]
		static void InitConvict()
		{
			backhand = SpellHandler.Get(SpellId.Backhand);
		}

		public ConvictAttackAction(NPC convict)
			: base(convict)
		{
		}

		public override void Start()
		{
			timeSinceLastInterval = DateTime.Now;
			base.Start();
		}

		public override void Update()
		{
			var timeNow = DateTime.Now;
			var timeBetween = timeNow - timeSinceLastInterval;

			if (timeBetween.TotalSeconds >= interVal)
			{
				timeSinceLastInterval = timeNow;
				if (CheckSpellCast())
				{
					// idle a little after casting a spell
					m_owner.Idle(1000);
					return;
				}
			}

			base.Update();
		}

		private bool CheckSpellCast()
		{
			backhandTick++;

			if (backhandTick >= 12)
			{
				var chr = m_owner.GetNearbyRandomHostileCharacter();
				if (chr != null)
				{
					backhandTick = 0;
					m_owner.SpellCast.Start(backhand, false, chr);
				}
				return true;
			}

			return false;
		}
	}
	#endregion
}