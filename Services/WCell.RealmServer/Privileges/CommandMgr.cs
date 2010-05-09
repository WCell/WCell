/*************************************************************************
 *
 *   file		: CommandMgr.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-01-31 12:35:36 +0100 (to, 31 jan 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 87 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core;
using WCell.RealmServer.Localization;
using WCell.Core.Initialization;
using WCell.RealmServer.Privileges;
using System.IO;

namespace WCell.RealmServer.Chat
{
	/// <summary>
	/// Handles the execution of in-game commands.
	/// </summary>
	public class CommandMgr : Manager<CommandMgr>
	{
		#region Fields

		private const string CMDMGR_SCRIPT_NAME = "commands.lua";
		private IScriptEngine m_scriptEnv;

		#endregion

		/// <summary>
		/// Default constructor.
		/// </summary>
		private CommandMgr()
		{
		}

		#region Methods

		protected override bool internalStart()
		{
			if (m_scriptEnv != null)
			{
                m_scriptEnv.Dispose();
				m_scriptEnv = null;
			}

			if (!ScriptEngineFactory.DoesEngineExist("Lua"))
			{
				Logger.Warn(Resources.CommandMgrCEFailed);
				return false;
			}

            m_scriptEnv = ScriptEngineFactory.GetEngine("Lua");

			if(!InitializeEnvironment())
			{
				return false;
			}

			string scriptFolder = GetScriptFolder();

			if (!m_scriptEnv.LoadResource(scriptFolder, LoadStrategy.Directory, false))
			{
				Logger.Warn(Resources.CommandMgrLoadFailed);
				return false;
			}

			Logger.Info(Resources.CommandMgrStarted);
			return true;
		}

		protected override bool internalStop()
		{
			if (m_scriptEnv != null)
			{
                m_scriptEnv.Dispose();
                m_scriptEnv = null;
			}

			Logger.Info(Resources.CommandMgrStopped);
			return true;
		}

		protected override bool internalRestart(bool forced)
		{
			if (m_scriptEnv != null)
			{
                m_scriptEnv.Dispose();
				m_scriptEnv = null;
			}

            if (!ScriptEngineFactory.DoesEngineExist("Lua"))
            {
                Logger.Warn(Resources.CommandMgrCEFailed);
                return false;
            }

            m_scriptEnv = ScriptEngineFactory.GetEngine("Lua");

			if(!InitializeEnvironment())
			{
				return false;
			}

			string scriptFolder = GetScriptFolder();

            if (!m_scriptEnv.LoadResource(scriptFolder, LoadStrategy.Directory, false))
			{
				Logger.Warn(Resources.CommandMgrLoadFailed);
				return false;
			}

			Logger.Info(Resources.CommandMgrStarted);
			return true;
		}

		/// <summary>
		/// Gets the full path to the command scripts folder.
		/// </summary>
		/// <returns>the full path to the folder</returns>
		private static string GetScriptFolder()
		{
			string scriptFolder = WCellDef.GetFullPath(WCellDef.SCRIPT_DIR, WCellDef.CMD_SCRIPT_DIR);

			DirectoryInfo scriptFolderInfo = new DirectoryInfo(scriptFolder);

			if (!scriptFolderInfo.Exists)
			{
				scriptFolderInfo.Create();
			}

			return scriptFolder;
		}

		/// <summary>
		/// Initializes the script environment by executing a command-specific file
		/// that contains special functions needed by scripts.
		/// </summary>
		/// <returns>true if the script was excuted; false otherwise</returns>
		private bool InitializeEnvironment()
		{
			// call our set-up script that loads our auxiliary command functions
			if (!m_scriptEnv.LoadResource(WCellDef.GetFullPath(WCellDef.SCRIPT_DIR, CMDMGR_SCRIPT_NAME), LoadStrategy.File, true))
			{
				Logger.Error(string.Format(Resources.ScriptLoadFailed, CMDMGR_SCRIPT_NAME));

				return false;
			}

			// set the privilege manager so commands can register themselves
			m_scriptEnv.ExecuteFunction("global", "setPrivilegeManager", PrivilegeMgr.Instance);

			return true;
		}

		/// <summary>
		/// Checks if a string is a command message. (i.e. .reboot "We're rebooting because..")
		/// </summary>
		/// <param name="message">the message string to check</param>
		/// <returns>true if the message is a command message; false otherwise</returns>
		public static bool IsCommandMessage(string message)
		{
			if (message.Length < 1 || message[0] != '.')
			{
				return false;
			}

			if (!PrivilegeMgr.Instance.DoesCommandExist(GetCommandName(message)))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Gets the actual command from a raw command message.
		/// </summary>
		/// <param name="commandMessage">the raw command message</param>
		/// <returns>the command name by itself</returns>
		private static string GetCommandName(string commandMessage)
		{
			return commandMessage.Substring(1, (commandMessage.IndexOf(" ") != -1 ? commandMessage.IndexOf(" ") - 1 : commandMessage.Length - 1));
		}

		/// <summary>
		/// Checks if the specified command module exists.
		/// </summary>
		/// <param name="moduleName">the name of the command module</param>
		/// <returns>true if the command module exists; false otherwise</returns>
		public bool DoesCommandModuleExist(string moduleName)
		{
			return m_scriptEnv.ModuleNames.Contains(moduleName);
		}

		/// <summary>
		/// Parses a command message into seperated tokens.
		/// </summary>
		/// <param name="commandMessage">the raw command message</param>
		/// <returns>a list of string tokens from the command message</returns>
		private static List<string> ParseCommand(string commandMessage)
		{
			StringBuilder tempString = new StringBuilder();
			string[] inputTokens;

			List<string> returnParams = new List<string>();

			int state = 0;

			inputTokens = commandMessage.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			foreach (string token in inputTokens)
			{
				if (token[0] != '"' && token[token.Length - 1] != '"' && state != 1)
				{
					returnParams.Add(token.Trim());
				}
				else
				{
					if (state == 0)
					{
						tempString.Append(token.Substring(1, token.Length - 1));

						state = 1;
					}
					else
					{
						if (token[token.Length - 1] == '"')
						{
							state = 0;

							tempString.Append(' ');
							tempString.Append(token.Substring(0, token.Length - 1));

							returnParams.Add(tempString.ToString());

							tempString.Remove(0, tempString.Length);
						}
						else
						{
							tempString.Append(' ');
							tempString.Append(token);
						}
					}
				}
			}

			return returnParams;
		}

		/// <summary>
		/// Handles a command message, parsing it and executing the neccessary command script.
		/// </summary>
		/// <param name="commandMessage">the command message</param>
		public void HandleCommand(RealmClient client, string commandMessage)
		{
			List<string> commandArgs = ParseCommand(commandMessage);

			Command cmd = PrivilegeMgr.Instance.GetCommand(GetCommandName(commandArgs[0]));

			if(cmd != null)
			{
				m_scriptEnv.ExecuteFunction(cmd.ModuleName, cmd.FunctionName, client, commandArgs.ToArray());
			}
		}

		#endregion

		#region Initialization/teardown

		[Initialization(InitializationPass.Fifth, Name = "Command manager")]
		public static bool Initialize()
		{
			return Instance.Start();
		}

		#endregion
	}
}
