using System;
using WCell.Constants.Login;

namespace WCell.RealmServer.Server
{
    public class ServerTypeAttribute : Attribute
    {
        private readonly RealmServerType m_ServerType;

        public ServerTypeAttribute(RealmServerType serverServerType)
        {
            m_ServerType = serverServerType;
        }

        public RealmServerType ServerServerType
        {
            get
            {
                return m_ServerType;
            }
        }
    }
}
