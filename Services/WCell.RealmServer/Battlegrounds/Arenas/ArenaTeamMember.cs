using WCell.Constants;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util;

namespace WCell.RealmServer.Battlegrounds.Arenas
{
    /// <summary>
    /// Represents the relationship between a Character and its Arena Team.
    /// </summary>
    public partial class ArenaTeamMember : INamed
    {
		private Character m_chr;
		private ArenaTeam m_Team;

        #region Properties
        /// <summary>
        /// The low part of the Character's EntityId. Use EntityId.GetPlayerId(Id) to get a full EntityId
        /// </summary>
		public uint Id
		{
			get { return (uint) CharacterLowId; }
		}

        /// <summary>
        /// The name of this ArenaTeamMember's character
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        public ArenaTeam ArenaTeam
        {
            get { return m_Team; }
            private set
            {
                m_Team = value;
                ArenaTeamId = value.Id;
            }
        }

        /// <summary>
        /// Class of ArenaTeamMember
        /// </summary>
        public ClassId Class
        {
            get
            {
                if (m_chr != null)
                {
                    return m_chr.Class;
                }
                return (ClassId)_class;
            }
            internal set { _class = (int)value; }
        }

        /// <summary>
        /// Returns the Character or null, if this member is offline
        /// </summary>
        public Character Character
        {
            get { return m_chr; }
            internal set
            {
                m_chr = value;

                if (m_chr != null)
                {
                    _name = m_chr.Name;
                    m_chr.ArenaTeamMember[(int)this.ArenaTeam.Slot] = this;
                }
            }
        }

        public uint GamesWeek
        {
            get { return (uint)_gamesWeek; }
            set { _gamesWeek = (int)value; }
        }

        public uint WinsWeek
        {
            get { return (uint)_winsWeek; }
            set { _winsWeek = (int)value; }
        }

        public uint GamesSeason
        {
            get { return (uint)_gamesSeason; }
            set { _gamesSeason = (int)value; }
        }

        public uint WinsSeason
        {
            get { return (uint)_winsSeason; }
            set { _winsSeason = (int)value; }
        }

        public uint PersonalRating
        {
            get { return (uint)_personalRating; }
            set { _personalRating = (int)value; }
        }
        

        #endregion

        #region Methods
        internal void Init(ArenaTeam team)
        {
            Init(team, World.GetCharacter((uint)CharacterLowId));
        }

        internal void Init(ArenaTeam team, Character chr)
        {
            ArenaTeam = team;
            Character = chr;
        }

        /// <summary>
        /// Removes this member from its team
        /// </summary>
        public void LeaveArenaTeam()
        {
            ArenaTeam.RemoveMember(this, true);
        }
        #endregion
    }
}
