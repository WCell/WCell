/*************************************************************************
 *
 *   file		: SendGroupResult.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-04-04 21:43:29 +0200 (fr, 04 apr 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 224 $
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
using WCell.RealmServer.Groups;
using WCell.RealmServer.Entities;
using WCell.RealmServer.World;
using WCell.Core;

namespace WCell.RealmServerConsole.Commands
{
    /// <summary>
    /// Command that displays all of the currently available commands
    /// </summary>
    public class SendGroupResult
    {
        private static Logger s_log = LogManager.GetLogger("Application");

        /// <summary>
        /// Sets the maximum ammount of characters displayed in the Who List
        /// </summary>
        /// <param name="arguments">the arguments of the command</param>
        [ConsoleCommand("groupresult")]
        public static void Execute(string[] arguments)
        {
            if (arguments.Length <= 2)
            {
                s_log.Error("Wrong parameters!: resultType, resultCode, resultName, destCharName");
                return;
            }

            int resultType;
            int resultCode;
            string resultName = string.Empty;
            string destCharName = string.Empty;

            try
            {
                resultType = Convert.ToInt32(arguments[0]);
                resultCode = Convert.ToInt32(arguments[1]);
                destCharName = arguments[2];

                if (arguments.Length == 4)
                    resultName = arguments[3];
            }
            catch (Exception)
            {
                s_log.Error("Wrong parameter types!");
                return;
            }

            Character character = WorldMgr.GetCharacterByName(destCharName, false);

            if (character == null)
            {
                s_log.Error("Unable to find given character!");
                return;
            }

            Group.SendResult(character.Client, (GroupResult)resultCode, (uint)resultType, resultName);
        }
    }
}