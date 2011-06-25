using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Login;

namespace WCell.RealmServer.Events
{
	public partial class RealmServer
	{
		public event Action<RealmStatus> StatusChanged;
	}

	public partial class AuthenticationClient
	{
		/// <summary>
		/// Is called when the RealmServer successfully connects to the AuthServer
		/// </summary>
		public event EventHandler Connected;

		/// <summary>
		/// Is called when the RealmServer disconnects from or loses connection to the AuthServer
		/// </summary>
		public event EventHandler Disconnected;
	}
}