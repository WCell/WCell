namespace WCell.Util.ReflectionUtil
{
	public class ReflectionMember
	{
		public readonly string FullName;

		public ReflectionMember(string fullName)
		{
			// TODO
			FullName = fullName;
			//Parse();
		}

		public object[] Arguments
		{
			get;
			protected set;
		}
	}
}