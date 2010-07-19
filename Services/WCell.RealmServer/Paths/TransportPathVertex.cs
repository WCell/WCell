using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.World;
using WCell.Util.Graphics;

namespace WCell.Core.Paths
{
	public struct TransportPathVertex
	{
		public MapId MapId;

		public Vector3 Position;

		public bool Teleport;

		public uint Time;
	}
}