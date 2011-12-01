using System.Globalization;
using WCell.Constants.Spells;
using WCell.Core.Addons;
using WCell.Core.Initialization;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.Util.Graphics;

namespace WCell.Addons
{
	/// <summary>
	/// WCell's own default addon. Contains some useful additions to the WCell RealmServer that should not be part of the Core.
	/// </summary>
	public class DefaultAddon : WCellAddonBase
	{
		/// <summary>
		/// The id of the unique teleport spell that can be used by staff
		/// to instantly teleport them to the target location.
		/// </summary>
		public const SpellId TeleSpellId = SpellId.UnusedPassengerProxyTest;

		public static string LangDirName = "Lang";
		public static DefaultAddon Instance
		{
			get; private set;
		}

		public DefaultAddon()
		{
			Instance = this;
		}

		public string LangDir
		{
			get { return Context.File.Directory + "/" + LangDirName + "/"; }
		}


		#region WCellAddon Members
		public override bool UseConfig
		{
			get { return true; }
		}

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
					unitCaster.TeleportTo(unitCaster.Map, ref targetPos);
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