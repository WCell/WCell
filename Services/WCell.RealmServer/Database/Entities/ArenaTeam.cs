using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cell.Core;
using WCell.Util.Logging;
using WCell.Constants.ArenaTeams;
using WCell.RealmServer.Battlegrounds.Arenas;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.Util;
using WCell.Util.Collections;
using WCell.Util.Threading;
using NHibernate.Criterion;
using System.Threading;

namespace WCell.RealmServer.Database.Entities
{
	//[ActiveRecord("ArenaTeam", Access = PropertyAccess.Property)]
	public class ArenaTeam : INamed, IEnumerable<ArenaTeamMember>, IChatTarget //WCellRecord<ArenaTeam>
	{
		//private static readonly NHIdGenerator _idGenerator = new NHIdGenerator(typeof(ArenaTeam), "_id");
		private static bool _idGeneratorInitialised;
		private static long _highestId;

		private static void Init()
		{
			//long highestId;
			try
			{
				ArenaTeam highestItem = null;
				highestItem = RealmWorldDBMgr.DatabaseProvider.Session.QueryOver<ArenaTeam>().OrderBy(record => record.Id).Desc.Take(1).SingleOrDefault();
				_highestId = highestItem != null ? highestItem.Id : 0;

				//_highestId = RealmWorldDBMgr.DatabaseProvider.Query<ArenaTeam>().Max(arenaTeam => arenaTeam.Id);
			}
			catch (Exception e)
			{
				RealmWorldDBMgr.OnDBError(e);
				ArenaTeam highestItem = null;
				highestItem = RealmWorldDBMgr.DatabaseProvider.Session.QueryOver<ArenaTeam>().OrderBy(record => record.Id).Desc.Take(1).SingleOrDefault();
				_highestId = highestItem != null ? highestItem.Id : 0;
				//_highestId = RealmWorldDBMgr.DatabaseProvider.Query<ArenaTeam>().Max(arenaTeam => arenaTeam.Id);
			}

			//_highestId = (long)Convert.ChangeType(highestId, typeof(long));

			_idGeneratorInitialised = true;
		}

		/// <summary>
		/// Returns the next unique Id for a new Item
		/// </summary>
		public static long NextId()
		{
			if (!_idGeneratorInitialised)
				Init();

			return Interlocked.Increment(ref _highestId);
		}

		public static long LastId
		{
			get
			{
				if (!_idGeneratorInitialised)
					Init();
				return Interlocked.Read(ref _highestId);
			}
		}

		#region Database Fields
		//[PrimaryKey(PrimaryKeyType.Assigned, "Id")]
		//private long _id;

		//[Field("Name", NotNull = true, Unique = true)]
		//private string _name;

		//[Field("LeaderLowId", NotNull = true)]
		//private int _leaderLowId;

		public virtual uint LeaderLowId { get; set; }

        //[Field("Type", NotNull = true)]
        //private int _type;
		#endregion

		#region Fields
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
		public virtual long Id { get; set; }

		/// <summary>
		/// Arena team's name
		/// </summary>
		/// <remarks>length is limited with MAX_ARENATEAM_LENGTH</remarks>
		public virtual string Name
		{
			get;
			private set;
		}

		/// <summary>
		/// Type of this arena team
		/// </summary>
		public virtual uint Type { get; set; }

        /// <summary>
        /// Slot of this arena team
        /// <summary>
        public virtual ArenaTeamSlot Slot
        {
            get { return m_slot; }
            set { m_slot = value; }
        }

        /// <summary>
        /// Arena team leader's ArenaTeamMember
        /// Setting it does not send event to the team. Use ArenaTeam.SendEvent
        /// </summary>
        public virtual ArenaTeamMember Leader
        {
            get { return m_leader; }
            set
            {
                if (value == null || value.ArenaTeam != this)
                    return;

                m_leader = value;
				LeaderLowId = value.Id;
            }
        }

        /// <summary>
        /// Stats of the arena team
        /// </summary>
        public virtual ArenaTeamStats Stats
        {
            set { m_stats = value; }
            get { return m_stats; }
        }
         
        /// <summary>
        /// Number of arena team's members
        /// </summary>
        public virtual int MemberCount
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
			Id = NextId();
			LeaderLowId = leader.EntityLowId;
			Name = name;
            Type = type;

            m_slot = ArenaMgr.GetSlotByType(type);

			m_leader = new ArenaTeamMember(leader, this, true);
            m_stats = new ArenaTeamStats(this);

			Members.Add(m_leader.Id, m_leader);

