namespace WCell.RealmServer.Gossips
{
    public delegate void GossipActionHandler(GossipConversation convo);

    /// <summary>
    /// Decides whether the item may be displayed/used, in the given convo
    /// </summary>
    public delegate bool GossipActionDecider(GossipConversation convo);

    /// <summary>
    /// A class determining whether an item should be shown to specific character and specifying reaction on
    /// item selection
    /// </summary>
    public interface IGossipAction
    {
        /// <summary>
        /// Whether this Action determines the next Action.
        /// False, if it this Action does not send a GossipMenu.
        /// True, if this Action determines the next GossipMenu to be sent, itself.
        /// </summary>
        bool Navigates { get; }

        /// <summary>
        /// Should item be shown to and used by this conversation's character?
        /// </summary>
        /// <returns>true if yes</returns>
        bool CanUse(GossipConversation convo);

        /// <summary>
        /// Handler of item selection
        /// </summary>
        void OnSelect(GossipConversation convo);
    }

    public class NonNavigatingDecidingGossipAction : NonNavigatingGossipAction
    {
        public GossipActionDecider Decider { get; set; }

        public NonNavigatingDecidingGossipAction(GossipActionHandler handler, GossipActionDecider decider)
            : base(handler)
        {
            Decider = decider;
        }

        public override bool CanUse(GossipConversation convo)
        {
            return Decider(convo);
        }
    }

    public class NonNavigatingGossipAction : IGossipAction
    {
        private GossipActionHandler m_Handler;

        public NonNavigatingGossipAction(GossipActionHandler handler)
        {
            m_Handler = handler;
        }

        public GossipActionHandler Handler
        {
            get { return m_Handler; }
            set { m_Handler = value; }
        }

        public virtual bool Navigates
        {
            get { return false; }
        }

        public virtual bool CanUse(GossipConversation convo)
        {
            return true;
        }

        public void OnSelect(GossipConversation convo)
        {
            m_Handler(convo);
        }
    }

    public class NavigatingGossipAction : IGossipAction
    {
        private GossipActionHandler m_Handler;

        public NavigatingGossipAction(GossipActionHandler handler)
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

        public virtual bool CanUse(GossipConversation convo)
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
    public class LevelRestrictedGossipAction : NonNavigatingGossipAction
    {
        readonly uint m_level;

        public LevelRestrictedGossipAction(uint level, GossipActionHandler handler)
            : base(handler)
        {
            m_level = level;
        }

        public override bool CanUse(GossipConversation convo)
        {
            return convo.Character.Level >= m_level;
        }
    }

    /// <summary>
    /// Gossip action. Associated Menu item can only be seen by staff members
    /// </summary>
    public class StaffRestrictedGossipAction : NonNavigatingGossipAction
    {
        public StaffRestrictedGossipAction(GossipActionHandler handler)
            : base(handler)
        {
        }

        public override bool CanUse(GossipConversation convo)
        {
            return convo.Character.Role.IsStaff;
        }
    }

    /// <summary>
    /// Gossip action. Associated Menu item can only be seen by players that are not staff members
    /// </summary>
    public class PlayerRestrictedGossipAction : NonNavigatingGossipAction
    {
        public PlayerRestrictedGossipAction(GossipActionHandler handler)
            : base(handler)
        {
        }

        public override bool CanUse(GossipConversation convo)
        {
            return !convo.Character.Role.IsStaff;
        }
    }
}