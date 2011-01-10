using System;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Factions;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Mail;
using WCell.Util.Data;
using WCell.Core.Database;

namespace WCell.RealmServer.Achievement
{
    [DataHolder]
    public class AchievementReward : IDataHolder
    {
        public uint AchievementEntryId;
        public GenderType Gender;
        public TitleId AllianceTitle;
        public TitleId HordeTitle;

        // Mail
        public ItemId Item;
        public NPCId Sender;

        [Persistent((int)ClientLocale.End)]
        public string[] Subjects;

        [Persistent((int)ClientLocale.End)]
        public string[] Bodies;

        /// <summary>
        /// Subject of the mail.
        /// </summary>
        [NotPersistent]
        public string DefaultSubject
        {
            get { return Subjects != null ? Subjects.LocalizeWithDefaultLocale() : "[unknown]"; }
        }

        /// <summary>
        /// Body of the mail.
        /// </summary>
        [NotPersistent]
        public string DefaultBody
        {
            get { return Bodies != null ? Bodies.LocalizeWithDefaultLocale() : "[unknown]"; }
        }

        public void GiveReward(Character character)
        {
            // Wrong gender
            if (character.Gender != Gender && Gender != GenderType.Neutral)
                return;

            if (character.FactionGroup == FactionGroup.Alliance && AllianceTitle != 0)
            {
                character.SetTitle(AllianceTitle,false);
            }
            else if (character.FactionGroup == FactionGroup.Horde && HordeTitle != 0)
            {
                character.SetTitle(HordeTitle, false);
            }

            if (Item != 0)
            {
				var mailMessage = new MailMessage(Subjects.Localize(character.Locale), Bodies.Localize(character.Locale))
            	                  	{
            	                  		ReceiverId = character.EntityId.Low,
            	                  		DeliveryTime = DateTime.Now,
            	                  		SendTime = DateTime.Now,
            	                  		ExpireTime = DateTime.Now.AddMonths(1),
            	                  		MessageStationary = MailStationary.Normal
            	                  	};
            	mailMessage.AddItem(Item);
            	MailMgr.SendMail(mailMessage);
            }
        }

        public void FinalizeDataHolder()
        {
            var achievementEntry = AchievementMgr.AchievementEntries[AchievementEntryId];
            if(achievementEntry == null)
            {
                ContentMgr.OnInvalidDBData("{0} had an invalid AchievementEntryId.", this);
                return;
            }
            achievementEntry.Rewards.Add(this);
        }
    }
}
