using System;
using System.Linq;
using WCell.Constants.Quests;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Quests;
using WCell.Util;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
	public class QuestCommand : RealmServerCommand
	{
		protected override void Initialize()
		{
			Init("Quest");
			EnglishParamInfo = "";
			EnglishDescription = "Provides a set of commands to dynamically change status of Quests and more.";
		}

		#region Reset
		public class ResetQuestCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Reset", "Start");
				EnglishParamInfo = "<questid>";
				EnglishDescription = "Removes all progress of the given Quest (if present) and starts it (again).";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var target = trigger.Args.Target;

				if (!(target is Character))
				{
					trigger.Reply("Invalid target: {0} - Character-target required.", target);
				}
				else
				{
					var chr = (Character)target;
					var id = trigger.Text.NextUInt(0);

					QuestTemplate templ = null;
					if (id > 0)
					{
						templ = QuestMgr.GetTemplate(id);
					}

					if (templ == null)
					{
						trigger.Reply("Invalid QuestId: {0}", id);
					}
					else
					{
						if (!chr.QuestLog.RemoveFinishedQuest(id))
						{
							// if its not already been finished, maybe it's still in progress?
							chr.QuestLog.Cancel(id);
						}

						var quest = chr.QuestLog.AddQuest(templ);
						if (quest == null)
						{
							trigger.Reply("Could not add Quest: " + templ);
						}
						else
						{
							trigger.Reply("Quest added: " + templ);
						}
					}
				}
			}
		}
		#endregion

		#region Cancel
		public class CancelQuestCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Remove", "Cancel");
				EnglishParamInfo = "<questid>";
				EnglishDescription = "Removes the given finished or active Quest.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var target = trigger.Args.Target;

				if (!(target is Character))
				{
					trigger.Reply("Invalid target: {0} - Character-target required.", target);
				}
				else
				{
					var chr = (Character)target;
					var id = trigger.Text.NextUInt(0);

					QuestTemplate quest = null;
					if (id > 0)
					{
						quest = QuestMgr.GetTemplate(id);
					}

					if (quest == null)
					{
						trigger.Reply("Invalid QuestId: {0}", id);
					}
					else
					{
						if (!chr.QuestLog.RemoveFinishedQuest(id))
						{
							// if its not already been finished, maybe it's still in progress?
							chr.QuestLog.Cancel(id);
							trigger.Reply("Removed active quest: {0}", quest);
						}
						else
						{
							trigger.Reply("Removed finished quest: {0}", quest);
						}
					}
				}
			}
		}
		#endregion

		#region GiveReward
		public class GiveQuestRewardCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("GiveReward", "Reward");
				EnglishParamInfo = "<questid> [<choiceSlot>]";
				EnglishDescription = "Gives the reward of the given quest to the Character. " +
					"The optional choiceSlot determines the choosable item (if any)";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var target = trigger.Args.Target;

				if (!(target is Character))
				{
					trigger.Reply("Invalid target: {0} - Character-target required.", target);
				}
				else
				{
					var chr = (Character)target;
					var id = trigger.Text.NextUInt(0);
					var slot = trigger.Text.NextUInt(0);

					QuestTemplate quest = null;
					if (id > 0)
					{
						quest = QuestMgr.GetTemplate(id);
					}

					if (quest == null)
					{
						trigger.Reply("Invalid QuestId: {0}", id);
					}
					else
					{
						quest.GiveRewards(chr, slot);
						trigger.Reply("Done.");
					}
				}
			}
		}
		#endregion

		#region Goto
		public class QuestGotoCommand : QuestSubCmd
		{
			protected override void Initialize()
			{
				Init("Goto");
				EnglishParamInfo = "<id>[ <starter index>[ <template index>]";
				EnglishDescription = "Teleports the target to the first starter of the given quest or the one at the given index.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var target = trigger.Args.Target;
				if (target == null)
				{
					trigger.Reply("No target given.");
					return;
				}
				var quest = GetTemplate(trigger);

				if (quest != null)
				{
					var starters = quest.Starters;
					if (starters.Count == 0)
					{
						trigger.Reply("Quest {0} has no Starters.", quest);
					}
					else
					{
						trigger.Reply("Found {0} Starters: " + starters.ToString(", "), starters.Count);
						int index;
						if (trigger.Text.HasNext)
						{
							index = trigger.Text.NextInt(-1);
							if (index < 0 || index >= starters.Count)
							{
								trigger.Reply("Invalid starter-index.");
								return;
							}
						}
						else
						{
							index = 0;
						}

						var starter = starters[index];
						var templates = starter.GetInWorldTemplates();

						if (templates == null)
						{
							trigger.Reply("Quest starters are not accessible.");
						}
						else
						{
							trigger.Reply("Found {0} templates: " + templates.ToString(", "), templates.Length);

							int templIndex;
							if (trigger.Text.HasNext)
							{
								templIndex = trigger.Text.NextInt(-1);
								if (templIndex < 0 || templIndex >= templates.Length)
								{
									trigger.Reply("Invalid template-index.");
									return;
								}
							}
							else
							{
								templIndex = 0;
							}

							var template = templates[templIndex];

							if (target.TeleportTo(template))
							{
								if (templates.Length > 1 || starters.Count > 1)
								{
									trigger.Reply("Going to {0} ({1})", starter, template);
								}
							}
							else
							{
								trigger.Reply("Template is located in {0} ({1}) and not accessible.",
									template.MapId, template.Position);
							}
						}
					}
				}
			}
		}
		#endregion

		#region Lookup
		public class QuestLookupCommand : QuestSubCmd
		{
			protected override void Initialize()
			{
				Init("Lookup", "Find");
				EnglishParamInfo = "<search terms>";
				EnglishDescription = "Lists all quests matching the given search term.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var searchTerms = trigger.Text.Remainder.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				var templates = QuestMgr.Templates.Where(templ => templ != null &&
					searchTerms.Iterate(term => !templ.DefaultTitle.ContainsIgnoreCase(term)));

				var count = templates.Count();
				trigger.Reply("Found {0} matching Quests.", count);

				var cap = 100;
				if (count > cap)
				{
					trigger.Reply("Cannot display more than " + cap + " matches at a time.");
				}
				else
				{
					foreach (var templ in templates)
					{
						trigger.Reply(templ.ToString());
					}
				}
			}
		}
		#endregion

		public abstract class QuestSubCmd : SubCommand
		{
			public QuestTemplate GetTemplate(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var id = GetQuestId(trigger);
				if (id != 0)
				{
					var template = QuestMgr.GetTemplate(id);

					if (template == null)
					{
						trigger.Reply("Invalid Id - Use: 'Quest Lookup <search term>' to find quest-ids.");
					}
					return template;
				}
				return null;
			}

			public uint GetQuestId(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var id = trigger.Text.NextUInt(0);
				if (id == 0)
				{
					trigger.Reply("Invalid Id - Use: 'Quest Lookup <search term>' to find quest-ids.");
					return 0;
				}
				return id;
			}
		}
	}

	#region SendQuest
	public class SendQuestCommand : RealmServerCommand
	{

		protected override void Initialize()
		{
			Init("QuestSend", "SendQuest");
			EnglishParamInfo = "";
			EnglishDescription = "Provides a set of debug commands to send quest packets dynamically.";
		}

		public override ObjectTypeCustom TargetTypes
		{
			get { return ObjectTypeCustom.Player; }
		}

		public class SendQuestInvalidCommand : SubCommand
		{
			protected SendQuestInvalidCommand() { }

			protected override void Initialize()
			{
				Init("Invalid");
				EnglishParamInfo = "<reason>";
				EnglishDescription = "Sends the SendQuestInvalid packet with the given reason";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var value = trigger.Text.NextEnum(QuestInvalidReason.Ok);
				var target = trigger.Args.Character.Target as Character;
				if (target != null)
				{
					QuestHandler.SendQuestInvalid(target, value);
				}
				trigger.Reply("Done.");
			}
		}

		public class SendQuestPushResultCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("PushResult");
				EnglishParamInfo = "<reason>";
				EnglishDescription = "Sends the SendQuestPushResult packet with the given reason, currently sends from triggering char";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var value = trigger.Text.NextEnum(QuestPushResponse.Busy);
				var target = trigger.Args.Character.Target as Character;
				if (target != null)
				{
					QuestHandler.SendQuestPushResult(trigger.Args.Character, value, target);
				}
				trigger.Reply("Done.");
			}
		}

		public class SendQuestGiverQuestDetailsCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("GiverQuestDetails");
				EnglishParamInfo = "<quest id>";
				EnglishDescription = "Sends the QuestGiverQuestDetails packet with the given quest id";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var qt = QuestMgr.GetTemplate(trigger.Text.NextUInt());
				if (qt != null)
				{
					var questGiver = trigger.Args.Character as IQuestHolder;
					var questReceiver = (Character)trigger.Args.Character.Target;
					if (questGiver != null)
					{
						QuestHandler.SendDetails(questGiver, qt, questReceiver, false);
					}
				}
				trigger.Reply("Done.");
			}
		}

		public class SendQuestGiverQuestQuestComplete : SubCommand
		{
			protected override void Initialize()
			{
				Init("GiverQuestComplete");
				EnglishParamInfo = "<quest id>";
				EnglishDescription = "Sends the QuestGiverQuestComplete packet with the given quest id";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var qt = QuestMgr.GetTemplate(trigger.Text.NextUInt());
				if (qt != null)
				{
					var questReceiver = trigger.Args.Character.Target as Character;
					if (questReceiver != null)
					{
						QuestHandler.SendComplete(qt, questReceiver);
					}
				}
				trigger.Reply("Done.");
			}
		}
	}
	#endregion
}