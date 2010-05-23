using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using WCell.Util.Variables;
using WCell.Core.Variables;
using NLog;
using System.Reflection;
using System.IO;

namespace WCell.Core.Addons
{
	public abstract class WCellAddonBase : IWCellAddon
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		protected VariableConfiguration<WCellVariableDefinition> config;

		/// <summary>
		/// The <see cref="WCellAddonContext"/> that was used to load this Addon.
		/// </summary>
		public static WCellAddonContext Context
		{
			get;
			private set;
		}

		protected WCellAddonBase()
		{
			config = new VariableConfiguration<WCellVariableDefinition>(OnError);

			var asm = GetType().Assembly;
			if (asm.Location == null)
			{
				OnError("Addon Assembly does not have a location. - Could not set Filename Configuration of: " +
						this.GetDefaultDescription());
			}
		}

		static void OnError(string msg)
		{
			log.Warn("<Config>" + msg);
		}

		public virtual bool UseConfig
		{
			get { return false; }
		}

		public abstract string Name { get; }

		public abstract string ShortName { get; }

		public abstract string Author { get; }

		public abstract string Website { get; }

		public abstract void TearDown();

		public IConfiguration Config
		{
			get { return config; }
		}

		public abstract string GetLocalizedName(CultureInfo culture);

        public override string ToString()
        {
            return Name + " (" + ShortName + ") by " + Author;
        }

		public void InitConfig(WCellAddonContext context)
		{
			Context = context;
			config.FilePath = Path.Combine(context.File.DirectoryName, GetType().Name + "Config.xml");
			if (UseConfig)
			{
				config.AutoSave = true;
				config.AddVariablesOfAsm<VariableAttribute>(GetType().Assembly);
				config.Load();
				try
				{
					config.Save();
				}
				catch (Exception e)
				{
					throw new Exception("Unable to save " + config.GetType().Name + " of addon: " + this, e);
				}
			}
		}
	}
}