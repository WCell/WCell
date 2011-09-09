namespace WCell.Addons.Default.Editor
{
	public class NPCSpawnEditor
	{
		//#map Spawn Menu Stuff

		//#map Figurines
		///// <summary>
		///// The visual representation of this SpawnPoint (or null)
		///// </summary>
		//public SpawnFigurine Figurine
		//{
		//    get
		//    {
		//        return m_figurine;
		//    }
		//    set
		//    {
		//        m_figurine = value;
		//        Map.AddObject(m_figurine, m_spawnEntry.Position);
		//    }
		//}

		///// <summary>
		///// The list of currently active WaypointFigurines
		///// </summary>
		//public LinkedList<WaypointFigurine> WPFigurines
		//{
		//    get { return m_wpFigurines; }
		//}

		///// <summary>
		///// Gets or sets whether figurines of this Spawnpoint are currently added
		///// </summary>
		//public bool IsVisible
		//{
		//    get
		//    {
		//        return m_figurine != null;
		//    }
		//    set
		//    {
		//        // Make sure old remainders (if any now get removed)
		//        if (IsVisible != value)
		//        {
		//            RemoveWPFigurines();
		//            if (value)
		//            {
		//                m_figurine = new SpawnFigurine(this);
		//                Map.AddObjectLater(m_figurine);
		//                AddWPFigurines();
		//            }
		//            else
		//            {
		//                if (m_figurine.IsInWorld)
		//                {
		//                    m_figurine.Delete();
		//                }
		//                m_figurine = null;
		//            }
		//        }
		//    }
		//}

		//public void ToggleVisiblity()
		//{
		//    IsVisible = !IsVisible;
		//}

		//public WaypointFigurine GetWPFigurine(WaypointEntry entry)
		//{
		//    if (m_wpFigurines == null)
		//    {
		//        return null;
		//    }
		//    foreach (var fig in m_wpFigurines)
		//    {
		//        if (fig.Waypoint == entry)
		//        {
		//            return fig;
		//        }
		//    }
		//    return null;
		//}

		//public LinkedListNode<WaypointFigurine> GetWPFigurineNode(WaypointEntry entry)
		//{
		//    if (m_wpFigurines == null)
		//    {
		//        return null;
		//    }
		//    var node = m_wpFigurines.First;
		//    while (node != null)
		//    {
		//        if (node.Value.Waypoint == entry)
		//        {
		//            return node;
		//        }
		//        node = node.Next;
		//    }
		//    return node;
		//}

		//private void AddWPFigurines()
		//{
		//    Figurine last = m_figurine;
		//    m_wpFigurines = new LinkedList<WaypointFigurine>();
		//    foreach (var wp in SpawnEntry.Waypoints)
		//    {
		//        var figurine = new WaypointFigurine(this, wp);
		//        last.Orientation = last.GetAngleTowards(wp);
		//        last.ChannelObject = figurine;
		//        last.ChannelSpell = ConnectingSpell;

		//        last = figurine;
		//        m_wpFigurines.AddLast(figurine);
		//        Map.AddObjectLater(figurine);
		//    }
		//}

		//private void RemoveWPFigurines()
		//{
		//    if (m_wpFigurines == null)
		//    {
		//        return;
		//    }

		//    foreach (var child in m_wpFigurines)
		//    {
		//        if (child.IsInWorld)
		//        {
		//            child.Delete();
		//        }
		//    }

		//    m_wpFigurines = null;
		//}

		//WaypointFigurine InsertFigurine(LinkedListNode<WaypointFigurine> last, WaypointEntry wp)
		//{
		//    Figurine lastFig;
		//    Figurine nextFig;
		//    var newFig = new WaypointFigurine(this, wp);
		//    if (last == null)
		//    {
		//        // first WP
		//        lastFig = m_figurine;
		//        nextFig = m_wpFigurines.First.Value;
		//    }
		//    else
		//    {
		//        lastFig = last.Value;
		//        nextFig = last.Next.Value;
		//    }

		//    if (lastFig != null)
		//    {
		//        lastFig.SetOrientationTowards(newFig);
		//        lastFig.ChannelObject = newFig;
		//        lastFig.ChannelSpell = ConnectingSpell;
		//    }
		//    if (nextFig != null)
		//    {
		//        newFig.SetOrientationTowards(nextFig);
		//        newFig.ChannelObject = nextFig;
		//        newFig.ChannelSpell = ConnectingSpell;
		//    }

		//    m_wpFigurines.AddLast(newFig);
		//    Map.AddObjectLater(newFig);
		//    return newFig;
		//}

		//#endmap
		///// <summary>
		///// Removes the given WP from this SpawnPoint's SpawnEntry
		///// </summary>
		///// <param name="wp"></param>
		//internal void RemoveWP(WaypointEntry wp)
		//{
		//    // remove from List
		//    m_spawnEntry.Waypoints.Remove(wp);

		//    if (m_GossipMenu != null)
		//    {
		//        // remove item from Gossip menu
		//        m_GossipMenu.RemoveWPItem(wp);
		//    }

		//    // figure out the figurines
		//    var figNode = GetWPFigurineNode(wp);
		//    if (figNode != null)
		//    {
		//        // delete
		//        figNode.Value.Delete();

		//        // update orientation
		//        var prev = figNode.Previous;
		//        figNode.List.Remove(figNode);
		//        if (prev != null)
		//        {
		//            var next = prev.Next;
		//            if (next != null)
		//            {
		//                prev.Value.Face(next.Value);
		//                prev.Value.ChannelObject = next.Value;
		//            }
		//            else
		//            {
		//                prev.Value.ChannelObject = null;
		//            }
		//        }
		//    }
		//}

		///// <summary>
		///// Moves the WP to the given new Position
		///// </summary>
		///// <param name="wp"></param>
		//internal void MoveWP(WaypointEntry wp, Vector3 pos)
		//{
		//    wp.Position = pos;

		//    var node = GetWPFigurineNode(wp);
		//    if (node != null)
		//    {
		//        var fig = node.Value;

		//        // update orientations
		//        var next = node.Next;
		//        if (next != null)
		//        {
		//            fig.SetOrientationTowards(next.Value);
		//        }
		//        if (node.Previous != null)
		//        {
		//            node.Previous.Value.SetOrientationTowards(fig);
		//        }

		//        // move over
		//        MovementHandler.SendMoveToPacket(fig, ref pos, fig.Orientation, 1000, MonsterMoveFlags.DefaultMask);
		//        fig.TeleportTo(ref pos);
		//    }
		//}

		///// <summary>
		///// Adds a new WP after the given oldWp or at the end if its new with 
		///// the given position and orientation.
		///// </summary>
		///// <param name="oldWp">May be null</param>
		//internal WaypointEntry InsertAfter(WaypointEntry oldWp, Vector3 pos, float orientation)
		//{
		//    var newWp = m_spawnEntry.CreateWP(pos, orientation);

		//    LinkedListNode<WaypointEntry> newNode;
		//    LinkedListNode<WaypointFigurine> oldFigNode;
		//    if (oldWp != null)
		//    {
		//        oldFigNode = GetWPFigurineNode(oldWp);
		//        var oldNode = oldWp.Node;
		//        var fig = oldNode.Value;
		//        newNode = oldNode.List.AddAfter(oldNode, newWp);
		//    }
		//    else
		//    {
		//        newNode = m_spawnEntry.Waypoints.AddLast(newWp);
		//        oldFigNode = null;
		//    }
		//    newWp.Node = newNode;

		//    var newFig = InsertFigurine(oldFigNode, newWp);
		//    newFig.Highlight();
		//    return newWp;
		//}

		//[NotPersistent]
		//public SpawnPointMenu GossipMenu
		//{
		//    get
		//    {
		//        if (m_GossipMenu == null)
		//        {
		//            m_GossipMenu = new SpawnPointMenu(this);
		//        }
		//        return m_GossipMenu;
		//    }
		//    set { m_GossipMenu = value; }
		//}
		//#endmap
	}
}
