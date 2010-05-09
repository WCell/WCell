using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using NLog;
using WCell.Util;
using WCell.Util.Variables;

namespace WCell.Core.Addons
{
	public class WCellAddonMgr<T> : WCellAddonMgr
		where T : WCellAddonMgr, new()
	{
		public static readonly T Instance = new T();
	}

	/// <summary>
	/// Static helper and container class
	/// </summary>
	public class WCellAddonMgr
	{
		/// <summary>
		/// All contexts of all Addons and utility libraries.
		/// </summary>
		public static readonly IList<WCellAddonContext> Contexts = new List<WCellAddonContext>();

		[NotVariable]
		public static Assembly[] CoreLibs;

		/// <summary>
		/// All existing AddonContexts by name of the addon's type
		/// </summary>
		public static readonly IDictionary<string, WCellAddonContext> ContextsByTypeName = new Dictionary<string, WCellAddonContext>(StringComparer.InvariantCultureIgnoreCase);

		/// <summary>
		/// All existing AddonContexts by ShortName (case-insensitive)
		/// </summary>
		public static readonly Dictionary<string, WCellAddonContext> ContextsByName = new Dictionary<string, WCellAddonContext>(StringComparer.InvariantCultureIgnoreCase);

		/// <summary>
		/// All existing AddonContexts by Filename (case-insensitive)
		/// </summary>
		public static readonly Dictionary<string, WCellAddonContext> ContextsByFile = new Dictionary<string, WCellAddonContext>(StringComparer.InvariantCultureIgnoreCase);

		public static IWCellAddon GetAddon(string shortName)
		{
			var context = GetContextByName(shortName);
			if (context != null)
			{
				return context.Addon;
			}
			return null;
		}

