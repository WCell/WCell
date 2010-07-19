namespace WCell.Intercommunication.DataTypes
{
	public interface IRoleGroup
	{
		/// <summary>
		/// The name of the role.
		/// </summary>
		string Name
		{
			get;
		}

		/// <summary>
		/// What kind of status this roll represents
		/// </summary>
		RoleStatus Status
		{
			get;
		}

		/// <summary>
		/// Whether the User may login, even if the server is full.
		/// </summary>
		bool MaySkipAuthQueue
		{
			get;
		}

		/// <summary>
		/// Whether the player's chat will be scrambled
		/// </summary>
		bool ScrambleChat
		{
			get;
		}

		bool IsStaff
		{
			get;
		}

		/// <summary>
		/// Whether or not the role makes the player a GM.
		/// </summary>
		bool AppearAsGM
		{
			get;
		}

		/// <summary>
		/// Whether or not the role makes the player a QA.
		/// </summary>
		bool AppearAsQA
		{
			get;
		}

		/// <summary>
		/// The actual Rank of this Role
		/// </summary>
		int Rank
		{
			get;
		}

		/// <summary>
		/// Whether this Role is allowed to call commands on others (eg. using double prefix)
		/// </summary>
		bool CanUseCommandsOnOthers
		{
			get;
		}

		/// <summary>
		/// Whether this Role sees ticket information and can handle tickets
		/// </summary>
		bool CanHandleTickets
		{
			get;
		}

		/// <summary>
		/// A list of the other roles the role inherits from, permissions-wise.
		/// </summary>
		string[] InheritanceList
		{
			get;
		}

		/// <summary>
		/// A list of the names of all allowed Commands.
		/// </summary>
		string[] CommandNames
		{
			get;
		}
	}
}