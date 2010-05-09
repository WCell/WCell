namespace WCell.Core.Paths
{
	public interface IPathVertex : IHasPosition
	{
		uint Id { get; }

		float Orientation { get; }

		uint WaitTime { get; }

		float GetDistanceToNext();
	}
}