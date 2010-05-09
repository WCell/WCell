using WCell.RealmServer.Entities;
using System;

namespace WCell.RealmServer.Gossips
{
	public delegate void GossipActionHandler(GossipConversation convo);

    /// <summary>
    /// A class determining whether an item should be shown to specific character and specifying reaction on
    /// item selection
    /// </summary>
    public interface IGossipAction
	{
		/// <summary>
		/// Whether this Action determines the next Action
		/// </summary>
		bool Navigates { get; }

        /// <summary>
        /// Should item be shown to this character?
        /// </summary>
        /// <param name="character">character</param>
        /// <returns>true if yes</returns>
        bool CanUse(Character character);

        /// <summary>
        /// Handler of item selection
        /// </summary>
		void OnSelect(GossipConversation convo);
	}

	public class DefaultGossipAction : IGossipAction
	{
		private GossipActionHandler m_Handler;

		public DefaultGossipAction(GossipActionHandler handler)
		{
			m_Handler = handler;
		}

		public GossipActionHandler Handler
		{
			get { return m_Handler; }
			set { m_Handler = value; }
		}

		public bool Navigates
		{
			get { return false; }
		}

		public virtual bool CanUse(Character character)
		{
			return true;
		}

		public void OnSelect(GossipConversation convo)
		{
			m_Handler(convo);
		}
	}

	public class NavigationGossipAction : IGossipAction
	{
		private GossipActionHandler m_Handler;

		public NavigationGossipAction(GossipActionHandler handler)
		{
			m_Handler = handler;
		}

		public GossipActionHandler Handler
		{
			get { return m_Handler; }
			set { m_Handler = value; }
		}

		public bool Navigates
		{
			get { return true; }
		}

		public virtual bool CanUse(Character character)
		{
			return true;
		}

		public void OnSelect(GossipConversation convo)
		{
			m_Handler(convo);
		}
	}

    /// <summary>
    /// Gossip action. Allows showing of the gossip item only to characters with level higher than specified
    /// </summary>
    public class LevelRequirementGossipAction : IGossipAction
    {
        readonly uint m_level;

        public LevelRequirementGossipAction(uint level)
        {
            m_level = level;
        }

    	public bool Navigates
    	{
			get { return false; }
    	}

    	public bool CanUse(Character character)
        {
            return character.Level >= m_level;
        }

		public virtual void OnSelect(GossipConversation convo)
        {
        }
    }
}
