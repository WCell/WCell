using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Calendars
{
    class CalendarEventInvite
    {
        //private Character m_owner;
        private uint m_id;
        private CalendarEvent m_event;
        //private CalendarInviteStatus m_inviteStatus;
        //private byte m_unknown1;
        //private byte m_unknown2;
        //private byte m_unknown3;
        private Character m_inviter;
        //private CalendarModType shouldbehere;

        public uint Id
        {
            get { return m_id; }
            set { m_id = value; }
        }
        public CalendarEvent EventId
        {
            get { return m_event; }
            set { m_event = value; }
        }
        public Character Inviter
        {
            get { return m_inviter; }
            set { m_inviter = value; }
        }
    }
}