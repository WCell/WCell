using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace WCell.Intercommunication.DataTypes
{
	public enum RoleStatus
	{
		Player,
		Staff,
		Admin
	}

	[Serializable]
	[DataContract]
	public class RoleGroupInfo
	{
		/// <summary>
		/// Represents the highest role that has been loaded (usually: Owner).
		/// </summary>
		public static RoleGroupInfo HighestRole;

		/// <summary>
		/// Represents the lowest role that has been loaded (usually: Guest).
		/// </summary>
		public static RoleGroupInfo LowestRole;

		/// <summary>
		/// Represents the command name that allows all commands for a Role.
		/// </summary>
		public const string AllCommands = "*";

		/// <summary>
		/// Represents the command name that allows all Commands that belong to the Role's status by default.
		/// </summary>
		public const string StatusCommands = "#";

		public static List<RoleGroupInfo> CreateDefaultGroups()
		{
			var groups = new List<RoleGroupInfo>();

			var allCommands = new[] { AllCommands };
			var defaultCommands = new[] { StatusCommands };

			groups.Add(new RoleGroupInfo("Guest", 0, RoleStatus.Player, false, false, false, false, false, true,
				defaultCommands, defaultCommands));

			groups.Add(new RoleGroupInfo("Player", 1, RoleStatus.Player, false, false, false, false, false, true, new[] {
					"Guest"
				}, defaultCommands));
			groups.Add(new RoleGroupInfo("Vip", 5, RoleStatus.Player, false, false, false, false, true, true, new[] {
					"Player"
				}, defaultCommands));
			groups.Add(new RoleGroupInfo("QA", 100, RoleStatus.Staff, false, true, false, true, true, false, new[] {
					"Vip"
				}, defaultCommands));
			groups.Add(new RoleGroupInfo("GM", 500, RoleStatus.Staff, true, false, true, true, true, false, new[] {
					"QA"
				}, defaultCommands));
			groups.Add(new RoleGroupInfo("Developer", 1000, RoleStatus.Admin, true, false, true, true, true, false, new[] {
					"GM"
				}, defaultCommands));
			groups.Add(new RoleGroupInfo("Admin", 5000, RoleStatus.Admin, true, false, true, true, true, false, new[] {
					"GM"
				}, defaultCommands));
			groups.Add(new RoleGroupInfo("Owner", 10000, RoleStatus.Admin, true, false, true, true, true, false, new[] {
					"Admin"
				}, allCommands));
			return groups;
		}

		int m_rank;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public RoleGroupInfo()
		{
			Name = "";
			Rank = 0;
			InheritanceList = new string[0];
			CommandNames = new string[0];
		}

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="roleName">the name of the role</param>
		/// <param name="gm">whether or not this role makes you a GM</param>
		/// <param name="qa">whether or not this role makes you a QA</param>
		/// <param name="inherits">the other roles this role inherits from</param>
		public RoleGroupInfo(string roleName,
			int rank, RoleStatus status, bool gm, bool qa, bool canCommandOthers, bool canHandleTickets,
			bool maySkipAuthQueue, bool scrambleChat,
			string[] inherits)
			: this(roleName, rank, status, gm, qa, canCommandOthers, canHandleTickets, 
			maySkipAuthQueue, scrambleChat, inherits, null)
		{
		}

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="roleName">the name of the role</param>
		/// <param name="gm">whether or not this role makes you a GM</param>
		/// <param name="qa">whether or not this role makes you a QA</param>
		/// <param name="inherits">the other roles this role inherits from</param>
		public RoleGroupInfo(string roleName,
			int rank, RoleStatus status, bool gm, bool qa, bool canCommandOthers, bool canHandleTickets,
			bool maySkipAuthQueue, bool scrambleChat,
			string[] inherits, string[] commands)
		{
			Name = roleName;
			Rank = rank;
			Status = status;
			AppearAsGM = gm;
			AppearAsQA = qa;
			InheritanceList = inherits;
			CommandNames = commands;
			CanUseCommandsOnOthers = canCommandOthers;
			CanHandleTickets = canHandleTickets;
			MaySkipAuthQueue = maySkipAuthQueue;
			ScrambleChat = scrambleChat;

			if (HighestRole == null || HighestRole.Rank < rank)
			{
				HighestRole = this;
			}

			if (LowestRole == null || LowestRole.Rank > m_rank)
			{
				LowestRole = this;
			}
		}

		#region Properties

		/// <summary>
		/// The name of the role.
		/// </summary>
		[XmlAttribute]
		[DataMember]
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		/// What kind of status this roll represents
		/// </summary>
		[XmlAttribute]
		[DataMember]
		public RoleStatus Status
		{
			get;
			set;
		}

		/// <summary>
		/// Whether the User may login, even if the server is full.
		/// </summary>
		[XmlAttribute]
		[DataMember]
		public bool MaySkipAuthQueue
		{
			get;
			set;
		}

		/// <summary>
		/// Whether the player's chat will be scrambled
		/// </summary>
		[XmlAttribute]
		[DataMember]
		public bool ScrambleChat
		{
			get;
			set;
		}

		public bool IsStaff { get { return Status >= RoleStatus.Staff; } }

		/// <summary>
		/// Whether or not the role makes the player a GM.
		/// </summary>
		[XmlAttribute]
		[DataMember]
		public bool AppearAsGM
		{
			get;
			set;
		}

		/// <summary>
		/// Whether or not the role makes the player a QA.
		/// </summary>
		[XmlAttribute]
		[DataMember]
		public bool AppearAsQA
		{
			get;
			set;
		}

		/// <summary>
		/// The actual Rank of this Role
		/// </summary>
		[XmlAttribute]
		[DataMember]
		public int Rank
		{
			get
			{
				return m_rank;
			}
			set
			{
				m_rank = value;

				if (HighestRole == null || HighestRole.Rank < m_rank)
				{
					HighestRole = this;
				}

				if (LowestRole == null || LowestRole.Rank > m_rank)
				{
					LowestRole = this;
				}
			}
		}

		/// <summary>
		/// Whether this Role is allowed to call commands on others (eg. using double prefix)
		/// </summary>
		[XmlAttribute]
		[DataMember]
		public bool CanUseCommandsOnOthers
		{
			get;
			set;
		}

		/// <summary>
		/// Whether this Role sees ticket information and can handle tickets
		/// </summary>
		[XmlAttribute]
		[DataMember]
		public bool CanHandleTickets
		{
			get;
			set;
		}

		/// <summary>
		/// A list of the other roles the role inherits from, permissions-wise.
		/// </summary>
		[XmlArray("Inheritance")]
		[XmlArrayItem("InheritsFrom")]
		[DataMember]
		public string[] InheritanceList
		{
			get;
			set;
		}

		/// <summary>
		/// A list of the names of all allowed Commands.
		/// </summary>
		[XmlArray("Commands")]
		[XmlArrayItem("Command")]
		[DataMember]
		public string[] CommandNames
		{
			get;
			set;
		}

		#endregion

		public override string ToString()
		{
			return Name + " (Rank: " + Rank + ")";
		}
	}
}