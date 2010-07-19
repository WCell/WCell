using System.IO;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Network
{
	public  static class NetworkExtensions
	{
		/// <summary>
		/// Writes a <see cref="Vector3" /> to the stream.
		/// </summary>
		/// <param name="point">the vector to write</param>
		public static void Write(this BinaryWriter writer, Vector3 point)
		{
			writer.Write(point.X);
			writer.Write(point.Y);
			writer.Write(point.Z);
		}
	}
}