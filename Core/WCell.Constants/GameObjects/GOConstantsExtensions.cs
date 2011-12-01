namespace WCell.Constants.GameObjects
{
	public static class GOConstantsExtensions
	{
		public static bool HasAnyFlag(this GameObjectFlags flags, GameObjectFlags flags2)
		{
			return (flags & flags2) != 0;
		}
	}
}
