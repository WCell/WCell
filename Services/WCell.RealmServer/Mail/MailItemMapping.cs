using System;
using Castle.ActiveRecord;
using WCell.RealmServer.Mail;
using WCell.RealmServer.Mail;

namespace WCell.RealmServer.Database
{
    [ActiveRecord("MailItemMapping", Access = PropertyAccess.Property)]
    public class MailItemMapping : ActiveRecordBase<MailItemMapping>
    {
        [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
		public long ItemRecordGuid
		{
			get;
			set;
		}

		[BelongsTo]
        public MailMessage MailMessage
        {
            get;
            set;
        }

        [Version(UnsavedValue = "null")]
        public DateTime? LastModofiedOn
        { 
            get; 
            set;
        }
    }
}