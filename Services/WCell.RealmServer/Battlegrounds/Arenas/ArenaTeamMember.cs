using System;
using WCell.Constants;
using WCell.Constants.ArenaTeams;
using WCell.RealmServer.Database;
using WCell.Util.Threading;
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
		private Character _character;
		private ArenaTeam _team;

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
            get { return _team; }
            private set
            {
                _team = value;
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
                if (_character != null)
                {
                    return _character.Class;
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
            get { return _character; }
            internal set
            {
                _character = value;

                if (_character != null)
                {
                    _name = _character.Name;
                    _character.ArenaTeamMember[(int)this.ArenaTeam.Slot] = this;
                }
            }
        }

        public uint GamesWeek;

        public uint WinsWeek;

        public uint GamesSeason;

        public uint WinsSeason;

        public uint PersonalRating;
        

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
