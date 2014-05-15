using FluentNHibernate.Mapping;
using WCell.RealmServer.Mail;

namespace WCell.RealmServer.Database.Mappings
{
	public class MailMessageMap : ClassMap<MailMessage>
	{
		public MailMessageMap()
		{
			Not.LazyLoad();
			Id(x => x.Guid).GeneratedBy.Assigned();
			Map(x => x.ReceiverId).Not.Nullable();
			Map(x => x.SenderId).Not.Nullable();
			Map(x => x.IncludedMoney).Not.Nullable();
			Map(x => x.CashOnDelivery).Not.Nullable();
			Map(x => x.MessageType).Not.Nullable();
			Map(x => x.MessageStationary).Not.Nullable();
			Map(x => x.Subject).Length(512).Not.Nullable();
			Map(x => x.Body).Length(1024*8).Not.Nullable();
			Map(x => x.TextId).Not.Nullable();
			Map(x => x.SendTime).Not.Nullable();
			Map(x => x.DeliveryTime).Not.Nullable();
			Map(x => x.ReadTime);
			Map(x => x.ExpireTime).Not.Nullable();
			Map(x => x.DeletedTime);
			Map(x => x.CopiedToItem).Not.Nullable();
			Map(x => x.IncludedItemCount);
		}
	}
}
