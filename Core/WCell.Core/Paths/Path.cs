using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using WCell.Util.Graphics;

namespace WCell.Core.Paths
{
	public class Path : LinkedList<Vector3>
	{
		public Path()
		{
		}

		public Path(IEnumerable<Vector3> collection) : base(collection)
		{
		}

		protected Path(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public Path(params Vector3[] path)
		{
			foreach (var pos in path)
			{
				AddLast(pos);
			}
		}
	}
}