namespace WCell.Core.Initialization
{
	public interface IInitializationInfo
	{
		string Name { get; }

		bool IsRequired { get; }
	}
}