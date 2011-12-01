using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Graphics;
using WCell.Util.ObjectPools;

namespace WCell.Terrain
{
	/// <summary>
	/// Helps alleviate the stress on the LOH caused by allocating vertex and index lists
	/// </summary>
	public static class LargeObjectPools
	{
		public static readonly ObjectPool<List<Vector3>> Vector3ListPool = new ObjectPool<List<Vector3>>(() => new List<Vector3>(500));

		public static readonly ObjectPool<List<int>> IndexListPool = new ObjectPool<List<int>>(() => new List<int>(500));

	}
}
