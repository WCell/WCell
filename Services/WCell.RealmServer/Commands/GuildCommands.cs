using System.Linq;
using WCell.Constants;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Guilds;
using WCell.Util;
using WCell.Util.Commands;
using System.Collections.Generic;

namespace WCell.RealmServer.Commands
{
	public class GuildCommand : RealmServerCommand
	{
		protected override void Initialize()
		{
			Init("Guild");
		}

		#region Create
		public class CreateGuildCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Create", "C");
				EnglishParamInfo = "[-[n <leadername>]] <name>";
				EnglishDescription = "Create a guild with given name. " +
					"-n allows you to select the leader by name.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var mod = trigger.Text.NextModifiers();
				var leaderName = "";
				if (mod.Contains("n"))
				{
					leaderName = trigger.Text.NextWord();
				}
				var name = trigger.Text.NextWord().Trim();

				if (!GuildMgr.CanUseName(name))
				{
					trigger.Reply("Invalid name: " + name);
					return;
				}

				if (leaderName.Length > 0)
				{
					Events.RealmServer.IOQueue.AddMessage(() =>
					{
						var leaderRecord = CharacterRecord.GetRecordByName(leaderName);
						if (leaderRecord == null)
						{
							trigger.Reply("Character with name \"{0}\" does not exist.", leaderName);
							return;
						}
						CreateGuild(trigger, name, leaderRecord);
					});
				}
				else
				{
					CharacterRecord leaderRecord;
					var chr = trigger.Args.Target as Character;
					if (chr != null)
					{
						leaderRecord = chr.Record;
					}
					else
					{
						trigger.Reply("Could not create Guild. You did not select a Character to be the Guild leader. Use the -n switch to specify the leader by name.");
						return;
					}

					var guild = CreateGuild(trigger, name, leaderRecord);
					guild.Leader.Character = chr;
				}
			}

