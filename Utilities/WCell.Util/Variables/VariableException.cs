using System;

namespace WCell.Util.Variables
{
	public class VariableException : Exception
	{
		public VariableException()
		{
		}

		public VariableException(string msg, params object[] args)
			: base(string.Format(msg, args))
		{
		}

		public VariableException(Exception innerException, string msg, params object[] args)
			: base(string.Format(msg, args), innerException)
		{
		}
	}
}