using System.Collections.Generic;
using System.Threading;
using Cell.Core.Collections;

namespace WCell.RealmServer.Misc
{
	/// <summary>
	/// Allows reusable uint Ids
	/// </summary>
	public class IdQueue
	{
		private int m_currentId;
		private readonly LockfreeQueue<uint> m_freeIds;

		public IdQueue()
		{
			m_freeIds = new LockfreeQueue<uint>();
		}

		public uint NextId()
		{
			if (m_freeIds.Count > 0)
			{
				return m_freeIds.Dequeue();
			}
			else
			{
				return (uint)Interlocked.Increment(ref m_currentId);
			}
		}

		public void RecycleId(uint id)
		{
			m_freeIds.Enqueue(id);
		}

		public void Load(List<uint> usedIds)
		{
			uint maxId = 0;

			for (uint i = 0; i < usedIds.Count; i++)
			{
				if (!usedIds.Contains(i))
				{
					RecycleId(i);
				}
				else
				{
					if (i > maxId)
						maxId = i;
				}
			}
		}
	}
}
