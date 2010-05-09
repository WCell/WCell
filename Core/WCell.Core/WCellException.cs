using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Core
{
    public class WCellException : Exception
    {
		public WCellException()
		{
		}

		public WCellException(string msg, params object[] args)
			: base(string.Format(msg, args))
		{
		}

        public WCellException(Exception innerException, string msg, params object[] args)
			: base(string.Format(msg, args), innerException)
		{
		}
    }
}
