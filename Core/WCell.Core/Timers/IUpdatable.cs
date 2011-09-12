using System;

namespace WCell.Core.Timers
{
	/// <summary>
	/// Defines the interface of an object that can be updated with respect to time.
	/// </summary>
	public interface IUpdatable
	{
		/// <summary>
		/// Updates the object.
		/// </summary>
		/// <param name="dt">the time since the last update in millis</param>
		void Update(int dt);
	}

	/// <summary>
	/// A simple wrapper that will execute a callback every time it is updated.
	/// </summary>
	public class SimpleUpdatable : IUpdatable
	{
		/// <summary>
		/// Creates a new <see cref="SimpleUpdatable" /> object.
		/// </summary>
		public SimpleUpdatable()
		{
		}

		/// <summary>
		/// Creates a new <see cref="SimpleUpdatable" /> object with the given callback.
		/// </summary>
		public SimpleUpdatable(Action callback)
		{
			Callback = callback;
		}

		/// <summary>
		/// The wrapped callback.
		/// </summary>
		public Action Callback;

		#region IUpdatable Members

		public void Update(int dt)
		{
			Callback();
		}

		#endregion
	}
}