using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WCell.Util;

namespace WCell.RealmServer.Global
{
    public class WorldInstanceCollection<TE, TM>
        where TE : struct, IConvertible
        where TM : InstancedMap
    {
        internal TM[][] Instances;
        readonly ReaderWriterLockSlim _lck = new ReaderWriterLockSlim();

        private int _count;

        public WorldInstanceCollection(TE size)
        {
            Instances = new TM[size.ToInt32(null)][];
        }

        public int Count
        {
            get { return _count; }
        }

        #region Get

        /// <summary>
        /// Gets an instance
        /// </summary>
        /// <returns>the <see cref="Map" /> object; null if the ID is not valid</returns>s
        public TM GetInstance(TE mapId, uint instanceId)
        {
            var instances = Instances.Get(mapId.ToUInt32(null));
            return instances != null ? instances.Get(instanceId) : null;
        }

        public TM[] GetInstances(TE map)
        {
            var instances = Instances.Get(map.ToUInt32(null));
            if (instances == null)
            {
                return new TM[0];
            }
            return instances.Where(instance => instance != null).ToArray();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        /// <remarks>Never returns null</remarks>
        public TM[] GetOrCreateInstances(TE map)
        {
            var instances = Instances.Get(map.ToUInt32(null));
            if (instances != null)
                return instances;

            _lck.EnterWriteLock();
            try
            {
                // get again, to make sure that the list was not already created while the lock was being acquired
                Instances[map.ToUInt32(null)] = instances = new TM[10];
            }
            finally
            {
                _lck.ExitWriteLock();
            }
            return instances;
        }

        public List<TM> GetAllInstances()
        {
            var list = new List<TM>();

            _lck.EnterReadLock();
            try
            {
                // foreach definitely needs a read lock
                foreach (var instanceSet in Instances)
                {
                    if (instanceSet != null)
                    {
                        list.AddRange(instanceSet.Where(instance => instance != null));
                    }
                }
            }
            finally
            {
                _lck.ExitReadLock();
            }
            return list;
        }

        #endregion Get

        internal void AddInstance(TE id, TM map)
        {
            var instances = GetOrCreateInstances(id);
            if (map.InstanceId >= instances.Length)
            {
                _lck.EnterWriteLock();
                try
                {
                    instances = GetOrCreateInstances(id);
                    Array.Resize(ref instances, (int)(map.InstanceId * ArrayUtil.LoadConstant));
                    Instances[id.ToUInt32(null)] = instances;
                }
                finally
                {
                    _lck.ExitWriteLock();
                }
            }
            instances[map.InstanceId] = map;

            Interlocked.Increment(ref _count);
        }

        internal void RemoveInstance(TE mapId, uint instanceId)
        {
            _lck.EnterWriteLock();
            try
            {
                var instances = GetOrCreateInstances(mapId);
                instances[instanceId] = null;

                --_count;
            }
            finally
            {
                _lck.ExitWriteLock();
            }
        }
    }
}
