using System;
using WCell.Constants;
using WCell.Constants.ArenaTeams;
using WCell.Util.Threading;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util;

namespace WCell.RealmServer.ArenaTeams
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
        /// The name of this ArenaTeamMemberr's character
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
