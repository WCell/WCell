/*************************************************************************
 *
 *   file		: Command.cs
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

namespace WCell.RealmServer.Privileges
{
	/// <summary>
	/// Defines a script-command registered in the command system.
	/// </summary>
    public class Command
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public Command()
		{
			CommandName = "";
			ModuleName = "";
			FunctionName = "";
		}

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="commandName">the name of the command</param>
		/// <param name="moduleName">the name of the module</param>
		/// <param name="functionName">the name of the function</param>
		public Command(string commandName, string moduleName, string functionName)
		{
			CommandName = commandName;
			ModuleName = moduleName;
			FunctionName = functionName;
		}

		#region Properties

		/// <summary>
		/// The name of the command.
		/// </summary>
		/// <remarks>This is what you'd call it from in-game. i.e. .debuginfo would have
		/// a name of 'debuginfo'.</remarks>
		public string CommandName
		{
			get;
			set;
		}

		/// <summary>
		/// The name of the command module to look in.
		/// </summary>
		/// <remarks>Command scripts are required to define themselves as modules,
		/// which allows simplification in the calling of the script that represents
		/// the command in question. </remarks>
		public string ModuleName
		{
			get;
			set;
		}

		/// <summary>
		/// The name of the function in the command module to call.
		/// </summary>
		public string FunctionName
		{
			get;
			set;
		}

		#endregion

		public void Process()
		{
		}
	}
}
