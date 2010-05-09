using WCell.Constants;

namespace WCell.RealmServer.Items
{
	public struct DamageInfo
	{
		public static readonly DamageInfo[] EmptyArray = new DamageInfo[0];

		public DamageSchoolMask School;
		public float Minimum;
		public float Maximum;

		public DamageInfo(DamageSchoolMask school, float min, float max)
		{
			School = school;
			Minimum = min;
			Maximum = max;
		}

		//public void WriteTo(XElement element)
		//{
		//    if (School == DamageSchoolMask.None &&
		//        Minimum == 0f &&
		//        Maximum == 0f)
		//        return;

		//    element.Add(
		//        new XElement("Damage",
		//            new XAttribute("Type", (uint)School),
		//            new XElement("Min", Minimum),
		//            new XElement("Max", Maximum))
		//        );

		//}
	}
}