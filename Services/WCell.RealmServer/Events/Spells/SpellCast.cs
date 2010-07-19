using System;
using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells
{
	public partial class SpellCast
	{
		/// <summary>
		/// Is called after all preparations have been made and the Spell is about to start casting.
		/// Return anything but <c>SpellFailedReason.None</c> to cancel casting.
		/// </summary>
		public static event Func<SpellCast, SpellFailedReason> Casting;

		/// <summary>
		/// Is called before SpellCast is cancelled for the given reason.
		/// </summary>
		public static event Action<SpellCast, SpellFailedReason> Cancelling;

		/// <summary>
		/// Is called after a SpellCast has been casted.
		/// </summary>
		public static event Action<SpellCast> Casted;
	}
}