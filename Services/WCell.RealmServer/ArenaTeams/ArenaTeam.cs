using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cell.Core;
using NLog;
using WCell.Constants.ArenaTeams;
using WCell.Core.Database;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.Util;
using WCell.Util.Collections;
using WCell.Util.NLog;
using WCell.Util.Threading;

namespace WCell.RealmServer.ArenaTeams
{
	public partial class ArenaTeam : WCellRecord<ArenaTeam>, INamed, IEnumerable<ArenaTeamMember>, IChatTarget
	{
		#region Fields
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		private SpinWaitLock m_syncRoot;
		private ArenaTeamMember m_leader;
        private ArenaTeamStats m_stats;
        private ArenaTeamSlot m_slot;

		public readonly ImmutableDictionary<uint, ArenaTeamMember> Members = new ImmutableDictionary<uint, ArenaTeamMember>();
		#endregion

		#region Properties
		/// <summary>
		/// The SyncRoot against which to synchronize this arena team(when iterating over it or making certain changes)
		/// </summary>
		public SpinWaitLock SyncRoot
		{
			get { return m_syncRoot; }
		}

		/// <summary>
		/// Id of this team
		/// </summary>
		public uint Id
		{
			get { return (uint)_id; }
		}

		/// <summary>
		/// Arena team's name
		/// </summary>
		/// <remarks>length is limited with MAX_ARENATEAM_LENGTH</remarks>
		public string Name
		{
            get { return _name; }
        }
        
        /// <summary>
        /// Type of this arena team
        /// </summary>
        public uint Type
        {
            get { return (uint)_type; }
        }

        /// <summary>
        /// Slot of this arena team
        /// <summary>
        public ArenaTeamSlot Slot
        {
            get { return m_slot; }
            set { m_slot = value; }
        }

        /// Arena team leader's ArenaTeamMember
        /// Setting it does not send event to the team. Use ArenaTeam.SendEvent
        /// </summary>
        public ArenaTeamMember Leader
        {
            get { return m_leader; }
            set
            {
                if (value == null || value.ArenaTeam != this)
                    return;

                m_leader = value;
                _leaderLowId = (int)value.Id;
            }
        }

        /// <summary>
        /// Stats of the arena team
        /// </summary>
        public ArenaTeamStats Stats
        {
            set { m_stats = value; }
            get { return m_stats; }
        }
         
        /// <summary>
        /// Number of arena team's members
        /// </summary>
        public int MemberCount
        {
            get { return Members.Count; }
        }

        #endregion

        #region Constructor
        public ArenaTeam() 
        {
        }
      
		/// <summary>
		/// Creates a new ArenaTeamRecord row in the database with the given information.
		/// </summary>
		/// <param name="leader">leader's character record</param>
		/// <param name="name">the name of the new character</param>
		/// <returns>the <seealso cref="ArenaTeam"/> object</returns>
		public ArenaTeam(CharacterRecord leader, string name, uint type)
			: this()
		{
			_id = _idGenerator.Next();
			_leaderLowId = (int)leader.EntityLowId;
			_name = name;
            _type = (int)type;

            m_slot = ArenaTeamMgr.GetSlotByType(type);

			m_leader = new ArenaTeamMember(leader, this, true);
            m_stats = new ArenaTeamStats(this);

			Members.Add(m_leader.Id, m_leader);
            m_leader.Create();
			
            RealmServer.Instance.AddMessage(Create);
            Register();
		}
        #endregion

        #region Init
        /// <summary>
        /// Initialize the Team after loading from DB
        /// </summary>
        internal void InitAfterLoad()
        {
            var members = ArenaTeamMember.FindAll(this);
            foreach (var atm in members)
            {
                atm.Init(this);
                Members.Add(atm.Id, atm);
            }
            m_stats = ArenaTeamStats.FindByPrimaryKey(this.Id);
            m_slot = ArenaTeamMgr.GetSlotByType(Type);

            m_leader = this[LeaderLowId];
            if (m_leader == null)
            {
                OnLeaderDeleted();
            }
            if (m_leader != null)
            {
                Register();
            }
        }

        /// <summary>
        /// Initializes arena team after its creation or restoration from DB
        /// </summary>
        internal void Register()
        {
            ArenaTeamMgr.RegisterArenaTeam(this);
        }
        #endregion

