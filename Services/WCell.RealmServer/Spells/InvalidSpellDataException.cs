using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace WCell.RealmServer.Spells
{
	public class InvalidSpellDataException : Exception
	{
		public InvalidSpellDataException()
		{
		}

		public InvalidSpellDataException(string message, params object[] args) : base(string.Format(message, args))
		{
		}

		public InvalidSpellDataException(Exception innerException, string message, params object[] args) : base(string.Format(message, args), innerException)
		{
		}

		protected InvalidSpellDataException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
