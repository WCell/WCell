namespace WCell.Util.DB
{
	public static class Patterns
	{
		public const string ArrayFieldIndex = "{#}";

		public static string Compile(string pattern, int i)
		{
			return pattern.Replace(ArrayFieldIndex, i.ToString());
		}
	}
}