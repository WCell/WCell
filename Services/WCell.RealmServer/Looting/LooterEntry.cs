using WCell.Constants;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Looting
{
	/// <summary>
	/// TODO: Keep track of roll-results etc
	/// Every Character has a LooterEntry which represents the interface between a Character and its current Loot
	/// </summary>
	public class LooterEntry
	{
		internal Character m_owner;
		Loot m_loot;

		public LooterEntry(Character chr)
		{
			m_owner = chr;
		}

		/// <summary>
		/// The Looter
		/// </summary>
		public Character Owner
		{
			get
			{
				return m_owner;
			}
		}

		/// <summary>
		/// The Loot that the Character is currently looking at
		/// </summary>
		public Loot Loot
		{
			get
			{
				return m_loot;
			}
			internal set
			{
				if (m_owner == null)
				{
					m_loot = null;
					return;
				}

				if (m_loot != value)
				{
					var oldLoot = m_loot;
					m_loot = value;
					if (value == null)
					{
						m_owner.UnitFlags &= ~UnitFlags.Looting;
						if (oldLoot.MustKneelWhileLooting)
						{
							//m_owner.StandState = StandState.Stand;
						}
						//loot.RemoveLooter(this);

						//if (m_loot.UsesRoundRobin && 
						//    m_owner.GroupMember != null && m_owner.Group.RoundRobinMember == m_owner.GroupMember &&
						//    m_loot.RemainingCount > 0)
						//{
						//    // the RoundRobin looter gives up the rest of the loot

						//}
					}
					else
					{
						m_owner.UnitFlags |= UnitFlags.Looting;
						if (value.MustKneelWhileLooting)
						{
							// TODO: Fix this - It causes the client to release the loot
							//m_owner.StandState = StandState.Kneeling;
						}
					}
				}
			}
		}

		/// <summary>
		/// Requires loot to already be generated
		/// </summary>
		/// <param name="lootable"></param>
		public void TryLoot(ILootable lootable)
		{
			Release(); // make sure that the Character is not still looting something else

			var loot = lootable.Loot;
			if (loot == null)
			{
				LootHandler.SendLootFail(m_owner, lootable);
				// TODO: Kneel and unkneel?
			}
			else if (MayLoot(loot))
			{
				// we are either already a looter or become a new one
				m_owner.CancelAllActions();
				Loot = loot;

				LootHandler.SendLootResponse(m_owner, loot);
			}
			else
			{
				LootHandler.SendLootFail(m_owner, lootable);
			}
		}

		/// <summary>
		/// Returns whether this Looter is entitled to loot anything from the given loot
		/// </summary>
		public bool MayLoot(Loot loot)
		{
			if (m_owner == null)
			{
				return false;
			}

			if (((loot.Looters.Count == 0 || loot.Looters.Contains(this))) ||							// we are one of the initial looters OR:
				 m_owner.GodMode ||
				 (loot.Group != null && m_owner.Group == loot.Group && (								// we are part of the group AND:
					 (loot.FreelyAvailableCount > 0) ||													//	there are freely available items or
					 m_owner.GroupMember == loot.Group.MasterLooter)))									//	this is the MasterLooter
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Releases the current loot and (maybe) makes it available to everyone else.
		/// </summary>
		public void Release()
		{
			if (m_loot != null)
			{
				if (m_owner != null)
				{
					LootHandler.SendLootReleaseResponse(m_owner, m_loot);
				}

				m_loot.RemoveLooter(this);
				if (m_loot.Looters.Count == 0)
				{
					// last looter released
					m_loot.IsReleased = true;
				}
				Loot = null;
			}
		}
	}
}