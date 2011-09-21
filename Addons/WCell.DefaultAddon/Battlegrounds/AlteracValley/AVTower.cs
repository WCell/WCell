using WCell.Constants;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.NPCs;

namespace WCell.Addons.Default.Battlegrounds.AlteracValley
{
	public abstract class AlteracTower : AVItem
	{

		// TODO: Spawn a different flag (GO) whenever a state changes 
		// TODO: (horde capped, challenged, alliance capped, neutral)

		#region Fields
		private BattlegroundSide _side = BattlegroundSide.End;

		public GOEntry FlagStand;

		/// <summary>
		/// The character currently capturing the flag.
		/// </summary>
		public Character Capturer;
		public uint Score;
		public AlteracValley Instance;
		public NPC Warmaster;

		#endregion

		protected AlteracTower(AlteracValley instance, GOEntry flagstand)
		{
			Instance = instance;
			FlagStand = flagstand;
			var entry = WarmasterEntry;
			if (entry != null)
			{
				entry.Activated += (warmaster) =>
				{
					Warmaster = warmaster;
				};
			}

		}

		public abstract string Name
		{
			get;
		}

		/// <summary>
		/// The warmaster attached to this tower.
		/// </summary>
		public abstract NPCEntry WarmasterEntry
		{
			get;
		}

		/// <summary>
		/// The side currently in control of this base.
		/// If End, base is neutral.
		/// </summary>
		public BattlegroundSide BaseOwner
		{
			get { return _side; }
			set { _side = value; }
		}

		public void RegisterFlagstand()
		{
			FlagStand.Used += (go, chr) =>
			{
				if (CanBurn(chr))
				{
					BurnTower(chr);
				}

				return true;
			};
		}

		private void BurnTower(Character character)
		{
			Warmaster.Delete();
			// Notify the BG, remove this warmaster from the AIGroup.
			ChatMgr.SendSystemMessage(Instance.Characters, Name + " has been destroyed by the " + BaseOwner);

			// Check this: (the flag/tower should burn and become unusable)
			FlagStand.FirstSpawnEntry.AnimProgress = 255;
		}

		private bool CanBurn(Character chr)
		{
			return chr.Battlegrounds.Team.Side != BaseOwner;
		}

		public void Destroy()
		{
			//Warmaster.
		}

		#region AVItem Members

		public void Capture()
		{
			throw new System.NotImplementedException();
		}

		#endregion
	}
}