using WCell.Constants;
using WCell.Constants.GameObjects;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Looting;
using System;

namespace WCell.RealmServer.GameObjects
{
	public abstract class GameObjectHandler
	{
		protected GameObject m_go;

		/// <summary>
		/// The unit who is currently using the GO
		/// </summary>
		protected Unit m_user;

		public GameObject GO
		{
			get { return m_go; }
		}

		/// <summary>
		/// The unit who is currently using the GO
		/// </summary>
		//public Unit User
		//{
		//    get
		//    {
		//        return m_user;
		//    }
		//}

		/// <summary>
		/// Whether this GO can be used by the given user
		/// </summary>
		/// <param name="chr"></param>
		/// <returns></returns>
		public bool CanBeUsedBy(Character chr)
		{
			if (chr.GodMode)
			{
				return true;
			}

			// must be enabled
			if (!chr.CanSee(m_go) || m_go.State == GameObjectState.Disabled)
			{
				return false;
			}

			// must have the right Faction (if limited to either side)
			if (m_go.Faction != Faction.NullFaction && m_go.Faction.Group != chr.Faction.Group)
			{
				return false;
			}

			// Check for Group-requirements
			if (m_go.Entry.IsPartyOnly && m_go.Owner != null && !m_go.Owner.IsAlliedWith(chr))
			{
				return false;
			}

			var unit = chr;
			if (!unit.IsAlive)
			{
				return false;
			}
			if (!unit.CanInteract)
			{
				return false;
			}

			return m_go.IsInRadiusSq(chr, GOMgr.DefaultInteractDistanceSq);
			// && m_go.IsInFrontOf(obj);
		}

		protected internal virtual void Initialize(GameObject go)
		{
			m_go = go;
		}

		/// <summary>
		/// Tries to use this Object and returns whether the user succeeded using it.
		/// </summary>
		public bool TryUse(Character user)
		{
			if (!CanBeUsedBy(user))
			{
				return false;
			}

			// can't use objects that are in use
			if ((m_go.Flags & GameObjectFlags.InUse) != 0)
			{
				return false;
			}

			if (!m_go.Entry.AllowMounted)
			{
				user.Dismount();
			}

			var lck = m_go.Entry.Lock;
			if (lck != null)
			{
				if (lck.RequiresAttack)
				{
					// TODO: Attack Swing
				}
				else if (lck.RequiresKneeling)
				{
					user.StandState = StandState.Kneeling;
				}
			}
			return m_go.TryOpen(user) && DoUse(user);
		}

		private bool DoUse(Character user)
		{
			return Use(user) && m_go.Entry.NotifyUsed(m_go, user);
		}

		/// <summary>
		/// Makes the given Unit use this GameObject
		/// </summary>
		public abstract bool Use(Character user);

		/// <summary>
		/// Called when the GameObject is being destroyed
		/// </summary>
		protected internal virtual void OnRemove()
		{
		}

		/// <summary>
		/// GO is being removed -> Clean up everthing that needs cleanup
		/// </summary>
		public virtual void Dispose()
		{
		}
	}
}
