using System;
using System.Collections;
using System.Collections.Generic;
using WCell.RealmServer.Entities;
using WCell.RealmServer.AI.Groups;

using AggressorPair = System.Collections.Generic.KeyValuePair<WCell.RealmServer.Entities.Unit, int>;

namespace WCell.RealmServer.AI
{
	/// <summary>
	/// Collection representing threat values of an NPC against its foes
	/// </summary>
	public class ThreatCollection
	{
		/// <summary>
		/// Percentage of the current highest threat to become the new one.
		/// </summary>
		public static int RequiredHighestThreatPct = 110;

		public readonly List<AggressorPair> AggressorPairs;
		private Unit m_CurrentAggressor;
		private int m_highestThreat;
		private Unit m_taunter;

		protected internal AIGroup m_group;

		#region Constructors
		public ThreatCollection()
		{
			AggressorPairs = new List<AggressorPair>(5);
		}

		#endregion

		#region Properties
		/// <summary>
		/// Use this indexer to get or set absolute values of Threat.
		/// Returns -1 for non-aggressor Units.
		/// </summary>
		/// <param name="unit"></param>
		/// <returns></returns>
		public int this[Unit unit]
		{
			get
			{
				foreach (var aggressor in AggressorPairs)
				{
					if (aggressor.Key == unit)
					{
						return aggressor.Value;
					}
				}
				return -1;
			}
			internal set
			{
				if (!unit.CanGenerateThreat)
				{
					return;
				}

				AggressorPair aggressor;
				var index = GetIndex(unit);
				var newIndex = index;

				if (index == -1)
				{
					index = AggressorPairs.Count - 1;
					newIndex = index;

					// moving up
					while (newIndex - 1 >= 0 && AggressorPairs[newIndex - 1].Value < value)
					{
						--newIndex;
					}

					aggressor = new AggressorPair(unit, value);
					AggressorPairs.Insert(newIndex, aggressor);
				}
				else
				{
					aggressor = AggressorPairs[index];
					if (value == aggressor.Value)
					{
						return;
					}

					if (value > aggressor.Value)
					{
						// moving up
						while (newIndex - 1 >= 0 && AggressorPairs[newIndex - 1].Value < value)
						{
							--newIndex;
						}

						AggressorPairs.RemoveAt(index);
						AggressorPairs.Insert(newIndex, new AggressorPair(aggressor.Key, value));
					}
					else
					{
						// moving down
						while (newIndex + 1 < AggressorPairs.Count && AggressorPairs[newIndex + 1].Value < value)
						{
							++newIndex;
						}

						AggressorPairs.Insert(newIndex, new AggressorPair(aggressor.Key, value));
						AggressorPairs.RemoveAt(index);
					}
				}

				if (m_taunter == null)
				{
					// update current aggressor, if there is no taunter
					if (unit == m_CurrentAggressor)
					{
						// updated current aggressor's threat
						if (newIndex == 0)
						{
							// still at the top
							m_highestThreat = value;
						}
						else if (IsNewHighestThreat(AggressorPairs[0].Value))
						{
							// moved down
							m_CurrentAggressor = AggressorPairs[0].Key;
							m_highestThreat = AggressorPairs[0].Value;
							OnNewAggressor(m_CurrentAggressor);
						}
					}
					else if ((newIndex == 0 && IsNewHighestThreat(value)) || m_CurrentAggressor == null)
					{
						// someone who was not the aggressor
						m_CurrentAggressor = unit;
						m_highestThreat = value;
						OnNewAggressor(m_CurrentAggressor);
					}
				}
			}
		}

		public AggressorPair GetThreat(Unit unit)
		{
			foreach (var aggressor in AggressorPairs)
			{
				if (aggressor.Key == unit)
				{
					return aggressor;
				}
			}
			return default(AggressorPair);
		}

		public int GetIndex(Unit unit)
		{
			for (var i = 0; i < AggressorPairs.Count; i++)
			{
				var aggressor = AggressorPairs[i];
				if (aggressor.Key == unit)
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// The NPC is forced to only attack the taunter
		/// </summary>
		public Unit Taunter
		{
			get { return m_taunter; }
			set
			{
				if (value != null)
				{
					// Taunted
					m_taunter = m_CurrentAggressor = value;
					m_highestThreat = int.MaxValue;
					OnNewAggressor(value);
				}
				else
				{
					// Taunt wore off
					m_taunter = null;
					FindNewAggressor();
				}
			}
		}

		public Unit CurrentAggressor
		{
			get { return m_CurrentAggressor; }
		}

		/// <summary>
		/// The AIGroup the owner of this collection belongs to
		/// </summary>
		public AIGroup Group
		{
			get { return m_group; }
			internal set { m_group = value; }
		}

		#endregion

		private void FindNewAggressor()
		{
			if (AggressorPairs.Count == 0)
			{
				m_CurrentAggressor = null;
				m_highestThreat = 0;
			}
			else
			{
				var pair = AggressorPairs[0];
				m_CurrentAggressor = pair.Key;
				m_highestThreat = pair.Value;
				OnNewAggressor(pair.Key);
			}
		}

		/// <summary>
		/// Whether the given amount is at least RequiredHighestThreatPct more than the current highest Threat
		/// </summary>
		/// <param name="threat"></param>
		/// <returns></returns>
		public bool IsNewHighestThreat(int threat)
		{
			// must have 10% more
			return threat > ((m_highestThreat * RequiredHighestThreatPct) + 50) / 100;
		}

		/// <summary>
		/// Call this method when encountering a new Unit
		/// </summary>
		/// <param name="unit"></param>
		public void AddNew(Unit unit)
		{
			this[unit] += 0;
		}

		void OnNewAggressor(Unit unit)
		{
			if (m_group != null)
			{
				m_group.Aggro(unit);
			}
		}

		public void Remove(Unit unit)
		{
			for (var i = 0; i < AggressorPairs.Count; i++)
			{
				var pair = AggressorPairs[i];
				if (pair.Key == unit)
				{
					AggressorPairs.RemoveAt(i);
				}
			}

			if (m_taunter == unit)
			{
				Taunter = null;
			}
			else if (m_CurrentAggressor == unit)
			{
				FindNewAggressor();
			}
		}

		/// <summary>
		/// Removes all Threat
		/// </summary>
		public void Clear()
		{
			AggressorPairs.Clear();
			m_CurrentAggressor = null;
			m_highestThreat = -1;
			m_taunter = null;
		}

		/// <summary>
		/// Returns an array of size 0-max, containing the Units with the highest Threat and their amount.
		/// </summary>
		/// <param name="max"></param>
		public AggressorPair[] GetHighestThreatAggressorPairs(int max)
		{
			var targets = new AggressorPair[Math.Min(max, AggressorPairs.Count)];

			for (var i = targets.Length; i >= 0; i--)
			{
				targets[i] = AggressorPairs[i];
			}

			return targets;
		}

		/// <summary>
		/// Returns an array of size 0-max, containing the Units with the highest Threat.
		/// </summary>
		/// <param name="max"></param>
		public Unit[] GetHighestThreatAggressors(int max)
		{
			var targets = new Unit[Math.Min(max, AggressorPairs.Count)];

			for (var i = targets.Length; i >= 0; i--)
			{
				targets[i] = AggressorPairs[i].Key;
			}

			return targets;
		}
	}
}