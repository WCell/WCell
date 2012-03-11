using NLog;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.GameObjects.Handlers
{
    /// <summary>
    /// GO Type 19
    /// </summary>
    public class MailboxHandler : GameObjectHandler
    {
        private static Logger sLog = LogManager.GetCurrentClassLogger();

        public override bool Use(Character user)
        {
            if (!(user is Character))
            {
                return false;
            }
            var chr = (Character)user;
            chr.MailAccount.MailBox = m_go;
            return true;
        }
    }
}