        #region ArenaTeamMembers
        public ArenaTeamMember AddMember(Character chr)
        {
            var member = AddMember(chr.Record);
            if (member != null)
            {
                member.Character = chr;
            }
            return member;
        }

        /// <summary>
        /// Adds a new arena team member
        /// Calls ArenaTeamMgr.OnJoinTeam
        /// </summary>
        /// <param name="chr">character to add</param>
        /// <returns>ArenaTeamMember of new member</returns>
        public ArenaTeamMember AddMember(CharacterRecord chr)
        {
            ArenaTeamMember newMember;

            if(Members.Count >= Type*2)
                return null;

            SyncRoot.Enter();
            try
            {
                if (Members.TryGetValue(chr.EntityLowId, out newMember))
                {
                    return newMember;
                }
                newMember = new ArenaTeamMember(chr, this, false);
                newMember.Character.SetArenaTeamInfoField(Slot, ArenaTeamInfoType.ARENA_TEAM_ID, Id);
                newMember.Character.SetArenaTeamInfoField(Slot, ArenaTeamInfoType.ARENA_TEAM_MEMBER, 1);

                Members.Add(newMember.Id, newMember);

            	newMember.Create();
                Update();
            }
            catch (Exception e)
            {
                LogUtil.ErrorException(e, string.Format("Could not add member {0} to arena team {1}", chr, this));
                return null;
            }
            finally
            {
                SyncRoot.Exit();
            }

            ArenaTeamMgr.RegisterArenaTeamMember(newMember);

            //ArenaTeamHandler.SendEventToTeam(this, ArenaTeamEvents.JOINED_SS, newMember);

            return newMember;
        }

        public bool RemoveMember(uint memberId)
        {
            var member = this[memberId];
            if (member != null)
            {
                return RemoveMember(member, true);
            }
            return false;
        }

        public bool RemoveMember(string name)
        {
            var member = this[name];
            if (member != null)
            {
                return RemoveMember(member, true);
            }
            return false;
        }

        /// <summary>
        /// Removes ArenaTeamMember from the arena team
        /// </summary>
        /// <param name="member">member to remove</param>
        /// <param name="update">if true, sends event to the team</param>
        public bool RemoveMember(ArenaTeamMember member)
        {
            return RemoveMember(member, true);
        }

        /// <summary>
        /// Removes ArenaTeamMember from the arena team
        /// </summary>
        /// <param name="member">member to remove</param>
        /// <param name="update">if false, changes to the team will not be promoted anymore (used when the team is being disbanded)</param>
        public bool RemoveMember(ArenaTeamMember member, bool update)
        {
            OnRemoveMember(member);

            if (update && member == m_leader)
            {
                OnLeaderDeleted();
            }

            if (m_leader == null)
            {
                // Team has been disbanded
                return true;
            }

            m_syncRoot.Enter();
            try
            {
                if (!Members.Remove(member.Id))
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                LogUtil.ErrorException(e, string.Format("Could not delete member {0} from arena team {1}", member.Name,
                    this));
                return false;
            }
            finally
            {
                m_syncRoot.Exit();
            }

            if (update)
            {
                //ArenaTeamHandler.SendEventToTeam(this, ArenaTeamEvents.LEAVED_SS, member);
            }

            RealmServer.Instance.AddMessage(() =>
            {
                member.Delete();
                if (update)
                {
                    Update();
                }
            });
            return true;
        }

        /// <summary>
        /// Called before the given member is removed to clean up everything related to the given member
        /// </summary>
        protected void OnRemoveMember(ArenaTeamMember member)
        {
            ArenaTeamMgr.UnregisterArenaTeamMember(member);

            var chr = member.Character;
            if (chr != null)
            {
                chr.ArenaTeamMember[(int)Slot] = null;
                chr.SetArenaTeamInfoField(Slot, ArenaTeamInfoType.ARENA_TEAM_ID, 0);
                chr.SetArenaTeamInfoField(Slot, ArenaTeamInfoType.ARENA_TEAM_TYPE, 0);
                chr.SetArenaTeamInfoField(Slot, ArenaTeamInfoType.ARENA_TEAM_MEMBER, 0);
                chr.SetArenaTeamInfoField(Slot, ArenaTeamInfoType.ARENA_TEAM_GAMES_WEEK, 0);
                chr.SetArenaTeamInfoField(Slot, ArenaTeamInfoType.ARENA_TEAM_GAMES_SEASON, 0);
                chr.SetArenaTeamInfoField(Slot, ArenaTeamInfoType.ARENA_TEAM_WINS_SEASON, 0);
                chr.SetArenaTeamInfoField(Slot, ArenaTeamInfoType.ARENA_TEAM_PERSONAL_RATING, 0);
            }
        }

