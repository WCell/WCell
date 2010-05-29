/*************************************************************************
 *
 *   file		: CmdTrigger.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-04-01 22:58:39 +0800 (Tue, 01 Apr 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 211 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using WCell.Util.Strings;

namespace WCell.Util.Commands
{
	/// <summary>
	/// CmdTriggers trigger Commands. There are different kinds of triggers which are handled differently, 
	/// according to where they came from.
	/// 
	/// </summary>
	/// TODO: Have a reply-stream.
	public abstract class CmdTrigger<C> : ITriggerer
		where C : ICmdArgs
	{
		protected StringStream m_text;

		/// <summary>
		/// The alias that has been used to trigger this command.
		/// </summary>
		public string Alias;
		internal protected BaseCommand<C> cmd;
		internal protected BaseCommand<C> selectedCmd;

		public C Args;

		protected CmdTrigger()
		{
		}

	    protected CmdTrigger(StringStream text, C args)
		{
			m_text = text;
			Args = args;
		}

	    protected CmdTrigger(C args)
	    {
	        Args = args;
	    }

	    protected CmdTrigger(StringStream text, BaseCommand<C> selectedCmd, C args)
        {
            m_text = text;
            this.selectedCmd = selectedCmd;
            Args = args;
        }

	    /// <summary>
		/// That command that has been triggered or null if the command for this <code>Alias</code> could
		/// not be found.
		/// </summary>
		public BaseCommand<C> Command
		{
			get
			{
				return cmd;
			}
		}

		/// <summary>
		/// That command that was selected when triggering this Trigger.
		/// </summary>
		public BaseCommand<C> SelectedCommand
		{
			get
			{
				return selectedCmd;
			}
			set
			{
				selectedCmd = value;
			}
		}

		/// <summary>
		/// A <code>StringStream</code> which contains the supplied arguments.
		/// </summary>
		public StringStream Text
		{
			get
			{
				return m_text;
			}
			set
			{
				m_text = value;
			}
		}

		/// <summary>
		/// Replies accordingly with the given text.
		/// </summary>
		public abstract void Reply(string text);

		/// <summary>
		/// Replies accordingly with the given formatted text.
		/// </summary>
		public abstract void ReplyFormat(string text);

		public void Reply(string format, params Object[] args)
		{
			Reply(string.Format(format, args));
		}

		public void ReplyFormat(string format, params Object[] args)
		{
			ReplyFormat(string.Format(format, args));
		}

	    public T EvalNext<T>(T deflt)
        {
            var obj = cmd.mgr.EvalNext(this, deflt);
            if (obj is T)
            {
                return (T) obj;
            }
            // obj is not of the correct type
            return default(T);
        }

        public NestedCmdTrigger<C> Nest(C args)
        {
            return new NestedCmdTrigger<C>(this, args);
        }

	    public NestedCmdTrigger<C> Nest(string text)
	    {
	        return Nest(new StringStream(text));
	    }

	    public NestedCmdTrigger<C> Nest(StringStream text)
        {
            return new NestedCmdTrigger<C>(this, Args, text);
        }

		public SilentCmdTrigger<C> Silent(C args)
		{
			return new SilentCmdTrigger<C>(m_text, args);
		}
	}
}
