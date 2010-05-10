using System.Globalization;

using WCell.Core.Addons;

using WCell.RealmServer.Spells;
using WCell.RealmServer.Entities;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Chat;
using WCell.Util.Graphics;

namespace WCell.Addons
{
	/// <summary>
	/// WCell's own default addon. Contains some useful additions to the WCell RealmServer that should not be part of the Core.
	/// </summary>
	public class DefaultAddon : WCellAddonBase
	{
		public const SpellId TeleSpellId = //SpellId.UnusedNPCPORTTEST;
			SpellId.UnusedDistractTest;

		public override bool UseConfig
		{
			get { return true; }
		}

		#region WCellAddon Members
		public override string Name
		{
			get { return "Default Addon"; }
		}

		public override string ShortName
		{
			get { return "Default"; }
		}

		public override string Author
		{
			get { return "The WCell Team"; }
		}

		public override string Website
		{
			get { return "http://www.wcell.org"; }
		}

		public override string GetLocalizedName(CultureInfo culture)
		{
			return "Default Addon";
		}

		public override void TearDown()
		{
			// does nothing
		}

		/// <summary>
		/// Add something to the Last Initialization-Step, so that WCell is already fully initialized before this method is called.
		/// </summary>
		[Initialization(InitializationPass.Last, "Initialize Default Addon")]
		public static void Init()
		{
			// Make some deprecated spell (with an area target) an instant teleport spell:
			SpellHandler.Get(TeleSpellId).SpecialCast = delegate(Spell spell, WorldObject caster, WorldObject target, ref Vector3 targetPos)
			{
				if (caster is Unit)
				{
					var unitCaster = caster as Unit;
					unitCaster.TeleportTo(unitCaster.Region, ref targetPos);
				}
			};

			// add the spell to any newly created staff-Character
			Character.Created += chr =>
			{
				if (chr.Account.Role.IsStaff)
				{
					chr.Spells.AddSpell(TeleSpellId);
				}
			};

			// let staff members join the staff channel
			Character.LoggedIn += (chr, firstConnect) =>
			{
				if (firstConnect)
				{
					if (chr.Account.Role.IsStaff && ChatChannel.Staff != null)
					{
						chr.AddMessage(() => ChatChannel.Staff.TryJoin(chr));
					}
				}
			};
		}
		#endregion
	}
}