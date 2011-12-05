/*************************************************************************
 *
 *   file		: RealmServerConfiguration.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-04-23 15:13:50 +0200 (fr, 23 apr 2010) $
 
 *   revision		: $Rev: 1282 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml.Serialization;
using NLog;
using WCell.Constants;
using WCell.Constants.Login;
using WCell.Constants.Realm;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Res;
using WCell.Util;
using WCell.Util.NLog;
using WCell.Util.Variables;
using RealmServ = WCell.RealmServer.RealmServer;

namespace WCell.RealmServer
{
	/// <summary>
	/// Configuration for the realm server
	/// TODO: Allow to re-load config during runtime (using World-sync)
	/// </summary>
	[XmlRoot("WCellConfig")]
	public class RealmServerConfiguration : WCellConfig<RealmServerConfiguration>
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		private const string ConfigFilename = "RealmServerConfig.xml";

		private static RealmServerConfiguration s_instance;
		public static RealmServerConfiguration Instance
		{
			get { return s_instance; }
		}

		public override string FilePath
		{
			get { return GetFullPath(ConfigFilename); }
			set
			{// cannot modify Filename
				throw new InvalidOperationException("Cannot modify Filename");
			}
		}

		public static readonly string BinaryRoot = "../";

		private static string contentDirName = BinaryRoot + "Content/";

		public readonly static HashSet<string> BadWords = new HashSet<string>();

		[NotVariable]
		public override bool AutoSave
		{
			get;
			set;
		}

		private static bool m_Loaded;

		public static bool Loaded
		{
			get { return m_Loaded; }
			protected set
			{
				m_Loaded = value;
				RealmServ.Instance.SetTitle("{0} - {1} ...", RealmServ.Instance, RealmLocalizer.Instance.Translate(DefaultLocale, RealmLangKey.Initializing));
			}
		}

		public static string LangDirName = "Lang";

		public static string LangDir
		{
			get { return GetContentPath(LangDirName) + "/"; }
		}

		private static ClientLocale defaultLocale = ClientLocale.English;

		public static ClientLocale DefaultLocale
		{
			get { return defaultLocale; }
			set
			{
				defaultLocale = value;
				WCellConstants.DefaultLocale = value;
			}
		}

		[Initialization(InitializationPass.Config, "Initialize Config")]
		public static bool Initialize()
		{
			if (!Loaded)
			{
				Loaded = true;
				BadWordString = "";

				s_instance.AddVariablesOfAsm<VariableAttribute>(typeof(RealmServerConfiguration).Assembly);
				//s_instance = Load(RealmServerConfiguration.ConfigPath);
				try
				{
					if (!s_instance.Load())
					{
						s_instance.Save(true, false);
						log.Warn("Config-file \"{0}\" not found - Created new file.", Instance.FilePath);
						log.Warn("Please take a little time to configure your server and then restart the Application.");
						log.Warn("See http://wiki.wcell.org/index.php/Configuration for more information.");
						return false;
					}
				}
				catch (Exception e)
				{
					LogUtil.ErrorException(e, "Unable to load Configuration.");
					log.Error("Please correct the invalid values in your configuration file and restart the Applicaton.");
					return false;
				}
			}

			Loaded = true;
			return true;
		}

		[Initialization(InitializationPass.Fifth)]
		public static void InitializeRoles()
		{
			foreach (var def in s_instance.Definitions.Values)
			{
				//def.Initialize();
			}
		}

		[Initialization(InitializationPass.Last)]
		public static void PerformAutoSave()
		{
			if (s_instance.AutoSave)
			{
				s_instance.Save(true, true);
			}
		}

		internal static void OnError(string msg)
		{
			log.Warn("<Config>" + msg);
		}

		internal static void OnError(string msg, params object[] args)
		{
			log.Warn("<Config>" + String.Format(msg, args));
		}

		private readonly AppConfig m_cfg;

		protected RealmServerConfiguration()
			: base(OnError)
		{
			RootNodeName = "WCellConfig";
			s_instance = this;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="executablePath">The path of the executable whose App-config to load</param>
		public RealmServerConfiguration(string executablePath)
			: this()
		{
			m_cfg = new AppConfig(executablePath);
		}

		/// <summary>
		/// The host address to listen on for game connections.
		/// </summary>
		public static string Host = IPAddress.Loopback.ToString();

		/// <summary>
		/// The address to be sent to the players to connect to.
		/// </summary>
		public static string ExternalAddress = Host;

		/// <summary>
		/// The port to listen on for game connections.
		/// </summary>
		public static int Port = 8085;

		private static string realmName = "Change the RealmName in the Config!";

		/// <summary>
		/// The name of this server
		/// </summary>
		public static string RealmName
		{
			get { return realmName; }
			set
			{
				realmName = value;
				if (RealmServ.Instance.IsRunning)
				{
					RealmServ.Instance.SetTitle(RealmServ.Instance.ToString());
					RealmServ.Instance.UpdateRealm();
				}
			}
		}

		/// <summary>
		/// The location of the configuration dir
		/// </summary>
		//public static string ConfigDir = "cfg";

		private static RealmServerType serverType = RealmServerType.PVP;

		/// <summary>
		/// Type of server
		/// </summary>
		public static RealmServerType ServerType
		{
			get { return serverType; }
			set
			{
				if (serverType != value)
				{
					serverType = value;
					if (RealmServ.Instance.IsRunning)
					{
						RealmServ.Instance.UpdateRealm();
					}
				}
			}
		}

		private static RealmStatus status = RealmStatus.Open;

		/// <summary>
		/// The status can be Open or Locked (a Locked Realm can only be accessed by Staff members)
		/// </summary>
		public static RealmStatus Status
		{
			get { return status; }
			set
			{
				if (status != value)
				{
					var oldStatus = status;
					status = value;
					if (RealmServ.Instance.IsRunning)
					{
						RealmServ.Instance.OnStatusChange(oldStatus);
					}
				}
			}
		}

		private static RealmCategory category = RealmCategory.Development;

		/// <summary>
		/// The Category of this RealmServer
		/// </summary>
		public static RealmCategory Category
		{
			get { return category; }
			set
			{
				category = value;
				if (RealmServ.Instance.IsRunning)
				{
					RealmServ.Instance.UpdateRealm();
				}
			}
		}

		private static RealmFlags flags = RealmFlags.Recommended;

		/// <summary>
		/// The flags of this RealmServer
		/// </summary>
		public static RealmFlags Flags
		{
			get { return flags; }
			set
			{
				flags = value;
				if (RealmServ.Instance.IsRunning)
				{
					RealmServ.Instance.UpdateRealm();
				}
			}
		}

		[Variable("BadWords")]
		/// <summary>
		/// 
		/// </summary>
		public static string BadWordString
		{
			get
			{
				return BadWords.ToString("; ");
			}
			set
			{
				BadWords.Clear();
				BadWords.AddRange(value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
			}
		}

		private static bool registerExternalAddress;

		/// <summary>
		/// Whether or not to try and register the outside-most IP this computer
		/// goes through as a realm on the authentication server.
		/// </summary>
		public static bool RegisterExternalAddress
		{
			get { return registerExternalAddress; }
			set
			{
				registerExternalAddress = value;
				if (RealmServ.Instance.IsRunning)
				{
					RealmServ.Instance.UpdateRealm();
				}
			}
		}

		/// <summary>
		/// The type of database we're connecting to. (e.g. MySQL, mssql2005, Oracle, etc)
		/// </summary>
		public static string DBType = "mysql5";

		private static string dbConnectionString = @"Server=127.0.0.1;Port=3306;Database=WCellRealmServer;CharSet=utf8;Uid=root;Pwd=;";

		/// <summary>
		/// The connection string for the authentication server database.
		/// </summary>
		[Variable(IsFileOnly = true)]
		public static string DBConnectionString
		{
			get { return dbConnectionString; }
			set { dbConnectionString = value; }
		}

		/// <summary>
		/// The address of the auth server.
		/// </summary>
		public static string AuthenticationServerAddress = @"net.tcp://127.0.0.1:7470";

		/// <summary>
		/// The username for the auth server connection.
		/// </summary>
		//public static string AuthenticationServerUsername = @"changeme";

		/// <summary>
		/// The password of the auth server connection.
		/// </summary>
		//public static string AuthenticationServerPassword = @"changeme";

		/// <summary>
		/// The amount of players this server can hold/allows for.
		/// </summary>
		public static int MaxClientCount = 3000;

		/// <summary>
		/// The highest supported version
		/// </summary>
		public static ClientId ClientId = ClientId.Cataclysm;

		/// <summary>
		/// whether or not to use blizz like character name restrictions (blizzlike = 'Lama', not blizzlike = 'lAmA').
		/// </summary>
		public static bool CapitalizeCharacterNames = true;

		/// <summary>
		/// The level to use for Zlib compression. (1 = fastest, 9 = best compression)
		/// </summary>
		public static int CompressionLevel = 7;

		private static float ingameMinutePerSecond = 0.0166666666666f;

		/// <summary>
		/// The speed of time in ingame minute per real-time second.
		/// If set to one, one minute ingame will pass by in one second.
		/// Default: 0.016666666666666666f (1/60)
		/// </summary>
		[Variable("TimeSpeed")]
		public static float IngameMinutesPerSecond
		{
			get { return ingameMinutePerSecond; }
			set
			{
				if (value > 60)
				{
					// client can't display anything faster than that
					value = 60;
				}

				if (RealmServ.Instance.IsRunning)
				{
					// make sure to reset before changing the speed
					RealmServ.ResetTimeStart();
					ingameMinutePerSecond = value;
					foreach (var chr in World.GetAllCharacters())
					{
						CharacterHandler.SendTimeSpeed(chr);
					}
				}
				else
				{
					ingameMinutePerSecond = value;
				}
			}
		}

		#region Paths
		/// <summary>
		/// The directory in which to look for XML and DBC files
		/// </summary>
		[Variable("ContentDir")]
		public static string ContentDirName
		{
			get { return contentDirName; }
			set { contentDirName = value; }
		}

		public static string ContentDir
		{
			get { return GetFullPath(contentDirName); }
		}

		public static string DBCFolderName = "dbc" + WCellInfo.RequiredVersion.BasicString;

		/// <summary>
		/// The directory that holds the DBC files
		/// </summary>
		public string DBCFolder
		{
			get
			{
				var dir = Path.Combine(ContentDir, DBCFolderName) + "/";
				if (!Directory.Exists(dir))
				{
					var msg = String.Format(WCell_RealmServer.NotFound, "DBC Directory", new DirectoryInfo(dir).FullName + " (Please export the DBC files of the correct version, using the MPQTool)");
					throw new FileNotFoundException(msg);
				}
				return dir;
			}
		}

		public static string GetDBCFile(string filename)
		{
			if (!filename.EndsWith(".dbc", StringComparison.InvariantCultureIgnoreCase))
			{
				filename += ".dbc";
			}
			var path = Path.Combine(Instance.DBCFolder, filename);

			if (!File.Exists(path))
			{
				var dir = new DirectoryInfo(path);
				var msg = String.Format(WCell_RealmServer.NotFound, "DBC File (" + filename + ")", dir.FullName);
				throw new FileNotFoundException(msg);
			}

			return path;
		}

		public static string GetContentPath(string file)
		{
			if (!Path.IsPathRooted(file))
			{
				return Path.Combine(GetFullPath(ContentDir), file);
			}
			return file;
		}

		public static string GetFullPath(string file)
		{
			if (!Path.IsPathRooted(file))
			{
				return Path.Combine(s_instance.m_cfg.ExecutableFile.Directory.FullName, file);
			}
			return file;
		}
		#endregion

		public static string CacheDirName = "Cache";

		/// <summary>
		/// The directory that holds the DBC files
		/// </summary>
		public string CacheDir
		{
			get
			{
				var path = Path.Combine(ContentDir, CacheDirName);
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
				return path;
			}
		}

		public string GetCacheFile(string filename)
		{
			return Path.GetFullPath(Path.Combine(CacheDir, filename));
		}

		/// <summary>
		/// The highest level, a Character can reach.
		/// Also see: 
		/// TODO: Wrap access to *all* level-related arrays
		/// </summary>
		public static int MaxCharacterLevel = 80;
	}
}