		public static WCellAddonContext GetContextByName(string shortName)
		{
			WCellAddonContext addon;
			ContextsByName.TryGetValue(shortName, out addon);
			return addon;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="typeName">The full typename of the WCellAddon</param>
		/// <returns></returns>
		public static WCellAddonContext GetContextByTypeName(string typeName)
		{
			WCellAddonContext context;
			ContextsByTypeName.TryGetValue(typeName, out context);
			return context;
		}

		public static WCellAddonContext GetContext(Type addonType)
		{
			return GetContextByTypeName(addonType.FullName);
		}

		public static WCellAddonContext GetContext<A>()
			where A : IWCellAddon
		{
			return GetContextByTypeName(typeof(A).FullName);
		}

		#region Loading Addons
		/// <summary>
		/// Automatically loads all Addons from the given folder, ignoring any sub-folders or files
		/// that are in ignoreString, seperated by semicolon.
		/// </summary>
		/// <param name="folderName">The dir to look in for the Addon-Assemblies.</param>
		/// <param name="ignoreString">eg.: MyDllFile; My2ndFileIsJustALib; AnotherAddonFile</param>
		public static void LoadAddons(string folderName, string ignoreString)
		{
			var folder = new DirectoryInfo(folderName);
			var ignores = ignoreString.Split(new[] { ';' }).TransformArray(s => s.ToLower().Trim().Replace(".dll", ""));

			LoadAddons(folder, ignores);
		}

		public static void LoadAddons(DirectoryInfo folder, string[] ignoredNames)
		{
			if (CoreLibs == null)
			{
				CoreLibs = AppDomain.CurrentDomain.GetAssemblies();
			}

			if (folder.Exists)
			{
				RecurseLoadAddons(folder, ignoredNames);

				foreach (var context in Contexts)
				{
					context.InitAddon();
				}
			}
		}

		static void RecurseLoadAddons(DirectoryInfo folder, string[] ignoredNames)
		{
			foreach (var file in folder.GetFileSystemInfos())
			{
				if (ignoredNames.Contains(file.Name.ToLower().Replace(".dll", "")))
				{
					continue;
				}

				if (file is DirectoryInfo)
				{
					LoadAddons((DirectoryInfo)file, ignoredNames);
				}
				else if (file.Name.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
				{
					var ok = true;
					foreach (var asm in CoreLibs)
					{
						if (asm.FullName.ContainsIgnoreCase(file.Name.Replace(".dll", "")))
						{
							LogManager.GetCurrentClassLogger().Warn(" \"" + asm.FullName + "\" is a core Assembly - When compiling custom Addons, please make sure to set 'Copy Local' of all core-references to 'False'!");
							ok = false;
							break;
						}
					}
					if (ok)
					{
						LoadAddon((FileInfo) file);
					}
				}
			}
		}

		public static WCellAddonContext LoadAddon(FileInfo file)
		{
			WCellAddonContext oldContext;
			var path = file.FullName;
			if (ContextsByFile.TryGetValue(path, out oldContext))
			{
				if (!Unload(oldContext))
				{
					return null;
				}
			}

			// copy into memory before loading, so the file can be overridden
			// debugger won't attach in that case
			//var bytes = File.ReadAllBytes(path);
			//var asm = Assembly.Load(bytes);
			var asm = Assembly.LoadFrom(path);
			var context = new WCellAddonContext(file, asm);

			Contexts.Add(context);
			ContextsByFile.Add(context.File.FullName, context);
			return context;
		}

		public WCellAddonContext TryLoadAddon(string libName)
		{
			var context = GetContextByName(libName);
			if (context == null)
			{
				if (!libName.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
				{
					libName = libName + ".dll";
				}
				return LoadAndInitAddon(libName);
			}

			return LoadAndInitAddon(context.File);
		}

		public virtual WCellAddonContext LoadAndInitAddon(string libName)
		{
			var file = new FileInfo(libName);
			if (!file.Exists)
			{
				return null;
			}
			return LoadAndInitAddon(file);
		}

		/// <summary>
		/// Loads an Addon from the given file.
		/// Returns null if file does not exist.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public WCellAddonContext LoadAndInitAddon(FileInfo file)
		{
			var context = LoadAddon(file);
			context.InitAddon();
			return context;
		}
		#endregion

		#region Init Addon
		internal static void RegisterAddon(WCellAddonContext context)
		{
			var fullName = context.Addon.GetType().FullName;
			if (context.ShortName.Length == 0)
			{
				LogManager.GetCurrentClassLogger().Warn("Addon of Type \"{0}\" did not specify a ShortName.", context.Addon.GetType().FullName);
			}
			else
			{
				if (context.ShortName.ContainsIgnoreCase("addon"))
				{
					LogManager.GetCurrentClassLogger().Warn("The Addon ShortName \"{0}\" contains the word \"Addon\" - The name should be short and not contain unnecessary information.", context.ShortName);
				}
				if (ContextsByName.ContainsKey(context.ShortName))
				{
					throw new ArgumentException(string.Format("Found more than one addon with ShortName \"{0}\": {1} and {2}",
															  context.ShortName,
															  GetAddon(context.ShortName),
															  context.Addon));
				}
				ContextsByName.Add(context.ShortName, context);

				if (fullName.Equals(context.ShortName, StringComparison.InvariantCultureIgnoreCase))
				{
					return;
				}
			}

			if (ContextsByTypeName.ContainsKey(fullName))
			{
				throw new InvalidProgramException(
					"Tried to register two Addons with the same TypeName: " + fullName);
			}
			ContextsByTypeName.Add(fullName, context);
		}
		#endregion

		#region Unload
		public static bool Unload(WCellAddonContext context)
		{
			var addon = context.Addon;
			if (addon == null)
			{
				// only Addon-libs are unloadable
				return false;
			}

			var log = LogManager.GetCurrentClassLogger();
			log.Info("Unloading Addon: " + context + " ...");
			TearDown(addon);
			Contexts.Remove(context);
			ContextsByFile.Remove(context.File.FullName);
			ContextsByName.Remove(context.ShortName);
			ContextsByTypeName.Remove(addon.GetType().FullName);
			log.Info("Done. - Unloaded Addon: " + context);
			return true;
		}

		private static void TearDown(IWCellAddon addon)
		{
			addon.TearDown();
			// TODO: Disconnect articulation points
		}
		#endregion
	}

	public static class WCellAddonUtil
	{
		/// <summary>
		/// 
		/// </summary>
		public static string GetDefaultDescription(this IWCellAddon addon)
		{
			return String.Format("{0} v{1} by {2} ({3})", addon.Name, addon.GetType().Assembly.GetName().Version, addon.Author, addon.Website);
		}
	}
}
