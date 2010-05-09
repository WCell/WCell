using System;
using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Battlegrounds
{
	public interface IBattlegroundRelation
	{
		bool IsEnqueued { get; }

		int Count { get; }

		BattlegroundTeamQueue Queue { get; }

		ICharacterSet Characters { get; }
	}

	/// <summary>
	/// Represents the relation between one or multiple Character and a Battleground.
	/// This is enqueued in the <see cref="BattlegroundQueue"/>s once a Character
	/// requests to join one.
	/// Each Character maintains the relation during the entire stay in a <see cref="Battleground"/>.
	/// </summary>
	public class BattlegroundRelation : IBattlegroundRelation
	{
		private readonly DateTime _created;
		private readonly ICharacterSet _participants;
		private readonly BattlegroundTeamQueue _queue;

		public BattlegroundRelation(BattlegroundTeamQueue queue, ICharacterSet participants) :
			this(queue, participants, true)
		{
		}

		public BattlegroundRelation(BattlegroundTeamQueue queue, ICharacterSet participants, bool isEnqueued)
		{
			_queue = queue;
			_participants = participants;
			IsEnqueued = isEnqueued;
			_created = DateTime.Now;
		}

		public DateTime Created
		{
			get { return _created; }
		}

		public BattlegroundId BattlegroundId
		{
			get
			{
				BattlegroundQueue queue = _queue.ParentQueueBase;
				if(queue != null)
				{
					return queue.Template.Id;
				}

				return BattlegroundId.End;
			}
		}

		public TimeSpan QueueTime
		{
			get { return DateTime.Now - _created; }
		}

		#region IBattlegroundRelation Members

		/// <summary>
		/// Whether this is still enqueued
		/// </summary>
		public bool IsEnqueued { get; internal set; }

		public int Count
		{
			get { return Characters.Count; }
		}

		public BattlegroundTeamQueue Queue
		{
			get { return _queue; }
		}

		public ICharacterSet Characters
		{
			get { return _participants; }
		}

		#endregion

		internal void Cancel()
		{
			_participants.ForeachCharacter(chr => { chr.Battlegrounds.CancelIfEnqueued(BattlegroundId); });
		}
	}
}