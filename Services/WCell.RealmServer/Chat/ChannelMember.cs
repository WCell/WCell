using WCell.Constants.Chat;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Chat
{
	/// <summary>
	/// Defines a member of a chat channel.
	/// </summary>
	public class ChannelMember
	{
		/// <summary>
		/// The member of the channel.
		/// </summary>
		public readonly IUser User;

		/// <summary>
		/// The member's channel flags.
		/// </summary>
		public ChannelMemberFlags Flags;

		/// <summary>
		/// Creates a new <see cref="ChannelMember" /> given the user.
		/// </summary>
		/// <param name="user">the user being represented</param>
		public ChannelMember(IUser user)
		{
			User = user;
		}

		/// <summary>
		/// Creates a new <see cref="ChannelMember" /> given the user and their flags.
		/// </summary>
		/// <param name="user">the user being represented</param>
		/// <param name="flags">the flags of the user</param>
		public ChannelMember(IUser user, ChannelMemberFlags flags)
		{
			User = user;
			Flags = flags;
		}

		/// <summary>
		/// Whether or not the user is the owner of the channel.
		/// </summary>
		public bool IsOwner
		{
			get { return Flags.HasFlag(ChannelMemberFlags.Owner); }
			set
			{
				if (value)
				{
					Flags |= ChannelMemberFlags.Owner;
				}
				else
				{
					Flags &= ~ChannelMemberFlags.Owner;
				}
			}
		}

		/// <summary>
		/// Whether or not the user is a moderator on the channel.
		/// </summary>
		public bool IsModerator
		{
            get { return Flags.HasFlag(ChannelMemberFlags.Moderator); }
			set
			{
				if (value)
				{
					Flags |= ChannelMemberFlags.Moderator;
				}
				else
				{
					Flags &= ~ChannelMemberFlags.Moderator;
				}
			}
		}

		/// <summary>
		/// Whether the user is voiced on the channel.
		/// </summary>
		public bool IsVoiced
		{
			get { return Flags.HasFlag(ChannelMemberFlags.Voiced); }
			set
			{
				if (value)
				{
					Flags |= ChannelMemberFlags.Voiced;
				}
				else
				{
					Flags &= ~ChannelMemberFlags.Voiced;
				}
			}
		}

		/// <summary>
		/// Whether the user is muted on the channel.
		/// </summary>
		public bool IsMuted
		{
			get { return Flags.HasFlag(ChannelMemberFlags.Muted); }
			set
			{
				if (value)
				{
					Flags |= ChannelMemberFlags.Muted;
				}
				else
				{
					Flags &= ~ChannelMemberFlags.Muted;
				}
			}
		}

		/// <summary>
		/// Whether the user is voice muted on the channel.
		/// </summary>
		public bool IsVoiceMuted
		{
            get { return Flags.HasFlag(ChannelMemberFlags.VoiceMuted); }
			set
			{
				if (value)
				{
					Flags |= ChannelMemberFlags.VoiceMuted;
				}
				else
				{
					Flags &= ~ChannelMemberFlags.VoiceMuted;
				}
			}
		}

		/// <summary>
		/// Operator overload for the greater-than operator.
		/// </summary>
		/// <param name="member">the first <see cref="ChannelMember" /></param>
		/// <param name="member2">the second <see cref="ChannelMember" /></param>
		/// <returns>true if the first member is greater than the second, based on role, ownership
		/// of the channel, and moderator status on the channel</returns>
		public static bool operator >(ChannelMember member, ChannelMember member2)
		{
			return (member.User.Role.IsStaff && member.User.Role > member2.User.Role) ||
				(!member2.IsOwner && (member.IsOwner || (!member2.IsModerator && member.IsModerator)));
		}

		/// <summary>
		/// Operator overload for the greater-than operator.
		/// </summary>
		/// <param name="member1">the first <see cref="ChannelMember" /></param>
		/// <param name="member2">the second <see cref="ChannelMember" /></param>
		/// <returns>true if the first member is greater than the second, based on role, ownership
		/// of the channel, and moderator status on the channel</returns>
		public static bool operator <(ChannelMember member1, ChannelMember member2)
		{
			return member2 > member1;
		}
	}
}
