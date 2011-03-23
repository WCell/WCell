/*************************************************************************
 *
 *   file		: Command.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-03-30 18:11:48 +0800 (Sun, 30 Mar 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 207 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using WCell.Util.Logging;
using System.Reflection;
using WCell.Util;

namespace WCell.Util.Commands
{
	/// <summary>
	/// Basic Command Class, Inherit your Commands from here. Automatically creates one instance
	/// per IrcClient when the Class is loaded, using the default constructor.
	/// </summary>
	public abstract class Command<C> : BaseCommand<C>
		where C : ICmdArgs
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		public delegate void CommandCallback(CmdTrigger<C> trigger);
		public event CommandCallback Executed;
		internal void ExecutedNotify(CmdTrigger<C> trigger)
		{
			if (Executed != null)
			{
				Executed(trigger);
			}
		}

		/*private static Command CreateInstance() {
			System.Reflection.ConstructorInfo ctor = typeof(Command).GetConstructor(Type.EmptyTypes);
			return (Command)ctor.Invoke(new object[0]);
		}*/

		/// <summary>
		/// In the Constructor you deliver the alias names. Calling this ctor automatically sets
		/// the Instance to the newly invoked instance.
		/// </summary>
		protected Command()
		{
		}

		/// <summary>
		/// Determines whether the given command may ever be used in this Context, depending
		/// on the trigger's parameters that the triggerer cannot currently change and 
		/// are not already checked globally by the TriggerValidator.
		/// </summary>
		public virtual bool MayTrigger(CmdTrigger<C> trigger, BaseCommand<C> cmd, bool silent)
		{
			return true;
        }

        public override void Process(CmdTrigger<C> trigger)
        {
            if (m_subCommands != null)
            {
                TriggerSubCommand(trigger);
            }
        }
	}
}