using System;
using System.Linq;
using System.Reflection;
using System.IO;

using WCell.Util;

namespace WCell.Core.Addons
{
	/// <summary>
	/// Contains all information related to an Addon.
	/// </summary>
	public class WCellAddonContext
	{
		protected FileInfo m_file;
		protected Assembly m_assembly;
		protected IWCellAddon m_addon;
		protected WCellAddonAttribute m_attr;

		public WCellAddonContext(FileInfo file, Assembly asm)
		{
			m_file = file;
			m_assembly = asm;
		}

		public string ShortName
		{
			get { return m_addon != null ? m_addon.ShortName : ""; }
		}

		public FileInfo File
		{
			get { return m_file; }
		}

		/// <summary>
		/// The containing assembly (might be null if descriptor has not been loaded yet)
		/// </summary>
		public Assembly Assembly
		{
			get { return m_assembly; }
		}

		/// <summary>
		/// The created Addon (might be null if descriptor has not been loaded yet or if this a library which does not get initialized)
		/// </summary>
		public IWCellAddon Addon
		{
			get { return m_addon; }
		}

		public void InitAddon()
		{
			if (m_addon == null)
			{
				foreach (var type in m_assembly.GetTypes())
				{
					var interfaces = type.GetInterfaces();
					if (interfaces.Contains(typeof(IWCellAddon)))
					{
						var addonAttributes = type.GetCustomAttributes<WCellAddonAttribute>();
						m_attr = addonAttributes.Length > 0 ? addonAttributes[0] : null;
						m_addon = (IWCellAddon)Activator.CreateInstance(type);

						if (m_addon != null)
						{
							WCellAddonMgr.RegisterAddon(this);
						}
						break;
					}
				}
			}
		}

		public override string ToString()
		{
			if (m_addon == null)
			{
				return "AddonContext for " + m_assembly.FullName;
			}
			return "AddonContext for " + m_addon.GetDefaultDescription();
		}
	}
}