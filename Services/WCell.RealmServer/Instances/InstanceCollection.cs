using System;
using System.Collections.Generic;
using Cell.Core;
using WCell.Constants;
using WCell.Constants.World;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using NLog;
using WCell.RealmServer.Chat;

namespace WCell.RealmServer.Instances
{
	public enum BindingType
	{
		/// <summary>
		/// Normal, resettable
		/// </summary>
		Soft,
		/// <summary>
		/// Heroic and Raids
		/// </summary>
		Hard,
		End
	}

	/// <summary>
	/// Manages all Instance-relations of a Character.
	/// </summary>
	public class InstanceCollection
	{
		public readonly ObjectPool<List<InstanceBinding>> InstanceBindingListPool = new ObjectPool<List<InstanceBinding>>(() => new List<InstanceBinding>(4));

		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Our character id
		/// </summary>
		private uint m_characterId;

		/// <summary>
		/// Our character
		/// </summary>
		private Character m_character;

		/// <summary>
		/// Bindings of normal, resettable instances
		/// </summary>
		private readonly List<InstanceBinding>[] m_bindings;

		public InstanceCollection(Character player)
			: this(player.EntityId.Low)
		{
			m_bindings = new List<InstanceBinding>[(int)BindingType.End];
			m_character = player;
		}

		public InstanceCollection(uint lowId)
		{
			m_characterId = lowId;
		}

		#region Properties
		public bool HasFreeInstanceSlot
		{
			get
			{
				var bindings = m_bindings[(int)BindingType.Soft];
				if (bindings == null)
				{
					return true;
				}

				if (bindings.Count < InstanceMgr.MaxInstancesPerHour)
					return true;

				RemoveExpiredSoftBindings();
				return (bindings.Count < InstanceMgr.MaxInstancesPerHour);
			}
		}

		public Character Character
		{
			get { return m_character; }
			internal set
			{
				m_character = value;
				if (m_character != null)
				{
					m_characterId = m_character.EntityId.Low;
				}
			}
		}

		/// <summary>
		/// EntityId.Low of the Owner of this log
		/// </summary>
		public uint CharacterId
		{
			get { return m_characterId; }
		}

		#endregion

		public void ClearBindings()
		{
			foreach (var bindings in m_bindings)
			{
				if (bindings != null)
				{
					lock (bindings)
					{
						bindings.Clear();
					}
				}
			}
		}

		/// <summary>
		/// Binds the instance
		/// </summary>
		public void BindTo(BaseInstance instance)
		{
			var bindings = GetOrCreateBindingList(instance.Difficulty.BindingType);
			lock (bindings)
			{
				if (bindings.Count >= InstanceMgr.MaxInstancesPerHour)
				{
					log.Error("{0} was saved to \"{1}\" but exceeded the MaxInstancesPerCharPerHour limit.",
							  m_character, instance);
				}
				bindings.Add(new InstanceBinding(instance.InstanceId, instance.Id, instance.Difficulty.Index));
			}
		}

		/// <summary>
		/// Returns the Cooldown object for the Instance with the given MapId.
		/// </summary>
		/// <param name="map">The MapId of the Instance in question.</param>
		/// <returns></returns>
		public InstanceBinding GetBinding(MapId map, BindingType type)
		{
			var bindings = m_bindings[(int)type];
			if (bindings == null)
			{
				return null;
			}

			lock (bindings)
			{
				foreach (var binding in bindings)
				{
					if (binding.RegionId == map)
					{
						return binding;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Checks the list of stored Raid and Heroic instances and the list of recently run Normal 
		/// instances for a reference to the given region.
		/// </summary>
		/// <param name="template">The RegionInfo of the Instance in question.</param>
		/// <returns>The Instance if found, else null.</returns>
		public BaseInstance GetActiveInstance(RegionTemplate template)
		{
			var chr = Character;
			if (chr == null)
			{
				return null;
			}

			var binding = GetBinding(template.Id, template.GetDifficulty(chr.GetInstanceDifficulty(template.IsRaid)).BindingType);
			if (binding != null)
			{
				var instance = World.GetInstance(binding);
				if (instance.IsActive)
				{
					return instance as BaseInstance;
				}
			}
			return null;
		}

		/// <summary>
		/// Tries to reset all owned Instances.
		/// Requires to be in Character's context if online.
		/// </summary>
		public bool TryResetInstances()
		{
			var chr = Character;

			// TODO: 
			//lock (m_HardBindings)
			//{
			//    if (chr != null)
			//    {
			//        for (var i = 0; i < m_HardBindings.Count; i++)
			//        {
			//            var instance = m_HardBindings[i].Instance;
			//            if (instance != null && !instance.CanReset(chr))
			//            {
			//                InstanceHandler.SendResetFailure(Character, instance.Id, InstanceResetFailed.PlayersInside);
			//                return false;
			//            }
			//        }
			//    }

			//    for (var i = 0; i < m_HardBindings.Count; i++)
			//    {
			//        var instance = m_HardBindings[i].Instance;
			//        if (instance != null)
			//        {
			//            instance.Reset();
			//        }
			//    }
			//}
			return true;
		}

		/// <summary>
		/// Sends the list of Raids completed and In progress and when they will reset.
		/// </summary>
		public void SendRaidTimes()
		{
			if (Character != null)
			{
				SendRaidTimes(Character);
			}
		}

		/// <summary>
		/// Sends the list of Raids completed and In progress and when they will reset.
		/// </summary>
		public void SendRaidTimes(IChatTarget listener)
		{
			var bindings = m_bindings[(int)BindingType.Hard];
			if (bindings != null)
			{
				foreach (var binding in bindings)
				{
					listener.SendMessage("Raid {0} #{1}, Until: {1}", binding.RegionId, binding.InstanceId, binding.NextResetTime);
				}
			}
		}

		/// <summary>
		/// Warning: Requires Character to be logged in and to be in Character's context.
		/// Often you might want to use ForeachBinding() instead.
		/// </summary>
		public void ForeachBinding(BindingType type, Action<InstanceBinding> callback)
		{
			var bindings = m_bindings[(int)type];
			if (bindings != null)
			{
				lock (bindings)
				{
					foreach (var binding in bindings)
					{
						callback(binding);
					}
				}
			}
		}

		/// <summary>
		/// Updates the List of stored InstanceCooldowns, removing the expired ones.
		/// </summary>
		private void RemoveExpiredSoftBindings()
		{
			var bindings = m_bindings[(int)BindingType.Soft];
			if (bindings == null)
			{
				return;
			}

			lock (bindings)
			{
				for (var i = bindings.Count - 1; i >= 0; i--)
				{
					var binding = bindings[i];
					if (binding.BindTime.AddMinutes(InstanceMgr.DungeonExpiryMinutes) > DateTime.Now)
					{
						bindings.RemoveAt(i);
						binding.DeleteLater();
					}
				}
			}
		}

		private List<InstanceBinding> GetOrCreateBindingList(BindingType type)
		{
			lock (m_bindings)
			{
				var bindings = m_bindings[(int)type];
				if (bindings == null)
				{
					m_bindings[(int)type] = bindings = InstanceBindingListPool.Obtain();
				}
				return bindings;
			}
		}

		internal void Dispose()
		{
			foreach (var list in m_bindings)
			{
				if (list != null)
				{
					InstanceBindingListPool.Recycle(list);
				}
			}
		}
	}
}