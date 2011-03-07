using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Instances;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;
using WCell.RealmServer.Handlers;
using WCell.Core.Timers;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Battlegrounds
{
	/// <summary>
	/// Represents a team in a Battleground
	/// </summary>
	public class BattlegroundTeam : ICharacterSet
	{
		public readonly BattlegroundTeamQueue Queue;
		public readonly BattlegroundSide Side;

		private Battleground _battleground;
		private int _count;
		private List<Character> _members = new List<Character>();
		private Vector3 _startPosition;
		private float _startOrientation;
		private int _reservedSlots;

		public BattlegroundTeam(BattlegroundTeamQueue queue, BattlegroundSide side, Battleground battleground)
		{
			Queue = queue;
			Side = side;
			_battleground = battleground;

			var templ = battleground.Template;
			if (side == BattlegroundSide.Alliance)
			{
				_startPosition = templ.AllianceStartPosition;
				_startOrientation = templ.AllianceStartOrientation;
			}
			else
			{
				_startPosition = templ.HordeStartPosition;
				_startOrientation = templ.HordeStartOrientation;
			}
		}

		/// <summary>
		/// Amount of reserved slots.
		/// This is used to hold slots open for invited Players.
		/// </summary>
		public int ReservedSlots
		{
			get { return _reservedSlots; }
			set { _reservedSlots = value; }
		}

		/// <summary>
		/// The <see cref="BattlegroundTeam"/> which currently fights against this one
		/// in a <see cref="Battleground"/>
		/// </summary>
		public BattlegroundTeam OpposingTeam
		{
			get { return _battleground.GetTeam(Side.GetOppositeSide()); }
		}

		public Vector3 StartPosition
		{
			get { return _startPosition; }
		}

		public float StartOrientation
		{
			get { return _startOrientation; }
		}

		public Battleground Battleground
		{
			get { return _battleground; }
		}

		/// <summary>
		/// A full team has reached its max player count: <see cref="BattlegroundTemplate.MaxPlayersPerTeam"/>
		/// </summary>
		public bool IsFull
		{
			get { return CharacterCount + _reservedSlots >= _battleground.Template.MaxPlayersPerTeam; }
		}

		/// <summary>
		/// The amount of all Players in this team, including offline ones
		/// </summary>
		public int TotalCount
		{
			get { return _members.Count; }
		}

		/// <summary>
		/// Amount of available slots (can be negative)
		/// </summary>
		public int OpenPlayerSlotCount
		{
			get
			{
				if (_battleground.AddPlayersToBiggerTeam)
				{
					return _battleground.Template.MaxPlayersPerTeam - CharacterCount - _reservedSlots;
				}
				else
				{
					return OpposingTeam.CharacterCount - CharacterCount - _reservedSlots;
				}
			}
		}

		#region ICharacterSet
		/// <summary>
		/// The amount of online Characters
		/// </summary>
		public int CharacterCount
		{
			get { return _count; }
		}

		public FactionGroup FactionGroup
		{
			get { return Side.GetFactionGroup(); }
		}

		/// <summary>
		/// Iterates over all online Characters in this team
		/// </summary>
		/// <param name="callback"></param>
		public void ForeachCharacter(Action<Character> callback)
		{
			_battleground.EnsureContext();

			foreach (var member in _members)
			{
				callback(member);
			}
		}

		public Character[] GetAllCharacters()
		{
			_battleground.EnsureContext();

			return _members.ToArray();
		}
		#endregion

		public void AddMember(Character chr)
		{
			BattlegroundHandler.SendPlayerJoined(this, chr);
			chr.Battlegrounds.Team = this;
			_members.Add(chr);
			_count++;
		}

		public void RemoveMember(Character chr)
		{
			BattlegroundHandler.SendPlayerLeft(this, chr);
			chr.Battlegrounds.Team = null;
			_members.Remove(chr);
			_count--;
		}

		/// <summary>
		/// Distributes honor to Teammates within 40 yards of honorable kill.
		/// </summary>
		/// <param name="earner">Character that made the honorable kill.</param>
		/// <param name="honorPoints">Honor earned by the earner.</param>
		public void DistributeSharedHonor(Character earner, Character victim, uint honorPoints)
		{
			if (TotalCount < 1) return;
			var bonus = honorPoints / (uint)TotalCount;

			ForeachCharacter((chr) =>
			{
				if (chr.IsInRange(new SimpleRange(0.0f, 40.0f), earner))
				{
					chr.GiveHonorPoints(bonus);
					chr.KillsToday++;
					chr.LifetimeHonorableKills++;
					HonorHandler.SendPVPCredit(chr, bonus * 10, victim);
				}
			});
		}

		#region Queue
		/// <summary>
		/// Make sure that Battleground.HasQueue is true before calling this method.
		/// Adds the given set of Characters to this team's queue. 
		/// Will invite immediately if there are enough open slots and
		/// <see cref="WCell.RealmServer.Battlegrounds.Battleground.IsAddingPlayers"/> is true.
		/// </summary>
		/// <param name="chrs"></param>
		public BattlegroundRelation Enqueue(ICharacterSet chrs)
		{
			_battleground.EnsureContext();

			var relation = new BattlegroundRelation(Queue, chrs);
			var shouldInvite = _battleground.IsAddingPlayers && chrs.CharacterCount <= OpenPlayerSlotCount;

			if (!shouldInvite)
			{
				Queue.Enqueue(relation);
			}
			else
			{
				ReservedSlots += chrs.CharacterCount;
				relation.IsEnqueued = false;
			}

			chrs.ForeachCharacter(chr => chr.ExecuteInContext(() =>
			{
				var index = chr.Battlegrounds.AddRelation(relation);
				if (shouldInvite)
				{
					chr.Battlegrounds.InviteTo(this, index, relation);
				}
			}));

			return relation;
		}

		public int Invite(ICharacterSet chrs)
		{
			var added = 0;
			ReservedSlots += chrs.CharacterCount;

			chrs.ForeachCharacter(chr =>
			{
				if (chr.IsInWorld)
				{
					chr.ExecuteInContext(() => { chr.Battlegrounds.InviteTo(this); });
					++added;
				}
			});
			return added;
		}
		#endregion

		public void Send(RealmPacketOut packet)
		{
			ForeachCharacter(chr => chr.Send(packet));
		}

		public override string ToString()
		{
			return Side + " (" + (_count + _reservedSlots) + "/" + _battleground.Template.MaxPlayersPerTeam + ")";
		}

		public void Dispose()
		{
			_members = null;
			_battleground = null;
		}
	}
}