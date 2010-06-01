using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Core.Initialization
{
	public class InitializationException : Exception
	{
		public InitializationException()
		{
		}

		public InitializationException(string msg) : base(msg)
		{
		}

		public InitializationException(string msg, params object[] args)
			: base(string.Format(msg, args))
		{
		}

		public InitializationException(Exception e, string msg, params object[] args)
			: base(string.Format(msg, args), e)
		{
		}
	}
}
