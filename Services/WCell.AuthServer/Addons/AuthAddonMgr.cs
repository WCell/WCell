using WCell.AuthServer.Commands;
using WCell.Core.Addons;
using WCell.Core.Initialization;
using NLog;

namespace WCell.AuthServer.Addons
{
	/// <summary>
	/// Static helper and container class of all kinds of Addons
	/// </summary>
	public class AuthAddonMgr : WCellAddonMgr<AuthAddonMgr>
	{
		static readonly Logger log = LogManager.GetCurrentClassLogger();

		public static string AddonDir = "AuthServerAddons";

		//[Variable("IgnoredAddonFiles")]
		public static string IgnoredAddonFiles = "";
		
		[Initialization(InitializationPass.First, "Initialize Addons")]
		public static void Initialize(InitMgr mgr)
		{
			LoadAddons(AddonDir, IgnoredAddonFiles);

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
			mgr.PerformInitialization();
		}

		protected static void InitAddon(WCellAddonContext context, InitMgr mgr)
		{
			var addon = context.Addon;

			// add all initialization steps of the Assembly
			mgr.AddStepsOfAsm(context.Assembly);

			// register all Commands of the Assembly
			AuthCommandHandler.Instance.AddCmdsOfAsm(context.Assembly);

			if (addon != null)
			{
				// init config
				if (addon is WCellAddonBase)
				{
					((WCellAddonBase)addon).InitConfig(context);
				}
			}
		}
	}
}