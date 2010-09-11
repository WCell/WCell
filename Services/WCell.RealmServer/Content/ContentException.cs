using System;

namespace WCell.RealmServer.Content
{
	public class ContentException : Exception
	{
		public ContentException()
		{
		}

		public ContentException(string msg, params object[] args)
			: base(string.Format(msg, args))
		{
		}

		public ContentException(Exception innerException, string msg, params object[] args)
			: base(string.Format(msg, args), innerException)
		{
		}

		public override string Message
		{
			get
			{
				return "<" + ContentMgr.ContentProviderName +"> " + base.Message;
			}
		}
	}
}