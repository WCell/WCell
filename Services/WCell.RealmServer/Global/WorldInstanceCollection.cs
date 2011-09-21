using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WCell.Util;

namespace WCell.RealmServer.Global
{
	public class WorldInstanceCollection<E, M>
		where E : struct, IConvertible
		where M : InstancedMap
	{
		internal M[][] m_instances;
		readonly ReaderWriterLockSlim lck = new ReaderWriterLockSlim();

		private int m_Count;

		public WorldInstanceCollection(E size)
		{
			m_instances = new M[size.ToInt32(null)][];
		}

		public int Count
		{
			get { return m_Count; }
		}

		#region Get
		/// <summary>
		/// Gets an instance
		/// </summary>
		/// <returns>the <see cref="Map" /> object; null if the ID is not valid</returns>s
		public M GetInstance(E mapId, uint instanceId)
		{
			var instances = m_instances.Get(mapId.ToUInt32(null));
			if (instances != null)
			{
				return instances.Get(instanceId);	
			}
			return null;
		}

		public M[] GetInstances(E map)
		{
			var instances = m_instances.Get(map.ToUInt32(null));
			if (instances == null)
			{
				return new M[0];
			}
			return instances.Where(instance => instance != null).ToArray();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="map"></param>
		/// <returns></returns>
		/// <remarks>Never returns null</remarks>
		public M[] GetOrCreateInstances(E map)
		{
			var instances = m_instances.Get(map.ToUInt32(null));
			if (instances == null)
			{
				lck.EnterWriteLock();
				try
				{
					// get again, to make sure that the list was not already created while the lock was being acquired
					instances = m_instances.Get(map.ToUInt32(null));
					m_instances[map.ToUInt32(null)] = instances = new M[10];
				}
				finally
				{
					lck.ExitWriteLock();
				}
			}
			return instances;
		}

		public List<M> GetAllInstances()
		{
			var list = new List<M>();

			lck.EnterReadLock();
			try
			{
				// foreach definitely needs a read lock
				foreach (var instanceSet in m_instances)
				{
					if (instanceSet != null)
					{
						foreach (var instance in instanceSet)
						{
							if (instance != null)
							{
								list.Add(instance);
							}
						}
					}
				}
			}
			finally
			{
				lck.ExitReadLock();
			}
			return list;
		}
		#endregion

		internal void AddInstance(E id, M map)
		{
			var instances = GetOrCreateInstances(id);
			if (map.InstanceId >= instances.Length)
			{
				lck.EnterWriteLock();
				try
				{
					instances = GetOrCreateInstances(id);
					Array.Resize(ref instances, (int)(map.InstanceId * ArrayUtil.LoadConstant));
					m_instances[id.ToUInt32(null)] = instances;
				}
				finally
				{
					lck.ExitWriteLock();
				}
			}
			instances[map.InstanceId] = map;

			Interlocked.Increment(ref m_Count);
		}

		internal void RemoveInstance(E mapId, uint instanceId)
		{
			lck.EnterWriteLock();
			var instances = GetOrCreateInstances(mapId);
			instances[instanceId] = null;

			Interlocked.Decrement(ref m_Count);
		}
	}
}
