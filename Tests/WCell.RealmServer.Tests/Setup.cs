using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Factions;
using WCell.Core.Database;
using WCell.RealmServer.Database;
using WCell.Core;
using WCell.Constants;
using WCell.RealmServer.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.RealmServer.RacesClasses;
using WCell.RealmServer.Network;
using WCell.RealmServer.Items;
using WCell.RealmServer.Privileges;
using WCell.Intercommunication.DataTypes;
using Castle.ActiveRecord;
using WCell.RealmServer.Global;
using WCell.RealmServer.NPCs;
using System.IO;
using WCell.RealmServer.Debugging;
using WCell.Util.Graphics;
using WCell.Util.Logging;
using WCell.Util.Threading;
using WCell.Constants.World;
using WCell.RealmServer.Tests.Entities;
using WCell.RealmServer.Tests.Misc;
using WCell.RealmServer.Tests.Tools;
using System.Threading;
using WCell.Constants.Items;
using WCell.Util;
using WCell.RealmServer.Addons;
using System.Diagnostics;

namespace WCell.RealmServer.Tests
{
	public static class Setup
	{
		public static string RunDir = "../../../Run/";
		
		//public static string RealmServerDir { get { return RunDir + "Debug/"; } }

		public static string RealmServerDebugDir { get { return RunDir + "Debug/"; } }

		/// <summary>
		/// The location of WCell's executable in the Debug/ folder
		/// </summary>
		public static string WCellRealmServerConsoleExe { get { return RealmServerDebugDir + "WCell.RealmServerConsole.exe"; } }

		public static string ContentDir { get { return RunDir + "Content/"; } }
		public static string RealmAddonDir { get { return RealmServerDebugDir + "/RealmServerAddons/"; } }

		public static string DumpDir { get { return "Dumps/"; } }
		public static string LogFile { get { return "LastTestOutput.txt"; } }

		static TextWriter m_output;

		private static CharacterPool m_hordeCharacterPool;
		private static CharacterPool m_allianceCharacterPool;
		static AccountPool m_accountPool;
		static NPCPool m_npcPool;
		static ItemPool m_itemPool;
		static bool initialized = false;

		static Vector3 m_defaultPos;

		#region Properties
		public static CharacterPool AllianceCharacterPool
		{
			get
			{
				if (m_allianceCharacterPool == null)
				{
					m_allianceCharacterPool = new CharacterPool(FactionGroup.Alliance);
				}
				return m_allianceCharacterPool;
			}
		}

		public static CharacterPool HordeCharacterPool
		{
			get
			{
				if (m_hordeCharacterPool == null)
				{
					m_hordeCharacterPool = new CharacterPool(FactionGroup.Horde);
				}
				return m_hordeCharacterPool;
			}
		}

		public static AccountPool AccountPool
		{
			get
			{
				if (m_accountPool == null)
				{
					m_accountPool = new AccountPool();
				}
				return m_accountPool;
			}
		}

		public static NPCPool NPCPool
		{
			get
			{
				if (m_npcPool == null)
				{
					EnsureMinimalSetup();
					m_npcPool = new NPCPool();
				}
				return m_npcPool;
			}
		}

		public static ItemPool ItemPool
		{
			get
			{
				if (m_itemPool == null)
				{
					m_itemPool = new ItemPool();
				}
				return m_itemPool;
			}
		}

		/// <summary>
		/// Always returns the same Account.
		/// Make sure not to login multiple Characters with the same Account,
		/// since it might lead to unexpected behavior.
		/// </summary>
		public static TestAccount DefaultAccount
		{
			get
			{
				return AccountPool.First;
			}
		}

		public static TestCharacter DefaultCharacter
		{
			get
			{
				return AllianceCharacterPool.First;
			}
		}

		public static FakeRealmClient DefaultClient
		{
			get
			{
				return DefaultCharacter.FakeClient;
			}
		}

		/// <summary>
		/// Some default region
		/// </summary>
		public static Region Kalimdor
		{
			get
			{
				World.InitializeWorld();
				return World.Kalimdor;
			}
		}

		/// <summary>
		/// Some default region
		/// </summary>
		public static Region EasternKingdoms
		{
			get
			{
				World.InitializeWorld();
				return World.EasternKingdoms;
			}
		}

		public static Vector3 DefaultPosition
		{
			get
			{
				if (m_defaultPos == Vector3.Zero)
				{
					m_defaultPos = new Vector3(1000, 1000, 10);
				}
				return m_defaultPos;
			}
			set
			{
				m_defaultPos = value;
			}
		}

		public static TextWriter TestOutput
		{
			get { return m_output; }
		}
		#endregion

		/// <summary>
		/// Creates a new unique CharacterRecord (not unique within the DB though)
		/// </summary>
		public static CharacterRecord CreateCharRecord()
		{
			var record = new CharacterRecord(0) { EntityLowId = CharacterRecord.NextId() };
			//record.EntityLowId = nextCharId++;
			return record;
		}

		#region Initialization etc

