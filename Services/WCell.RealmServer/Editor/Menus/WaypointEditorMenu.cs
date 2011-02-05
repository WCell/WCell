using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Editor.Figurines;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Spawns;

namespace WCell.RealmServer.Editor.Menus
{
	public class WaypointEditorMenu : SpawnEditorMenu
	{
		//private readonly NPCSpawnPoint m_point;

		//public WaypointEditorMenu(NPCSpawnPoint point)
		//    : base(point.SpawnEntry.Entry.NameGossipId)
		//{
		//    m_point = point;

		//    KeepOpen = true;

		//    AddItem(new GossipMenuItem("Go to Spawn", HandleTeleport));
		//    AddItem(new GossipMenuItem("Remove Spawn", HandleRemove,
		//        "This will remove the Spawn with all its Waypoints (" + m_point.SpawnEntry.Entry + ")"));

		//    AddItem(new GossipMenuItem("Waypoint List", WPMenu = CreateWaypointMenu()));
		//    AddItem(new GossipMenuItem("Add new Wapyoint", HandleAddWP));

		//    AddQuitMenuItem();
		//}

		///// <summary>
		///// The Waypoint-submenu
		///// </summary>
		//public GossipMenu WPMenu
		//{
		//    get;
		//    internal set;
		//}

		///// <summary>
		///// Returns the GossipMenuItem of the given Waypoint from this menu
		///// </summary>
		///// <param name="wp"></param>
		//public void RemoveWPItem(WaypointEntry wp)
		//{
		//    foreach (var item in WPMenu.GossipItems)
		//    {
		//        if (item is WPItem && ((WPItem)item).Wp == wp)
		//        {
		//            WPMenu.GossipItems.Remove(item);
		//            return;
		//        }
		//    }
		//}

		///// <summary>
		///// Creates and returns the sub-menu for Waypoints
		///// </summary>
		///// <returns></returns>
		//private GossipMenu CreateWaypointMenu()
		//{
		//    var menu = new GossipMenu(m_point.SpawnEntry.Entry.NameGossipId);

		//    menu.AddGoBackItem("Go back...");
		//    foreach (var wp in m_point.SpawnEntry.Waypoints)
		//    {
		//        menu.AddItem(new WPItem(m_point, wp));
		//    }
		//    menu.AddQuitMenuItem();
		//    return menu;
		//}

		///// <summary>
		///// Handle what happens when clicking on the Teleport option
		///// </summary>
		//void HandleTeleport(GossipConversation convo)
		//{
		//    convo.Character.TeleportTo(m_point);
		//}

		///// <summary>
		///// Handle what happens when clicking on the Remove option
		///// </summary>
		//private void HandleRemove(GossipConversation convo)
		//{
		//    m_point.RemoveSpawnLater();
		//}

		///// <summary>
		///// Handle what happens when clicking on the Add WP button
		///// </summary>
		//private void HandleAddWP(GossipConversation convo)
		//{
		//    var chr = convo.Character;
		//    m_point.InsertAfter(null, chr.Position, chr.Orientation);
		//}

		//public NPCSpawnPoint Point
		//{
		//    get { return m_point; }
		//}

		///// <summary>
		///// A GossipMenuItem for each Waypoint
		///// </summary>
		//public class WPItem : GossipMenuItem
		//{
		//    private readonly NPCSpawnPoint m_Point;
		//    private WaypointEntry m_wp;

		//    public WPItem(NPCSpawnPoint point, WaypointEntry wp)
		//    {
		//        m_Point = point;
		//        m_wp = wp;
		//        Text = "WP #" + wp.Id;

		//        SubMenu = new GossipMenu(point.SpawnEntry.Entry.NameGossipId);
		//        SubMenu.AddGoBackItem();
		//        SubMenu.AddRange(
		//            new GossipMenuItem("Go to Point" + Text, HandleGoto),
		//            new GossipMenuItem("Remove", HandleRemove),
		//            new GossipMenuItem("Move Point here", HandleMoveOver),
		//            new GossipMenuItem("Insert New", HandleInsert));
		//    }

		//    public WaypointEntry Wp
		//    {
		//        get { return m_wp; }
		//    }

		//    public NPCSpawnPoint Point
		//    {
		//        get { return m_Point; }
		//    }

		//    /// <summary>
		//    /// Go to the WP
		//    /// </summary>
		//    void HandleGoto(GossipConversation convo)
		//    {
		//        convo.Character.TeleportTo(m_Point.Map, m_wp.Position);
		//    }

		//    /// <summary>
		//    /// Remove the WP
		//    /// </summary>
		//    void HandleRemove(GossipConversation convo)
		//    {
		//        m_Point.RemoveWP(m_wp);

		//        // the WP is now gone, so let's send the Menu again (without this Item in it)
		//        convo.Invalidate();
		//    }

		//    /// <summary>
		//    /// Move the Waypoint over to the Character
		//    /// </summary>
		//    void HandleMoveOver(GossipConversation convo)
		//    {
		//        m_Point.MoveWP(m_wp, convo.Character.Position);
		//    }

		//    /// <summary>
		//    /// Insert a new WP
		//    /// </summary>
		//    void HandleInsert(GossipConversation convo)
		//    {
		//        m_Point.InsertAfter(m_wp, convo.Character.Position, convo.Character.Orientation);
		//    }
		//}
		public WaypointEditorMenu(MapEditor editor, NPCSpawnPoint spawnPoint, EditorFigurine figurine) : base(editor, spawnPoint, figurine)
		{
		}
	}
}
