using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.Constants.Updates;
using WCell.RealmServer.Global;
using WCell.Util.Graphics;
using WCell.Util.Threading;
using System.Threading;
using WCell.RealmServer.Entities;
using WCell.Constants;
using WCell.RealmServer.Debugging;

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

		public static void InBetween(IComparable min, IComparable max, IComparable value)
		{
			Assert.IsTrue(min.CompareTo(max) < 0, "Min ({0}) is not smaller than Max ({1})", min, max);
			Assert.IsTrue(min.CompareTo(value) <= 0, "value ({0}) is not >= Min ({1})", value, min);
			Assert.IsTrue(max.CompareTo(value) >= 0, "value ({0}) is not <= Max ({1})", value, max);
		}

		public static void InBetween(IComparable min, IComparable max, IComparable value, string msg, params object[] args)
		{
			msg = string.Format(msg, args);
			Assert.IsTrue(min.CompareTo(value) <= 0, msg);
			Assert.IsTrue(max.CompareTo(value) >= 0, msg);
		}

		public static void Approx(long expected, long delta, long value)
		{
			InBetween(expected - delta, expected + delta, value);
		}

		public static void Approx(long expected, long delta, long value, string msg, params object[] args)
		{
			InBetween(expected - delta, expected + delta, value, msg, args);
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

		#region Maps
		/// <summary>
		/// Adds the given Object to this Map in the next Map tick.
		/// </summary>
		/// <param name="map"></param>
		/// <param name="obj"></param>
		/// <param name="newPos"></param>
		public static void TransferObject(this Map map, WorldObject obj, Vector3 newPos, bool wait)
		{
			if (obj.Map == map)
			{
				// already in that map
				return;
			}

			var moveTask =
				new Message2<WorldObject, Vector3>(obj, newPos);

			var transferMsg = new Message(() => {
				moveTask.Callback = (worldObj, objLocation) => {
					map.AddObjectNow(worldObj, objLocation);
					if (wait)
					{
						lock (moveTask)
						{
							Monitor.PulseAll(moveTask);
						}
					}

					Assert.IsTrue(map.IsRunning);
				};

				if (wait)
				{
					map.AddMessageAndWait(moveTask);
				}
				else
				{
					map.AddMessage(moveTask);
				}

				Assert.IsTrue(map.IsRunning);

				int delay = map.GetWaitDelay();
				if (wait)
				{
					lock (moveTask)
					{
						Monitor.Wait(moveTask, delay);
						Assert.AreEqual(map, obj.Map, "Object {0} was not added to Map after " + delay + " milliseconds.", obj);
					}
					Assert.AreEqual(obj.Map, map);
				}
			});

			var oldMap = obj.Map;
			if (oldMap != null)
			{
				// object is still in map -> Remove first
				Action removeTask = () => {
					obj.Map.RemoveObjectNow(obj);
					transferMsg.Execute();
				};

				var context = obj.ContextHandler;
				if (wait)
				{
					if (context != null)
						oldMap.AddMessageAndWait(new Message(removeTask));
				}
				else
				{
					if (context != null)
						context.AddMessage(removeTask);
				}
			}
			else
			{
				// object is not in world, can be moved instantly (not entirely true, but lets assume so for now)
				transferMsg.Execute();
			}
		}
		#endregion

		#region WorldObject
		/// <summary>
		///  Ensures that this Character is in the world (at some default location, if not already in world)
		/// </summary>
		public static void EnsureInWorld(this WorldObject obj)
		{
			Setup.EnsureInWorld(obj);
			Assert.IsNotNull(obj.Map);
			Assert.IsTrue(obj.Map.IsRunning);
		}

		/// <summary>
		/// Ensures that this Character is alive
		/// </summary>
		public static void EnsureLiving(this Unit unit)
		{
			unit.SetUInt32(UnitFields.BASE_HEALTH, 1000);
			unit.SetUInt32(UnitFields.HEALTH, 1000);

			GreaterThan(unit.Health, 0);
		}

		/// <summary>
		/// Ensures that the unit has the given amount of baseHealth and Health
		/// set to MaxHealth.
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="baseHealth"></param>
		public static void EnsureHealth(this Unit unit, int baseHealth)
		{
			unit.BaseHealth = baseHealth;
			unit.Health = (int)unit.MaxHealth;
		}

		/// <summary>
		/// Ensures that the unit has the given amount of basePower and Power
		/// set to MaxPower.
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="basePower"></param>
		public static void EnsurePower(this Unit unit, int basePower)
		{
			unit.BasePower = basePower;
			unit.Power = (int)unit.MaxPower;
		}

		/// <summary>
		///  Ensures that this Character is in the world (at some default location, if not already in world)
		/// </summary>
		public static void EnsureInWorldAndLiving(this Unit unit)
		{
			unit.EnsureInWorld();
			unit.EnsureLiving();
		}

		public static void CleanSweep(this Map map)
		{
			if (map.IsRunning)
			{
				// if already running, make sure that all other objects get removed
				map.AddMessageAndWait(new Message(() => {
					foreach (var obj in map.CopyObjects())
					{
						map.RemoveObjectNow(obj);
						Assert.IsNull(obj.Map.GetObject(obj.EntityId));
					}
				}));
			}
		}

		/// <summary>
		///  Ensures that this Character is in the world and there is no one else in the same map
		/// </summary>
		public static void EnsureAloneInWorld(this Unit unit)
		{
			if (unit.Map != null && unit.Map.IsRunning && unit.Map.ObjectCount > 1)
			{
				unit.Map.CleanSweep();
			}

			unit.EnsureInWorld();
		}

		/// <summary>
		///  Ensures that this Character is the only one in its map.
		///  Used for tests with update packets.
		/// </summary>
		public static void EnsureAloneInWorldAndLiving(this Unit unit)
		{
			unit.EnsureAloneInWorld();
			unit.EnsureLiving();
		}
		#endregion
	}
}