		public static void EnsureMinimalSetup()
		{
			if (!initialized)
			{
				initialized = true;

				if (!Directory.Exists(RunDir))
				{
					// Maybe the CWD randomly moved to Run/Debug/TestResults/TestRunName123424239453285894983435/Out/
					// Use Directory.SetCurrentDirectory(...); to change it
					RunDir = "../../../../";
					if (!Directory.Exists(RunDir))
					{
						throw new DirectoryNotFoundException(string.Format("RunDir was not found at {0} (CWD = {1})",
						                                                   new DirectoryInfo(RunDir).FullName,
						                                                   new DirectoryInfo(Directory.GetCurrentDirectory()).FullName));
					}
				}

				if (!File.Exists(WCellRealmServerConsoleExe))
				{
					throw new DirectoryNotFoundException(string.Format("WCellRealmServerConsole.exe was not found at {0} (CWD = {1})",
						new FileInfo(WCellRealmServerConsoleExe).FullName, new DirectoryInfo(Directory.GetCurrentDirectory()).FullName));
				}

				// since console will not show, lets echo console output to a file:
				Console.SetOut(m_output = new IndentTextWriter(LogFile) {
					AutoFlush = true
				});
				//LogUtil.SetupStreamLogging(m_output);

				RealmServer.EntryLocation = WCellRealmServerConsoleExe;
				var realmServ = RealmServer.Instance;				// make sure to create the RealmServer instance first

				RealmServerConfiguration.Instance.AutoSave = false;

				RealmServer.ConsoleActive = false;
				RealmServerConfiguration.ContentDirName = Path.GetFullPath(ContentDir);
				RealmServerConfiguration.Initialize();
				RealmAddonMgr.AddonDir = RealmAddonDir;

				DebugUtil.DumpDirName = DumpDir;
				DebugUtil.Init();

				LogUtil.ExceptionRaised += new Action<string,Exception>(LogUtil_ExceptionRaised);

				// some sample roles
				PrivilegeMgr.Instance.SetGroupInfo(new[] {
					RoleGroupInfo.LowestRole = new RoleGroupInfo("Guest", 1, RoleStatus.Player, true, true, true, true, false, true, null, new[] { "*" }),
					RoleGroupInfo.HighestRole = new RoleGroupInfo("Admin", 1000, RoleStatus.Admin, true, true, true, true, true, false, null, new [] { "*" })
				});
			}
		}

		private static void LogUtil_ExceptionRaised(string msg, Exception ex)
		{
			// Test failed -> Check in: LogFile
			RealmServer.Instance.ShutdownIn(0);
			Debugger.Break();
			Process.GetCurrentProcess().Kill();
		}

		public static void EnsureDBSetup()
		{
			EnsureMinimalSetup();
			if (!RealmDBUtil.Initialized)
			{
				ResetDB();
			}
		}

		public static void EnsureBasicSetup()
		{
			EnsureMinimalSetup();

			if (!RealmServer.Instance.IsRunning)
			{
				var dbSetup = RealmDBUtil.Initialized;

				RealmServer.Instance.Start();
				RealmServer.Instance.AuthClient.IsRunning = false;

				Assert.IsTrue(RealmServer.Instance.IsRunning,
							  "RealmServer failed to initialize - See the log file for more information: " +
							  new FileInfo(LogFile).FullName);

				if (!dbSetup)
				{
					ResetDB();
				}
			}
		}

		public static void ResetDB()
		{
			RealmDBUtil.Initialize();

			int count = CharacterRecord.GetCount();

			if (count > 500)
			{
				throw new Exception("Cannot run tests on production servers since it requires to drop the complete Database. " +
									"Test run aborted because " + count + " Characters were found (must not be more than 500). Drop the Database manually to proceed.");
			}

			DatabaseUtil.DropSchema();
			DatabaseUtil.CreateSchema();
		}

		public static void EnsureItemsLoaded()
		{
			EnsureBasicSetup();
			ItemMgr.ForceInitialize();

			//ItemMgr.InitMisc();

			//// add Dummy items
			//foreach (ItemId itemId in Enum.GetValues(typeof(ItemId)))
			//{
			//    new ItemTemplate {
			//        Id = (uint)itemId,
			//        ItemId = itemId,
			//        Name = itemId.ToString(),
			//        Resistances = new int[0],
			//        Damages = new[] { new DamageInfo(DamageSchoolMask.Physical, 1, 2) },
			//        Sockets = new SocketInfo[3]

			//    }.FinalizeAfterLoad();
			//}
		}

		public static void EnsureNPCsLoaded()
		{
			EnsureBasicSetup();
			NPCMgr.ForceInitialize();
		}
		#endregion

		public static void WriteLine(string str, params object[] args)
		{
			m_output.WriteLine(string.Format(str, args));
		}

		/// <summary>
		/// Adds the given object to the default region at a default location
		/// </summary>
		public static void AddToDefaultRegion(WorldObject obj)
		{
			var region = Kalimdor;
			region.AddMessageAndWait(new Message(() => {
				Kalimdor.AddObjectNow(obj, ref m_defaultPos);
				if (obj is Character)
				{
					Kalimdor.ForceUpdateCharacters();
				}
			}));

			Assert.AreEqual(obj.Region, Kalimdor);
			//Assert.IsTrue(obj.IsInWorld);
		}

		/// <summary>
		/// Ensures that the given object is in the World
		/// </summary>
		public static void EnsureInWorld(WorldObject obj)
		{
			if (!obj.IsInWorld)
			{
				AddToDefaultRegion(obj);
			}
		}

		public static void WriteRamUsage(string msg, params object[] args)
		{
			var ramBefore = GC.GetTotalMemory(false);
			GC.Collect();
			msg = string.Format(msg, args);
			WriteLine(msg + ": Ram usage: {0}, After collect: {1}", ramBefore, GC.GetTotalMemory(true));
		}

		static void Main(string[] args)
		{
		}
	}
}