using NLog;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.Util;

namespace WCell.RealmServer.GameObjects.GOEntries
{
	public class GOGooberEntry : GOEntry
	{
		private static readonly Logger sLog = LogManager.GetCurrentClassLogger();


	    /// <summary>
	    /// The LockId from Lock.dbc
	    /// </summary>
	    public uint LockId
	    {
            get { return Fields[0]; }
	    }

	    /// <summary>
	    /// The Id of the quest required to be active in order to interact with this goober.
	    /// </summary>
	    public uint QuestId
	    {
            get { return Fields[1]; }
	    }

	    /// <summary>
	    /// The Id of an Event associated with this goober (?)
	    /// </summary>
	    public uint EventId
	    {
            get { return Fields[2]; }
	    }


	    /// <summary>
	    /// The time delay before this goober auto-closes after being opened. (?)
	    /// </summary>
	    public uint AutoClose
	    {
            get { return Fields[3]; }
	    }

	    /// <summary>
	    /// ???
	    /// </summary>
	    public uint CustomAnim
	    {
            get { return Fields[4]; }
	    }

	    /// <summary>
	    /// Time between allowed interactions with this goober (?)
	    /// </summary>
	    public uint Cooldown
	    {
            get { return Fields[6]; }
	    }

	    /// <summary>
	    /// The Id of a PageText object associated with this goober.
	    /// </summary>
	    public uint PageId
	    {
            get { return Fields[7]; }
	    }

	    /// <summary>
	    /// The LanguageId from Languages.dbc
	    /// </summary>
	    public ChatLanguage LanguageId
	    {
            get { return (ChatLanguage) Fields[8]; }
	    }

	    /// <summary>
	    /// The PageTextMaterialId from PageTextMaterial.dbc
	    /// </summary>
	    public PageMaterial PageTextMaterialId
	    {
            get { return (PageMaterial) Fields[9]; }
	    }

	    /// <summary>
	    /// The SpellId associated with this goober.
	    /// </summary>
	    public SpellId SpellId
	    {
            get { return (SpellId) Fields[10]; }
	    }

		/// <summary>
		/// The Spell associated with this goober.
		/// </summary>
		public Spell Spell;

	    /// <summary>
	    /// ???
	    /// </summary>
	    public bool NoDamageImmune
	    {
            get { return Fields[11] > 0; }
	    }

	    /// <summary>
	    /// ???
	    /// </summary>
	    public bool Large
	    {
            get { return Fields[13] > 0; }
	    }

	    /// <summary>
	    /// The Id of a text object to be displayed when opening this goober (?)
	    /// </summary>
	    public uint OpenTextId
	    {
            get { return Fields[14]; }
	    }

	    /// <summary>
	    /// The Id of a text object to be displayed when closing this goober (?)
	    /// </summary>
	    public uint CloseTextId
	    {
            get { return Fields[15]; }
	    }

	    public uint FloatingTooltip
	    {
            get { return Fields[18]; }
	    }

	    public uint GossipId
	    {
            get { return Fields[19]; }
	    }

	    public bool WorldStateSetsState
	    {
            get { return Fields[20] != 0; }
	    }

		protected internal override void InitEntry()
		{
			Lock = LockEntry.Entries.Get(LockId);
            Spell = SpellHandler.Get(SpellId);

			IsConsumable = Fields[5] > 0;

			LinkedTrapId = Fields[12];

			LosOk = Fields[16] > 0;
			AllowMounted = Fields[17] > 0;
		}
	}
}