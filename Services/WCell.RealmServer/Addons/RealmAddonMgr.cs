using System.IO;
using NLog;
using WCell.Core.Addons;
using WCell.Core.Initialization;
using WCell.RealmServer.Commands;

namespace WCell.RealmServer.Addons
{
	/// <summary>
	/// Static helper and container class of all kinds of Addons
	/// </summary>
	public class RealmAddonMgr : WCellAddonMgr<RealmAddonMgr>
	{
		static readonly Logger log = LogManager.GetCurrentClassLogger();

		public static string AddonDir = "RealmServerAddons";

		/// <summary>
		/// A semicolon-separated (;) list of all libs or folders in the AddonDir that are not to be loaded.
		/// </summary>
		public static string IgnoredAddonFiles = "";

		private static bool inited;

		[Initialization(InitializationPass.First, "Initialize Addons")]
		public static void Initialize(InitMgr mgr)
		{
			if (inited)
			{
				return;
			}
			inited = true;

			LoadAddons(RealmServerConfiguration.BinaryRoot + AddonDir, IgnoredAddonFiles);

			if (Contexts.Count > 0)
			{
				log.Info("Found {0} Addon(s):", Contexts.Count);
				foreach (var context in Contexts)
				{
					log.Info(" Loaded: " + (context.Addon != null
								? context.Addon.GetDefaultDescription()
								: (context.Assembly.GetName().Name)));
					InitAddon(context, mgr);
				}
			}
			else
			{
				log.Info("No addons found.");
			}
		}

		public static void InitAddon(WCellAddonContext context)
		{
			var mgr = new InitMgr();
			InitAddon(context, mgr);
			mgr.AddGlobalMgrsOfAsm(typeof(RealmAddonMgr).Assembly);		// Add all GlobalMgrs
			mgr.PerformInitialization();
		}

		protected static void InitAddon(WCellAddonContext context, InitMgr mgr)
		{
			var addon = context.Addon;

			// add all initialization steps of the Assembly
			mgr.AddStepsOfAsm(context.Assembly);

			// register all Commands of the Assembly
			RealmCommandHandler.Instance.AddCmdsOfAsm(context.Assembly);

			if (addon != null)
			{
				// init config
				if (addon is WCellAddonBase)
				{
					((WCellAddonBase)addon).InitAddon(context);
				}
			}
		}

		public override WCellAddonContext LoadAndInitAddon(string libName)
		{
			if (!Path.IsPathRooted(libName))
			{
				libName = Path.Combine(AddonDir, libName);
			}

			var file = new FileInfo(libName);
			if (!file.Exists)
			{
				libName = Path.Combine(file.Directory.FullName, "WCell." + file.Name);
				file = new FileInfo(libName);
				if (!file.Exists)
				{
					return null;
				}
			}
			return base.LoadAndInitAddon(file);
		}
	}
}