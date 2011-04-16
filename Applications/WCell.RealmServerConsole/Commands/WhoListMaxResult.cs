/*************************************************************************
 *
 *   file		: WhoListMaxResult.cs
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

using System;
using NLog;
using WCell.RealmServer.Interaction;

namespace WCell.RealmServerConsole.Commands
{
    /// <summary>
    /// Command that displays all of the currently available commands
    /// </summary>
    public class WhoListMaxResult
    {
        private static Logger s_log = LogManager.GetLogger("Application");

        /// <summary>
        /// Sets the maximum ammount of characters displayed in the Who List
        /// </summary>
        /// <param name="arguments">the arguments of the command</param>
        [ConsoleCommand("wholistmaxresult")]
        public static void Execute(string[] arguments)
        {
            if (arguments.Length != 1)
            {
                s_log.Error("You must supply the ammount of characters to display!");
                return;
            }

            uint maxResult;

            try
            {
                maxResult = Convert.ToUInt32(arguments[0]);
            }
            catch (Exception)
            {
                s_log.Error("Your ammount entered is incorrect. It should be a positive number!");
                return;
            }

            //Set the Who List max display characters to the specified one.
            WhoList.SetMaxResultCount(maxResult);
        }
    }
}