using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Instances
{
	/// <summary>
	/// Represents a Group or single Character (who can own an instance)
	/// </summary>
	public interface IInstanceHolderSet// : ICharacterSet
	{
		/// <summary>
		/// The leader
		/// </summary>
		Character InstanceLeader { get; }

		/// <summary>
		/// The InstanceInfo of the leader
		/// </summary>
		InstanceCollection InstanceLeaderCollection { get; }

		/// <summary>
		/// Iterates over all the InstanceInfo of all Characters in the set
		/// </summary>
		void ForeachInstanceHolder(Action<InstanceCollection> callback);

		/// <summary>
		/// Gets the Instance of the given Map that this Holder already holds
		/// or null.
		/// </summary>
		/// <returns></returns>
		BaseInstance GetActiveInstance(RegionInfo region);
	}
}