        private void OnLeaderDeleted()
        {
            // leader was deleted
            ArenaTeamMember highestMember = null;
            foreach (var member in Members.Values)
            {
                    highestMember = member;
            }

            if (highestMember == null)
            {
                Disband();
            }
            else
            {
                ChangeLeader(highestMember);
            }
        }
        #endregion

        #region IEnumerable<ArenaTeamMember> Members

        public IEnumerator<ArenaTeamMember> GetEnumerator()
        {
            foreach (ArenaTeamMember member in Members.Values)
            {
                yield return member;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (ArenaTeamMember member in Members.Values)
            {
                yield return member;
            }
        }

        #endregion

        #region IChatTarget

        public void SendSystemMsg(string msg)
        {
            foreach (var member in Members.Values)
            {
                if (member.Character != null)
                    member.Character.SendSystemMessage(msg);
            }
        }

        public void SendMessage(string message)
        {
            // TODO: What to do if there is no chatter argument?
            throw new NotImplementedException();
            //ChatMgr.SendGuildMessage(sender, this, message);
        }

        /// <summary>
        /// Say something to this target
        /// </summary>		
        public void SendMessage(IChatter sender, string message)
        {
            //ChatMgr.SendGuildMessage(sender, this, message);
        }
        #endregion

        #region Other
        /// <summary>
        /// Requests member by his low id
        /// </summary>
        /// <param name="lowMemberId">low id of member's character</param>
        /// <returns>requested member or null</returns>
        public ArenaTeamMember this[uint lowMemberId]
        {
            get
            {
                foreach (var member in Members.Values)
                {
                    if (member.Id == lowMemberId)
                        return member;
                }
                
                return null;
            }
        }
        /// <summary>
		/// Requests member by his name
		/// </summary>
		/// <param name="name">name of member's character (not case-sensitive)</param>
		/// <returns>requested member</returns>
		public ArenaTeamMember this[string name]
		{
			get
			{
				name = name.ToLower();

				foreach (var member in Members.Values)
				{
					if (member.Name.ToLower() == name)
						return member;
				}

				return null;
			}
		}

		/// <summary>
		/// Disbands the arena team
		/// </summary>
		/// <param name="update">if true, sends event to the team</param>
		public void Disband()
		{
			m_syncRoot.Enter();
			try
			{
				//ArenaTeamHandler.SendEventToTeam(this, ArenaTeamEvents.DISBANDED_S);

				var members = Members.Values.ToArray();

				foreach (var member in members)
				{
					RemoveMember(member, false);
				}

				ArenaTeamMgr.UnregisterArenaTeam(this);
				RealmServer.Instance.AddMessage(() => Delete());
			}
			finally
			{
				m_syncRoot.Exit();
			}
		}

		/// <summary>
		/// Changes leader of the arena team
		/// </summary>
		/// <param name="newLeader">ArenaTeamMember of new leader</param>
		/// <param name="update">if true, sends event to the team</param>
		public void ChangeLeader(ArenaTeamMember newLeader)
		{
			if (newLeader.ArenaTeam != this)
				return;

			var currentLeader = Leader;
            currentLeader.Character.SetArenaTeamInfoField(Slot, ArenaTeamInfoType.ARENA_TEAM_MEMBER, 1);

			Leader = newLeader;
            newLeader.Character.SetArenaTeamInfoField(Slot, ArenaTeamInfoType.ARENA_TEAM_MEMBER, 0);

			RealmServer.Instance.AddMessage(new Message(() =>
			{
				if (currentLeader != null)
				{
					currentLeader.Update();
				}
				newLeader.Update();
				Update();
			}));

			if (currentLeader != null)
			{
				//ArenaTeamHandler.SendEventToTeam(this, ArenaTeamEvents.LEADER_CHANGED_SSS, newLeader, currentLeader);
			}
        }
        #endregion
    };
}
