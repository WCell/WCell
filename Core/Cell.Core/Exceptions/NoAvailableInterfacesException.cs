using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cell.Core.Exceptions
{
    [Serializable]
	public class NoAvailableAdaptersException : Exception
	{
		public NoAvailableAdaptersException()
			: base()
		{
		}

		public NoAvailableAdaptersException(string message)
			: base(message)
		{
		}
	}
}
