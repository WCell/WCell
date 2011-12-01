/*************************************************************************
 *
 *   file		: Manager.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-02-16 05:30:51 +0100 (ma, 16 feb 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 757 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using WCell.Util.Threading;

namespace WCell.Core
{
	/// <summary>
	/// Base class used for all Manager classes
	/// </summary>
	public abstract class Manager<T> : Singleton<T>, IContextHandler
		where T : class
	{
		public bool IsInContext
		{
			get { throw new NotImplementedException(); }
		}

		public void AddMessage(IMessage message)
		{
			throw new NotImplementedException();
		}

		public void AddMessage(Action action)
		{
			throw new NotImplementedException();
		}

		public bool ExecuteInContext(Action action)
		{
			throw new NotImplementedException();
		}

		public void EnsureContext()
		{
			
			throw new NotImplementedException();
		}


		public struct ManagerOperation
		{
			
		}
	}
}