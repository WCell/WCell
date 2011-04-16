using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace WCell.Util.Data
{
	public class DataHolderException : Exception
	{
		public DataHolderException()
		{
		}

		public DataHolderException(string msg, params object[] args)
			: base(string.Format(msg, args))
		{
		}

		public DataHolderException(Exception innerException, string msg, params object[] args)
			: base(string.Format(msg, args), innerException)
		{
		}
	}
}