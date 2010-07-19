using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace WCell.Intercommunication.DataTypes
{
	[DataContract]
	public class UpdateRealmResponse
	{
		public UpdateRealmResponse()
		{
		}

		public void AddCommand(string commandStr)
		{
			if (Commands == null)
			{
				Commands = new List<string>(3);
			}
			Commands.Add(commandStr);
		}

		[DataMember]
		public List<string> Commands
		{
			get;
			set;
		}
	}
}