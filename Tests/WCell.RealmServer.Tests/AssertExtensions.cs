using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.RealmServer.World;
using WCell.Core.Threading;
using System.Threading;
using WCell.RealmServer.Entities;
using Microsoft.Xna.Framework;
using WCell.Core;
using WCell.RealmServer.Debug;

namespace WCell.RealmServer.Tests
{
	public static class Asser
	{
		public static void GreaterThan(IComparable greater, IComparable smaller)
		{
			GreaterThan(greater, smaller, "{0} is not > {1}", greater, smaller);
		}

		public static void GreaterThan(IComparable greater, IComparable smaller, string message, params object[] args)
		{
			Assert.IsTrue(greater.CompareTo(smaller) > 0, string.Format(message, args));
		}

		public static void GreaterOrEqual(IComparable greater, IComparable smaller)
		{
			GreaterOrEqual(greater, smaller, "{0} is not >= {1}", greater, smaller);
		}

		public static void GreaterOrEqual(IComparable greater, IComparable smaller, string message, params object[] args)
		{
			Assert.IsTrue(greater.CompareTo(smaller) >= 0, string.Format(message, args));
		}


		public static void LessOrEqual(IComparable smaller, IComparable greater)
		{
			LessOrEqual(greater, smaller, "{0} is not <= {1}", greater, smaller);
		}

		public static void LessOrEqual(IComparable smaller, IComparable greater, string message, params object[] args)
		{
			Assert.IsTrue(smaller.CompareTo(greater) <= 0, string.Format(message, args));
		}

		/// <summary>
		/// Assert that the given value does not have the given flag set.
		/// </summary>
		public static void FlatNotSet<T>(T value, T flag)
			where T : struct
		{
			var longValue = (uint)Convert.ChangeType(value, typeof(uint));
			var longFlag = (uint)Convert.ChangeType(flag, typeof(uint));
			Assert.AreEqual((uint)0, longValue & longFlag, "Flag {0} is set in value {1}", flag, value);
		}

		/// <summary>
		/// Assert that the given value does not have the given flag set.
		/// </summary>
		public static void FlatNotSet<T>(T value, T flag, string msg, params object[] args)
			where T : struct
		{
			var longValue = (uint)Convert.ChangeType(value, typeof(uint));
			var longFlag = (uint)Convert.ChangeType(flag, typeof(uint));
			Assert.AreEqual((uint)0, longValue & longFlag, string.Format(msg, args));
		}

		/// <summary>
		/// Assert that the given value does not have the given flag set.
		/// </summary>
		public static void FlatIsSet<T>(T value, T flag)
			where T : struct
		{
			var longValue = (uint)Convert.ChangeType(value, typeof(uint));
			var longFlag = (uint)Convert.ChangeType(flag, typeof(uint));
			Assert.AreNotEqual((uint)0, longValue & longFlag, "Flag {0} is not set in Value {1}", flag, value);
		}

		/// <summary>
		/// Assert that the given value does not have the given flag set.
		/// </summary>
		public static void FlatIsSet<T>(T value, T flag, string msg, params object[] args)
			where T : struct
		{
			var longValue = (uint)Convert.ChangeType(value, typeof(uint));
			var longFlag = (uint)Convert.ChangeType(flag, typeof(uint));
			Assert.AreNotEqual((uint)0, longValue & longFlag, string.Format(msg, args));
		}

		#region Regions
		/// <summary>
		/// Adds the given Object to this Region in the next Region tick.
		/// </summary>
		/// <param name="region"></param>
		/// <param name="obj"></param>
		/// <param name="newPos"></param>
		public static void MoveObjectAndWait(this Region region, WorldObject obj, Vector3 newPos)
		{
			region.Start();	// make sure, Region is running
			Assert.IsTrue(region.Running);

			// We can't add something that we already have
			var obj2 = region.GetObject(obj.EntityId);
			Assert.IsNull(obj2, "Object {0} was already added to Region {1}", obj, region);

			region.Start();

			Message2<WorldObject, Vector3> moveTask =
				new Message2<WorldObject, Vector3>(obj, newPos);

			if (obj.Region != null)
			{
				obj.Region.RemoveObjectInstantly(obj);
			}

			moveTask.Callback = (worldObj, objLocation) => {
				if (worldObj is Character)
				{
					var chr = (Character)worldObj;
					chr.ResetInitialObjectKnowledge();
					Assert.IsFalse(chr.KnowsOf(chr));
				}

				region.AddObjectInstantly(worldObj, ref objLocation);
				lock (moveTask)
				{
					Monitor.PulseAll(moveTask);
				}

				Assert.IsTrue(region.Running);
			};
			region.AddMessage(moveTask);

			bool added = false;
			int delay = region.GetWaitDelay();
			lock (moveTask)
			{
				Monitor.Wait(moveTask, delay);
				Assert.IsTrue(added, "Object {0} was not added to Region after " + delay + " milliseconds.", obj);
			}
			added = true;

			Assert.AreEqual(obj.Region, region);
		}
		#endregion
	}
}