using WCell.RealmServer.Gossips;

namespace WCell.RealmServer.RacesClasses
{
	public delegate void ArchetypeSelectionHandler(GossipConversation convo, Archetype archetype);

	public class ArchetypeSelectionMenu : GossipMenu
	{
		public static readonly StaticGossipEntry RaceTextEntry = new StaticGossipEntry(813255, "Select your Race");
		public static readonly StaticGossipEntry ClassTextEntry = new StaticGossipEntry(813256, "Select your Class");

		public ArchetypeSelectionMenu(ArchetypeSelectionHandler callback) :
			this(callback, RaceTextEntry.GossipId, ClassTextEntry.GossipId)
		{
		}

		public ArchetypeSelectionMenu(ArchetypeSelectionHandler callback, uint raceTextId, uint clssTextId)
			: base(raceTextId)
		{
			foreach (var clss in ArchetypeMgr.BaseClasses)
			{
				if (clss != null)
				{
					AddItem(new GossipMenuItem(clss.Id.ToString(), new ClassSelectionMenu(clss, callback, clssTextId)));
				}
			}
		}
	}

	public class ClassSelectionMenu : GossipMenu
	{
		public readonly BaseClass Class;

		public ClassSelectionMenu(BaseClass clss, ArchetypeSelectionHandler handler, uint textId)
			: base(textId)
		{
			Class = clss;
			foreach (var archetype in ArchetypeMgr.Archetypes[(int)clss.Id])
			{
				if (archetype != null)
				{
					var arche = archetype;	// local reference
					AddItem(new GossipMenuItem(archetype.ToString(), convo => handler(convo, arche)));
				}
			}
		}
	}
}