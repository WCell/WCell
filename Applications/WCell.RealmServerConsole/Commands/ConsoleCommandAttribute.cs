/*************************************************************************
 *
 *   file		: ConsoleCommandAttribute.cs
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

namespace WCell.RealmServerConsole.Commands
{
    /// <summary>
    /// Identifies a method that represents a console command
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class ConsoleCommandAttribute : Attribute
    {
        private string m_commandString;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="command">the command this method represents</param>
        public ConsoleCommandAttribute(string command)
        {
            m_commandString = command;
        }

        /// <summary>
        /// The command this method represents
        /// </summary>
        public string CommandString
        {
            get { return m_commandString; }
        }
    }
}