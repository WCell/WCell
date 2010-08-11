using System;
using WCell.Constants;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Entities
{
    public partial class Character
	{
		/// <param name="character"></param>
		/// <param name="firstLogin">Indicates whether the Character starts a new session or if 
		/// the client re-connected to a Character that was already logged in.</param>
		public delegate void CharacterLoginHandler(Character chr, bool firstLogin);
		public delegate void CharacterLogoutHandler(Character chr);

		#region Login/Logout
		/// <summary>
		/// Is called when the Player logs in or reconnects to a Character that was logged in before and not logged out yet (due to logout delay).
		/// </summary>
		public static event CharacterLoginHandler LoggedIn;
		/// <summary>
		/// Is called right befrore the Character is disposed and removed.
		/// </summary>
		public static event CharacterLogoutHandler LoggingOut;
		#endregion

		#region Misc
		/// <summary>
		/// Is called when the given newly created Character logs in the first time.
		/// </summary>
		public static event Action<Character> Created;
		
		/// <summary>
		/// Is called when the given Character gains a new Level.
		/// </summary>
		public static event Action<Character> LevelChanged;
		#endregion
	}
}