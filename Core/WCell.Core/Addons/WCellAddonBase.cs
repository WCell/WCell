using System;
using System.Globalization;
using WCell.Util.Variables;
using WCell.Core.Variables;
using WCell.Util.Logging;
using System.IO;

namespace WCell.Core.Addons
{
	public abstract class WCellAddonBase<A> : WCellAddonBase
		where A : WCellAddonBase
	{
		public static A Instance
		{
			get;
			private set;
		}

		protected WCellAddonBase()
		{
			if (Instance != null)
			{
				throw new InvalidOperationException("Tried to create Addon twice: " + this);
			}
			Instance = this as A;
			if (Instance == null)
			{
				throw new InvalidOperationException("Addon has wrong Type parameter - Expected: " + typeof(A).FullName + " - Found: " + GetType());
			}
		}
	}

	public abstract class WCellAddonBase : IWCellAddon
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		protected IConfiguration config;

		/// <summary>
		/// The <see cref="WCellAddonContext"/> that was used to load this Addon.
		/// </summary>
		public WCellAddonContext Context
		{
			get;
			private set;
		}

		protected WCellAddonBase()
		{
		}

		static void OnError(string msg)
		{
			log.Warn("<Config>" + msg);
		}

		public virtual bool UseConfig
		{
			get { return false; }
		}

		public virtual IConfiguration CreateConfig()
		{
			var cfg = new VariableConfiguration<WCellVariableDefinition>(OnError);
			cfg.FilePath = Path.Combine(Context.File.DirectoryName, GetType().Name + "Config.xml");
			cfg.AutoSave = true;
			cfg.AddVariablesOfAsm<VariableAttribute>(GetType().Assembly);
			return cfg;
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

		public void InitAddon(WCellAddonContext context)
		{
			Context = context;
			if (UseConfig)
			{
				config = CreateConfig();
				if (config.Load())
				{
					try
					{
						// TODO: Do not save before startup completed
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
}