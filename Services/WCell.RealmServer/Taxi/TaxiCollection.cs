using System.Collections.Generic;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Taxi
{
	/// <summary>
	/// Represents the relationship between the Character and his/her known TaxiNodes
	/// </summary>
	public class TaxiCollection
	{
	    readonly Character m_owner;
		Dictionary<uint, PathNode> m_nodes;

		public TaxiCollection(Character chr)
		{
			m_owner = chr;
			m_nodes = new Dictionary<uint, PathNode>();
		}

		public Character Owner
		{
			get
			{
				return m_owner;
			}
		}

		public void AddNode(PathNode node)
		{
			m_nodes[node.Id] = node;
			TaxiHandler.SendTaxiPathActivated(m_owner.Client);
		}
	}
}
