using WCell.Constants.Misc;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Chat
{
    public static partial class ChatMgr
    {
        /// <summary>
        /// Delegate used for parsing an incoming chat message.
        /// </summary>
        /// <param name="type">the type of chat message indicated by the client</param>
        /// <param name="language">the chat language indicated by the client</param>
        /// <param name="packet">the actual chat message packet</param>
        public delegate void ChatParserDelegate(Character sender, ChatMsgType type, ChatLanguage language, RealmPacketIn packet);

        /// <summary>
        /// Delegate used for passing chat notification information.
        /// </summary>
        /// <param name="chatter">the person hatting</param>
        /// <param name="message">the chat message</param>
        /// <param name="lang">the language of the message</param>
        /// <param name="chatType">the type of message</param>
        /// <param name="target">the target of the message (channel, whisper, etc)</param>
        public delegate void ChatNotifyDelegate(IChatter chatter, string message, ChatLanguage lang,
            ChatMsgType chatType, IGenericChatTarget target);

        /// <summary>
        /// Event for chat notifications.
        /// </summary>
        public static event ChatNotifyDelegate MessageSent;
    }
}