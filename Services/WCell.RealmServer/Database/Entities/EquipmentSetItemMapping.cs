using WCell.Core;
using WCell.Database;

namespace WCell.RealmServer.Database.Entities
{
	[ShouldAutoMap]
	public class EquipmentSetItemMapping //TODO: Work out if there is any point even keeping this class?
	{
		private long Id
		{
			get;
			set;
		}

		//TODO: remove this comment if we can name it set instead.
		/// <summary>
		/// Cannot be named "Set" because
		/// NHibernate doesn't quote table names right now.
		/// </summary>
		public EquipmentSet Set { get; set; }

		public EntityId ItemEntityId { get; set; }
	}
}