			Guild CreateGuild(CmdTrigger<RealmServerCmdArgs> trigger, string name, CharacterRecord record)
			{
				var guild = new Guild(record, name);

				trigger.Reply("Guild created");

				return guild;
			}
		}
		#endregion

		#region Join
		public class JoinGuildCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Add", "A");
				EnglishParamInfo = "[-[n <membername>]] <name>";
				EnglishDescription = "Let somebody join the guild with the given name. " +
					"-n allows you to select the new member by name.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var mod = trigger.Text.NextModifiers();
				var memberName = "";
				if (mod.Contains("n"))
				{
					memberName = trigger.Text.NextWord();
				}

				var name = trigger.Text.NextWord().Trim();
				var guild = GuildMgr.GetGuild(name);

				if (guild == null)
				{
					trigger.Reply("Guild does not exist: " + name);
					return;
				}

				if (memberName.Length > 0)
				{
					Events.RealmServer.IOQueue.AddMessage(() =>
					{
						var record = CharacterRecord.GetRecordByName(memberName);
						if (record == null)
						{
							trigger.Reply("Character with name \"{0}\" does not exist.", memberName);
							return;
						}
						guild.AddMember(record);
					});
				}
				else
				{
					var chr = trigger.Args.Target as Character;
					if (chr == null)
					{
						trigger.Reply("You did not select a valid member. Use the -n switch to specify the new member by name.");
						return;
					}

					guild.AddMember(chr);
				}
			}
		}
		#endregion

        #region Promote
        public class GuildPromoteCommand : SubCommand
        {
            protected override void Initialize()
            {
                Init("Promote", "P");
                EnglishParamInfo = "<guild name>";
                EnglishDescription = "Promotes a member of a guild with the given name to the next rank.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var name = trigger.Text.NextWord().Trim();
                var guild = GuildMgr.GetGuild(name);

                if (guild == null)
                {
                    trigger.Reply("Guild does not exist: " + name);
                    return;
                }

                var chr = trigger.Args.Target as Character;
                    if (chr == null)
                    {
                        trigger.Reply("You did not select a valid member.");
                        return;
                    }

                    if(chr.GuildMember.RankId > 0)
                        chr.GuildMember.RankId--;
            }
        }
        #endregion

		#region Leave
		public class LeaveGuildCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Leave", "L");
				EnglishParamInfo = "[-[n <membername>]] <name>";
				EnglishDescription = "Let somebody leave the guild with the given name. " +
					"-n allows you to select the member by name.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var mod = trigger.Text.NextModifiers();
				var memberName = "";
				if (mod.Contains("n"))
				{
					memberName = trigger.Text.NextWord();
				}

				var name = trigger.Text.NextWord().Trim();
				var guild = GuildMgr.GetGuild(name);

				if (guild == null)
				{
					trigger.Reply("Guild does not exist: " + name);
					return;
				}

				if (memberName.Length > 0)
				{
					if (!guild.RemoveMember(memberName))
					{
						trigger.Reply("{0} is not a member of \"{1}\".", memberName, guild.Name);
						return;
					}
				}
				else
				{
					var chr = trigger.Args.Target as Character;
					if (chr == null)
					{
						trigger.Reply("You did not select a valid member. Use the -n switch to specify the new member by name.");
						return;
					}

					if (chr.GuildMember == null || !guild.RemoveMember(chr.GuildMember))
					{
						trigger.Reply("{0} is not a member of \"{1}\".", chr.Name, guild.Name);
						return;
					}
				}

				trigger.Reply("Done.");
			}
		}
		#endregion

		#region List
		public class GuildListCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("List", "L");
				EnglishParamInfo = "[<searchterm>]";
				EnglishDescription = "Lists all Guild or only those with a matching name.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				IEnumerable<Guild> guilds;

				if (trigger.Text.HasNext)
				{
					var searchTerm = trigger.Text.Remainder;
					guilds = GuildMgr.GuildsById.Values.Where(gld => gld.Name.ContainsIgnoreCase(searchTerm));
				}
				else
				{
					guilds = GuildMgr.GuildsById.Values;
				}

				var count = guilds.Count();
				if (count == 0)
				{
					trigger.Reply("No Guilds found.");
				}
				else
				{
					trigger.Reply("{0} Guilds found:", count);
					foreach (var guild in guilds)
					{
						trigger.Reply(guild.ToString());
					}
				}
			}
		}
		#endregion

		#region Disband
		public class DisbandGuildCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Disband", "D");
				EnglishParamInfo = "<name> <name confirm>";
				EnglishDescription = "Disbands the guild with the given name.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var mod = trigger.Text.NextModifiers();
				var name = trigger.Text.NextWord().Trim();
				var nameConfirm = trigger.Text.NextWord().Trim();

				if (nameConfirm != name)
				{
					trigger.Reply("The confirmation name did not match the name. Please type the name twice.");
					return;
				}

				var guild = GuildMgr.GetGuild(name);
				if (guild == null)
				{
					trigger.Reply("Guild does not exist: " + name);
					return;
				}

				guild.Disband();
				trigger.Reply("{0} has been disbanded.", guild.Name);
			}
		}
		#endregion

		#region Chat
		public class GuildChatCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Say", "Msg");
				EnglishParamInfo = "[-n <name>] <text>";
				EnglishDescription = "Sends the given text to your, your target's or the specified Guild. -n can be ommited if not used by/on a Character.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				Guild guild;
				var mod = trigger.Text.NextModifiers();
				var target = trigger.Args.Target;
				if (!(target is Character) || mod == "n")
				{
					var name = trigger.Text.NextWord().Trim();
					guild = GuildMgr.GetGuild(name);
					if (guild == null)
					{
						trigger.Reply("Invalid Guild: " + name);
						return;
					}
				}
				else
				{
					guild = ((Character)target).Guild;
					if (guild  == null)
					{
						trigger.Reply(target + " is not a member of any Guild.");
						return;
					}
				}

				guild.SendMessage(trigger.Args.User, trigger.Text.Remainder);
			}
		}
		#endregion
	}
}