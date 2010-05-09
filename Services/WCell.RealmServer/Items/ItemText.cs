using NHibernate.Criterion;

namespace WCell.RealmServer.Database
{
	public partial class ItemText
	{
		public static ItemText FindItemTextById(long itemTextId)
		{
			return FindOne(Restrictions.Eq("ItemTextId", itemTextId));
		}
	}
}