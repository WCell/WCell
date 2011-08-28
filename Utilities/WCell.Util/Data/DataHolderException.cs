using System;

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