			RealmWorldDBMgr.DatabaseProvider.Save(m_leader);
			RealmWorldDBMgr.DatabaseProvider.Save(this);
            Register();
		}
        #endregion

        #region Init
        /// <summary>
        /// Load & initialize the Team
        /// </summary>
        protected internal virtual void InitAfterLoad()
        {
            var members = ArenaTeamMember.FindAll(this);
            foreach (var atm in members)
            {
                atm.Init(this);
                Members.Add(atm.Id, atm);
            }
			m_stats = RealmWorldDBMgr.DatabaseProvider.Query<ArenaTeamStats>().First(ats => ats.Id == Id);
            m_slot = ArenaMgr.GetSlotByType(Type);

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
        protected internal virtual void Register()
        {
            ArenaMgr.RegisterArenaTeam(this);
        }
        #endregion

        #region ArenaTeamMembers
        public virtual ArenaTeamMember AddMember(Character chr)
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
        public virtual ArenaTeamMember AddMember(CharacterRecord chr)
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

				RealmWorldDBMgr.DatabaseProvider.Save(newMember);
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

            ArenaMgr.RegisterArenaTeamMember(newMember);

            //ArenaTeamHandler.SendEventToTeam(this, ArenaTeamEvents.JOINED_SS, newMember);

            return newMember;
        }

        public virtual bool RemoveMember(uint memberId)
        {
            var member = this[memberId];
            if (member != null)
            {
                return RemoveMember(member, true);
            }
            return false;
        }

        public virtual bool RemoveMember(string name)
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
        public virtual bool RemoveMember(ArenaTeamMember member)
        {
            return RemoveMember(member, true);
        }

        /// <summary>
        /// Removes ArenaTeamMember from the arena team
        /// </summary>
        /// <param name="member">member to remove</param>
        /// <param name="update">if false, changes to the team will not be promoted anymore (used when the team is being disbanded)</param>
        public virtual bool RemoveMember(ArenaTeamMember member, bool update)
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

            RealmServer.IOQueue.AddMessage(() =>
            {
				RealmWorldDBMgr.DatabaseProvider.Delete(member);
                if (update)
                {
					RealmWorldDBMgr.DatabaseProvider.SaveOrUpdate(this);
                }
            });
            return true;
        }

        /// <summary>
        /// Called before the given member is removed to clean up everything related to the given member
        /// </summary>
        protected virtual void OnRemoveMember(ArenaTeamMember member)
        {
            ArenaMgr.UnregisterArenaTeamMember(member);

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

        public virtual IEnumerator<ArenaTeamMember> GetEnumerator()
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

        public virtual void SendSystemMsg(string msg)
        {
            foreach (var member in Members.Values)
            {
                if (member.Character != null)
                    member.Character.SendSystemMessage(msg);
            }
        }

        public virtual void SendMessage(string message)
        {
            // TODO: What to do if there is no chatter argument?
            throw new NotImplementedException();
            //ChatMgr.SendGuildMessage(sender, this, message);
        }

        /// <summary>
        /// Say something to this target
        /// </summary>		
        public virtual void SendMessage(IChatter sender, string message)
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
        public virtual ArenaTeamMember this[uint lowMemberId]
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
		public virtual ArenaTeamMember this[string name]
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
		public virtual void Disband()
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

				ArenaMgr.UnregisterArenaTeam(this);
				RealmServer.IOQueue.AddMessage(() => RealmWorldDBMgr.DatabaseProvider.Delete(this));
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
		public virtual void ChangeLeader(ArenaTeamMember newLeader)
		{
			if (newLeader.ArenaTeam != this)
				return;

			var currentLeader = Leader;
            currentLeader.Character.SetArenaTeamInfoField(Slot, ArenaTeamInfoType.ARENA_TEAM_MEMBER, 1);

			Leader = newLeader;
            newLeader.Character.SetArenaTeamInfoField(Slot, ArenaTeamInfoType.ARENA_TEAM_MEMBER, 0);

			RealmServer.IOQueue.AddMessage(new Message(() =>
			{
				if (currentLeader != null)
				{
					RealmWorldDBMgr.DatabaseProvider.SaveOrUpdate(currentLeader);
				}
				RealmWorldDBMgr.DatabaseProvider.SaveOrUpdate(newLeader);
				RealmWorldDBMgr.DatabaseProvider.SaveOrUpdate(this);
			}));

			if (currentLeader != null)
			{
				//ArenaTeamHandler.SendEventToTeam(this, ArenaTeamEvents.LEADER_CHANGED_SSS, newLeader, currentLeader);
			}
        }
        #endregion
    };
}
