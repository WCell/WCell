using System;

namespace WCell.Util.Toolshed
{
	public class ToolException : Exception
	{
		public ToolException()
		{
		}

		public ToolException(string msg, params object[] args)
			: base(string.Format(msg, args))
		{
		}

		public ToolException(Exception innerException, string msg, params object[] args)
			: base(string.Format(msg, args), innerException)
		{
		}
	}
}