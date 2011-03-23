using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spells.Targeting
{
	public delegate void TargetAdder(SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason);
	public delegate void TargetFilter(SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failReason);
	public delegate int TargetEvaluator(SpellEffectHandler effectHandler, WorldObject target);

	/// <summary>
	/// 
	/// </summary>
	public class TargetDefinition
	{
		//public readonly bool CheckInRange;
		public readonly TargetAdder Adder;
		public TargetFilter Filter;

		public TargetDefinition(TargetAdder adder, params TargetFilter[] filters)
		{
			Adder = adder;
			if (filters != null)
			{
				foreach (var filter in filters)
				{
					AddFilter(filter);
				}
			}
		}

		internal void Collect(SpellTargetCollection targets, ref SpellFailedReason failReason)
		{
			if (Adder != null)
			{
				Adder(targets, Filter, ref  failReason);
			}
		}

		/// <summary>
		/// Composites the given filter into the existing filter
		/// </summary>
		public void AddFilter(TargetFilter filter)
		{
			if (Filter == null)
			{
				Filter = filter;
			}
			else
			{
				// create new filter that filters through both filters
				var oldFilter = Filter;
				Filter = (SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failReason) =>
				{
					oldFilter(effectHandler, target, ref failReason);
					if (failReason == SpellFailedReason.Ok)
					{
						filter(effectHandler, target, ref failReason);
					}
				};
			}
		}

		public override bool Equals(object obj)
		{
			if (obj is TargetDefinition)
			{
				var def2 = (TargetDefinition)obj;
				return def2.Adder == Adder && def2.Filter == Filter;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Adder.GetHashCode() * Filter.GetHashCode();
		}
	}
}