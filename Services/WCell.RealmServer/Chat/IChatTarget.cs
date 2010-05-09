using WCell.Constants;
using WCell.Constants.Misc;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Chat
{
	/// <summary>
	/// Defines an object that can accept simple messages.
	/// </summary>
	public interface IGenericChatTarget
	{
		/// <summary>
		/// Sends a message to the target.
		/// </summary>
		/// <param name="message">the message to send</param>
		void SendMessage(string message);
	}

	/// <summary>
	/// Defines an object that can accept messages from other players in different languages.
	/// </summary>
	public interface IChatTarget : IGenericChatTarget
	{
		/// <summary>
		/// Send a message to the target.
		/// </summary>
		/// <param name="sender">the target of the message</param>
		/// <param name="message">the message to send</param>
		void SendMessage(IChatter sender, string message);
	}

	/// <summary>
	/// Defines an object that can actively chat.
	/// </summary>
	public interface IChatter : INamedEntity, IPacketReceivingEntity, IChatTarget
	{
		/// <summary>
		/// The chat tags of the object.
		/// </summary>
		ChatTag ChatTag { get; }

		/// <summary>
		/// The spoken language of the player.
		/// </summary>
		ChatLanguage SpokenLanguage { get; }
	}

	/// <summary>
	/// Helper class for chat-related extension methods and other misc. methods.
	/// </summary>
	public static class ChatHelper
	{
		/// <summary>
		/// Sends a system message to the target.
		/// </summary>
		/// <param name="target">the target being sent a system message</param>
		/// <param name="msg">the message to send</param>
		/// <param name="args">any arguments to be formatted in the message</param>
		public static void SendMessage(this IGenericChatTarget target, string msg, params object[] args)
		{
			target.SendMessage(string.Format(msg, args));
		}
	}
}
