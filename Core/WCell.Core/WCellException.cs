using System;

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