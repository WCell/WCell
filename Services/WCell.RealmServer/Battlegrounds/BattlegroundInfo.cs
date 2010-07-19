using WCell.RealmServer.Global;
using WCell.Constants;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Battlegrounds
{
	/// <summary>
	/// Contains all the neccessary battleground information about a player.
	/// </summary>
	public class BattlegroundInfo
	{
		#region Fields
		private Character _chr;
		private BattlegroundRelation[] _relations;
		private BattlegroundInvitation _invitation;
		private int _relationCount;

		private Region _entryRegion;
		private Vector3 _entryPosition;
		private float _entryOrientation;

		private bool _isDeserter;

		private BattlegroundTeam _team;
		private BattlegroundStats _stats;
		#endregion

		/// <summary>
		/// Contains all battlegrounds-related information about a character.
		/// </summary>
		/// <param name="chr"></param>
		public BattlegroundInfo(Character chr)
		{
			_chr = chr;
			_relations = new BattlegroundRelation[BattlegroundMgr.MaxQueuesPerChar];
		}

		#region Properties
		/// <summary>
		/// The character this information is associated with.
		/// </summary>
		public Character Character
		{
			get { return _chr; }
			internal set { _chr = value; }
		}

		/// <summary>
		/// The battlegrounds team this character is associated with.
		/// </summary>
		public BattlegroundTeam Team
		{
			get { return _team; }
			internal set
			{
				_team = value;
				_chr.Record.BattlegroundTeam = value == null ? BattlegroundSide.End : value.Side;
			}
		}

		/// <summary>
		/// Stats of current or last Battleground (or null)
		/// </summary>
		public BattlegroundStats Stats
		{
			get { return _stats; }
			internal set { _stats = value; }
		}

		/// <summary>
		/// Holds the outstanding, if any, invitation to a battleground team.
		/// </summary>
		public BattlegroundInvitation Invitation
		{
			get { return _invitation; }
			internal set
			{
				if (value != _invitation)
				{
					if (value == null)
					{
						_chr.RemoveUpdateAction(_invitation.CancelTimer);
					}

					_invitation = value;
				}
			}
		}

		/// <summary>
		/// The region that this character was originally in before going to the battlegrounds.
		/// </summary>
		public Region EntryRegion
		{
			get { return _entryRegion; }
			set { _entryRegion = value; }
		}

		/// <summary>
		/// The position that this character was originally at before going to the battlegrounds.
		/// </summary>
		public Vector3 EntryPosition
		{
			get { return _entryPosition; }
			set { _entryPosition = value; }
		}

		/// <summary>
		/// The orientation that this character was originally in before going to the battlegrounds.
		/// </summary>
		public float EntryOrientation
		{
			get { return _entryOrientation; }
			set { _entryOrientation = value; }
		}

		/// <summary>
		/// Whether or not this character is considered a deserter. (Deserters cannot join battlegrounds)
		/// </summary>
		public bool IsDeserter
		{
			get { return _isDeserter; }
			set
			{
				_isDeserter = value;

				if (_isDeserter)
				{
					CancelAllRelations();
				}
			}
		}

		/// <summary>
		/// Whether or not this character is enqueued for any battlegrounds.
		/// </summary>
		public bool IsEnqueuedForBattleground
		{
			get
			{
				return _relationCount > 0;
			}
		}

		/// <summary>
		/// The battlegrounds relations for this character, if any.
		/// </summary>
		public BattlegroundRelation[] Relations
		{
			get { return _relations; }
		}

		/// <summary>
		/// The number of current battlegrounds relations.
		/// </summary>
		public int RelationCount
		{
			get { return _relationCount; }
		}

		/// <summary>
		/// Whether or not this character can queue for any more battlegrounds.
		/// </summary>
		public bool HasAvailableQueueSlots
		{
			get { return _relationCount < BattlegroundMgr.MaxQueuesPerChar; }
		}
		#endregion

		/// <summary>
		/// Returns the character to their original location prior to entering the battlegrounds.
		/// </summary>
		public void TeleportBack()
		{
			if (_entryRegion == null || _entryRegion.IsDisposed || _entryPosition.X == 0)
			{
				_chr.TeleportToBindLocation();
			}
			else
			{
				_chr.TeleportTo(_entryRegion, ref _entryPosition, _entryOrientation);
			}

			_entryRegion = null;
		}

		/// <summary>
		/// Sets the entry position of the character.
		/// </summary>
		public void SetCharacterEntry(Region region, ref Vector3 pos, float orientation)
		{
			_entryRegion = region;
			_entryPosition = pos;
			_entryOrientation = orientation;
		}

		/// <summary>
		/// Gets the <see cref="BattlegroundRelation"/> for the given Battleground for
		/// the Character.
		/// </summary>
		/// <param name="bgId"></param>
		/// <returns></returns>
		public BattlegroundRelation GetRelation(BattlegroundId bgId)
		{
			for (var i = 0; i < _relations.Length; i++)
			{
				var request = _relations[i];

				if (request != null && request.Queue.ParentQueue.Template.Id == bgId)
				{
					return request;
				}
			}

			return null;
		}

		public bool IsEnqueuedFor(BattlegroundId bgId)
		{
			var relation = GetRelation(bgId);

			return relation != null && relation.IsEnqueued;
		}

		public bool CancelIfEnqueued(BattlegroundId bgId)
		{
			_chr.ContextHandler.EnsureContext();

			for (var i = 0; i < _relations.Length; i++)
			{
				var request = _relations[i];
				if (request != null && request.Queue.ParentQueue.Template.Id == bgId)
				{
					if (request.IsEnqueued)
					{
						request.Cancel();
						return true;
					}
					return false;
				}
			}

			return false;
		}

		public void Cancel(BattlegroundInvitation invite)
		{
			RemoveRelation(invite.QueueIndex);
		}

		public int RemoveRelation(BattlegroundRelation relation)
		{
			return RemoveRelation(relation.BattlegroundId);
		}

		/// <summary>
		/// Removes the <see cref="BattlegroundRelation"/> for the given Battleground.
		/// This also cancels invitations and leaves the Battleground.
		/// If it was a Queue request for the Group and this is the GroupLeader, it also
		/// removes everyone else from the Queue.
		/// </summary>
		/// <param name="id"></param>
		/// <returns>The index of the removed relation or -1 if none removed</returns>
		public int RemoveRelation(BattlegroundId id)
		{
			_chr.EnsureContext();

			for (var i = 0; i < _relations.Length; i++)
			{
				var relation = _relations[i];
				if (relation != null && relation.Queue.ParentQueue.Template.Id == id)
				{
					RemoveRelation(i, relation, true);
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Removes the corresponding relation and removes it from the queue
		/// if it is enqueued.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public int CancelRelation(BattlegroundId id)
		{
			_chr.EnsureContext();

			for (var i = 0; i < _relations.Length; i++)
			{	
				var relation = _relations[i];
				if (relation != null && relation.Queue.ParentQueue.Template.Id == id)
				{
					CancelRelation(i, relation, true);
					return i;
				}
			}
			return -1;
		}

		public int CancelRelation(int index, BattlegroundRelation relation, bool charActive)
		{
			_chr.EnsureContext();

			var queue = relation.Queue;
			if (queue != null)
			{
				queue.Remove(relation);
			}

			RemoveRelation(index, relation, charActive);

			return index;
		}

		/// <summary>
		/// Make sure the given index is valid
		/// </summary>
		/// <param name="index"></param>
		public void RemoveRelation(int index)
		{
			RemoveRelation(index, _relations[index], true);
		}

		internal void RemoveRelation(int index, BattlegroundRelation relation, bool isCharActive)
		{
			_chr.EnsureContext();
			_relations[index] = null;

			var invite = Invitation;
			if (invite != null)
			{
				// cancel reserved slot
				invite.Team.ReservedSlots--;
				Invitation = null;
			}

			var bgId = relation.BattlegroundId;
			var bg = _chr.Region as Battleground;
			if (bg != null &&
				bg.Template.Id == bgId &&
				!relation.IsEnqueued &&
				!_chr.IsTeleporting &&
				isCharActive)
			{
				bg.TeleportOutside(_chr);
			}

			if (isCharActive)
			{
				BattlegroundHandler.ClearStatus(_chr, index);
			}

			if (relation.IsEnqueued && relation.Characters.Count > 1)
			{
				var group = _chr.Group;
				if (group != null && group.IsLeader(_chr))
				{
					// also dequeue everyone else
					relation.Characters.ForeachCharacter(chr =>
					{
						if (chr != _chr)
						{
							chr.ExecuteInContext(() =>
							{
								chr.Battlegrounds.RemoveRelation(bgId);
							});
						}
					});
				}
			}
		}

		/// <summary>
		/// Invites this Character to the given Battleground or enqueues him/her.
		/// </summary>
		internal void InviteTo(BattlegroundTeam team)
		{
			var index = GetIndex(team.Battleground.Template.Id);
			BattlegroundRelation relation;
			if (index == -1)
			{
				index = AddRelation(relation = new BattlegroundRelation(team.Queue, _chr, false));
			}
			else
			{
				relation = _relations[index];
			}
			InviteTo(team, index, relation);
		}

		internal void InviteTo(BattlegroundTeam team, int queueIndex, BattlegroundRelation relation)
		{
			_chr.EnsureContext();

			relation.IsEnqueued = false;
			var bg = team.Battleground;
			var invite = new BattlegroundInvitation(team, queueIndex);
			_chr.Battlegrounds.Invitation = invite;

			invite.CancelTimer = _chr.CallDelayed(BattlegroundMgr.InvitationTimeoutMillis, obj =>
			{
				RemoveRelation(bg.Template.Id);
			});

			BattlegroundHandler.SendStatusInvited(_chr);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="relation"></param>
		/// <returns>The index of the newly created relation</returns>
		public int AddRelation(BattlegroundRelation relation)
		{
			_chr.EnsureContext();

			var index = (int)ArrayUtil.AddOnlyOne(ref _relations, relation);

			_relationCount++;
			var queue = relation.Queue.ParentQueue;
			if (queue != null)
			{
				BattlegroundHandler.SendStatusEnqueued(_chr, index, relation, queue);
			}

			return index;
		}

		public int GetIndex(BattlegroundRelation request)
		{
			var queue = request.Queue.ParentQueue;
			if (queue != null)
			{
				return GetIndex(queue.Template.Id);
			}

			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		/// <remarks>Requires Context</remarks>
		public int GetIndex(BattlegroundId id)
		{
			_chr.ContextHandler.EnsureContext();

			for (var i = 0; i < _relations.Length; i++)
			{
				var request = _relations[i];
				if (request != null && request.Queue.ParentQueue.Template.Id == id)
				{
					return i;
				}
			}

			return -1;
		}

		/// <summary>
		/// Cancel all relations
		/// </summary>
		public void CancelAllRelations()
		{
			for (var i = 0; i < _relations.Length; i++)
			{
				var relation = _relations[i];
				if (relation != null)
				{
					CancelRelation(i, relation, false);
				}
			}
		}

		internal void OnLogout()
		{
			CancelAllRelations();
		}

		public bool IsParticipating(BattlegroundId bgId)
		{
			if (Team != null)
			{
				return Team.Battleground.Template.Id == bgId;
			}
			return false;
		}
	}
}