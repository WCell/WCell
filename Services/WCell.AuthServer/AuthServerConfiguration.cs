/*************************************************************************
 *
 *   file		: AuthServerConfiguration.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-28 13:29:18 +0100 (to, 28 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1230 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.IO;
using System.Net;
using System.Reflection;
using NLog;
using WCell.Core;
using WCell.Constants;
using WCell.Core.Addons;
using System.Collections.Generic;
using WCell.AuthServer.Privileges;
using WCell.Core.Initialization;
using WCell.Util;
using WCell.Util.Variables;
using WCell.Util.NLog;

namespace WCell.AuthServer
{
	/// <summary>
	/// Configuration for the authentication server.
	/// </summary>
	public class AuthServerConfiguration : WCellConfig<AuthServerConfiguration>
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		private static AuthServerConfiguration s_instance;
		public static AuthServerConfiguration Instance
		{
			get { return s_instance; }
		}

		public override string FilePath
		{
			get { return GetFullPath("AuthServerConfig.xml"); }
		}

		static void OnError(string msg)
		{
			log.Warn("<Config>" + msg);
		}

		[Initialization(InitializationPass.Config, "Initialize Config")]
		public static bool Init()
		{
			s_instance.AddVariablesOfAsm<VariableAttribute>(typeof(AuthServerConfiguration).Assembly);
			try
			{
				if (!s_instance.Load())
				{
					s_instance.Save(false, false);
					log.Warn("Config-file \"{0}\" not found - Created new \"{0}\". Please take a little time to configure your server and then restart the Application.",Instance.FilePath);
					return false;
				}
				else
				{
					if (s_instance.AutoSave)
					{
						s_instance.Save(true, true);
					}
					return true;
				}
			}
			catch (Exception e)
			{
				LogUtil.ErrorException(e, "Unable to load Configuration.");
				log.Error("Please correct the invalid values in your configuration file and restart the Applicaton.");
				return false;
			}
		}

		private string m_executablePath;
		private readonly AppConfig m_cfg;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="executablePath">The path of the executable whose App-config to load</param>
		public AuthServerConfiguration(string executablePath)
			: base(OnError)
		{
			s_instance = this;
			m_executablePath = executablePath;
			m_cfg = new AppConfig(executablePath);
			RealmIPs.Add("127.0.0.1");
		}

		/// <summary>
		/// The host address to listen on for incoming connections.
		/// </summary>
		public static string Host = IPAddress.Loopback.ToString();

		/// <summary>
		/// The port to listen on for incoming login connections.
		/// </summary>
		public static int Port = 3724;

		/// <summary>
		/// Whether or not clients are forced to patch according to the
		/// patch configuration.
		/// </summary>
		public static bool ForcePatches = false;

		/// <summary>
		/// Whether or not to try and negotiate port forwarding thru UPnP.
		/// </summary>
		public static bool UseUPnP = false;

		/// <summary>
		/// Whether or not accounts are auto-created.
		/// </summary>
		public static bool AutocreateAccounts = true;

		/// <summary>
		/// The type of database we're connecting to. (e.g. MySQL, MSSQL 2005, Oracle, etc)
		/// </summary>
		public static string DBType = "mysql5";

		/// <summary>
		/// The connection string for the authentication server database.
		/// </summary>
		public static string DBConnectionString = @"Server=127.0.0.1;Port=3306;Database=WCellAuthServer;CharSet=utf8;Uid=root;Pwd=;";

		/// <summary>
		/// The listening address of the IPC service.
		/// </summary>
		public static string IPCAddress = @"net.tcp://127.0.0.1:7470";

		/// <summary>
		/// IPs of the realms to connect to this server
		/// </summary>
		public static List<string> RealmIPs = new List<string>(3);

		/// <summary>
		/// Removes offline RealmServers from the realm list (true) or only flags it as offline (false = Default)
		/// </summary>
		public static bool RemoveOfflineRealms;

		/// <summary>
		/// The username for the IPC service.
		/// </summary>
		//public static string IPCUsername = @"changeme";

		/// <summary>
		/// The password of the IPC service.
		/// </summary>
		//public static string IPCPassword = @"changeme";

		/// <summary>
		/// The extra config file (contains privileges amongst others)
		/// </summary>
		public static string ConfigDir = "cfg";

		/// <summary>
		/// Whether to keep Accounts cached in Memory (rather than regularly querying Accounts from DB)
		/// </summary>
		public static bool CacheAccounts = true;

		/// <summary>
		/// The default priv Level for new Accounts
		/// </summary>
		public static string DefaultRole = "Player";

		public static string GetFullPath(string file)
		{
			if (!Path.IsPathRooted(file))
			{
				return Path.Combine(s_instance.m_cfg.ExecutableFile.Directory.FullName, file);
			}
			return file;
		}

#if DEBUG
		[NotVariable]
		public static bool AutoStartRealm = false;
#endif
	}
}