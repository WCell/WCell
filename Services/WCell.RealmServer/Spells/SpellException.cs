using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace WCell.RealmServer.Spells
{
	public class SpellException : Exception
	{
		public SpellException()
		{
		}

		public SpellException(string message, params object[] args) : base(string.Format(message, args))
		{
		}

		public SpellException(Exception innerException, string message, params object[] args)
			: base(string.Format(message, args), innerException)
		{
		}

		protected SpellException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
