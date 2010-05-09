/*************************************************************************
 *
 *   file		: HelpCommand.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-03-10 01:52:03 +0100 (ma, 10 mar 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 190 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using NLog;

namespace WCell.RealmServerConsole.Commands
{
    /// <summary>
    /// Command that displays all of the currently available commands
    /// </summary>
    public class HelpCommand
    {
        private static Logger s_log = LogManager.GetLogger("Application");

        /// <summary>
        /// Displays all available commands
        /// </summary>
        /// <param name="arguments">the arguments of the command</param>
        [ConsoleCommand("help")]
        public static void Execute(string[] arguments)
        {
            s_log.Info("The following commands are available: {0}", CommandConsole.GetCommandList());
        }
    }
}