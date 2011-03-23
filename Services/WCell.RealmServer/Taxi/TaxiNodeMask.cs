using System;
using WCell.Util.Logging;
using WCell.Constants;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Taxi
{
	public class TaxiNodeMask
	{
		private static Logger sLog = LogManager.GetCurrentClassLogger();

		#region Fields

		private uint[] fields;

		#endregion

		#region Properties

		public uint[] Mask
		{
			get { return fields; }
			internal set { fields = value; }
		}

		#endregion

		#region Constructors

		public TaxiNodeMask()
		{
			fields = new uint[TaxiConstants.TaxiMaskSize];
		}

		public TaxiNodeMask(uint[] mask)
		{
			if (mask.Length < TaxiConstants.TaxiMaskSize)
			{
				Array.Resize(ref mask, TaxiConstants.TaxiMaskSize);
			}
			fields = mask;
		}

		#endregion

		public void Activate(PathNode node)
		{
			Activate(node.Id);
		}

		public void Activate(uint nodeId)
		{
			var field = fields[(nodeId / 32)];
			field |= (uint)(1 << ((int)(nodeId % 32)));
			fields[(nodeId / 32)] = field;

		}

		public bool IsActive(PathNode node)
		{
		    return node != null && IsActive(node.Id);
		}

	    public bool IsActive(uint nodeId)
		{
			uint field = Mask[(nodeId / 32)];
			uint mask = (uint)(1 << ((int)(nodeId % 32)));
			return ((mask & field) == mask);
		}
	}
}